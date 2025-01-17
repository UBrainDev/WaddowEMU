﻿using System;

using Plus.Net;
using Plus.Core;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Interfaces;
using Plus.HabboHotel.Users.UserDataManagement;

using ConnectionManager;

using Plus.Communication.Packets.Outgoing.Sound;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;


using Plus.Communication.Encryption.Crypto.Prng;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.Communication.Packets.Outgoing.BuildersClub;
using Plus.HabboHotel.Moderation;

using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Permissions;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Campaigns;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Fleck;
using System.Collections.Concurrent;

namespace Plus.HabboHotel.GameClients
{
    public class GameClient
    {
        private readonly int _id;
        private Habbo _habbo;
        public string MachineId;
        private bool _disconnected;
        public ARC4 RC4Client = null;
        private GamePacketParser _packetParser;
        private ConnectionInformation _connection;
        public int PingCount { get; set; }
        public bool LoggingOut = false;
        public ConcurrentDictionary<string, int> Timers;

        public GameClient(int ClientId, ConnectionInformation pConnection)
        {
            this._id = ClientId;
            this._connection = pConnection;
            this._packetParser = new GamePacketParser(this);

            this.PingCount = 0;
        }

        private void SwitchParserRequest()
        {
            _packetParser.SetConnection(_connection);
            _packetParser.onNewPacket += parser_onNewPacket;
            byte[] data = (_connection.parser as InitialPacketParser).currentData;
            _connection.parser.Dispose();
            _connection.parser = _packetParser;
            _connection.parser.handlePacketData(data);
        }

