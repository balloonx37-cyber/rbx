if Nexus then Nexus:Stop() end

if not game:IsLoaded() then
    task.delay(60, function()
        if NoShutdown then return end

        if not game:IsLoaded() then
            return game:Shutdown()
        end

        local Code = game:GetService'GuiService':GetErrorCode().Value

        if Code >= Enum.ConnectionError.DisconnectErrors.Value then
            return game:Shutdown()
        end
    end)
    
    game.Loaded:Wait()
end

local Nexus = {}
local WSConnect = syn and syn.websocket.connect or
    (Krnl and (function() repeat task.wait() until Krnl.WebSocket and Krnl.WebSocket.connect return Krnl.WebSocket.connect end)()) or
    WebSocket and WebSocket.connect

if not WSConnect then
    if messagebox then
        messagebox(('Nexus encountered an error while launching!\n\n%s'):format('Your exploit (' .. (identifyexecutor and identifyexecutor() or 'UNKNOWN') .. ') is not supported'), 'Roblox Account Manager', 0)
    end
    
    return
end

local TeleportService = game:GetService'TeleportService'
local InputService = game:GetService'UserInputService'
local HttpService = game:GetService'HttpService'
local RunService = game:GetService'RunService'
local GuiService = game:GetService'GuiService'
local Players = game:GetService'Players'
local LocalPlayer = Players.LocalPlayer if not LocalPlayer then repeat LocalPlayer = Players.LocalPlayer task.wait() until LocalPlayer end task.wait(0.5)

local UGS = UserSettings():GetService'UserGameSettings'
local OldVolume = UGS.MasterVolume

LocalPlayer.OnTeleport:Connect(function(State)
    if State == Enum.TeleportState.Started and Nexus.IsConnected then
        Nexus:Stop() -- Apparently doesn't disconnect websockets on teleport so this has to be here
    end
end)

local Signal = {} do
    Signal.__index = Signal

    function Signal.new()
        local self = setmetatable({ _BindableEvent = Instance.new'BindableEvent' }, Signal)
        
        return self
    end

    function Signal:Connect(Callback)
        assert(typeof(Callback) == 'function', 'function expected, got ' .. typeof(Callback))

        return self._BindableEvent.Event:Connect(Callback)
    end

    function Signal:Fire(...)
        self._BindableEvent:Fire(...)
    end

    function Signal:Wait()
        return self._BindableEvent.Event:Wait()
    end

    function Signal:Disconnect()
        if self._BindableEvent then
            self._BindableEvent:Destroy()
        end
    end
end

