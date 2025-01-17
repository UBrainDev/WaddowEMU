﻿using System;
using System.Collections.Generic;
using System.Text;
using ConnectionManager;

using Plus.Core;
using Plus.HabboHotel.Users.Messenger;


using System.Linq;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing;

using log4net;
using System.Data;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.Interfaces;
using System.Collections;
using Plus.Communication.Packets.Outgoing.Handshake;
using System.Diagnostics;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.GameClients
{
    public class GameClientManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.GameClients.GameClientManager");

        private ConcurrentDictionary<int, GameClient> _clients;
        private ConcurrentDictionary<int, GameClient> _userIDRegister;
        private ConcurrentDictionary<string, GameClient> _usernameRegister;

        private readonly Queue timedOutConnections;

        private readonly Stopwatch clientPingStopwatch;

        public GameClientManager()
        {
            this._clients = new ConcurrentDictionary<int, GameClient>();
            this._userIDRegister = new ConcurrentDictionary<int, GameClient>();
            this._usernameRegister = new ConcurrentDictionary<string, GameClient>();

            timedOutConnections = new Queue();

            clientPingStopwatch = new Stopwatch();
            clientPingStopwatch.Start();
        }

        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
        }

        public GameClient GetClientByUserID(int userID)
        {
            if (_userIDRegister.ContainsKey(userID))
                return _userIDRegister[userID];
            return null;
        }

        public GameClient GetClientByUsername(string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                return _usernameRegister[username.ToLower()];
            return null;
        }

        public bool TryGetClient(int ClientId, out GameClient Client)
        {
            return this._clients.TryGetValue(ClientId, out Client);
        }

        public bool UpdateClientUsername(GameClient Client, string OldUsername, string NewUsername)
        {
            if (Client == null || !_usernameRegister.ContainsKey(OldUsername.ToLower()))
                return false;

            _usernameRegister.TryRemove(OldUsername.ToLower(), out Client);
            _usernameRegister.TryAdd(NewUsername.ToLower(), Client);
            return true;
        }

        public string GetNameById(int Id)
        {
            GameClient client = GetClientByUserID(Id);

            if (client != null)
                return client.GetHabbo().Username;

            string username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT username FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                username = dbClient.getString();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
        {
            foreach (int id in users)
            {
                GameClient client = GetClientByUserID(id);
                if (client != null)
                    yield return client;
            }
        }

        public void StaffAlert(ServerPacket Message, int Exclude = 0)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().Rank < 2 || client.GetHabbo().Id == Exclude)
                    continue;

                client.SendMessage(Message);
            }
        }

        public void ModAlert(string Message)
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null)
                    continue;

                if (client.GetHabbo().GetPermissions().HasRight("mod_tool") && !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert"))
                {
                    try { client.SendWhisper(Message, 5); }
                    catch { }
                }
            }
        }

        public void DoAdvertisingReport(GameClient Reporter, GameClient Target)
        {
            if (Reporter == null || Target == null || Reporter.GetHabbo() == null || Target.GetHabbo() == null)
                return;

            StringBuilder Builder = new StringBuilder();
            Builder.Append("New report submitted!\r\r");
            Builder.Append("Reporter: " + Reporter.GetHabbo().Username + "\r");
            Builder.Append("Reported User: " + Target.GetHabbo().Username + "\r\r");
            Builder.Append(Target.GetHabbo().Username + "s last 10 messages:\r\r");

            DataTable GetLogs = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + Target.GetHabbo().Id + "' ORDER BY `id` DESC LIMIT 10");
                GetLogs = dbClient.getTable();

                if (GetLogs != null)
                {
                    int Number = 11;
                    foreach (DataRow Log in GetLogs.Rows)
                    {
                        Number -= 1;
                        Builder.Append(Number + ": " + Convert.ToString(Log["message"]) + "\r");
                    }
                }
            }

            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                    continue;

                if (Client.GetHabbo().GetPermissions().HasRight("mod_tool") && !Client.GetHabbo().GetPermissions().HasRight("staff_ignore_advertisement_reports"))
                    Client.SendMessage(new MOTDNotificationComposer(Builder.ToString()));
            }
        }


        public void SendMessage(ServerPacket Packet, string fuse = "")
        {
            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null)
                    continue;

                if (!string.IsNullOrEmpty(fuse))
                {
                    if (!Client.GetHabbo().GetPermissions().HasRight(fuse))
                        continue;
                }

                Client.SendMessage(Packet);
            }
        }

        public void sendPoliceRadio(string Msg)
        {
            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().TravailId != 4 || Client.GetHabbo().Travaille == false)
                    continue;

                Client.SendWhisper("[RADIO DE POLICE] " + Msg);
            }
        }

        public void sendAlertOffi(string msg)
        {
            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || !Client.GetHabbo().InRoom)
                    continue;

                Client.GetHabbo().sendMsgOffi(msg);
            }
        }

        public void checkIfWinSalade()
        {
            int Count = 0;

            GameClient Session = null;

            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().Rank == 8 || Client.GetHabbo().TravailId == 18 && Client.GetHabbo().RankId == 2)
                    continue;

                if (Client.GetHabbo().CurrentRoomId == PlusEnvironment.Salade)
                {
                    Count += 1;
                    Session = Client;
                }
            }

            if (Count == 1 && Session != null || Count == 0)
            {
                PlusEnvironment.Salade = 0;
                if (Count == 1)
                {
                    Session.GetHabbo().winSalade();
                    sendAlertOffi(Session.GetHabbo().Username + " a gagné la salade.");
                }
            }
        }

        public string getPoliceMenotte(string Username)
        {
            foreach (GameClient Client in this._clients.Values.ToList())
            {
                if(Username == Client.GetHabbo().MenottedUsername)
                {
                    return Client.GetHabbo().Username;
                }
            }

            return null;
        }

        public void CreateAndStartClient(int clientID, ConnectionInformation connection)
        {
            GameClient Client = new GameClient(clientID, connection);
            if (this._clients.TryAdd(Client.ConnectionID, Client))
                Client.StartConnection();
            else
                connection.Dispose();
        }

        public void DisposeConnection(int clientID)
        {
            GameClient Client = null;
            if (!TryGetClient(clientID, out Client))
                return;

            if (Client != null)
                Client.Dispose(clientID);
        }

        public void removeConnection(int clientID)
        {
            GameClient Client = null;
            this._clients.TryRemove(clientID, out Client);
        }

        public void LogClonesOut(int UserID)
        {
            GameClient client = GetClientByUserID(UserID);
            if (client != null)
                client.Disconnect();
        }

        public void RegisterClient(GameClient client, int userID, string username)
        {
            if (_usernameRegister.ContainsKey(username.ToLower()))
                _usernameRegister[username.ToLower()] = client;
            else
                _usernameRegister.TryAdd(username.ToLower(), client);

            if (_userIDRegister.ContainsKey(userID))
                _userIDRegister[userID] = client;
            else
                _userIDRegister.TryAdd(userID, client);
        }

        public void UnregisterClient(int userid, string username)
        {
            GameClient Client = null;
            _userIDRegister.TryRemove(userid, out Client);
            _usernameRegister.TryRemove(username.ToLower(), out Client);
        }

        public void CloseAll()
        {
            foreach (GameClient client in this.GetClients.ToList())
            {
                if (client == null)
                    continue;

                if (client.GetHabbo() != null)
                {
                    try
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery(client.GetHabbo().GetQueryString);
                        }
                        Console.Clear();
                        log.Info("<<- SERVER SHUTDOWN ->> IVNENTORY IS SAVING");
                    }
                    catch
                    {
                    }
                }
            }

            log.Info("Done saving users inventory!");
            log.Info("Closing server connections...");
            try
            {
                foreach (GameClient client in this.GetClients.ToList())
                {
                    if (client == null || client.GetConnection() == null)
                        continue;

                    try
                    {
                        client.GetConnection().Dispose();
                    }
                    catch { }

                    Console.Clear();
                    log.Info("<<- SERVER SHUTDOWN ->> CLOSING CONNECTIONS");

                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }

            if (this._clients.Count > 0)
                this._clients.Clear();

            log.Info("Connections closed!");
        }

        private void TestClientConnections()
        {
            if (clientPingStopwatch.ElapsedMilliseconds >= 30000)
            {
                clientPingStopwatch.Restart();

                try
                {
                    List<GameClient> ToPing = new List<GameClient>();

                    foreach (GameClient client in this._clients.Values.ToList())
                    {
                        if (client.PingCount < 6)
                        {
                            client.PingCount++;

                            ToPing.Add(client);
                        }
                        else
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(client);
                            }
                        }
                    }

                    DateTime start = DateTime.Now;

                    foreach (GameClient Client in ToPing.ToList())
                    {
                        try
                        {
                            Client.SendMessage(new PongComposer());
                        }
                        catch
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(Client);
                            }
                        }
                    }

                }
                catch (Exception e)
                {

                }
            }
        }

        private void HandleTimeouts()
        {
            if (timedOutConnections.Count > 0)
            {
                lock (timedOutConnections.SyncRoot)
                {
                    while (timedOutConnections.Count > 0)
                    {
                        GameClient client = null;

                        if (timedOutConnections.Count > 0)
                            client = (GameClient)timedOutConnections.Dequeue();

                        if (client != null)
                            client.Disconnect();
                    }
                }
            }
        }

        public int Count
        {
            get { return this._clients.Count; }
        }

        public ICollection<GameClient> GetClients
        {
            get
            {
                return this._clients.Values;
            }
        }

        public int userBankWorking()
        {
            int count = 0;
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client != null && Client.GetHabbo() != null && Client.GetHabbo().TravailId == 3 && Client.GetHabbo().Travaille == true)
                {
                    count++;
                }
            }
            return count;
        }

        public int userCroupierWorking()
        {
            int count = 0;
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client != null && Client.GetHabbo() != null && Client.GetHabbo().TravailId == 16 && Client.GetHabbo().RankId > 1 && Client.GetHabbo().Travaille == true)
                {
                    count++;
                }
            }
            return count;
        }

        public bool checkCapture(int RoomId)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client.GetHabbo().CurrentRoomId == RoomId)
                {
                    RoomUser User = Client.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Username);

                    if (User.Capture != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void endPurge()
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null || Client.GetHabbo().CurrentRoom == null)
                    continue;

                if(Client.GetHabbo().ArmeEquiped != null && !Client.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && Client.GetHabbo().ArmeEquiped != "taser")
                {
                    Client.GetHabbo().ArmeEquiped = null;
                    Room Room = Client.GetHabbo().CurrentRoom;
                    RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                    Client.GetHabbo().resetEffectEvent();
                    User.OnChat(User.LastBubble, "* Se déséquipe de son arme *", true);
                }
            }
        }

        public void sendServerMessage(string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null)
                    continue;

                Client.SendWhisper("[OFFICIEL] " + Msg);
            }
        }

        public void PurgeSound(bool Pause)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null)
                    continue;

                try
                {
                    if (Pause)
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "purge;stop");
                    }
                    else
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "purge;start");
                    }
                }
                catch { }
            }
        }

        public string appelPolice(int Id)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null || Client.GetHabbo().TravailId != 4 || Client.GetHabbo().RankId != 1 || Client.GetHabbo().Travaille == false || Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true || Client.GetHabbo().inCallWithUsername != null || Client.GetHabbo().receiveCallUsername != null || Client.GetHabbo().isCalling == true || Client.GetHabbo().Id == Id)
                    continue;

                return Client.GetHabbo().Username;
            }

            return null;
        }

        public void sendStaffMsg(string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetHabbo() == null || Client.GetHabbo().Rank != 8)
                    continue;

                Client.SendWhisper("[ALERTE STAFF] " + Msg);
            }
        }

        public int footballCountUserPlay(Room room, string Color)
        {
            int count = 0;
            foreach (RoomUser roomUser in room.GetRoomUserManager().GetUserList().ToList())
            {
                if (roomUser == null || roomUser.GetClient() == null || roomUser.IsPet || roomUser.IsBot || roomUser.GetClient().GetHabbo().footballTeam == null)
                    continue;

                if(roomUser.GetClient().GetHabbo().footballTeam == Color)
                {
                    count += 1;
                }
            }

            return count;
        }

        public void footballEndMatch()
        {
            PlusEnvironment.footballResetScore();

            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo().CurrentRoomId == 56 || Client.GetHabbo().footballTeam == null)
                    continue;

                Client.GetHabbo().footballTeam = null;
                Client.GetHabbo().footballSpawnChair();
            }
        }

        public void footballUpdateHtmlScore()
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo().CurrentRoomId == 56)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "footballScore;show;" + PlusEnvironment.footballGoalGreen + ";" + PlusEnvironment.footballGoalBlue);
            }
        }

        public void footballSendUserInSpawn()
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo().CurrentRoomId == 56)
                    continue;

                if (Client.GetHabbo().footballTeam == "green")
                {
                    Client.GetHabbo().footballSpawnInItem("green");
                }
                else if (Client.GetHabbo().footballTeam == "blue")
                {
                    Client.GetHabbo().footballSpawnInItem("blue");
                }
            }
        }

        public void sendLastActionMsg(string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null)
                    continue;

                try
                {
                    PlusEnvironment.LastActionMessage += 1;
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "last_action;" + PlusEnvironment.LastActionMessage + ";" + Msg);
                }
                catch(Exception E)
                {

                }
            }
        }

        public void sendGangMsg(int Gang, string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null)
                    continue;

                if (Client.GetHabbo().Gang != Gang)
                    continue;

                Client.SendWhisper("[GANG] " + Msg);
            }
        }

        public void sendUserGangMsg(int Gang, string Username, string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null)
                    continue;

                if (Client.GetHabbo().Gang != Gang)
                    continue;

                Client.SendWhisper("[GANG] " + Username + " : " + Msg);
            }
        }

        public void resetQuizz()
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null)
                    continue;

                if (Client.GetHabbo().Quizz_Points != 0)
                    Client.GetHabbo().Quizz_Points = 0;
            }
        }

        public void sendGouvMsg( string Msg)
        {
            foreach (GameClient Client in this.GetClients.ToList())
            {
                if (Client == null || Client.GetConnection() == null || Client.GetHabbo() == null)
                    continue;

                Client.SendWhisper("[MESSAGE DU GOUVERNEMENT] " + Msg);
            }
        }
    }
}