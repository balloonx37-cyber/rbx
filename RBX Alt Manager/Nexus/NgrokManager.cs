using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace RBX_Alt_Manager.Nexus
{
    // Launches ngrok as a child process to expose the Web Control HTTP server (see
    // AccountControl.OpenServer) to the public internet, not just the local network -
    // AllowExternalConnections (IPAddress.Any) only reaches other devices on the same LAN,
    // since home/office routers don't forward inbound ports by default. ngrok tunnels around
    // that without the user needing to touch their router, at the cost of needing a free
    // ngrok.com account (ngrok requires its own per-user auth token - there's no way to
    // provision that automatically on the user's behalf).
    internal static class NgrokManager
    {
        private const string DownloadUrl = "https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-windows-amd64.zip";
        private static readonly HttpClient Client = new HttpClient();

        private static Process NgrokProcess;

        public static bool IsRunning => NgrokProcess != null && !NgrokProcess.HasExited;

        private static string ExePath => Path.Combine(Environment.CurrentDirectory, "ngrok.exe");

        public static async Task EnsureDownloadedAsync(Action<string> StatusCallback = null)
        {
            if (File.Exists(ExePath)) return;

            StatusCallback?.Invoke("Downloading ngrok...");

            string ZipPath = Path.Combine(Environment.CurrentDirectory, "ngrok.zip");

            byte[] Bytes = await Client.GetByteArrayAsync(DownloadUrl);
            File.WriteAllBytes(ZipPath, Bytes);

            StatusCallback?.Invoke("Extracting ngrok...");

            ZipFile.ExtractToDirectory(ZipPath, Environment.CurrentDirectory);

            File.Delete(ZipPath);
        }

        // Starts "ngrok http {Port}" and polls ngrok's own local API (127.0.0.1:4040) for the
        // assigned public URL, since ngrok picks a random subdomain each run (free tier has no
        // fixed domain) - there's no way to know the URL in advance, only after the tunnel is
        // actually established.
        public static async Task<string> StartAsync(int Port, string AuthToken)
        {
            if (string.IsNullOrWhiteSpace(AuthToken))
                throw new InvalidOperationException("An ngrok auth token is required. Get a free one at https://dashboard.ngrok.com/get-started/your-authtoken");

            await EnsureDownloadedAsync();

            Stop();

            NgrokProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ExePath,
                    // --host-header=rewrite makes ngrok send "Host: localhost:{Port}" to the
                    // local server instead of forwarding the public ngrok hostname as-is. Without
                    // it, every request 404s - the WebSocketSharp HttpListener this app's web
                    // server is built on validates the Host header against the address it's bound
                    // to and silently rejects anything else, which isn't configurable from here
                    // (confirmed by reproducing the same 404 locally with a forged Host header).
                    Arguments = $"http {Port} --authtoken {AuthToken} --host-header=rewrite --log=stdout",
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            NgrokProcess.Start();

            // ngrok's local API takes a moment to come up after the process starts - poll
            // instead of a single immediate request, which would almost always race the API
            // listener not being ready yet.
            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(500);

                if (NgrokProcess.HasExited)
                    throw new InvalidOperationException("ngrok exited immediately - check that the auth token is valid.");

                string PublicUrl = await TryGetPublicUrlAsync();

                if (!string.IsNullOrEmpty(PublicUrl))
                    return PublicUrl;
            }

            Stop();
            throw new TimeoutException("Timed out waiting for ngrok to establish a tunnel.");
        }

        private static async Task<string> TryGetPublicUrlAsync()
        {
            try
            {
                string Json = await Client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
                JObject Parsed = JObject.Parse(Json);

                foreach (JObject Tunnel in Parsed["tunnels"])
                {
                    string Url = Tunnel["public_url"]?.ToString();

                    if (!string.IsNullOrEmpty(Url) && Url.StartsWith("https://"))
                        return Url;
                }
            }
            catch { /* API not up yet, or process not ready - keep polling */ }

            return null;
        }

        public static void Stop()
        {
            try
            {
                if (NgrokProcess != null && !NgrokProcess.HasExited)
                    NgrokProcess.Kill();
            }
            catch { }
            finally
            {
                NgrokProcess?.Dispose();
                NgrokProcess = null;
            }
        }
    }
}
