using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using PrimS.Telnet;

namespace sibalzer.streamdeck.prismatik.Helpers
{
    internal static class PrismatikApiClient
    {
        public static Client PRISMATIC_CLIENT;
        private static readonly object ro_PRISMATIC_CLIENT_LOCK_OBJ = new object();

        public static void SetupTelnetClient(string apiKey)
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (PRISMATIC_CLIENT != null && PRISMATIC_CLIENT.IsConnected) return;

                try
                {
                    PRISMATIC_CLIENT = new Client("127.0.0.1", 3636, new CancellationToken());
                }
                catch (SocketException)
                {
                    PRISMATIC_CLIENT = null;
                    return;
                }

                if (!PRISMATIC_CLIENT.IsConnected)
                {
                    PRISMATIC_CLIENT = null;
                    return;
                }

                var welcomeMessage = PRISMATIC_CLIENT.ReadAsync().Result;
                if (!welcomeMessage.Contains("Lightpack API"))
                {
                    PRISMATIC_CLIENT = null;
                    return;
                }

                if (!AuthenticateTelnet(apiKey)) PRISMATIC_CLIENT = null;
            }
        }

        private static bool AuthenticateTelnet(string apiKey)
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;

                PRISMATIC_CLIENT.WriteLine($"apikey:{apiKey}");
                var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

                return authResponse.Contains("ok");
            }
        }

        private static bool GetStatusApi()
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;

                PRISMATIC_CLIENT.WriteLine("getstatusapi");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;

                return useResponse.Contains("statusapi:idle");
            }
        }

        private static bool Lock()
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;

                PRISMATIC_CLIENT.WriteLine($"lock");
                var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

                return authResponse.Contains("lock:success");
            }
        }
        
        private static bool Unlock()
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;

                PRISMATIC_CLIENT.WriteLine($"unlock");
                var authResponse = PRISMATIC_CLIENT.ReadAsync().Result;

                return authResponse.Contains("unlock:success") || authResponse.Contains("unlock:not");
            }
        }

        public static string[] GetProfiles()
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return new string[0];

                if (!Lock()) return new string[0];

                PRISMATIC_CLIENT.WriteLine("getprofiles");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
                useResponse = useResponse.Replace("profiles:", "");
                Unlock();
                return useResponse.Split(';');
            }
        }

        public static string GetProfile()
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return "";

                if (!Lock()) return "";

                PRISMATIC_CLIENT.WriteLine("getprofiles");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
                useResponse = useResponse.Replace("profile:", "");
                Unlock();
                return useResponse;
            }
        }

        public static bool SetBrightness(uint brightness)
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;
                
                if (!Lock()) return false;

                PRISMATIC_CLIENT.WriteLine($"setbrightness:{brightness}");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
                Unlock();
                return useResponse.Contains("ok");
            }
        }

        public static bool SetProfile(string profile)
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;
                
                if (!Lock()) return false;

                PRISMATIC_CLIENT.WriteLine($"setprofile:{profile}");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
                Unlock();
                return useResponse.Contains("ok");
            }
        }

        public static bool SetStatus(bool status)
        {
            lock (ro_PRISMATIC_CLIENT_LOCK_OBJ)
            {
                if (!PRISMATIC_CLIENT.IsConnected) return false;
                
                if (!Lock()) return false;

                var statusString = status ? "on" : "off" ;

                PRISMATIC_CLIENT.WriteLine($"setprofile:{statusString}");
                var useResponse = PRISMATIC_CLIENT.ReadAsync().Result;
                Unlock();
                return useResponse.Contains("ok");
            }
        }
    }
}