do -- Nexus
    local BTN_CLICK = 'ButtonClicked:'

    Nexus.Connected = Signal.new()
    Nexus.Disconnected = Signal.new()
    Nexus.MessageReceived = Signal.new()

    Nexus.Commands = {}
    Nexus.Connections = {}

    Nexus.ShutdownTime = 45
    Nexus.ShutdownOnTeleportError = true

    function Nexus:Send(Command, Payload)
        assert(self.Socket ~= nil, 'websocket is nil')
        assert(self.IsConnected, 'websocket not connected')
        assert(typeof(Command) == 'string', 'Command must be a string, got ' .. typeof(Command))

        if Payload then
            assert(typeof(Payload) == 'table', 'Payload must be a table, got ' .. typeof(Payload))
        end

        local Message = HttpService:JSONEncode {
            Name = Command,
            Payload = Payload
        }

        self.Socket:Send(Message)
    end

    function Nexus:SetAutoRelaunch(Enabled)
        self:Send('SetAutoRelaunch', { Content = Enabled and 'true' or 'false' })
    end
    
    function Nexus:SetPlaceId(PlaceId)
        self:Send('SetPlaceId', { Content = PlaceId })
    end
    
    function Nexus:SetJobId(JobId)
        self:Send('SetJobId', { Content = JobId })
    end

    function Nexus:Echo(Message)
        self:Send('Echo', { Content = Message })
    end

    function Nexus:Log(...)
        local T = {}

        for Index, Value in pairs{ ... } do
            table.insert(T, tostring(Value))
        end

        self:Send('Log', {
            Content = table.concat(T, ' ')
        })
    end

    function Nexus:CreateElement(ElementType, Name, Content, Size, Margins, Table)
        assert(typeof(Name) == 'string', 'string expected on argument #1, got ' .. typeof(Name))
        assert(typeof(Content) == 'string', 'string expected on argument #2, got ' .. typeof(Content))

        assert(Name:find'%W' == nil, 'argument #1 cannot contain whitespace')

        if Size then assert(typeof(Size) == 'table' and #Size == 2, 'table with 2 arguments expected on argument #3, got ' .. typeof(Size)) end
        if Margins then assert(typeof(Margins) == 'table' and #Margins == 4, 'table with 4 arguments expected on argument #4, got ' .. typeof(Margins)) end
        
        local Payload = {
            Name = Name,
            Content = Content,
            Size = Size and table.concat(Size, ','),
            Margin = Margins and table.concat(Margins, ',')
        }

        if Table then
            for Index, Value in pairs(Table) do
                Payload[Index] = Value
            end
        end

        self:Send(ElementType, Payload)
    end

    function Nexus:CreateButton(...)
        return self:CreateElement('CreateButton', ...)
    end

    function Nexus:CreateTextBox(...)
        return self:CreateElement('CreateTextBox', ...)
    end

    function Nexus:CreateNumeric(Name, Value, DecimalPlaces, Increment, Size, Margins)
        return self:CreateElement('CreateNumeric', Name, tostring(Value), Size, Margins, { DecimalPlaces = DecimalPlaces, Increment = Increment })
    end

    function Nexus:CreateLabel(...)
        return self:CreateElement('CreateLabel', ...)
    end

    function Nexus:NewLine(...)
        return self:Send('NewLine')
    end

    function Nexus:GetText(Name)
        return self:WaitForMessage('ElementText:', 'GetText', { Name = Name })
    end

    function Nexus:SetRelaunch(Seconds)
        self:Send('SetRelaunch', { Seconds = Seconds })
    end

    function Nexus:WaitForMessage(Header, Message, Payload)
        if Message then
            task.defer(self.Send, self, Message, Payload)
        end

        local Message

        while true do
            Message = self.MessageReceived:Wait()

            if Message:sub(1, #Header) == Header then
                break
            end
        end

        return Message:sub(#Header + 1)
    end

    function Nexus:Connect(Host, Bypass)
        if not Bypass and self.IsConnected then return 'Ignoring connection request, Nexus is already connected' end

        while true do
            for Index, Connection in pairs(self.Connections) do
                Connection:Disconnect()
            end
        
            table.clear(self.Connections)

            if self.IsConnected then
                self.IsConnected = false
                self.Socket = nil
                self.Disconnected:Fire()
            end

            if self.Terminated then break end

            if not Host then
                Host = 'localhost:5242'
            end

            local Success, Socket = pcall(WSConnect, ('ws://%s/Nexus?name=%s&id=%s&jobId=%s&placeId=%s'):format(Host, LocalPlayer.Name, LocalPlayer.UserId, game.JobId, game.PlaceId))

            if not Success then task.wait(12) continue end

            self.Socket = Socket
            self.IsConnected = true

            table.insert(self.Connections, Socket.OnMessage:Connect(function(Message)
                self.MessageReceived:Fire(Message)
            end))

            table.insert(self.Connections, Socket.OnClose:Connect(function()
                self.IsConnected = false
                self.Disconnected:Fire()
            end))

            self.Connected:Fire()

            -- The connect URL's jobId/placeId are only accurate at the moment this line runs.
            -- If the previous socket survives a teleport without OnClose ever firing (this
            -- exploit's websocket doesn't get torn down by TeleportService - see the OnTeleport
            -- handler above), the account manager can end up reusing a connection tracked under
            -- the same username and never notice the server changed. Resending both explicitly
            -- right after every (re)connect keeps the Job ID / player count columns from going
            -- stale after a teleport or rejoin.
            self:SetPlaceId(game.PlaceId)
            self:SetJobId(game.JobId)

            while self.IsConnected do
                local Success, Error = pcall(self.Send, self, 'ping')

                if not Success or self.Terminated then
                    break
                end

                task.wait(1)
            end
        end
    end

    function Nexus:Stop()
        self.IsConnected = false
        self.Terminated = true
        self.Disconnected:Fire()

        if self.Socket then
            pcall(function() self.Socket:Close() end)
        end
    end

    function Nexus:AddCommand(Name, Function)
        self.Commands[Name] = Function
    end

    function Nexus:RemoveCommand(Name)
        self.Commands[Name] = nil
    end

    function Nexus:OnButtonClick(Name, Function)
        self:AddCommand('ButtonClicked:' .. Name, Function)
    end

    Nexus.MessageReceived:Connect(function(Message)
        local S = Message:find(' ')

        if S then
            local Command, Message = Message:sub(1, S - 1):lower(), Message:sub(S + 1)

            if Nexus.Commands[Command] then
                local Success, Error = pcall(Nexus.Commands[Command], Message)

                if not Success and Error then
                    Nexus:Log(('Error with command `%s`: %s'):format(Command, Error))
                end
            end
        elseif Nexus.Commands[Message] then
            local Success, Error = pcall(Nexus.Commands[Message], Message)

            if not Success and Error then
                Nexus:Log(('Error with command `%s`: %s'):format(Message, Error))
            end
        end
    end)
end

do -- Default Commands
    Nexus:AddCommand('execute', function(Message)
        local Function, Error = loadstring(Message)
        
        if Function then
            local Env = getfenv(Function)
            
            Env.Player = LocalPlayer
            Env.print = function(...)
                local T = {}

                for Index, Value in pairs{ ... } do
                    table.insert(T, tostring(Value))
                end

                Nexus:Log(table.concat(T, ' '))
            end

            if newcclosure then Env.print = newcclosure(Env.print) end

            local S, E = pcall(Function)

            if not S then
                Nexus:Log(E)
            end
        else
            Nexus:Log(Error)
        end
    end)

    Nexus:AddCommand('teleport', function(Message)
        local S = Message:find(' ')
        local PlaceId, JobId = S and Message:sub(1, S - 1) or Message, S and Message:sub(S + 1)
        
        if JobId then
            TeleportService:TeleportToPlaceInstance(tonumber(PlaceId), JobId)
        else
            TeleportService:Teleport(tonumber(PlaceId))
        end
    end)

    Nexus:AddCommand('rejoin', function(Message)
        TeleportService:TeleportToPlaceInstance(game.PlaceId, game.JobId)
    end)

    Nexus:AddCommand('teleportjob', function(Message)
        local S = Message:find(' ')

        if not S then
            Nexus:Log('teleportjob requires PlaceId and JobId parameters')
            return
        end

        local PlaceId = tonumber(Message:sub(1, S - 1))
        local JobId = Message:sub(S + 1)

        if not PlaceId then
            Nexus:Log('Invalid PlaceId for teleportjob')
            return
        end

        if not JobId or JobId == '' then
            Nexus:Log('Invalid JobId for teleportjob')
            return
        end

        TeleportService:TeleportToPlaceInstance(PlaceId, JobId)
    end)

    Nexus:AddCommand('mute', function()
        if (UGS.MasterVolume - OldVolume) > 0.01 then
            OldVolume = UGS.MasterVolume
        end

        UGS.MasterVolume = 0
    end)

    Nexus:AddCommand('unmute', function()
        UGS.MasterVolume = OldVolume
    end)

    Nexus:AddCommand('performance', function(Message)
        if _PERF then return end
        
        _PERF = true
        _TARGETFPS = 8

        if Message and tonumber(Message) then
            _TARGETFPS = tonumber(Message)
        end

        local OldLevel = settings().Rendering.QualityLevel

        RunService:Set3dRenderingEnabled(false)
        settings().Rendering.QualityLevel = 1

        InputService.WindowFocused:Connect(function()
            RunService:Set3dRenderingEnabled(true)
            settings().Rendering.QualityLevel = OldLevel
            setfpscap(60)
        end)

        InputService.WindowFocusReleased:Connect(function()
            OldLevel = settings().Rendering.QualityLevel

            RunService:Set3dRenderingEnabled(false)
            settings().Rendering.QualityLevel = 1
            setfpscap(_TARGETFPS)
        end)

        setfpscap(_TARGETFPS)
    end)
end

do -- Health Monitor
    -- Lightweight real-time health tracking: polls every HealthPollInterval seconds
    -- and sends a single SetHealth message with HP + alive status. The ping loop
    -- already sends 1 msg/sec so this adds negligible overhead.
    local Workspace = game:GetService'Workspace'

    Nexus.HealthPollInterval = 3

    local function FindCharacter()
        local Char = LocalPlayer.Character
        if Char and Char:FindFirstChildOfClass'Humanoid' then return Char end

        -- Some games use custom character systems that never set Player.Character;
        -- the character model lives directly in Workspace under the player's name.
        return Workspace:FindFirstChild(LocalPlayer.Name)
    end

    local function SendHealth()
        if not Nexus.IsConnected then return end

        local Char = FindCharacter()
        local Hum = Char and Char:FindFirstChildOfClass'Humanoid'

        if Hum then
            Nexus:Send('SetHealth', {
                Content = tostring(math.floor(Hum.Health)),
                Alive = Hum.Health > 0 and 'true' or 'false'
            })
        else
            Nexus:Send('SetHealth', { Content = '-1', Alive = 'false' })
        end
    end

    task.spawn(function()
        while true do
            if Nexus.IsConnected then
                SendHealth()
            end

            task.wait(Nexus.HealthPollInterval)
        end
    end)

    -- On-demand query: send "health" from the control panel to get an immediate update
    Nexus:AddCommand('health', SendHealth)
end

do -- Money Monitor
    -- Reads "Hand Balance" and "Bank Balance" from GUI TextLabels (used by games
    -- like BlockSpin that store money in custom UI rather than leaderstats).
    -- Polls every MoneyPollInterval seconds; negligible overhead.
    Nexus.MoneyPollInterval = 3

    local function FindMoneyLabels()
        local Hand, Bank = nil, nil

        local function Search(Obj, Depth)
            if Depth > 8 then return end
            for _, Child in ipairs(Obj:GetChildren()) do
                if Child:IsA'TextLabel' or Child:IsA'TextButton' then
                    local T = Child.Text or ''
                    if T:find'Hand Balance' then Hand = Child end
                    if T:find'Bank Balance' then Bank = Child end
                end
                Search(Child, Depth + 1)
            end
        end

        local PG = LocalPlayer:FindFirstChild'PlayerGui'
        if PG then Search(PG, 0) end

        return Hand, Bank
    end

    local function ParseMoney(Text)
        if not Text then return -1 end
        local Num = Text:match('%$?([%d,]+)')
        if not Num then return -1 end
        local Clean = Num:gsub(',', '')
        return tonumber(Clean) or -1
    end

    local function SendMoney()
        if not Nexus.IsConnected then return end

        local Hand, Bank = FindMoneyLabels()
        local HandVal = ParseMoney(Hand and Hand.Text)
        local BankVal = ParseMoney(Bank and Bank.Text)

        Nexus:Send('SetMoney', {
            Content = tostring(HandVal),
            Bank = tostring(BankVal)
        })
    end

    task.spawn(function()
        while true do
            if Nexus.IsConnected then
                SendMoney()
            end

            task.wait(Nexus.MoneyPollInterval)
        end
    end)

    -- On-demand query: send "money" from the control panel to get an immediate update
    Nexus:AddCommand('money', SendMoney)
end

do -- Connections
    GuiService.ErrorMessageChanged:Connect(function()
        if NoShutdown then return end

        local Code = GuiService:GetErrorCode().Value

        if Code >= Enum.ConnectionError.DisconnectErrors.Value then
            if not Nexus.ShutdownOnTeleportError and Code > Enum.ConnectionError.PlacelaunchOtherError.Value then
                return
            end
            
            task.delay(Nexus.ShutdownTime, game.Shutdown, game)
        end
    end)
end

local GEnv = getgenv()
GEnv.Nexus = Nexus
GEnv.performance = Nexus.Commands.performance -- fix the sirmeme error so that people stop being annoying saying "omg performance() doesnt work" (https://youtu.be/vVfg9ym2MNs?t=389)

if not Nexus_Version then
    Nexus:Connect()
end