        private void parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                PlusEnvironment.GetGame().GetPacketManager().TryExecutePacket(this, Message);
            }
            catch (Exception e)
            {
                Logging.LogPacketException(Message.ToString(), e.ToString());
            }
        }

        private void PolicyRequest()
        {
            _connection.SendData(PlusEnvironment.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                   "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                   "<cross-domain-policy>\r\n" +
                   "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                   "</cross-domain-policy>\x0"));
        }


        public void StartConnection()
        {
            if (_connection == null)
                return;

            this.PingCount = 0;

            (_connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.startPacketProcessing();
        }

        public bool TryAuthenticate(string AuthTicket)
        {
            try
            {
                byte errorCode = 0;
                UserData userData = UserDataFactory.GetUserData(AuthTicket, out errorCode);
                if (errorCode == 1 || errorCode == 2)
                {
                    Disconnect();
                    return false;
                }

                #region Ban Checking
                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord = null;
                if (!string.IsNullOrEmpty(MachineId))
                {
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(MachineId, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }

                if (userData.user != null)
                {
                    //Now let us check for a username ban record..
                    BanRecord = null;
                    if (PlusEnvironment.GetGame().GetModerationManager().IsBanned(userData.user.Username, out BanRecord))
                    {
                        if (PlusEnvironment.GetGame().GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect();
                            return false;
                        }
                    }
                }
                #endregion

                PlusEnvironment.GetGame().GetClientManager().RegisterClient(this, userData.userID, userData.user.Username);
                _habbo = userData.user;
                if (_habbo != null)
                {
                    userData.user.Init(this, userData);

                    SendMessage(new AuthenticationOKComposer());
                    SendMessage(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    //FurniListNotification -> why?


                    // HOME ROOM
                    if (_habbo.Prison != 0)
                    {
                        SendMessage(new NavigatorSettingsComposer(1));
                    }
                    else if (_habbo.HomeRoom == PlusEnvironment.Salade)
                    {
                        SendMessage(new NavigatorSettingsComposer(1));
                    }
                    else if (_habbo.HomeRoom == 80 || _habbo.HomeRoom == 102 || _habbo.HomeRoom == 105 || _habbo.HomeRoom == 103 || _habbo.HomeRoom == 106 || _habbo.HomeRoom == 110 || _habbo.HomeRoom == 107 || _habbo.HomeRoom == 108 || _habbo.HomeRoom == 109 || _habbo.HomeRoom == 111 || _habbo.HomeRoom == 115 || _habbo.HomeRoom == 116 || _habbo.HomeRoom == 120 || _habbo.HomeRoom == 121 || _habbo.HomeRoom == 122 || _habbo.HomeRoom == 123 || _habbo.HomeRoom == 125 || _habbo.HomeRoom == 126 || _habbo.HomeRoom == 127)
                    {
                        SendMessage(new NavigatorSettingsComposer(1));
                    }
                    else
                    {
                        SendMessage(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    }
                    SendMessage(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendMessage(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingAllParts));
                    //1984
                    //2102
                    SendMessage(new UserRightsComposer(_habbo.Rank));
                    SendMessage(new AvailabilityStatusComposer());
                    //1044
                    SendMessage(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    //3674
                    //3437
                    SendMessage(new BuildersClubMembershipComposer());
                    SendMessage(new CfhTopicsInitComposer());

                    SendMessage(new BadgeDefinitionsComposer(PlusEnvironment.GetGame().GetAchievementManager()._achievements));
                    SendMessage(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference, _habbo.AllowMessengerInvites, _habbo.FocusPreference, FriendBarStateUtility.GetInt(_habbo.FriendbarState)));
                    //SendMessage(new TalentTrackLevelComposer());

                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (this._habbo.MachineId != MachineId)
                        {
                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery("UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }
                        }

                        _habbo.MachineId = MachineId;
                    }

                    PermissionGroup PermissionGroup = null;
                    if (PlusEnvironment.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup))
                    {
                        if (!String.IsNullOrEmpty(PermissionGroup.Badge))
                            if (!_habbo.GetBadgeComponent().HasBadge(PermissionGroup.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(PermissionGroup.Badge, true, this);
                    }

                    SubscriptionData SubData = null;
                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(this._habbo.VIPRank, out SubData))
                    {
                        if (!String.IsNullOrEmpty(SubData.Badge))
                        {
                            if (!_habbo.GetBadgeComponent().HasBadge(SubData.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(SubData.Badge, true, this);
                        }
                    }

                    if (!PlusEnvironment.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                        PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_habbo.Id);

                    _habbo.InitProcess();

                    if (userData.user.GetPermissions().HasRight("mod_tickets"))
                    {
                        SendMessage(new ModeratorInitComposer(
                          PlusEnvironment.GetGame().GetModerationManager().UserMessagePresets,
                          PlusEnvironment.GetGame().GetModerationManager().RoomMessagePresets,
                          PlusEnvironment.GetGame().GetModerationManager().UserActionPresets,
                          PlusEnvironment.GetGame().GetModerationTool().GetTickets));
                    }

                    if (_habbo.Mutuelle != 0 && DateTime.Now > _habbo.MutuelleDate)
                    {
                        _habbo.Mutuelle = 0;
                        _habbo.updateMutuelle();
                    }

                    if (_habbo.PoliceCasier != null && _habbo.Prison == 0)
                    {
                        if (_habbo.PoliceCasier.Contains("[TELEPHONE]") && _habbo.Telephone == 1)
                        {
                            _habbo.PoliceCasier = _habbo.PoliceCasier.Replace("[TELEPHONE]", "");
                        }

                        if (_habbo.PoliceCasier.Contains("[AK47]") && _habbo.Ak47 == 1)
                        {
                            _habbo.PoliceCasier = _habbo.PoliceCasier.Replace("[AK47]", "");
                        }

                        if (_habbo.PoliceCasier.Contains("[UZI]") && _habbo.Uzi == 1)
                        {
                            _habbo.PoliceCasier = _habbo.PoliceCasier.Replace("[UZI]", "");
                        }

                        if (_habbo.PoliceCasier.Contains("[SABRE]") && _habbo.Sabre == 1)
                        {
                            _habbo.PoliceCasier = _habbo.PoliceCasier.Replace("[SABRE]", "");
                        }

                        if (_habbo.PoliceCasier.Contains("[BATTE]") && _habbo.Batte == 1)
                        {
                            _habbo.PoliceCasier = _habbo.PoliceCasier.Replace("[BATTE]", "");
                        }

                        _habbo.updatePoliceCasier();
                    }

                    SendMessage(new MOTDNotificationComposer("Bienvenue sur Waddow " + _habbo.Username + " !\n_____________________________________________________\n\nLe rétro n'est pas finit . il y a donc encore beaucoup de bugs ."/* +  PlusEnvironment.GetDBConfig().DBData["welcome_message"]*/));
                    //Nous sommes en version BETA, cela signifie qu'il ce peut que vous rencontriez quelques bugs.\nMerci de les signaler sur le forum.\n\nTous les looks de métiers arriveront très prochainement !\n\n Vous pouvez utiliser la commande :look pour changer de look\nSTADE DE FOOT DISPONIBLE DEPUIS LE PARC (Centre ville)
                    PlusEnvironment.GetGame().GetRewardManager().CheckRewards(this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Bug during user login: " + e);
            }
            return false;
        }

        public void SendWhisper(string Message, int Colour = 0)
        {
            if (this == null || GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
            if (User == null)
                return;

            SendMessage(new WhisperComposer(User.VirtualId, Message, 0, (Colour == 0 ? User.LastBubble : Colour)));
        }

        public void SendNotification(string Message)
        {
            SendMessage(new BroadcastMessageAlertComposer(Message));
        }

        public void SendMessage(IServerPacket Message)
        {
            byte[] bytes = Message.GetBytes();

            if (Message == null)
                return;

            if (GetConnection() == null)
                return;

            GetConnection().SendData(bytes);
        }

        public int ConnectionID
        {
            get { return _id; }
        }

        public ConnectionInformation GetConnection()
        {
            return _connection;
        }

        public Habbo GetHabbo()
        {
            return _habbo;
        }

        public void Disconnect()
        {
            try
            {
                if (GetHabbo() != null)
                {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(GetHabbo().GetQueryString);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }


            if (!_disconnected)
            {
                if (_connection != null)
                    _connection.Dispose();
                _disconnected = true;
            }
        }

        public void Dispose(int ClientId)
        {
            if (GetHabbo() != null && GetHabbo().InRoom)
            {
                if (GetHabbo().Travaille == true)
                {
                    GetHabbo().stopWork();
                }

                RoomUser User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Id);
                User.isDisconnecting = true;
                User.OnChat(User.LastBubble, "* Quitte la ville dans 3 secondes *", true);
                GetHabbo().Effects().ApplyEffect(1);
            }

            System.Timers.Timer timer1 = new System.Timers.Timer(3000);
            timer1.Interval = 3000;
            timer1.Elapsed += delegate
            {
                if (GetHabbo() != null)
                    GetHabbo().OnDisconnect();

                PlusEnvironment.GetGame().GetClientManager().removeConnection(ClientId);
                this.MachineId = string.Empty;
                this._disconnected = true;
                this._habbo = null;
                this._connection = null;
                this.RC4Client = null;
                this._packetParser = null;
                timer1.Stop();
            };
            timer1.Start();
        }
    }
}