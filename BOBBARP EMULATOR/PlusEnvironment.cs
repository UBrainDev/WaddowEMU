using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Plus.Core;
using Fleck;
using Plus.HabboHotel;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.Communication.Packets.Incoming;

using Plus.Net;
using Plus.Utilities;
using log4net;

using System.Data;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Encryption.Keys;
using Plus.Communication.Encryption;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;
using Plus.Database;
using System.Linq;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus
{
    public static class PlusEnvironment
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.PlusEnvironment");

        public const string PrettyVersion = "Waddow version BETA";

        private static ConfigurationData _configuration;
        private static Encoding _defaultEncoding;
        private static ConnectionHandling _connectionManager;
        private static Game _game;
        private static DatabaseManager _manager;
        public static ConfigData ConfigData;
        public static MusSocket MusSystem;
        public static CultureInfo CultureInfo;
        
        public static DateTime ServerStarted;
        public static Dictionary<int, DateTime> Trash = new Dictionary<int, DateTime>();
        public static Dictionary<string, DateTime> usersSuspendus = new Dictionary<string, DateTime>();
        public static bool restart = false;
        public static bool Purge = false;
        public static bool PurgeLoading = false;
        public static Dictionary<int, int> PurgeGangKills = new Dictionary<int, int>();
        public static string footballEngagot = "green";
        public static int footballGoalGreen;
        public static int footballGoalBlue;
        public static int Fontaine = 0;
        public static bool SaladeAttente = false;
        public static int Salade = 0;
        public static int RouletteEtat = 0;
        public static bool RouletteTurning = false;
        public static int Quizz = 0;
        public static string QuizzReponse = null;
        public static int ManVsZombie = 0;
        public static bool ManVsZombieLoading = false;
        public static int LastActionMessage = 0;
        private static readonly List<char> Allowedchars = new List<char>(new[]
            {
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
            });

        private static ConcurrentDictionary<int, Habbo> _usersCached = new ConcurrentDictionary<int, Habbo>();

        public static string SWFRevision = "";

        public static void Initialize()
        {
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine(" _          __  ___   _____   _____   _____   _          __");
            Console.WriteLine(@"| |        / / /   | |  _  \ |  _  \ /  _  \ | |        / /");
            Console.WriteLine(@"| |  __   / / / /| | | | | | | | | | | | | | | |  __   / /");
            Console.WriteLine(@"| | /  | / / / /_| | | | | | | | | | | | | | | | /  | / /   ");
            Console.WriteLine(@"| |/   |/ / / ___  | | |_| | | |_| | | |_| | | |/   |/ /    ");
            Console.WriteLine(@"|___/|___/ /_/   |_| |_____/ |_____/ \_____/ |___/|___/     ");
            

            Console.WriteLine("                        " + PrettyVersion );
            Console.WriteLine("                           Développé par OvB , UBrainDev et Soubes" );

            Console.WriteLine("");
            Console.Title = "Chargement de Waddow";
            _defaultEncoding = Encoding.Default;

            Console.WriteLine("");
            Console.WriteLine("");

            CultureInfo = CultureInfo.CreateSpecificCulture("fr-FR");


            try
            {

                _configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"config.ini"));

                var connectionString = new MySqlConnectionStringBuilder
                {
                    ConnectionTimeout = 10,
                    Database = GetConfig().data["db.name"],
                    DefaultCommandTimeout = 30,
                    Logging = false,
                    MaximumPoolSize = uint.Parse(GetConfig().data["db.pool.maxsize"]),
                    MinimumPoolSize = uint.Parse(GetConfig().data["db.pool.minsize"]),
                    Password = GetConfig().data["db.password"],
                    Pooling = true,
                    Port = uint.Parse(GetConfig().data["db.port"]),
                    Server = GetConfig().data["db.hostname"],
                    UserID = GetConfig().data["db.username"],
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true,
                };

                _manager = new DatabaseManager(connectionString.ToString());

                if (!_manager.IsConnected())
                {
                    log.Error("Failed to connect to the specified MySQL server.");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                    return;
                }

                log.Info("Connected to Database!");

                //Reset our statistics first.
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                    dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0';");
                    dbClient.RunQuery("UPDATE `users` SET `online` = '0' WHERE `online` = '1'");
                    dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
                }

                //Get the configuration & Game set.
                ConfigData = new ConfigData();
                _game = new Game();

                //Have our encryption ready.
                HabboEncryptionV2.Initialize(new RSAKeys());

                //Make sure MUS is working.
                MusSystem = new MusSocket(GetConfig().data["mus.tcp.bindip"], int.Parse(GetConfig().data["mus.tcp.port"]), GetConfig().data["mus.tcp.allowedaddr"].Split(Convert.ToChar(";")), 0);

                //Accept connections.
                _connectionManager = new ConnectionHandling(int.Parse(GetConfig().data["game.tcp.port"]), int.Parse(GetConfig().data["game.tcp.conlimit"]), int.Parse(GetConfig().data["game.tcp.conperip"]), GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");
                _connectionManager.init();

                _game.StartGameLoop();

                TimeSpan TimeUsed = DateTime.Now - ServerStarted;
                log.Info("WADDOW EMULATOR -> DÉMARRÉ! (" + TimeUsed.Seconds + " s, " + TimeUsed.Milliseconds + " ms)");
                setEuroRPWinner();
                actualiseWantedList();
            }
            catch (KeyNotFoundException e)
            {
                Logging.WriteLine("Please check your configuration file - some values appear to be missing.", ConsoleColor.Red);
                Logging.WriteLine("Press any key to shut down ...");
                Logging.WriteLine(e.ToString());
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (InvalidOperationException e)
            {
                Logging.WriteLine("Failed to initialize PlusEmulator: " + e.Message, ConsoleColor.Red);
                Logging.WriteLine("Press any key to shut down ...");
                Console.ReadKey(true);
                Environment.Exit(1);
                return;
            }
            catch (Exception e)
            {
                Logging.WriteLine("Fatal error during startup: " + e, ConsoleColor.Red);
                Logging.WriteLine("Press a key to exit");

                Console.ReadKey();
                Environment.Exit(1);
            }
        }



        public static bool EnumToBool(string Enum)
        {
            return (Enum == "1");
        }

        public static string BoolToEnum(bool Bool)
        {
            return (Bool == true ? "1" : "0");
        }

        public static int GetRandomNumber(int Min, int Max)
        {
            return RandomNumber.GenerateNewRandom(Min, Max);
        }

        public static double GetUnixTimestamp()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }

        public static long Now()
        {
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            double unixTime = ts.TotalMilliseconds;
            return (long)unixTime;
        }

        public static string FilterFigure(string figure)
        {
            foreach (char character in figure)
            {
                if (!isValid(character))
                    return "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62";
            }

            return figure;
        }

        private static bool isValid(char character)
        {
            return Allowedchars.Contains(character);
        }

        public static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            if (string.IsNullOrEmpty(inputStr))
            {
                return false;
            }

            for (int i = 0; i < inputStr.Length; i++)
            {
                if (!isValid(inputStr[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetUsernameById(int UserId)
        {
            string Name = "Unknown User";

            GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);
            if (Client != null && Client.GetHabbo() != null)
                return Client.GetHabbo().Username;

            UserCache User = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(UserId);
            if (User != null)
                return User.Username;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", UserId);
                Name = dbClient.getString();
            }

            if (string.IsNullOrEmpty(Name))
                Name = "Unknown User";

            return Name;
        }

        public static void updateGangPurgeKill(int GangId)
        {
            if(!PurgeGangKills.ContainsKey(GangId))
            {
                PurgeGangKills.Add(GangId, 1);
                return;
            }

            PurgeGangKills[GangId] += 1;
        }

        public static void footballResetScore()
        {
            footballEngagot = "green";
            footballGoalGreen = 0;
            footballGoalBlue = 0;
            GetGame().GetClientManager().footballUpdateHtmlScore();
            Room Room = GetGame().GetRoomManager().LoadRoom(56);

            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FOOTBALL)
                {
                    foreach (Item item2 in Room.GetRoomItemHandler().GetFloor)
                    {
                        if (item2.GetBaseItem().SpriteId == 3498)
                        {
                            Room.SendMessage(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, item2.GetX + 1,
                                item2.GetY + 1, 0, 0, 0, item.Id));
                            Room.GetRoomItemHandler().SetFloorItem(item, item2.GetX + 1, item2.GetY + 1, 0);
                        }
                    }
                }
            }

            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetBaseItem().ItemName == ("fball_score_"))
                {
                    Room.SendMessage(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, item.GetX,
                                item.GetY, 0, 0, 0, item.Id));
                    Room.GetRoomItemHandler().SetFloorItem(item, item.GetX, item.GetY, 0);
                }
            }
        }

        public static void footballUpdateScore(string Team, string Engagot)
        {
            RoomUser Commentateur = null;
            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(56);
            Room.GetRoomUserManager().TryGetBot(1, out Commentateur);
            
            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FOOTBALL)
                {
                    foreach (Item item2 in Room.GetRoomItemHandler().GetFloor)
                    {
                        if (item2.GetBaseItem().SpriteId == 3498)
                        {
                            Room.SendMessage(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, item2.GetX + 1,
                                item2.GetY +1, 0, 0, 0, item.Id));
                            Room.GetRoomItemHandler().SetFloorItem(item, item2.GetX + 1, item2.GetY + 1, 0);
                        }
                    }
                }
            }

            if (Team == "green")
            {
                footballGoalGreen += 1;
                if(footballGoalGreen == 3)
                {
                    if (Commentateur != null)
                    {
                        Commentateur.Chat("L'équipe verte l'emporte 3-" + footballGoalBlue + " contre l'équipe bleu !", false, 0);
                    }
                    GetGame().GetClientManager().footballEndMatch();
                    GetGame().GetClientManager().footballUpdateHtmlScore();
                    return;
                }
                else
                {
                    footballEngagot = Engagot;
                    GetGame().GetClientManager().footballSendUserInSpawn();
                    GetGame().GetClientManager().footballUpdateHtmlScore();
                    return;
                }
            }

            if (Team == "blue")
            {
                footballGoalBlue += 1;
                if (footballGoalBlue == 3)
                {
                    if (Commentateur != null)
                    {
                        Commentateur.Chat("L'équipe bleu l'emporte 3-" + footballGoalGreen + " contre l'équipe verte !", false, 0);
                    }
                    GetGame().GetClientManager().footballEndMatch();
                    GetGame().GetClientManager().footballUpdateHtmlScore();
                    return;
                }
                else
                {
                    footballEngagot = Engagot;
                    GetGame().GetClientManager().footballSendUserInSpawn();
                    GetGame().GetClientManager().footballUpdateHtmlScore();
                    return;
                }
            }
        }

        public static void suspendreUser(string username)
        {
            usersSuspendus.Add(username, DateTime.Now);
            System.Timers.Timer timer1 = new System.Timers.Timer(300000);
            timer1.Interval = 300000;
            timer1.Elapsed += delegate
            {
                usersSuspendus.Remove(username);
                timer1.Stop();
            };
            timer1.Start();
        }

        public static void gangWinPurge()
        {
            if (PurgeGangKills.Count == 0)
                return;

            int nbKill = PurgeGangKills.Values.Max();
            var gangId = PurgeGangKills.FirstOrDefault(x => x.Value == PurgeGangKills.Values.Max()).Key;
            string Message = "Record de kills pendant la purge avec un nombre de " + nbKill + " kills.";

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO gang_exploits (gang_id, exploits, date) VALUE (@gang_id, @exploit, NOW())");
                dbClient.AddParameter("gang_id", gangId);
                dbClient.AddParameter("exploit", Message);
                dbClient.RunQuery();
            }
            PurgeGangKills.Clear();
        }

        public static bool checkIfItemExist(string name, string type)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `items_list` WHERE name = @name AND type = @isTelephone LIMIT 1");
                dbClient.AddParameter("name", name);
                dbClient.AddParameter("isTelephone", type);
                int foundedResult = dbClient.getInteger();
                if (foundedResult > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static int getTypeOfItem(string name)
        {
            int Type;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `idType` FROM `items_list` WHERE name = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                Type = dbClient.getInteger();
            }
            return Type;
        }

        public static string getNameOfItem(string name)
        {
            string NameItem;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `name` FROM `items_list` WHERE name = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                NameItem = dbClient.getString();
            }
            return NameItem;
        }

        public static int getTaxeOfItem(string name)
        {
            int Taxe;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `taxe` FROM `items_list` WHERE name = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                Taxe = dbClient.getInteger();
            }
            return Taxe;
        }

        public static int getPriceOfItem(string name)
        {
            int Price;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `prix` FROM `items_list` WHERE name = @name LIMIT 1");
                dbClient.AddParameter("name", name);
                Price = dbClient.getInteger();
            }
            return Price;
        }

        public static Habbo GetHabboById(int UserId)
        {
            try
            {
                GameClient Client = GetGame().GetClientManager().GetClientByUserID(UserId);
                if (Client != null)
                {
                    Habbo User = Client.GetHabbo();
                    if (User != null && User.Id > 0)
                    {
                        if (_usersCached.ContainsKey(UserId))
                            _usersCached.TryRemove(UserId, out User);
                        return User;
                    }
                }
                else
                {
                    try
                    {
                        if (_usersCached.ContainsKey(UserId))
                            return _usersCached[UserId];
                        else
                        {
                            UserData data = UserDataFactory.GetUserData(UserId);
                            if (data != null)
                            {
                                Habbo Generated = data.user;
                                if (Generated != null)
                                {
                                    Generated.InitInformation(data);
                                    _usersCached.TryAdd(UserId, Generated);
                                    return Generated;
                                }
                            }
                        }
                    }
                    catch { return null; }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static Habbo GetHabboByUsername(String UserName)
        {
            try
            {
                using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
                    dbClient.AddParameter("user", UserName);
                    int id = dbClient.getInteger();
                    if (id > 0)
                        return GetHabboById(Convert.ToInt32(id));
                }
                return null;
            }
            catch { return null; }
        }

        public static void actualiseWantedList()
        {
            using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM users_wanted WHERE added_date < DATE_SUB(NOW(), INTERVAL 1 DAY)");
            }

            Thread.Sleep(60000);
        }

        public static int checkPositionEvent(int ItemId)
        {
            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(126);
            foreach (Item item in Room.GetRoomItemHandler().GetFloor)
            {
                if (item.Id == ItemId)
                {
                    return item.GetX;
                }
            }

            return 0;
        }
        

        public static void setEuroRPWinner()
        {
            System.Timers.Timer timer1 = new System.Timers.Timer(60000);
            timer1.Interval = 60000;
            timer1.Elapsed += delegate
            {
                DateTime now = DateTime.Now; ;
                if (now.DayOfWeek == DayOfWeek.Friday && now.Hour == 21 && now.Minute == 00)
                {
                    using (IQueryAdapter dbClient = GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT COUNT(0) FROM `eurorp_participants`");
                        int participantsCount = dbClient.getInteger();

                        if (participantsCount > 1)
                        {
                            dbClient.SetQuery("SELECT participant_id FROM eurorp_participants ORDER by RAND() LIMIT 1");
                            int winner = dbClient.getInteger();
                            dbClient.SetQuery("SELECT montant FROM eurorp");
                            decimal montant = dbClient.getInteger();
                            montant = (montant * 75) / 100;
                            int montantFinal = Convert.ToInt32(Math.Round(montant));

                            dbClient.SetQuery("UPDATE `eurorp` SET montant = '0', last_winner = @new_winner, lot = '0', last_montant = @last_montant");
                            dbClient.AddParameter("new_winner", winner);
                            dbClient.AddParameter("last_montant", montantFinal);
                            dbClient.RunQuery();
                            dbClient.RunQuery("TRUNCATE TABLE eurorp_participants");
                            PlusEnvironment.GetGame().GetClientManager().sendAlertOffi("[EURORP] Le tirage de cette semaine a été effectué sous contrôle d'un huissier de justice.");
                        }
                        else
                        {
                            Console.WriteLine("[EURORP] Il n'y a pas assez de participants pour faire le tirage.");
                        }
                    }
                }
            };
            timer1.Start();

        }


        public static void PerformShutDown()
        {
            Console.Clear();
            log.Info("Fermeture en cours...");
            Console.Title = "ARRÊT DE WADDOW";

            PlusEnvironment.GetGame().GetClientManager().SendMessage(new BroadcastMessageAlertComposer(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("shutdown_alert")));
            GetGame().StopGameLoop();
            Thread.Sleep(2500);
            GetConnectionManager().Destroy();//Stop listening.
            GetGame().GetPacketManager().UnregisterAll();//Unregister the packets.
            GetGame().GetPacketManager().WaitForAllToComplete();
            GetGame().GetClientManager().CloseAll();//Close all connections
            GetGame().GetRoomManager().Dispose();//Stop the game loop.

            using (IQueryAdapter dbClient = _manager.GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET online = '0', `auth_ticket` = NULL, `purchase` = NULL");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }

            log.Info("WADDOW S'EST arrêté correctement.");

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        public static void restartServeur()
        {
            Console.Clear();
            log.Info("Redémarrage du serveur...");
            Console.Title = "REDÉMARRAGE DE WADDOW";

            GetGame().StopGameLoop();
            Thread.Sleep(2500);
            GetConnectionManager().Destroy();//Stop listening.
            GetGame().GetPacketManager().UnregisterAll();//Unregister the packets.
            GetGame().GetPacketManager().WaitForAllToComplete();
            GetGame().GetClientManager().CloseAll();//Close all connections
            GetGame().GetRoomManager().Dispose();//Stop the game loop.

            using (IQueryAdapter dbClient = _manager.GetQueryReactor())
            {
                dbClient.RunQuery("TRUNCATE `catalog_marketplace_data`");
                dbClient.RunQuery("UPDATE `users` SET online = '0', `auth_ticket` = NULL, `purchase` = NULL");
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }

            log.Info("Waddow redémarré correctement.");

            Thread.Sleep(1000);
            Application.Restart();
            Environment.Exit(0);
        }

        public static ConfigurationData GetConfig()
        {
            return _configuration;
        }

        public static ConfigData GetDBConfig()
        {
            return ConfigData;
        }

        public static Encoding GetDefaultEncoding()
        {
            return _defaultEncoding;
        }

        public static ConnectionHandling GetConnectionManager()
        {
            return _connectionManager;
        }

        public static Game GetGame()
        {
            return _game;
        }

        public static DatabaseManager GetDatabaseManager()
        {
            return _manager;
        }

        public static ICollection<Habbo> GetUsersCached()
        {
            return _usersCached.Values;
        }

        public static bool RemoveFromCache(int Id, out Habbo Data)
        {
            return _usersCached.TryRemove(Id, out Data);
        }
    }
}