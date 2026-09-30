#pragma warning disable CS8632
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RBX_Alt_Manager.Classes
{
    public static class Presence
    {
        public static Dictionary<UserPresenceType, Color> Colors = new Dictionary<UserPresenceType, Color> {
            { UserPresenceType.Online, Color.FromArgb(255, 0, 162, 255) },
            { UserPresenceType.InGame, Color.FromArgb(255, 2, 183, 87) },
            { UserPresenceType.InStudio, Color.FromArgb(255, 70, 41, 216) }
        };

        // Timeout guards against a hung/slow request piling up over a long-running session
        // with many accounts, since nothing else here cancels a stuck call.
        public static RestClient PresenceClient = new RestClient(new RestClientOptions("https://presence.roblox.com/") { MaxTimeout = 15000 });

        // Multiple timers (Auto Re-join UI, Auto Relaunch) can call UpdatePresence independently.
        // Without a guard, a slow/hung call plus repeated timer ticks over a long session can
        // stack up outstanding requests indefinitely. Skip a call if one is already in flight.
        private static readonly SemaphoreSlim UpdatePresenceLock = new SemaphoreSlim(1, 1);

        public static async Task UpdatePresence(params long[] UserIds)
        {
            if (!Utilities.IsConnectedToInternet()) return;
            if (!await UpdatePresenceLock.WaitAsync(0)) return;

            try
            {
                List<Account> Updated = new List<Account>();
                var Response = await PresenceClient.PostAsync(new RestRequest("v1/presence/users", Method.Post).AddJsonBody(new { userIds = UserIds }));

                if (Response.IsSuccessful && JsonConvert.DeserializeObject(Response.Content) is JObject Data && Data.ContainsKey("userPresences"))
                {
                    foreach (var Presence in Data["userPresences"].ToObject<List<UserPresence>>())
                        if (AccountManager.AccountsList.FirstOrDefault(acc => acc.UserID == Presence.userId) is Account account)
                        {
                            account.Presence = Presence;

                            Updated.Add(account);
                        }
                }

                AccountManager.Instance.InvokeIfRequired(() => AccountManager.Instance.AccountsView.RefreshObjects(Updated));
            }
            catch { }
            finally
            {
                UpdatePresenceLock.Release();
            }
        }

        public static async Task<Dictionary<long, UserPresence>> GetPresence(params long[] UserIds)
        {
            if (!Utilities.IsConnectedToInternet()) return null;

            var Dict = new Dictionary<long, UserPresence>();
            var Response = await PresenceClient.PostAsync(new RestRequest("v1/presence/users", Method.Post).AddJsonBody(new { userIds = UserIds }));

            if (Response.IsSuccessful && JsonConvert.DeserializeObject(Response.Content) is JObject Data && Data.ContainsKey("userPresences"))
            {
                foreach (var Presence in Data["userPresences"].ToObject<List<UserPresence>>())
                    Dict.Add(Presence.userId, Presence);

                return Dict;
            }

            return null;
        }

        public static async Task<UserPresence> GetPresenceSingular(long UserId) => ((await GetPresence(UserId)) is Dictionary<long, UserPresence> Dict && Dict.ContainsKey(UserId)) ? Dict[UserId] : null;
    }

    public class UserPresence
    {
        public UserPresenceType userPresenceType { get; set; }
        public string lastLocation { get; set; }
        public long? placeId { get; set; }
        public long? rootPlaceId { get; set; }
        public string? gameId { get; set; }
        public long? universeId { get; set; }
        public long userId { get; set; }
        public DateTime lastOnline { get; set; }
    }

    public enum UserPresenceType
    {
        Offline,
        Online,
        InGame,
        InStudio
    }
}