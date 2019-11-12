using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using log4net;

using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GroupsRank;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Users.Badges;
using Plus.HabboHotel.Users.Inventory;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;
using Plus.HabboHotel.Users.UserDataManagement;

using Plus.HabboHotel.Users.Process;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;


using Plus.HabboHotel.Users.Navigator.SavedSearches;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Users.Clothing;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Users.Permissions;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Users.Calendar;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Items;
using Fleck;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Users
{
    public class Habbo
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Users");

        //Generic player values.
        private int _id;
        private string _username;
        private int _rank;
        private string _motto;
        private string _look;
        private int _antiAFK = 0;
        private int _antiAFKAttempt = 0;
        private string _conduit;
        private Dictionary<int, DateTime> _EventDirectionary = new Dictionary<int, DateTime>();
        
        private int _EventCount = 0;
        private string _EventItem = null;
        private string _footballTeam = null;
        private bool _isCalling = false;
        private string _inCallWithUsername = null;
        private string _receiveCallUsername = null;
        private int _timeAppel = 0;
        private string _callingToken = null;
        private string _gender;
        private string _footballLook;
        private string _footballGender;
        private bool _telephoneEteint = false;
        private int _credits;
        private int _duckets;
        private int _diamonds;
        private int _gotwPoints;
        private int _homeRoom;
        private double _lastOnline;
        private string _lastIp;
        private double _accountCreated;
        private List<int> _clientVolume;
        private double _lastNameChange;
        private string _machineId;
        private bool _chatPreference;
        private bool _focusPreference;
        private bool _isExpert;
        private int _vipRank;
        private int _Gps;
        private int _Sante;
        private int _Energie;
        private int _Sommeil;
        private int _Hygiene;
        private int _Telephone;
        private string _TelephoneName;
        private int _TelephoneForfait;
        private int _TelephoneForfaitSms;
        private int _TelephoneForfaitType;
        private DateTime _TelephoneForfaitReset;
        private int _Banque;
        private int _Savon;
        private int _Timer;
        private bool _Travaille;
        private bool _CanChangeRoom;
        private bool _Recharge;
        private bool _Menotted;
        private string _MenottedUsername;
        private int _Chargeur;
        private string _ArmeEquiped;
        private string _Commande;
        private int _Prison;
        private int _Hopital;
        private int _PrisonCount;
        private int _Morts;
        private int _Kills;
        private int _Coca;
        private int _Fanta;
        private int _Blur;
        private int _SmokeTimer;
        private int _AudiA8;
        private int _Porsche911;
        private int _FiatPunto;
        private int _VolkswagenJetta;
        private int _BmwI8;
        private string _Cb;
        private int _Sucette;
        private int _Pain;
        private int _Doliprane;
        private int _Mutuelle;
        private DateTime _MutuelleDate;
        private int _Gang;
        private int _GangRank;
        private int _Batte;
        private int _Sabre;
        private int _Ak47;
        private int _Ak47_Munitions;
        private int _Uzi;
        private int _Uzi_Munitions;

        //Abilitys triggered by generic events.
        private bool _appearOffline;
        private bool _allowTradingRequests;
        private bool _allowUserFollowing;
        private bool _allowFriendRequests;
        private bool _allowMessengerInvites;
        private bool _allowPetSpeech;
        private bool _allowBotSpeech;
        private bool _allowPublicRoomStatus;
        private bool _allowConsoleMessages;
        private bool _allowGifts;
        private bool _allowMimic;
        private bool _receiveWhispers;
        private bool _ignorePublicWhispers;
        private bool _playingFastFood;
        private FriendBarState _friendbarState;
        private int _christmasDay;
        private int _wantsToRideHorse;
        private int _timeAFK;
        private bool _disableForcedEffects;

        //Player saving.
        private bool _disconnected;
        private bool _habboSaved;
        private bool _changingName;

        //Counters
        private double _floodTime;
        private int _friendCount;
        private double _timeMuted;
        private double _tradingLockExpiry;
        private int _bannedPhraseCount;
        private double _sessionStart;
        private int _messengerSpamCount;
        private double _messengerSpamTime;
        private int _creditsTickUpdate;
        private int _energieUpdate;
        private int _OneMinuteUpdate;
        private double _Pierre;
        private int _Confirmed;
        private int _Coiffure;
        private string _PoliceCasier;
        private int _Carte;
        private int _Facebook;
        private int _Sac;
        private int _Creations;
        private int _Permis_arme;
        private int _Clipper;
        private int _Weed;
        private int _PhilipMo;
        private int _EventDay;
        private int _EventPoints;
        private int _Casino_Jetons;
        private int _Quizz_Points;
        private int _Cocktails;
        private int _WhiteHoverboard;
        private int _AudiA3;
        private decimal _Eau;
        private int _CasierWeed;
        private int _CasierCocktails;

        //Room related
        private int _tentId;
        private int _hopperId;
        private bool _isHopping;
        private int _teleportId;
        private bool _isTeleporting;
        private int _teleportingRoomId;
        private bool _roomAuthOk;
        private int _currentRoomId;

        //Advertising reporting system.
        private bool _hasSpoken;
        private bool _advertisingReported;
        private double _lastAdvertiseReport;
        private bool _advertisingReportBlocked;

        //Values generated within the game.
        private bool _wiredInteraction;
        private int _questLastCompleted;
        private bool _inventoryAlert;
        private bool _ignoreBobbaFilter;
        private bool _wiredTeleporting;
        private int _customBubbleId;
        private int _tempInt;
        private bool _onHelperDuty;

        //Fastfood
        private int _fastfoodScore;

        //Just random fun stuff.
        private int _petId;

        //Anti-script placeholders.
        private DateTime _lastGiftPurchaseTime;
        private DateTime _lastMottoUpdateTime;
        private DateTime _lastClothingUpdateTime;
        private DateTime _lastForumMessageUpdateTime;

        private int _giftPurchasingWarnings;
        private int _mottoUpdateWarnings;
        private int _clothingUpdateWarnings;

        private bool _sessionGiftBlocked;
        private bool _sessionMottoBlocked;
        private bool _sessionClothingBlocked;

        public List<int> RatedRooms;
        public List<int> MutedUsers;
        public List<RoomData> UsersRooms;

        private GameClient _client;
        private HabboStats _habboStats;
        private HabboMessenger Messenger;
        private ProcessComponent _process;
        public ArrayList FavoriteRooms;
        public Dictionary<int, int> quests;
        private BadgeComponent BadgeComponent;
        private InventoryComponent InventoryComponent;
        public Dictionary<int, Relationship> Relationships;
        public ConcurrentDictionary<string, UserAchievement> Achievements;
        public ConcurrentDictionary<string, int> ActiveCooldowns;
        public double StackHeight = 0;

        private DateTime _timeCached;

        private SearchesComponent _navigatorSearches;
        private EffectsComponent _fx;
        private ClothingComponent _clothing;
        private PermissionComponent _permissions;

        private IChatCommand _iChatCommand;

        public Habbo(int Id, string Username, int Rank, string Motto, string Look, string Gender, int Credits, int ActivityPoints, int HomeRoom,
            bool HasFriendRequestsDisabled, int LastOnline, string LastIp, bool AppearOffline, bool HideInRoom, double CreateDate, int Diamonds,
            string machineID, string clientVolume, bool ChatPreference, bool FocusPreference, bool PetsMuted, bool BotsMuted, bool AdvertisingReportBlocked, double LastNameChange,
            int GOTWPoints, bool IgnoreInvites, double TimeMuted, double TradingLock, bool AllowGifts, int FriendBarState, bool DisableForcedEffects, bool AllowMimic, int VIPRank, int Gps, int Sante, int Energie, int Sommeil, int Hygiene, int Telephone, string TelephoneName, int TelephoneForfait, int TelephoneForfaitSMS, int TelephoneForfaitType, DateTime TelephoneForfaitReset, int Banque, int Savon, int Timer, bool Travaille, int Prison, int Hopital, int PrisonCount, int Morts, int Kills, int Coca, int Fanta, int Blur, int AudiA8, int Porsche911, int FiatPunto, int VolkswagenJetta, int BmwI8, string Cb, int Sucette, int Pain, int Doliprane, string Commande, int Mutuelle, DateTime MutuelleDate, int Gang, int GangRank, string ArmeEquiped, int Batte, int Sabre, int Ak47, int Ak47_munitions, int Uzi, int Uzi_Munitions, bool Recharge, bool CanChangeRoom, int Chargeur, bool Menotted, string MenottedUsername, double Pierre, int Confirmed, int Coiffure, string PoliceCasier, int Carte, int Facebook, int Sac, int Creations, int Permis_arme, int Clipper, int Weed, int PhilipMo, int EventDay, int EventPoints, int Casino_Jetons, int Quizz_Points, int Cocktails, int WhiteHoverboard, int AudiA3, decimal Eau, int CasierWeed, int CasierCocktails)
        {
            this._id = Id;
            this._username = Username;
            this._rank = Rank;
            this._motto = Motto;
            this._look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Look);
            this._gender = Gender.ToLower();
            this._footballLook = PlusEnvironment.FilterFigure(Look.ToLower());
            this._footballGender = Gender.ToLower();
            this._credits = Credits;
            this._duckets = ActivityPoints;
            this._diamonds = Diamonds;
            this._gotwPoints = GOTWPoints;
            this._homeRoom = HomeRoom;
            this._lastOnline = LastOnline;
            this._lastIp = LastIp;
            this._accountCreated = CreateDate;
            this._clientVolume = new List<int>();
            foreach (string Str in clientVolume.Split(','))
            {
                int Val = 0;
                if (int.TryParse(Str, out Val))
                    this._clientVolume.Add(int.Parse(Str));
                else
                    this._clientVolume.Add(100);
            }

            this._lastNameChange = LastNameChange;
            this._machineId = machineID;
            this._chatPreference = ChatPreference;
            this._focusPreference = FocusPreference;
            this._isExpert = IsExpert == true;

            this._appearOffline = AppearOffline;
            this._allowTradingRequests = true;//TODO
            this._allowUserFollowing = true;//TODO
            this._allowFriendRequests = HasFriendRequestsDisabled;//TODO
            this._allowMessengerInvites = IgnoreInvites;
            this._allowPetSpeech = PetsMuted;
            this._allowBotSpeech = BotsMuted;
            this._allowPublicRoomStatus = HideInRoom;
            this._allowConsoleMessages = true;
            this._allowGifts = AllowGifts;
            this._allowMimic = AllowMimic;
            this._receiveWhispers = true;
            this._ignorePublicWhispers = false;
            this._playingFastFood = false;
            this._friendbarState = FriendBarStateUtility.GetEnum(FriendBarState);
            this._christmasDay = ChristmasDay;
            this._wantsToRideHorse = 0;
            this._timeAFK = 0;
            this._disableForcedEffects = DisableForcedEffects;
            this._vipRank = VIPRank;
            this._Gps = Gps;
            this._Sante = Sante;
            this._Energie = Energie;
            this._Sommeil = Sommeil;
            this._Hygiene = Hygiene;
            this._Telephone = Telephone;
            this._TelephoneName = TelephoneName;
            this._TelephoneForfait = TelephoneForfait;
            this._TelephoneForfaitSms = TelephoneForfaitSMS;
            this._TelephoneForfaitType = TelephoneForfaitType;
            this._TelephoneForfaitReset = TelephoneForfaitReset;
            this._Banque = Banque;
            this._Savon = Savon;
            this._Timer = Timer;
            this._Travaille = false;
            this._Prison = Prison;
            this._Hopital = Hopital;
            this._PrisonCount = PrisonCount;
            this._Kills = Kills;
            this._Morts = Morts;
            this._Coca = Coca;
            this._Fanta = Fanta;
            this._Blur = Blur;
            this._AudiA8 = AudiA8;
            this._Porsche911 = Porsche911;
            this._FiatPunto = FiatPunto;
            this._VolkswagenJetta = VolkswagenJetta;
            this._BmwI8 = BmwI8;
            this._Cb = Cb;
            this._Sucette = Sucette;
            this._Pain = Pain;
            this._Doliprane = Doliprane;
            this._Commande = null;
            this._Mutuelle = Mutuelle;
            this._MutuelleDate = MutuelleDate;
            this._Gang = Gang;
            this._GangRank = GangRank;
            this._ArmeEquiped = null;
            this._Batte = Batte;
            this._Sabre = Sabre;
            this._Ak47 = Ak47;
            this._Ak47_Munitions = Ak47_munitions;
            this._Uzi = Uzi;
            this._Uzi_Munitions = Uzi_Munitions;
            this._Recharge = false;
            this._Menotted = false;
            this._footballTeam = null;
            this._MenottedUsername = null;
            this.ActiveCooldowns = new ConcurrentDictionary<string, int>();
            this._CanChangeRoom = true;
            this._Chargeur = 0;
            this._Uzi_Munitions = Uzi_Munitions;
            this._Pierre = Pierre;
            this._Confirmed = Confirmed;
            this._Coiffure = Coiffure;
            this._PoliceCasier = PoliceCasier;
            this._Carte = Carte;
            this._Facebook = Facebook;
            this._Sac = Sac;
            this._Creations = Creations;
            this._Permis_arme = Permis_arme;
            this._Clipper = Clipper;
            this._Weed = Weed;
            this._PhilipMo = PhilipMo;
            this._EventDay = EventDay;
            this._EventPoints = EventPoints;
            this._Casino_Jetons = Casino_Jetons;
            this._Quizz_Points = Quizz_Points;
            this._Cocktails = Cocktails;
            this._WhiteHoverboard = WhiteHoverboard;
            this._AudiA3 = AudiA3;
            this._Eau = Eau;
            this._CasierWeed = CasierWeed;
            this._CasierCocktails = CasierCocktails;

            this._disconnected = false;
            this._habboSaved = false;
            this._changingName = false;

            this._floodTime = 0;
            this._friendCount = 0;
            this._timeMuted = TimeMuted;
            this._timeCached = DateTime.Now;

            this._tradingLockExpiry = TradingLock;
            if (this._tradingLockExpiry > 0 && PlusEnvironment.GetUnixTimestamp() > this.TradingLockExpiry)
            {
                this._tradingLockExpiry = 0;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + Id + "' LIMIT 1");
                }
            }

            this._bannedPhraseCount = 0;
            this._sessionStart = PlusEnvironment.GetUnixTimestamp();
            this._messengerSpamCount = 0;
            this._messengerSpamTime = 0;
            this._creditsTickUpdate = PlusStaticGameSettings.UserCreditsUpdateTimer;

            this._tentId = 0;
            this._hopperId = 0;
            this._isHopping = false;
            this._teleportId = 0;
            this._isTeleporting = false;
            this._teleportingRoomId = 0;
            this._roomAuthOk = false;
            this._currentRoomId = 0;

            this._hasSpoken = false;
            this._lastAdvertiseReport = 0;
            this._advertisingReported = false;
            this._advertisingReportBlocked = AdvertisingReportBlocked;

            this._wiredInteraction = false;
            this._questLastCompleted = 0;
            this._inventoryAlert = false;
            this._ignoreBobbaFilter = false;
            this._wiredTeleporting = false;
            this._customBubbleId = 0;
            this._onHelperDuty = false;
            this._fastfoodScore = 0;
            this._petId = 0;
            this._tempInt = 0;

            this._lastGiftPurchaseTime = DateTime.Now;
            this._lastMottoUpdateTime = DateTime.Now;
            this._lastClothingUpdateTime = DateTime.Now;
            this._lastForumMessageUpdateTime = DateTime.Now;

            this._giftPurchasingWarnings = 0;
            this._mottoUpdateWarnings = 0;
            this._clothingUpdateWarnings = 0;

            this._sessionGiftBlocked = false;
            this._sessionMottoBlocked = false;
            this._sessionClothingBlocked = false;

            this.FavoriteRooms = new ArrayList();
            this.MutedUsers = new List<int>();
            this.Achievements = new ConcurrentDictionary<string, UserAchievement>();
            this.Relationships = new Dictionary<int, Relationship>();
            this.RatedRooms = new List<int>();
            this.UsersRooms = new List<RoomData>();

            //TODO: Nope.
            this.InitPermissions();

            #region Stats
            DataRow StatRow = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", Id);
                StatRow = dbClient.getRow();

                if (StatRow == null)//No row, add it yo
                {
                    dbClient.RunQuery("INSERT INTO `user_stats` (`id`) VALUES ('" + Id + "')");
                    dbClient.SetQuery("SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                    dbClient.AddParameter("user_id", Id);
                    StatRow = dbClient.getRow();
                }

                try
                {
                    this._habboStats = new HabboStats(Convert.ToInt32(StatRow["roomvisits"]), Convert.ToDouble(StatRow["onlineTime"]), Convert.ToInt32(StatRow["respect"]), Convert.ToInt32(StatRow["respectGiven"]), Convert.ToInt32(StatRow["giftsGiven"]),
                        Convert.ToInt32(StatRow["giftsReceived"]), Convert.ToInt32(StatRow["dailyRespectPoints"]), Convert.ToInt32(StatRow["dailyPetRespectPoints"]), Convert.ToInt32(StatRow["AchievementScore"]),
                        Convert.ToInt32(StatRow["quest_id"]), Convert.ToInt32(StatRow["quest_progress"]), this.TravailId, Convert.ToString(StatRow["respectsTimestamp"]), Convert.ToInt32(StatRow["forum_posts"]));

                    if (Convert.ToString(StatRow["respectsTimestamp"]) != DateTime.Today.ToString("MM/dd"))
                    {
                        this._habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                        SubscriptionData SubData = null;

                        int DailyRespects = 10;

                        if (this._permissions.HasRight("mod_tool"))
                            DailyRespects = 20;
                        else if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VIPRank, out SubData))
                            DailyRespects = SubData.Respects;

                        this._habboStats.DailyRespectPoints = DailyRespects;
                        this._habboStats.DailyPetRespectPoints = DailyRespects;

                        dbClient.RunQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '" + DailyRespects + "', `dailyPetRespectPoints` = '" + DailyRespects + "', `respectsTimestamp` = '" + DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + Id + "' LIMIT 1");
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
            }

            Group G = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(this._habboStats.FavouriteGroupId, out G))
                this._habboStats.FavouriteGroupId = 0;

            #endregion
        }

        public int Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        public string Username
        {
            get { return this._username; }
            set { this._username = value; }
        }

        public int Rank
        {
            get { return this._rank; }
            set { this._rank = value; }
        }

        public string Motto
        {
            get { return this._motto; }
            set { this._motto = value; }
        }

        public string Look
        {
            get { return this._look; }
            set { this._look = value; }
        }

        public int TravailId
        {
            get
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT group_id FROM `group_memberships` WHERE user_id = @userId LIMIT 1");
                    dbClient.AddParameter("userId", this.Id);
                    int TravailIDreturn = dbClient.getInteger();
                    return TravailIDreturn;
                }
            }
        }

        public int RankId
        {
            get
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT rank_id FROM `group_memberships` WHERE user_id = @userId");
                    dbClient.AddParameter("userId", this.Id);
                    int rankIdReturn = dbClient.getInteger();
                    return rankIdReturn;
                }
            }
        }

        public Group TravailInfo
        {
            get {
                Group G = null;
                PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(this.TravailId, out G);
                return G;
            }
        }

        public GroupRank RankInfo
        {
            get
            {
                GroupRank G = null;
                PlusEnvironment.GetGame().getGroupRankManager().TryGetRank(this.TravailId, this.RankId, out G);
                return G;
            }
        }


        public string Conduit
        {
            get { return this._conduit; }
            set { this._conduit = value; }
        }

        public Dictionary<int, DateTime> EventDirectionary
        {
            get { return this._EventDirectionary; }
            set { this._EventDirectionary = value; }
        }

        public int EventCount
        {
            get { return this._EventCount; }
            set { this._EventCount = value; }
        }

        public string EventItem
        {
            get { return this._EventItem; }
            set { this._EventItem = value; }
        }

        public string footballTeam
        {
            get { return this._footballTeam; }
            set { this._footballTeam = value; }
        }

        public bool isCalling
        {
            get { return this._isCalling; }
            set { this._isCalling = value; }
        }

        public string inCallWithUsername
        {
            get { return this._inCallWithUsername; }
            set { this._inCallWithUsername = value; }
        }

        public int AntiAFK
        {
            get { return this._antiAFK; }
            set { this._antiAFK = value; }
        }

        public int AntiAFKAttempt
        {
            get { return this._antiAFKAttempt; }
            set { this._antiAFKAttempt = value; }
        }

        public bool TelephoneEteint
        {
            get { return this._telephoneEteint; }
            set { this._telephoneEteint = value; }
        }

        public string receiveCallUsername
        {
            get { return this._receiveCallUsername; }
            set { this._receiveCallUsername = value; }
        }

        public int timeAppel
        {
            get { return this._timeAppel; }
            set { this._timeAppel = value; }
        }

        public string callingToken
        {
            get { return this._callingToken; }
            set { this._callingToken = value; }
        }

        public string Gender
        {
            get { return this._gender; }
            set { this._gender = value; }
        }

        public string FootballLook
        {
            get { return this._footballLook; }
            set { this._footballLook = value; }
        }

        public string FootballGender
        {
            get { return this._footballGender; }
            set { this._footballGender = value; }
        }

        public int Credits
        {
            get { return this._credits; }
            set { this._credits = value; }
        }

        public int Duckets
        {
            get { return this._duckets; }
            set { this._duckets = value; }
        }

        public int Diamonds
        {
            get { return this._diamonds; }
            set { this._diamonds = value; }
        }

        public int GOTWPoints
        {
            get { return this._gotwPoints; }
            set { this._gotwPoints = value; }
        }

        public int HomeRoom
        {
            get { return this._homeRoom; }
            set { this._homeRoom = value; }
        }

        public double LastOnline
        {
            get { return this._lastOnline; }
            set { this._lastOnline = value; }
        }

        public string LastIp
        {
            get { return this._lastIp; }
            set { this._lastIp = value; }
        }

        public double AccountCreated
        {
            get { return this._accountCreated; }
            set { this._accountCreated = value; }
        }

        public List<int> ClientVolume
        {
            get { return this._clientVolume; }
            set { this._clientVolume = value; }
        }

        public double LastNameChange
        {
            get { return this._lastNameChange; }
            set { this._lastNameChange = value; }
        }

        public string MachineId
        {
            get { return this._machineId; }
            set { this._machineId = value; }
        }

        public bool ChatPreference
        {
            get { return this._chatPreference; }
            set { this._chatPreference = value; }
        }
        public bool FocusPreference
        {
            get { return this._focusPreference; }
            set { this._focusPreference = value; }
        }

        public bool IsExpert
        {
            get { return this._isExpert; }
            set { this._isExpert = value; }
        }

        public bool AppearOffline
        {
            get { return this._appearOffline; }
            set { this._appearOffline = value; }
        }

        public int VIPRank
        {
            get { return this._vipRank; }
            set { this._vipRank = value; }
        }

        public int Gps
        {
            get { return this._Gps; }
            set { this._Gps = value; }
        }

        public int PrisonCount
        {
            get { return this._PrisonCount; }
            set { this._PrisonCount = value; }
        }

        public int Coca
        {
            get { return this._Coca; }
            set { this._Coca = value; }
        }

        public int Fanta
        {
            get { return this._Fanta; }
            set { this._Fanta = value; }
        }

        public int Blur
        {
            get { return this._Blur; }
            set { this._Blur = value; }
        }

        public int SmokeTimer
        {
            get { return this._SmokeTimer; }
            set { this._SmokeTimer = value; }
        }

        public int AudiA8
        {
            get { return this._AudiA8; }
            set { this._AudiA8 = value; }
        }

        public int Porsche911
        {
            get { return this._Porsche911; }
            set { this._Porsche911 = value; }
        }

        public int FiatPunto
        {
            get { return this._FiatPunto; }
            set { this._FiatPunto = value; }
        }

        public int VolkswagenJetta
        {
            get { return this._VolkswagenJetta; }
            set { this._VolkswagenJetta = value; }
        }

        public int BmwI8
        {
            get { return this._BmwI8; }
            set { this._BmwI8 = value; }
        }

        public string Cb
        {
            get { return this._Cb; }
            set { this._Cb = value; }
        }

        public int Sucette
        {
            get { return this._Sucette; }
            set { this._Sucette = value; }
        }

        public int Pain
        {
            get { return this._Pain; }
            set { this._Pain = value; }
        }

        public int Doliprane
        {
            get { return this._Doliprane; }
            set { this._Doliprane = value; }
        }

        public int Kills
        {
            get { return this._Kills; }
            set { this._Kills = value; }
        }

        public int Morts
        {
            get { return this._Morts; }
            set { this._Morts = value; }
        }

        public int Prison
        {
            get { return this._Prison; }
            set { this._Prison = value; }
        }

        public int Hopital
        {
            get { return this._Hopital; }
            set { this._Hopital = value; }
        }

        public int Sante
        {
            get { return this._Sante; }
            set { this._Sante = value; }
        }

        public double Pierre
        {
            get { return this._Pierre; }
            set { this._Pierre = value; }
        }

        public int Confirmed
        {
            get { return this._Confirmed; }
            set { this._Confirmed = value; }
        }

        public int Coiffure
        {
            get { return this._Coiffure; }
            set { this._Coiffure = value; }
        }

        public string PoliceCasier
        {
            get { return this._PoliceCasier; }
            set { this._PoliceCasier = value; }
        }

        public int Carte
        {
            get { return this._Carte; }
            set { this._Carte = value; }
        }

        public int Facebook
        {
            get { return this._Facebook; }
            set { this._Facebook = value; }
        }

        public int Sac
        {
            get { return this._Sac; }
            set { this._Sac = value; }
        }

        public int Creations
        {
            get { return this._Creations; }
            set { this._Creations = value; }
        }

        public int Permis_arme
        {
            get { return this._Permis_arme; }
            set { this._Permis_arme = value; }
        }

        public int Clipper
        {
            get { return this._Clipper; }
            set { this._Clipper = value; }
        }

        public int Weed
        {
            get { return this._Weed; }
            set { this._Weed = value; }
        }

        public int PhilipMo
        {
            get { return this._PhilipMo; }
            set { this._PhilipMo = value; }
        }

        public int EventDay
        {
            get { return this._EventDay; }
            set { this._EventDay = value; }
        }

        public int EventPoints
        {
            get { return this._EventPoints; }
            set { this._EventPoints = value; }
        }

        public int Casino_Jetons
        {
            get { return this._Casino_Jetons; }
            set { this._Casino_Jetons = value; }
        }

        public int Quizz_Points
        {
            get { return this._Quizz_Points; }
            set { this._Quizz_Points = value; }
        }

        public int Cocktails
        {
            get { return this._Cocktails; }
            set { this._Cocktails = value; }
        }

        public int WhiteHoverboard
        {
            get { return this._WhiteHoverboard; }
            set { this._WhiteHoverboard = value; }
        }

        public int AudiA3
        {
            get { return this._AudiA3; }
            set { this._AudiA3 = value; }
        }

        public decimal Eau
        {
            get { return this._Eau; }
            set { this._Eau = value; }
        }

        public int CasierWeed
        {
            get { return this._CasierWeed; }
            set { this._CasierWeed = value; }
        }

        public int CasierCocktails
        {
            get { return this._CasierCocktails; }
            set { this._CasierCocktails = value; }
        }

        public int Energie
        {
            get { return this._Energie; }
            set { this._Energie = value; }
        }

        public int Sommeil
        {
            get { return this._Sommeil; }
            set { this._Sommeil = value; }
        }

        public int Hygiene
        {
            get { return this._Hygiene; }
            set { this._Hygiene = value; }
        }

        public int Telephone
        {
            get { return this._Telephone; }
            set { this._Telephone = value; }
        }

        public string TelephoneName
        {
            get { return this._TelephoneName; }
            set { this._TelephoneName = value; }
        }

        public int TelephoneForfait
        {
            get { return this._TelephoneForfait; }
            set { this._TelephoneForfait = value; }
        }

        public int TelephoneForfaitSms
        {
            get { return this._TelephoneForfaitSms; }
            set { this._TelephoneForfaitSms = value; }
        }

        public int TelephoneForfaitType
        {
            get { return this._TelephoneForfaitType; }
            set { this._TelephoneForfaitType = value; }
        }

        public DateTime TelephoneForfaitReset
        {
            get { return this._TelephoneForfaitReset; }
            set { this._TelephoneForfaitReset = value; }
        }

        public int Banque
        {
            get { return this._Banque; }
            set { this._Banque = value; }
        }

        public int Savon
        {
            get { return this._Savon; }
            set { this._Savon = value; }
        }

        public int Timer
        {
            get { return this._Timer; }
            set { this._Timer = value; }
        }

        public bool Travaille
        {
            get { return this._Travaille; }
            set { this._Travaille = value; }
        }
        public bool CanChangeRoom
        {
            get { return this._CanChangeRoom; }
            set { this._CanChangeRoom = value; }
        }

        public bool Recharge
        {
            get { return this._Recharge; }
            set { this._Recharge = value; }
        }

        public bool Menotted
        {
            get { return this._Menotted; }
            set { this._Menotted = value; }
        }

        public string MenottedUsername
        {
            get { return this._MenottedUsername; }
            set { this._MenottedUsername = value; }
        }

        public int Chargeur
        {
            get { return this._Chargeur; }
            set { this._Chargeur = value; }
        }

        public string ArmeEquiped
        {
            get { return this._ArmeEquiped; }
            set { this._ArmeEquiped = value; }
        }

        public string Commande
        {
            get { return this._Commande; }
            set { this._Commande = value; }
        }

        public int Mutuelle
        {
            get { return this._Mutuelle; }
            set { this._Mutuelle = value; }
        }

        public DateTime MutuelleDate
        {
            get { return this._MutuelleDate; }
            set { this._MutuelleDate = value; }
        }

        public int Gang
        {
            get { return this._Gang; }
            set { this._Gang = value; }
        }

        public int GangRank
        {
            get { return this._GangRank; }
            set { this._GangRank = value; }
        }

        public int Batte
        {
            get { return this._Batte; }
            set { this._Batte = value; }
        }

        public int Sabre
        {
            get { return this._Sabre; }
            set { this._Sabre = value; }
        }

        public int Ak47
        {
            get { return this._Ak47; }
            set { this._Ak47 = value; }
        }

        public int AK47_Munitions
        {
            get { return this._Ak47_Munitions; }
            set { this._Ak47_Munitions = value; }
        }

        public int Uzi
        {
            get { return this._Uzi; }
            set { this._Uzi = value; }
        }

        public int Uzi_Munitions
        {
            get { return this._Uzi_Munitions; }
            set { this._Uzi_Munitions = value; }
        }

        public int TempInt
        {
            get { return this._tempInt; }
            set { this._tempInt = value; }
        }

        public bool AllowTradingRequests
        {
            get { return this._allowTradingRequests; }
            set { this._allowTradingRequests = value; }
        }

        public bool AllowUserFollowing
        {
            get { return this._allowUserFollowing; }
            set { this._allowUserFollowing = value; }
        }

        public bool AllowFriendRequests
        {
            get { return this._allowFriendRequests; }
            set { this._allowFriendRequests = value; }
        }

        public bool AllowMessengerInvites
        {
            get { return this._allowMessengerInvites; }
            set { this._allowMessengerInvites = value; }
        }

        public bool AllowPetSpeech
        {
            get { return this._allowPetSpeech; }
            set { this._allowPetSpeech = value; }
        }

        public bool AllowBotSpeech
        {
            get { return this._allowBotSpeech; }
            set { this._allowBotSpeech = value; }
        }

        public bool AllowPublicRoomStatus
        {
            get { return this._allowPublicRoomStatus; }
            set { this._allowPublicRoomStatus = value; }
        }

        public bool AllowConsoleMessages
        {
            get { return this._allowConsoleMessages; }
            set { this._allowConsoleMessages = value; }
        }

        public bool AllowGifts
        {
            get { return this._allowGifts; }
            set { this._allowGifts = value; }
        }

        public bool AllowMimic
        {
            get { return this._allowMimic; }
            set { this._allowMimic = value; }
        }

        public bool ReceiveWhispers
        {
            get { return this._receiveWhispers; }
            set { this._receiveWhispers = value; }
        }

        public bool IgnorePublicWhispers
        {
            get { return this._ignorePublicWhispers; }
            set { this._ignorePublicWhispers = value; }
        }

        public bool PlayingFastFood
        {
            get { return this._playingFastFood; }
            set { this._playingFastFood = value; }
        }

        public FriendBarState FriendbarState
        {
            get { return this._friendbarState; }
            set { this._friendbarState = value; }
        }

        public int ChristmasDay
        {
            get { return this._christmasDay; }
            set { this._christmasDay = value; }
        }

        public int WantsToRideHorse
        {
            get { return this._wantsToRideHorse; }
            set { this._wantsToRideHorse = value; }
        }

        public int TimeAFK
        {
            get { return this._timeAFK; }
            set { this._timeAFK = value; }
        }

        public bool DisableForcedEffects
        {
            get { return this._disableForcedEffects; }
            set { this._disableForcedEffects = value; }
        }

        public bool ChangingName
        {
            get { return this._changingName; }
            set { this._changingName = value; }
        }

        public int FriendCount
        {
            get { return this._friendCount; }
            set { this._friendCount = value; }
        }

        public double FloodTime
        {
            get { return this._floodTime; }
            set { this._floodTime = value; }
        }

        public int BannedPhraseCount
        {
            get { return this._bannedPhraseCount; }
            set { this._bannedPhraseCount = value; }
        }

        public bool RoomAuthOk
        {
            get { return this._roomAuthOk; }
            set { this._roomAuthOk = value; }
        }

        public int CurrentRoomId
        {
            get { return this._currentRoomId; }
            set { this._currentRoomId = value; }
        }

        public int QuestLastCompleted
        {
            get { return this._questLastCompleted; }
            set { this._questLastCompleted = value; }
        }

        public int MessengerSpamCount
        {
            get { return this._messengerSpamCount; }
            set { this._messengerSpamCount = value; }
        }

        public double MessengerSpamTime
        {
            get { return this._messengerSpamTime; }
            set { this._messengerSpamTime = value; }
        }

        public double TimeMuted
        {
            get { return this._timeMuted; }
            set { this._timeMuted = value; }
        }

        public double TradingLockExpiry
        {
            get { return this._tradingLockExpiry; }
            set { this._tradingLockExpiry = value; }
        }

        public double SessionStart
        {
            get { return this._sessionStart; }
            set { this._sessionStart = value; }
        }

        public int TentId
        {
            get { return this._tentId; }
            set { this._tentId = value; }
        }

        public int HopperId
        {
            get { return this._hopperId; }
            set { this._hopperId = value; }
        }

        public bool IsHopping
        {
            get { return this._isHopping; }
            set { this._isHopping = value; }
        }

        public int TeleporterId
        {
            get { return this._teleportId; }
            set { this._teleportId = value; }
        }

        public bool IsTeleporting
        {
            get { return this._isTeleporting; }
            set { this._isTeleporting = value; }
        }

        public int TeleportingRoomID
        {
            get { return this._teleportingRoomId; }
            set { this._teleportingRoomId = value; }
        }

        public bool HasSpoken
        {
            get { return this._hasSpoken; }
            set { this._hasSpoken = value; }
        }

        public double LastAdvertiseReport
        {
            get { return this._lastAdvertiseReport; }
            set { this._lastAdvertiseReport = value; }
        }

        public bool AdvertisingReported
        {
            get { return this._advertisingReported; }
            set { this._advertisingReported = value; }
        }

        public bool AdvertisingReportedBlocked
        {
            get { return this._advertisingReportBlocked; }
            set { this._advertisingReportBlocked = value; }
        }

        public bool WiredInteraction
        {
            get { return this._wiredInteraction; }
            set { this._wiredInteraction = value; }
        }

        public bool InventoryAlert
        {
            get { return this._inventoryAlert; }
            set { this._inventoryAlert = value; }
        }

        public bool IgnoreBobbaFilter
        {
            get { return this._ignoreBobbaFilter; }
            set { this._ignoreBobbaFilter = value; }
        }

        public bool WiredTeleporting
        {
            get { return this._wiredTeleporting; }
            set { this._wiredTeleporting = value; }
        }

        public int CustomBubbleId
        {
            get { return this._customBubbleId; }
            set { this._customBubbleId = value; }
        }

        public bool OnHelperDuty
        {
            get { return this._onHelperDuty; }
            set { this._onHelperDuty = value; }
        }

        public int FastfoodScore
        {
            get { return this._fastfoodScore; }
            set { this._fastfoodScore = value; }
        }

        public int PetId
        {
            get { return this._petId; }
            set { this._petId = value; }
        }

        public int CreditsUpdateTick
        {
            get { return this._creditsTickUpdate; }
            set { this._creditsTickUpdate = value; }
        }

        public int EnergieUpdate
        {
            get { return this._energieUpdate; }
            set { this._energieUpdate = value; }
        }

        public int OneMinuteUpdate
        {
            get { return this._OneMinuteUpdate; }
            set { this._OneMinuteUpdate = value; }
        }

        public IChatCommand IChatCommand
        {
            get { return this._iChatCommand; }
            set { this._iChatCommand = value; }
        }

        public DateTime LastGiftPurchaseTime
        {
            get { return this._lastGiftPurchaseTime; }
            set { this._lastGiftPurchaseTime = value; }
        }

        public DateTime LastMottoUpdateTime
        {
            get { return this._lastMottoUpdateTime; }
            set { this._lastMottoUpdateTime = value; }
        }

        public DateTime LastClothingUpdateTime
        {
            get { return this._lastClothingUpdateTime; }
            set { this._lastClothingUpdateTime = value; }
        }

        public DateTime LastForumMessageUpdateTime
        {
            get { return this._lastForumMessageUpdateTime; }
            set { this._lastForumMessageUpdateTime = value; }
        }

        public int GiftPurchasingWarnings
        {
            get { return this._giftPurchasingWarnings; }
            set { this._giftPurchasingWarnings = value; }
        }

        public int MottoUpdateWarnings
        {
            get { return this._mottoUpdateWarnings; }
            set { this._mottoUpdateWarnings = value; }
        }

        public int ClothingUpdateWarnings
        {
            get { return this._clothingUpdateWarnings; }
            set { this._clothingUpdateWarnings = value; }
        }

        public bool SessionGiftBlocked
        {
            get { return this._sessionGiftBlocked; }
            set { this._sessionGiftBlocked = value; }
        }

        public bool SessionMottoBlocked
        {
            get { return this._sessionMottoBlocked; }
            set { this._sessionMottoBlocked = value; }
        }

        public bool SessionClothingBlocked
        {
            get { return this._sessionClothingBlocked; }
            set { this._sessionClothingBlocked = value; }
        }

        public HabboStats GetStats()
        {
            return this._habboStats;
        }

        public bool InRoom
        {
            get
            {
                return CurrentRoomId >= 1 && CurrentRoom != null;
            }
        }

        public Room CurrentRoom
        {
            get
            {
                if (CurrentRoomId <= 0)
                    return null;

                Room _room = null;
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(CurrentRoomId, out _room))
                    return _room;

                return null;
            }
        }

        public bool CacheExpired()
        {
            TimeSpan Span = DateTime.Now - _timeCached;
            return (Span.TotalMinutes >= 30);
        }

        public string GetQueryString
        {
            get
            {
                this._habboSaved = true;
                return "UPDATE `users` SET `online` = '0', `last_online` = '" + PlusEnvironment.GetUnixTimestamp() + "', `activity_points` = '" + this.Duckets + "', `credits` = '" + this.Credits + "', `vip_points` = '" + this.Diamonds + "', `gotw_points` = '" + this.GOTWPoints + "', `time_muted` = '" + this.TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(this._friendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + this._habboStats.RoomVisits + "', `onlineTime` = '" + (PlusEnvironment.GetUnixTimestamp() - SessionStart + this._habboStats.OnlineTime) + "', `respect` = '" + this._habboStats.Respect + "', `respectGiven` = '" + this._habboStats.RespectGiven + "', `giftsGiven` = '" + this._habboStats.GiftsGiven + "', `giftsReceived` = '" + this._habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + this._habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + this._habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + this._habboStats.AchievementPoints + "', `quest_id` = '" + this._habboStats.QuestID + "', `quest_progress` = '" + this._habboStats.QuestProgress + "', `groupid` = '" + this._habboStats.FavouriteGroupId + "',`forum_posts` = '" + this._habboStats.ForumPosts + "' WHERE `id` = '" + this.Id + "' LIMIT 1;";
            }
        }

        public IWebSocketConnection WebSocketConnection
        {
            get
            {
                RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
                if (PlusEnvironment.GetGame().GetWebEventManager() != null)
                    return PlusEnvironment.GetGame().GetWebEventManager().GetUsersConnection(User.GetClient());
                else
                    return null;
            }
        }

        public bool InitProcess()
        {
            this._process = new ProcessComponent();

            if (this._process.Init(this))
                return true;
            return false;
        }

        public bool InitSearches()
        {
            this._navigatorSearches = new SearchesComponent();
            if (this._navigatorSearches.Init(this))
                return true;
            return false;
        }

        public bool InitFX()
        {
            this._fx = new EffectsComponent();
            if (this._fx.Init(this))
                return true;
            return false;
        }

        public bool InitClothing()
        {
            this._clothing = new ClothingComponent();
            if (this._clothing.Init(this))
                return true;
            return false;
        }

        private bool InitPermissions()
        {
            this._permissions = new PermissionComponent();
            if (this._permissions.Init(this))
                return true;
            return false;
        }

        public void InitInformation(UserData data)
        {
            BadgeComponent = new BadgeComponent(this, data);
            Relationships = data.Relations;
        }

        public void Init(GameClient client, UserData data)
        {
            this.Achievements = data.achievements;

            this.FavoriteRooms = new ArrayList();
            foreach (int id in data.favouritedRooms)
            {
                FavoriteRooms.Add(id);
            }

            this.MutedUsers = data.ignores;

            this._client = client;
            BadgeComponent = new BadgeComponent(this, data);
            InventoryComponent = new InventoryComponent(Id, client);

            quests = data.quests;

            Messenger = new HabboMessenger(Id);
            Messenger.Init(data.friends, data.requests);
            this._friendCount = Convert.ToInt32(data.friends.Count);
            this._disconnected = false;
            UsersRooms = data.rooms;
            Relationships = data.Relations;

            this.InitSearches();
            this.InitFX();
            this.InitClothing();
        }


        public PermissionComponent GetPermissions()
        {
            return this._permissions;
        }

        public void OnDisconnect()
        {
            if (this._disconnected)
                return;

            try
            {
                if (this._process != null)
                    this._process.Dispose();
            }
            catch { }

            this._disconnected = true;

            PlusEnvironment.GetGame().GetClientManager().UnregisterClient(Id, Username);

            if (!this._habboSaved)
            {
                this._habboSaved = true;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `online` = '0', `last_online` = '" + PlusEnvironment.GetUnixTimestamp() + "', `activity_points` = '" + this.Duckets + "', `credits` = '" + this.Credits + "', `vip_points` = '" + this.Diamonds + "', `gotw_points` = '" + this.GOTWPoints + "', `time_muted` = '" + this.TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(this._friendbarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + this._habboStats.RoomVisits + "', `onlineTime` = '" + (PlusEnvironment.GetUnixTimestamp() - this.SessionStart + this._habboStats.OnlineTime) + "', `respect` = '" + this._habboStats.Respect + "', `respectGiven` = '" + this._habboStats.RespectGiven + "', `giftsGiven` = '" + this._habboStats.GiftsGiven + "', `giftsReceived` = '" + this._habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + this._habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + this._habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + this._habboStats.AchievementPoints + "', `quest_id` = '" + this._habboStats.QuestID + "', `quest_progress` = '" + this._habboStats.QuestProgress + "', `groupid` = '" + this._habboStats.FavouriteGroupId + "',`forum_posts` = '" + this._habboStats.ForumPosts + "' WHERE `id` = '" + this.Id + "' LIMIT 1;");

                    if (GetPermissions().HasRight("mod_tickets"))
                        dbClient.RunQuery("UPDATE `moderation_tickets` SET `status` = 'open', `moderator_id` = '0' WHERE `status` ='picked' AND `moderator_id` = '" + Id + "'");
                }

                #region resetMenotted
                if (this.Menotted == true)
                {
                    this.Prison = 1;
                    this.updatePrison();
                    this.Timer = 30;
                    this.updateTimer();
                    this.takeBien();
                    this.PrisonCount += 1;
                    this.updatePrisonCount();

                    if (PlusEnvironment.GetGame().GetClientManager().getPoliceMenotte(this.Username) != null)
                    {
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetGame().GetClientManager().getPoliceMenotte(this.Username));
                        if (TargetClient.GetHabbo() != null)
                        {
                            TargetClient.GetHabbo().MenottedUsername = null;
                        }
                    }
                }

                if (this.MenottedUsername != null)
                {
                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(this.MenottedUsername);
                    if (TargetClient != null && TargetClient.GetHabbo() != null)
                    {
                        TargetClient.GetHabbo().Menotted = false;
                        TargetClient.GetHabbo().resetEffectEvent();
                    }
                }
                #endregion
            }
            
            this.Dispose();

            this._client = null;

        }

        public void addPanier(string name)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            User.Purchase = User.Purchase + name + "-";
        }

        public int getPriceOfPanier()
        {
            int price = 0;
            price = price + (getNumberOfItemPanier("eau") * PlusEnvironment.getPriceOfItem("eau"));
            price = price + (getNumberOfItemPanier("coca") * PlusEnvironment.getPriceOfItem("Coca"));
            price = price + (getNumberOfItemPanier("fanta") * PlusEnvironment.getPriceOfItem("Fanta"));
            price = price + (getNumberOfItemPanier("sucette") * PlusEnvironment.getPriceOfItem("Sucette"));
            price = price + (getNumberOfItemPanier("pain") * PlusEnvironment.getPriceOfItem("Baguette"));
            if (this.Mutuelle == 1)
            {
                decimal DolipraneDecimal = Convert.ToDecimal(getNumberOfItemPanier("doliprane") * PlusEnvironment.getPriceOfItem("Doliprane"));
                decimal SavonDecimal = Convert.ToDecimal(getNumberOfItemPanier("savon") * PlusEnvironment.getPriceOfItem("Savon"));
                price = price + (Convert.ToInt32((DolipraneDecimal / 100m) * 80m));
                price = price + (Convert.ToInt32((SavonDecimal / 100m) * 80m));
            }
            else if (this.Mutuelle == 2)
            {
                decimal DolipraneDecimal = Convert.ToDecimal(getNumberOfItemPanier("doliprane") * PlusEnvironment.getPriceOfItem("Doliprane"));
                decimal SavonDecimal = Convert.ToDecimal(getNumberOfItemPanier("savon") * PlusEnvironment.getPriceOfItem("Savon"));
                price = price + (Convert.ToInt32((DolipraneDecimal / 100m) * 65m));
                price = price + (Convert.ToInt32((SavonDecimal / 100m) * 65m));
            }
            else if (this.Mutuelle == 3)
            {
                decimal DolipraneDecimal = Convert.ToDecimal(getNumberOfItemPanier("doliprane") * PlusEnvironment.getPriceOfItem("Doliprane"));
                decimal SavonDecimal = Convert.ToDecimal(getNumberOfItemPanier("savon") * PlusEnvironment.getPriceOfItem("Savon"));
                price = price + (Convert.ToInt32((DolipraneDecimal / 100m) * 50m));
                price = price + (Convert.ToInt32((SavonDecimal / 100m) * 50m));
            }
            else if (this.Mutuelle == 4)
            {
                decimal DolipraneDecimal = Convert.ToDecimal(getNumberOfItemPanier("doliprane") * PlusEnvironment.getPriceOfItem("Doliprane"));
                decimal SavonDecimal = Convert.ToDecimal(getNumberOfItemPanier("savon") * PlusEnvironment.getPriceOfItem("Savon"));
                price = price + (Convert.ToInt32((DolipraneDecimal / 100m) * 35m));
                price = price + (Convert.ToInt32((SavonDecimal / 100m) * 35m));
            }
            else
            {
                price = price + (getNumberOfItemPanier("doliprane") * PlusEnvironment.getPriceOfItem("Doliprane"));
                price = price + (getNumberOfItemPanier("savon") * PlusEnvironment.getPriceOfItem("Savon"));
            }
            return price;
        }

        public void validerPanier()
        {
            Group Proxy = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(2, out Proxy))
                return;

            Group Pharmacie = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(10, out Pharmacie))
                return;

            Group Gouvernement = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                return;

            int Eau = getNumberOfItemPanier("eau");
            if (Eau != 0)
            {
                int Taxe = Eau * PlusEnvironment.getTaxeOfItem("Eau");
                Gouvernement.ChiffreAffaire += Taxe;
                Gouvernement.updateChiffre();
                int ChiffreAffaire = (Eau * PlusEnvironment.getPriceOfItem("Eau")) - (Eau * PlusEnvironment.getTaxeOfItem("Eau"));
                Proxy.ChiffreAffaire += ChiffreAffaire;
                Proxy.updateChiffre();
                this.Eau += Eau;
                this.updateEau();
            }

            int Coca = getNumberOfItemPanier("coca");
            if (Coca != 0)
            {
                int Taxe = Coca * PlusEnvironment.getTaxeOfItem("Coca");
                Gouvernement.ChiffreAffaire += Taxe;
                Gouvernement.updateChiffre();
                int ChiffreAffaire = (Coca * PlusEnvironment.getPriceOfItem("Coca")) - (Coca * PlusEnvironment.getTaxeOfItem("Coca"));
                Proxy.ChiffreAffaire += ChiffreAffaire;
                Proxy.updateChiffre();
                this.Coca += Coca;
                this.updateCoca();
            }

            int Fanta = getNumberOfItemPanier("fanta");
            if (Fanta != 0)
            {
                int Taxe = Fanta * PlusEnvironment.getTaxeOfItem("Fanta");
                Gouvernement.ChiffreAffaire += Taxe;
                Gouvernement.updateChiffre();
                int ChiffreAffaire = (Fanta * PlusEnvironment.getPriceOfItem("Fanta")) - (Fanta * PlusEnvironment.getTaxeOfItem("Fanta"));
                Proxy.ChiffreAffaire += ChiffreAffaire;
                Proxy.updateChiffre();
                this.Fanta += Fanta;
                this.updateFanta();
            }

            int Sucette = getNumberOfItemPanier("sucette");
            if (Sucette != 0)
            {
                int Taxe = Sucette * PlusEnvironment.getTaxeOfItem("Sucette");
                Gouvernement.ChiffreAffaire += Taxe;
                Gouvernement.updateChiffre();
                int ChiffreAffaire = (Sucette * PlusEnvironment.getPriceOfItem("Sucette")) - (Sucette * PlusEnvironment.getTaxeOfItem("Sucette"));
                Proxy.ChiffreAffaire += ChiffreAffaire;
                Proxy.updateChiffre();

                this.Sucette += Sucette;
                this.updateSucette();
            }

            int Pain = getNumberOfItemPanier("pain");
            if (Pain != 0)
            {
                int Taxe = Pain * PlusEnvironment.getTaxeOfItem("Baguette");
                Gouvernement.ChiffreAffaire += Taxe;
                Gouvernement.updateChiffre();
                int ChiffreAffaire = (Pain * PlusEnvironment.getPriceOfItem("Baguette")) - (Pain * PlusEnvironment.getTaxeOfItem("Baguette"));
                Proxy.ChiffreAffaire += ChiffreAffaire;
                Proxy.updateChiffre();

                this.Pain += Pain;
                this.updatePain();
            }

            int Savon = getNumberOfItemPanier("savon");
            if (Savon != 0)
            {
                int Taxe;
                int ChiffreAffaire;

                if (this.Mutuelle == 1)
                {
                    decimal SavonTaxeDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getTaxeOfItem("Savon"));
                    decimal SavonPriceDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getPriceOfItem("Savon"));
                    Taxe = (Convert.ToInt32((SavonTaxeDecimal / 100m) * 80m));
                    ChiffreAffaire = (Convert.ToInt32((SavonPriceDecimal / 100m) * 80m)) - Taxe;
                }
                else if (this.Mutuelle == 2)
                {
                    decimal SavonTaxeDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getTaxeOfItem("Savon"));
                    decimal SavonPriceDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getPriceOfItem("Savon"));
                    Taxe = (Convert.ToInt32((SavonTaxeDecimal / 100m) * 65m));
                    ChiffreAffaire = (Convert.ToInt32((SavonPriceDecimal / 100m) * 65m)) - Taxe;
                }
                else if (this.Mutuelle == 3)
                {
                    decimal SavonTaxeDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getTaxeOfItem("Savon"));
                    decimal SavonPriceDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getPriceOfItem("Savon"));
                    Taxe = (Convert.ToInt32((SavonTaxeDecimal / 100m) * 50m));
                    ChiffreAffaire = (Convert.ToInt32((SavonPriceDecimal / 100m) * 50m)) - Taxe;
                }
                else if (this.Mutuelle == 4)
                {
                    decimal SavonTaxeDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getTaxeOfItem("Savon"));
                    decimal SavonPriceDecimal = Convert.ToDecimal(Savon * PlusEnvironment.getPriceOfItem("Savon"));
                    Taxe = (Convert.ToInt32((SavonTaxeDecimal / 100m) * 35m));
                    ChiffreAffaire = (Convert.ToInt32((SavonPriceDecimal / 100m) * 35m)) - Taxe;
                }
                else
                {
                    Taxe = Savon * PlusEnvironment.getTaxeOfItem("Savon");
                    ChiffreAffaire = (Savon * PlusEnvironment.getPriceOfItem("Savon")) - Taxe;
                }

                Gouvernement.ChiffreAffaire += (Taxe + ChiffreAffaire);
                Gouvernement.updateChiffre();

                this.Savon += Savon;
                this.updateSavon();
            }

            int Doliprane = getNumberOfItemPanier("doliprane");
            if (Doliprane != 0)
            {
                int Taxe;
                int ChiffreAffaire;

                if (this.Mutuelle == 1)
                {
                    decimal DolipraneTaxeDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getTaxeOfItem("Doliprane"));
                    decimal DolipranePriceDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getPriceOfItem("Doliprane"));
                    Taxe = (Convert.ToInt32((DolipraneTaxeDecimal / 100m) * 80m));
                    ChiffreAffaire = (Convert.ToInt32((DolipranePriceDecimal / 100m) * 80m)) - Taxe;
                }
                else if (this.Mutuelle == 2)
                {
                    decimal DolipraneTaxeDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getTaxeOfItem("Doliprane"));
                    decimal DolipranePriceDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getPriceOfItem("Doliprane"));
                    Taxe = (Convert.ToInt32((DolipraneTaxeDecimal / 100m) * 65m));
                    ChiffreAffaire = (Convert.ToInt32((DolipranePriceDecimal / 100m) * 65m)) - Taxe;
                }
                else if (this.Mutuelle == 3)
                {
                    decimal DolipraneTaxeDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getTaxeOfItem("Doliprane"));
                    decimal DolipranePriceDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getPriceOfItem("Doliprane"));
                    Taxe = (Convert.ToInt32((DolipraneTaxeDecimal / 100m) * 50m));
                    ChiffreAffaire = (Convert.ToInt32((DolipranePriceDecimal / 100m) * 50m)) - Taxe;
                }
                else if (this.Mutuelle == 4)
                {
                    decimal DolipraneTaxeDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getTaxeOfItem("Doliprane"));
                    decimal DolipranePriceDecimal = Convert.ToDecimal(Doliprane * PlusEnvironment.getPriceOfItem("Doliprane"));
                    Taxe = (Convert.ToInt32((DolipraneTaxeDecimal / 100m) * 35m));
                    ChiffreAffaire = (Convert.ToInt32((DolipranePriceDecimal / 100m) * 35m)) - Taxe;
                }
                else
                {
                    Taxe = Doliprane * PlusEnvironment.getTaxeOfItem("Doliprane");
                    ChiffreAffaire = (Doliprane * PlusEnvironment.getPriceOfItem("Doliprane")) - Taxe;
                }

                Gouvernement.ChiffreAffaire += (Taxe + ChiffreAffaire);
                Gouvernement.updateChiffre();

                this.Doliprane += Doliprane;
                this.updateDoliprane();
            }
        }

        public int getNumberOfItemPanier(string Produit)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            string panier = User.Purchase;
            string[] panierValide = panier.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            int numberItem = 0;
            foreach (var item in panierValide)
            {
                if (item == Produit)
                {
                    numberItem++;
                }
            }

            return numberItem;
        }

        public int getNumberItemPanier()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            string panier = User.Purchase;
            string[] panierValide = panier.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            return panierValide.Length;
        }

        public string getNameOfThisGang(int Id)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT name FROM `gang` WHERE id = @gangId");
                dbClient.AddParameter("gangId", Id);
                string GangName = dbClient.getString();
                return GangName;
            }
        }

        public string getNameOfGang()
        {
            if (this.Gang == 0)
                return "%null%";

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT name FROM `gang` WHERE id = @gangId");
                dbClient.AddParameter("gangId", this.Gang);
                string GangName = dbClient.getString();
                return GangName;
            }
        }

        public int getOwnerOfGang()
        {
            if (this.Gang == 0)
                return 0;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT owner FROM `gang` WHERE id = @gangId");
                dbClient.AddParameter("gangId", this.Gang);
                int OwnerUser = dbClient.getInteger();
                return OwnerUser;
            }
        }

        public int countUserOfGang()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `gang` = @gang_id");
                dbClient.AddParameter("gang_id", this.Gang);
                int GangCount = dbClient.getInteger();
                return GangCount;
            }
        }

        public void insertLastAction(string Action)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO last_actions (user_id, action_maked) VALUES (@user_id, @action);");
                dbClient.AddParameter("user_id", this.Id);
                dbClient.AddParameter("action", Action);
                dbClient.RunQuery();
            }
        }

        public void createGang(string Name)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO gang (name, owner) VALUES (@name, '" + this.Id + "');");
                dbClient.AddParameter("name", Name);
                dbClient.RunQuery();

                dbClient.SetQuery("SELECT id FROM `gang` ORDER by id DESC LIMIT 1");
                int gangId = dbClient.getInteger();

                dbClient.SetQuery("UPDATE users SET gang = @gang_id, gang_rank = 4 WHERE id = @userId");
                dbClient.AddParameter("gang_id", gangId);
                dbClient.AddParameter("userId", this.Id);
                dbClient.RunQuery();

                this.Gang = gangId;
                this.GangRank = 4;
            }
        }

        public void stopWork()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);

            if (this.Commande != null)
            {
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(this.Commande);
                if (TargetClient.GetHabbo() != null && TargetClient.GetHabbo().CurrentRoom == this.CurrentRoom)
                {
                    Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                    RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                    TargetClient.GetHabbo().Commande = null;
                    TargetUser.Purchase = "";
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                }

                User.CarryItem(0);
                User.OnChat(User.LastBubble, "* Arrête de prendre la commande de " + TargetClient.GetHabbo().Username + " *", true);
                this.Commande = null;
                User.Purchase = "";
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(this.GetClient(), "panier", "send");
            }

            if (User.ConnectedMetier == true)
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "ordinateur;hide");
                User.ConnectedMetier = false;
                User.Frozen = false;
            }
            this.Travaille = false;
            User.OnChat(User.LastBubble, "* Arrête de travailler *", true);
            this.resetAvatarEvent();
        }

        public void updateXpGang(int Xp)
        {
            if (this.Gang == 0)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE gang SET xp = xp + @new_xp WHERE id = @gangId");
                dbClient.AddParameter("new_xp", Xp);
                dbClient.AddParameter("id", this.Gang);
                dbClient.RunQuery();
            }
        }

        public void takeBien()
        {
            if (this.Telephone == 1)
            {
                this.Telephone = 0;
                this.updateTelephone();
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(this.GetClient(), "telephone", "hide");
                if (!this.PoliceCasier.Contains("[TELEPHONE]"))
                {
                    this.PoliceCasier = this.PoliceCasier + "[TELEPHONE]";
                }
            }

            if (this.Uzi == 1)
            {
                this.Uzi = 0;
                this.updateUzi();

                if (!this.PoliceCasier.Contains("[UZI]"))
                {
                    this.PoliceCasier = this.PoliceCasier + "[UZI]";
                }
            }

            if (this.Ak47 == 1)
            {
                this.Ak47 = 0;
                this.updateAk47();

                if (!this.PoliceCasier.Contains("[AK47]"))
                {
                    this.PoliceCasier = this.PoliceCasier + "[AK47]";
                }
            }

            if (this.Sabre == 1)
            {
                this.Sabre = 0;
                this.updateSabre();

                if (!this.PoliceCasier.Contains("[SABRE]"))
                {
                    this.PoliceCasier = this.PoliceCasier + "[SABRE]";
                }
            }

            if (this.Batte == 1)
            {
                this.Batte = 0;
                this.updateBatte();

                if (!this.PoliceCasier.Contains("[BATTE]"))
                {
                    this.PoliceCasier = this.PoliceCasier + "[BATTE]";
                }
            }

            if (this.Weed > 0)
            {
                this.takeWeed();
            }

            if (this.Cocktails > 0)
            {
                this.Cocktails = 0;
                this.updateCocktails();
            }

            this.updatePoliceCasier();
        }

        public void takeWeed()
        {
            int Amende = this.Weed * 3;
            this.Weed = 0;
            this.updateWeed();
            this.Credits -= Amende;
            this.GetClient().SendMessage(new CreditBalanceComposer(this.Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);
            Group Gouvernement = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                return;

            if(Gouvernement != null)
            {
                Gouvernement.ChiffreAffaire += Amende;
                Gouvernement.updateChiffre();
            }
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            User.OnChat(User.LastBubble, "* Paye une amende de " + Amende + " crédits à la Police Nationale *", true);
        }

        public void EndPrison()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            this.resetAvatarEvent();
            this.Timer = 0;
            this.updateTimer();
            this.Prison = 0;
            this.updatePrison();
            User.isFarmingRock = 0;
            this.resetEffectEvent();
            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(20);
            this.sendToPrisonChair(User, Room, 7877);
            User.OnChat(User.LastBubble, "* Purge sa peine d'emprisonnement *", true);
        }

        public void endHopital(GameClient Session, int Paye)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            User.OnChat(User.LastBubble, "* Se rétablie *", true);
            this.resetAvatarEvent();
            if (Session.GetHabbo().Telephone == 1)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "telephone", "show");
            }
            
            if (Paye == 1)
            {
                int PrixSoin = Convert.ToInt32(PlusEnvironment.GetConfig().data["price.soin"]);
                if (this.Mutuelle == 0)
                {
                    User.OnChat(User.LastBubble, "* Paye 100% des soins reçus *", true);
                    this.Credits -= PrixSoin;
                    Session.SendMessage(new CreditBalanceComposer(this.Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);

                    Group Gouvernement = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                        return;

                    Gouvernement.ChiffreAffaire += PrixSoin;
                    Gouvernement.updateChiffre();
                }
                else if (this.Mutuelle == 1)
                {
                    User.OnChat(User.LastBubble, "* Paye 75% des soins reçus et laisse sa mutuelle payer les 25% restants *", true);
                    this.Credits -= (PrixSoin * 75) / 100;
                    Session.SendMessage(new CreditBalanceComposer(this.Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);

                    Group Gouvernement = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                        return;

                    Gouvernement.ChiffreAffaire += (PrixSoin * 75) / 100;
                    Gouvernement.updateChiffre();
                }
                else if (this.Mutuelle == 2)
                {
                    User.OnChat(User.LastBubble, "* Paye 50% des soins reçus et laisse sa mutuelle payer les 50% restants *", true);
                    this.Credits -= (PrixSoin * 50) / 100;
                    Session.SendMessage(new CreditBalanceComposer(this.Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);

                    Group Gouvernement = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                        return;

                    Gouvernement.ChiffreAffaire += (PrixSoin * 50) / 100;
                    Gouvernement.updateChiffre();
                }
                else if (this.Mutuelle == 3)
                {
                    User.OnChat(User.LastBubble, "* Paye 25% des soins reçus et laisse sa mutuelle payer les 75% restants *", true);
                    this.Credits -= (PrixSoin * 25) / 100;
                    Session.SendMessage(new CreditBalanceComposer(this.Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);

                    Group Gouvernement = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                        return;

                    Gouvernement.ChiffreAffaire += (PrixSoin * 25) / 100;
                    Gouvernement.updateChiffre();
                }
                else if (this.Mutuelle == 4)
                {
                    User.OnChat(User.LastBubble, "* Laisse sa mutuelle payer l'intégralité des frais de soins *", true);
                }
            }
            this.Timer = 0;
            this.updateTimer();
            this.Hopital = 0;
            this.updateHopital();
            Session.SendWhisper("Vous pouvez quitter l'hôpital.");
        }

        public void stopConduire()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (this.Conduit == "blackHoverboard" || this.Conduit == "pinkHoverboard" || this.Conduit == "whiteHoverboard")
            {
                User.OnChat(User.LastBubble, "* Descend de son hoverboard et le range *", true);
            }
            else
            {
                User.OnChat(User.LastBubble, "* Arrête de conduire *", true);
                User.OnChat(User.LastBubble, "* Éteint le contact *", true);
            }
            this.Conduit = null;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "voiture;stop");
            User.UltraFastWalking = false;
            User.SuperFastWalking = false;
            User.FastWalking = false;
            this.resetEffectEvent();
        }

        public void updateHopitalEtat(RoomUser User, int Timer)
        {
            if (this.Travaille == true)
            {
                this.stopWork();
            }

            if (User.makeCreation == true)
            {
                User.makeCreation = false;
            }

            this.Timer = Timer;
            this.updateTimer();
            this.Hopital = 1;
            this.updateHopital();
            this.sendToHospital(User);
        }

        public void updatePrisonEtat(RoomUser User, int Timer, int Etat)
        {
            if (this.Travaille == true)
            {
                this.Travaille = false;
            }

            this.Timer = Timer;
            this.updateTimer();
            this.Prison = Etat;
            this.updatePrison();
            this.sendToPrison(User, 0, Etat);
        }

        public void sendToHospital(RoomUser User)
        {
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(User.GetClient(), "telephone", "hide");
            this.updateAvatarEvent("lg-3136-92.hr-515-33.ch-680-83.sh-735-83.hd-3100-1370", "sh-300-83.ch-3030-83.lg-270-83.hd-180-1379.hr-3163-45", "[HOSPITALISÉ] Civil");
            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(3);
            this.sendToHopitalBed(User, Room);
        }

        public void sendToPrison(RoomUser User, int AppartAutoriser, int Etat)
        {
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(this.GetClient(), "telephone", "hide");
            this.updateAvatarEvent("hd-600-1.sh-725-110.ca-3085-92.ch-635-94.hr-515-33.lg-720-94", "hd-180-1379.sh-290-110.ch-215-94.ca-3085-92.hr-3163-45.lg-285-94", "[EMPRISONNÉ] Civil");
            if (AppartAutoriser != 18 && AppartAutoriser != 20)
            {
                Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(20);
                if (Etat == 2)
                {
                    this.sendToPrisonChair(User, Room, 2501);
                }
                else
                {
                    this.sendToPrisonChair(User, Room, 2618);
                }
            }
        }

        public void sendToHopitalBed(RoomUser User, Room Room)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                List<Item> Beds = new List<Item>();
                List<int> BedsId = new List<int>();

                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().SpriteId == 3590)
                    {
                        if (!Beds.Contains(item))
                            Beds.Add(item);

                        if (!BedsId.Contains(item.Id))
                            BedsId.Add(item.Id);
                    }
                }

                if (this.CurrentRoomId != Room.Id)
                {
                    Random rnd = new Random();
                    int random = BedsId[rnd.Next(BedsId.Count)];

                    User.GetClient().GetHabbo().IsTeleporting = true;
                    User.GetClient().GetHabbo().TeleportingRoomID = Room.Id;
                    User.GetClient().GetHabbo().TeleporterId = random;

                    User.GetClient().GetHabbo().CanChangeRoom = true;
                    User.GetClient().GetHabbo().PrepareRoom(User.GetClient().GetHabbo().TeleportingRoomID, "");
                }
                else
                {
                    Random rnd = new Random();
                    Item random = Beds[rnd.Next(Beds.Count)];
                    Room.GetGameMap().TeleportToItem(User, random);
                    Room.GetRoomUserManager().UpdateUserStatusses();
                }
            }
        }

        public void sendToPrisonChair(RoomUser User, Room Room, int SpriteId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                List<Item> Beds = new List<Item>();
                List<int> BedsId = new List<int>();

                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().SpriteId == SpriteId)
                    {
                        if (!Beds.Contains(item))
                            Beds.Add(item);

                        if (!BedsId.Contains(item.Id))
                            BedsId.Add(item.Id);
                    }
                }

                if (this.CurrentRoomId != Room.Id)
                {
                    Random rnd = new Random();
                    int random = BedsId[rnd.Next(BedsId.Count)];

                    User.GetClient().GetHabbo().IsTeleporting = true;
                    User.GetClient().GetHabbo().TeleportingRoomID = Room.Id;
                    User.GetClient().GetHabbo().TeleporterId = random;

                    User.GetClient().GetHabbo().CanChangeRoom = true;
                    User.GetClient().GetHabbo().PrepareRoom(User.GetClient().GetHabbo().TeleportingRoomID, "");
                }
                else
                {
                    Random rnd = new Random();
                    Item random = Beds[rnd.Next(Beds.Count)];
                    Room.GetGameMap().TeleportToItem(User, random);
                    Room.GetRoomUserManager().UpdateUserStatusses();
                }
            }
        }

        public void updateTimer()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET timer = @timer WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("timer", this.Timer);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateHopital()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET hopital = @hopital WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("hopital", this.Hopital);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePrison()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET prison = @prison WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("prison", this.Prison);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePrisonCount()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET prison_count = @prison_count WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("prison_count", this.PrisonCount);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateGang()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET gang = @gang WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gang", this.Gang);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateGangRank()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET gang_rank = @gang_rank WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gang_rank", this.GangRank);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public int getMaxItem(string NameItem)
        {
            int Number = 0;

            if(NameItem == "coca")
            {
                if (this.Sac == 1)
                    Number = 20;

                if (this.Sac == 2)
                    Number = 40;

                if (this.Sac == 3)
                    Number = 70;

                if (this.Sac == 4)
                    Number = 100;
            }

            if (NameItem == "fanta")
            {
                if (this.Sac == 1)
                    Number = 20;

                if (this.Sac == 2)
                    Number = 40;

                if (this.Sac == 3)
                    Number = 70;

                if (this.Sac == 4)
                    Number = 100;
            }

            if (NameItem == "pain")
            {
                if (this.Sac == 1)
                    Number = 5;

                if (this.Sac == 2)
                    Number = 15;

                if (this.Sac == 3)
                    Number = 30;

                if (this.Sac == 4)
                    Number = 50;
            }

            if (NameItem == "sucette")
            {
                if (this.Sac == 1)
                    Number = 15;

                if (this.Sac == 2)
                    Number = 30;

                if (this.Sac == 3)
                    Number = 50;

                if (this.Sac == 4)
                    Number = 70;
            }

            if (NameItem == "savon")
            {
                if (this.Sac == 1)
                    Number = 5;

                if (this.Sac == 2)
                    Number = 15;

                if (this.Sac == 3)
                    Number = 25;

                if (this.Sac == 4)
                    Number = 35;
            }

            if (NameItem == "doliprane")
            {
                if (this.Sac == 1)
                    Number = 10;

                if (this.Sac == 2)
                    Number = 25;

                if (this.Sac == 3)
                    Number = 40;

                if (this.Sac == 4)
                    Number = 60;
            }

            if (NameItem == "arme")
            {
                if (this.Sac == 1)
                    Number = 1;

                if (this.Sac == 2)
                    Number = 2;

                if (this.Sac == 3)
                    Number = 3;

                if (this.Sac == 4)
                    Number = 4;
            }

            if (NameItem == "eau")
            {
                if (this.Sac == 1)
                    Number = 15;

                if (this.Sac == 2)
                    Number = 25;

                if (this.Sac == 3)
                    Number = 35;

                if (this.Sac == 4)
                    Number = 50;
            }

            return Number;
        }

        public int GetNumberOfArmes()
        {
            int Number = 0;
            if (this.Ak47 == 1)
                Number = Number + 1;

            if (this.Batte == 1)
                Number = Number + 1;

            if (this.Uzi == 1)
                Number = Number + 1;

            if (this.Sabre == 1)
                Number = Number + 1;

            return Number;
        }

        public void updateForfaitTel()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telephoneForfaitTel = @telephoneForfaitTel WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telephoneForfaitTel", this.TelephoneForfait);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateForfaitSms()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telForfaitSms = @telForfaitSms WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telForfaitSms", this.TelephoneForfaitSms);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateForfaitType()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telForfaitType = @telForfaitType WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telForfaitType", this.TelephoneForfaitType);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateForfaitReset()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telForfaitReset = @telForfaitReset WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telForfaitReset", this.TelephoneForfaitReset);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateAudiA8()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET audia8 = @audia8 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("audia8", this.AudiA8);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePorsche911()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET porsche911 = @porsche911 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("porsche911", this.Porsche911);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateFiatPunto()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET fiatpunto = @fiatpunto WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("fiatpunto", this.FiatPunto);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateVolkswagenJetta()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET volkswagenjetta = @volkswagenjetta WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("volkswagenjetta", this.VolkswagenJetta);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateBMWI8()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET bmwi8 = @bmwi8 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("bmwi8", this.BmwI8);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateSavon()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET savon = @savon WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("savon", this.Savon);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateLook()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("look", this.Look);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateKill()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET kills = @kills WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("kills", this.Kills);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateMort()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET morts = @morts WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("morts", this.Morts);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateTelephone()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telephone = @telephone WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telephone", this.Telephone);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateTelephoneName()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telephone_name = @telephone_name WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telephone_name", this.TelephoneName);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateTelephoneForfaitSms()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET telForfaitSms = @telForfaitSms WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("telForfaitSms", this.TelephoneForfaitSms);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateUzi()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET uzi = @uzi WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("uzi", this.Uzi);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateBatte()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET batte = @batte WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("batte", this.Batte);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateSabre()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET sabre = @sabre WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("sabre", this.Sabre);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateAk47()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET ak47 = @ak47 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("ak47", this.Ak47);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateAK47Munitions()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET ak47_munitions = @ak47_munitions WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("ak47_munitions", this.AK47_Munitions);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateUziMunitions()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET uzi_munitions = @uzi_munitions WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("uzi_munitions", this.Uzi_Munitions);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateGangKill()
        {
            if (this.Gang == 0)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE gang SET kills = kills + 1 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", this.Gang);
                dbClient.RunQuery();
            }
        }

        public void updateGangMort()
        {
            if (this.Gang == 0)
                return;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE gang SET dead = dead + 1 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", this.Gang);
                dbClient.RunQuery();
            }
        }

        public void updateMutuelle()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET mutuelle = @mutuelle WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("mutuelle", this.Mutuelle);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateMutuelleDate()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET mutuelle_expiration = @mutuelle_date WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("mutuelle_date", this.MutuelleDate);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCoca()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET coca = @coca WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("coca", this.Coca);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePain()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET pain = @pain WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("pain", this.Pain);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateDoliprane()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET doliprane = @doliprane WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("doliprane", this.Doliprane);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePierre()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET pierre = @pierre WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("pierre", this.Pierre);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCarte()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET carte = @carte WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("carte", this.Carte);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateFacebook()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET facebook = @facebook WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("facebook", this.Facebook);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateGps()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET gps = @gps WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gps", this.Gps);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateSac()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET sac = @sac WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("sac", this.Sac);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePermisArme()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET permis_arme = @permis_arme WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("permis_arme", this.Permis_arme);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateClipper()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET clipper = @clipper WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("clipper", this.Clipper);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateWeed()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET weed = @weed WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("weed", this.Weed);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePhilipMo()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET philipmo = @philipmo WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("philipmo", this.PhilipMo);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateEventDay()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET event_day = @eventday WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("eventday", this.EventDay);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateEventPoints()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET event_points = @eventpoints WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("eventpoints", this.EventPoints);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCasinoJetons()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET casino_jetons = @casinojetons WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("casinojetons", this.Casino_Jetons);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }


        public void updateQuizzPoint()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET quizz_points = @quizz_points WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("quizz_points", this.Quizz_Points);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCocktails()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET cocktails = @cocktails WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("cocktails", this.Cocktails);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateAudiA3()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET audia3 = @audia3 WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("audia3", this.AudiA3);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateEau()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET eau = @eau WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("eau", this.Eau);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCasierWeed()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET casier_weed = @casier_weed WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("casier_weed", this.CasierWeed);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCasierCocktails()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET casier_cocktails = @casier_cocktails WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("casier_cocktails", this.CasierCocktails);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateWhiteHoverboard()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET white_hover = @white_hover WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("white_hover", this.WhiteHoverboard);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCreations()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET creations = @creations WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("creations", this.Creations);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateSucette()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET sucette = @sucette WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("sucette", this.Sucette);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateFanta()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET fanta = @fanta WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("fanta", this.Fanta);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateBlur()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET blur = @blur WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("blur", this.Blur);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void addCooldown(string Type, int Time)
        {
            if (this.ActiveCooldowns.ContainsKey(Type))
                return;

            this.ActiveCooldowns.TryAdd(Type, Time);

            System.Timers.Timer timer2 = new System.Timers.Timer(Time);
            timer2.Interval = Time;
            timer2.Elapsed += delegate
            {
                this.ActiveCooldowns.TryRemove(Type, out Time);
                timer2.Stop();
            };
            timer2.Start();
        }

        public bool getCooldown(string Type)
        {
            try
            {
                if (this.ActiveCooldowns.ContainsKey(Type))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return false;
        }

        public void updateAvatarEvent(string LookF, string LookM, string Motto)
        {
            string NewLook = "";
            if (this.Gender == "f")
            {
                NewLook = this.changeLook(this.Look, LookF);
            }
            else
            {
                NewLook = this.changeLook(this.Look, LookM);
            }

            this.Look = NewLook;
            this.Motto = Motto;

            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            this.GetClient().SendMessage(new UserChangeComposer(User, true));
            this.CurrentRoom.SendMessage(new UserChangeComposer(User, false));
        }

        public void footballSpawnChair()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User == null || this.CurrentRoomId != 56)
                return;

            int SpriteId = 3494;

            Room Room = this.CurrentRoom;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().SpriteId == SpriteId)
                    {
                        Room.GetGameMap().TeleportToItem(User, item);
                        Room.GetRoomUserManager().UpdateUserStatusses();
                    }
                }
            }

            this.resetAvatarEvent();
        }

        public void footballSpawnInItem(string Team)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User == null || this.CurrentRoomId != 56)
                return;

            int SpriteId = 0;
            int Direction = 0;

            if (Team == "green")
            {
                SpriteId = 2566;
                Direction = 0;
            }
            else if (Team == "blue")
            {
                SpriteId = 2582;
                Direction = 4;
            }

            Room Room = this.CurrentRoom;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                {
                    if (item.GetBaseItem().SpriteId == SpriteId)
                    {
                        if (Room.GetGameMap().SquareHasUsers(item.GetX, item.GetY))
                            continue;

                        Room.GetGameMap().TeleportToItem(User, item);
                        Room.GetRoomUserManager().UpdateUserStatusses();
                        User.SetRot(Direction, false);
                    }
                }
            }
        }

        public void resetEffectEvent()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (this.Menotted == true)
            {
                this.Effects().ApplyEffect(590);
            }
            else if (this.Conduit != null)
            {
                if (this.Conduit == "audia8")
                {
                    this.Effects().ApplyEffect(800);
                    User.SuperFastWalking = true;
                }
                else if (this.Conduit == "audia3")
                {
                    this.Effects().ApplyEffect(802);
                    User.SuperFastWalking = true;
                }
                else if (this.Conduit == "porsche911")
                {
                    this.Effects().ApplyEffect(2000);
                    User.UltraFastWalking = true;
                }
                else if (this.Conduit == "fiatpunto")
                {
                    this.Effects().ApplyEffect(21);
                    User.SuperFastWalking = true;
                }
                else if (this.Conduit == "volkswagenjetta")
                {
                    this.Effects().ApplyEffect(69);
                    User.SuperFastWalking = true;
                }
                else if (this.Conduit == "bmwi8")
                {
                    this.Effects().ApplyEffect(814);
                    User.UltraFastWalking = true;
                }
                else if (this.Conduit == "police")
                {
                    this.Effects().ApplyEffect(19);
                    User.UltraFastWalking = true;
                }
                else if (this.Conduit == "gouvernement")
                {
                    this.Effects().ApplyEffect(510);
                    User.UltraFastWalking = true;
                }
                else if (this.Conduit == "avion")
                {
                    this.Effects().ApplyEffect(176);
                    User.UltraFastWalking = true;
                }
                else if (this.Conduit == "blackHoverboard")
                {
                    this.Effects().ApplyEffect(504);
                    User.FastWalking = true;
                }
                else if (this.Conduit == "whiteHoverboard")
                {
                    this.Effects().ApplyEffect(191);
                    User.FastWalking = true;
                }
                else if (this.Conduit == "pinkHoverboard")
                {
                    this.Effects().ApplyEffect(505);
                    User.FastWalking = true;
                }
            }
            else if (this.ArmeEquiped != null)
            {
                if (this.ArmeEquiped == "batte")
                {
                    this.Effects().ApplyEffect(591);
                }
                else if (this.ArmeEquiped == "sabre")
                {
                    this.Effects().ApplyEffect(162);
                }
                else if (this.ArmeEquiped == "uzi")
                {
                    this.Effects().ApplyEffect(580);
                }
                else if (this.ArmeEquiped == "ak47")
                {
                    this.Effects().ApplyEffect(583);
                }
                else if (this.ArmeEquiped == "taser")
                {
                    this.Effects().ApplyEffect(592);
                }
                else if (this.ArmeEquiped == "cocktail")
                {
                    this.Effects().ApplyEffect(1005);
                }
            }
            else if (User.isFarmingRock > 0)
            {
                this.Effects().ApplyEffect(594);
            }
            else if (this.Hygiene < 15)
            {
                this.Effects().ApplyEffect(10);
            }
            else
            {
                this.Effects().ApplyEffect(-1);
            }
        }

        public void sendMsgOffi(string msg)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User != null)
            {
                this.GetClient().SendMessage(new WhisperComposer(User.VirtualId, msg, 0, 34));
            }
        }

        public void winSalade()
        {
            this.ArmeEquiped = null;
            this.resetEffectEvent();
            if (this.CurrentRoom != null)
            {
                RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
                if (User != null)
                {
                    User.OnChat(User.LastBubble, "* Gagne la salade [+100 CRÉDITS] *", true);
                }
            }
            this.Credits += 100;
            this.GetClient().SendMessage(new CreditBalanceComposer(this.Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + this.Credits + ";" + this.Duckets + ";" + this.EventPoints);
            PlusEnvironment.GetGame().GetClientManager().sendStaffMsg(this.Username + " a bien reçu son gain de la salade.");
        }

        public void resetAvatarEvent()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);

            if(this.CurrentRoomId == PlusEnvironment.ManVsZombie && User.ManVsZombieTeam == "zombie")
            {
                
            }
            else if (this.CurrentRoomId == PlusEnvironment.ManVsZombie && User.ManVsZombieTeam == "man")
            {
                this.updateAvatarEvent("fa-1212-63.hd-600-8.lg-710-110.hr-515-33.ch-635-1408.sh-725-110", "ch -3050-1408-1408.hd-180-1359.lg-280-110.hr-828-61.sh-290-110.fa-1212-63", "[Man VS Zombie] Humain");
            }
            else if (this.Travaille == true)
            {
                this.updateAvatarEvent(this.RankInfo.Look_F, this.RankInfo.Look_H, "[TRAVAILLE] " + this.RankInfo.Name + ", " + this.TravailInfo.Name);
            }
            else
            {
                using (IQueryAdapter motto = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    motto.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @id LIMIT 1");
                    motto.AddParameter("id", this.Id);
                    this.Motto = motto.getString();
                }

                using (IQueryAdapter look = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    look.SetQuery("SELECT `look` FROM `users` WHERE `id` = @id LIMIT 1");
                    look.AddParameter("id", this.Id);
                    this.Look = look.getString();
                }

                if (this.Look.Contains(".."))
                {
                    this.Look = this.Look.Replace("..", ".");

                    using (IQueryAdapter updateLook = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        updateLook.SetQuery("UPDATE users SET look = @look WHERE id = @id LIMIT 1");
                        updateLook.AddParameter("look", this.Look);
                        updateLook.AddParameter("id", this.Id);
                    }
                }
                
                this.GetClient().SendMessage(new UserChangeComposer(User, true));
                this.CurrentRoom.SendMessage(new UserChangeComposer(User, false));
            }
        }

        public void updateHomeRoom(int Id)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET home_room = @id_room WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id_room", Id);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateBanque()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET banque = @banque WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("banque", this.Banque);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatecoiffure()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET coiffure = @coiffure WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("coiffure", this.Coiffure);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateCb()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET cb = @cb WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("cb", this.Cb);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updatePoliceCasier()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET policecasier = @policecasier WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("policecasier", this.PoliceCasier);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateEnergie()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET energie = @energie WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("energie", this.Energie);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateSante()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET sante = @sante WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("sante", this.Sante);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void updateHygiene()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET hygiene = @hygiene WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("hygiene", this.Hygiene);
                dbClient.AddParameter("id", this.Id);
                dbClient.RunQuery();
            }
        }

        public void setFavoriteGroup(int GroupId)
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User == null)
                return;

            this.GetStats().FavouriteGroupId = GroupId;
            Group Group = null;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group);
            this.CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(this.Id));
            this.CurrentRoom.SendMessage(new HabboGroupBadgesComposer(Group));
            this.CurrentRoom.SendMessage(new UpdateFavouriteGroupComposer(this.Id, Group, User.VirtualId));
        }

        public void WantedLevelToggle(bool Toggle)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Toggle == true)
                {
                    dbClient.SetQuery("UPDATE users_wanted SET level = level + 1 WHERE user_id = @userId");
                    dbClient.AddParameter("userId", this.Id);
                    dbClient.RunQuery();
                }
                else
                {
                    dbClient.SetQuery("UPDATE users_wanted SET level = level - 1 WHERE user_id = @userId");
                    dbClient.AddParameter("userId", this.Id);
                    dbClient.RunQuery();
                }
            }
        }

        public void WantedToggle(bool Toggle, int Level)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (Toggle == true)
                {
                    dbClient.SetQuery("INSERT INTO users_wanted (user_id, level, added_date) VALUES (@userId, @level, @date);");
                    dbClient.AddParameter("userId", this.Id);
                    dbClient.AddParameter("level", Level);
                    dbClient.AddParameter("date", DateTime.Now);
                    dbClient.RunQuery();
                }
                else
                {
                    dbClient.SetQuery("DELETE FROM `users_wanted` WHERE `user_id` = @userId");
                    dbClient.AddParameter("userId", this.Id);
                    dbClient.RunQuery();
                }
            }
        }

        public int checkStarWanted()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `level` FROM `users_wanted` WHERE `user_id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", this.Id);
                int getWanted = dbClient.getInteger();
                return getWanted;
            }
        }

        public bool checkIfWanted()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id` FROM `users_wanted` WHERE `user_id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", this.Id);
                int getWanted = dbClient.getInteger();
                if (getWanted > 0)
                    return true;
            }
            return false;
        }

        public bool checkCoiff(string NewCoiff)
        {
            string lookSession = this.Look;
            string[] lookSessionReplace = lookSession.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        
            string hr;

            if (lookSession.Contains("hr-"))
            {
                hr = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains("hr-"))];
                if(hr == NewCoiff)
                {
                    return true;
                }
            }

            return false;
        }

        public void addTenu(int Code, int Color)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO looks_users (user_id, code, color) VALUES (@user_id, @code, @color);");
                dbClient.AddParameter("user_id", this.Id);
                dbClient.AddParameter("code", Code);
                dbClient.AddParameter("color", Color);
                dbClient.RunQuery();
            }
        }

        public void removeLookType(string Type)
        {
            string lookSession = this.Look;
            string[] lookSessionReplace = lookSession.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string hr;

            if (lookSession.Contains(Type))
            {
                hr = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains(Type))];
                lookSession = lookSession.Replace(hr, "");
            }

            if (lookSession.EndsWith("."))
            {
                lookSession = lookSession.Remove(lookSession.Length - 1);
            }

            this.Look = lookSession;
            this.updateLook();
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User != null)
            {
                this.GetClient().SendMessage(new UserChangeComposer(User, true));
                this.CurrentRoom.SendMessage(new UserChangeComposer(User, false));
            }
        }

        public void changeLookByType(string Type, string NewCoiff)
        {
            string lookSession = this.Look;
            string[] lookSessionReplace = lookSession.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string hr;

            if (lookSession.Contains(Type))
            {
                hr = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains(Type))];
                lookSession = lookSession.Replace(hr, Type + NewCoiff);
            }
            else
            {
                lookSession = lookSession + "." + Type + NewCoiff + ".";
            }

            if(lookSession.EndsWith("."))
            {
                lookSession = lookSession.Remove(lookSession.Length - 1);
            }

            if (lookSession.Contains(".."))
            {
                lookSession = lookSession.Replace("..", ".");
            }

            this.Look = lookSession;
            this.updateLook();
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            if (User != null)
            {
                this.GetClient().SendMessage(new UserChangeComposer(User, true));
                this.CurrentRoom.SendMessage(new UserChangeComposer(User, false));
            }
        }

        public string changeLookWithoutCoiff(string SessionLook, string LookExemple)
        {
            string lookSession = SessionLook;
            string[] lookSessionReplace = lookSession.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string look = LookExemple;
            string[] lookReplace = look.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string hr;

            if (lookSession.Contains("hr-"))
            {
                hr = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains("hr-"))];
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hr-"))], hr);
            }
            else
            {
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hr-"))], "");
            }

            return look;
        }

        public string changeLook(string SessionLook, string LookExemple)
        {
            string lookSession = SessionLook;
            string[] lookSessionReplace = lookSession.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string look = LookExemple;
            string[] lookReplace = look.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            string hr;
            string hd;

            if (lookSession.Contains("hr-"))
            {
                hr = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains("hr-"))];
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hr-"))], hr);
            }
            else
            {
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hr-"))], "");
            }

            if (lookSession.Contains("hd-"))
            {
                hd = lookSessionReplace[Array.FindIndex(lookSessionReplace, row => row.Contains("hd-"))];
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hd-"))], hd);
            }
            else
            {
                look = look.Replace(lookReplace[Array.FindIndex(lookReplace, row => row.Contains("hd-"))], "");
            }

            return look;
        }

        public void Dispose()
        {
            if (this.InventoryComponent != null)
                this.InventoryComponent.SetIdleState();

            if (this.UsersRooms != null)
                UsersRooms.Clear();

            if (this.InRoom && this.CurrentRoom != null)
                this.CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(this._client, false, false);

            if (Messenger != null)
            {
                this.Messenger.AppearOffline = true;
                this.Messenger.Destroy();
            }

            if (this._fx != null)
                this._fx.Dispose();

            if (this._clothing != null)
                this._clothing.Dispose();

            if (this._permissions != null)
                this._permissions.Dispose();
        }

        public void MaladieHygiene()
        {
            RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
            this.Sante = 0;
            this.updateSante();
            this.updateHopitalEtat(User, 10);
            User.GetClient().SendNotification("Vous avez développé une maladie suite à un manque d'hygiène et votre état de santé est critique. Vous avez été transporté jusqu'à l'hôpital.");
        }

        public void EnergieMalaise(int EnergieNumber)
        {
            if (this.Energie > EnergieNumber)
            {
                this.Energie = this.Energie - EnergieNumber;
                this.updateEnergie();
            }
            else if(this.Prison == 0 && this.Menotted == false && this.CurrentRoomId != 102 && this.CurrentRoomId != 126 && this.CurrentRoomId != 127 && this.CurrentRoomId != 80)
            {
                RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
                User.OnChat(User.LastBubble, "* Fait un malaise *", true);
                this.Energie = 0;
                this.updateEnergie();
                this.updateHopitalEtat(User, 10);
                User.GetClient().SendNotification("Vous avez fait un malaise, les ambulanciers vous ont transporté jusqu'à l'hôpital. Vous pourrez sortir dans 10 minutes.");
                if (this.CurrentRoomId == PlusEnvironment.Salade)
                {
                    PlusEnvironment.GetGame().GetClientManager().checkIfWinSalade();
                }
            }
        }

        public void EnergieTimer()
        {
            try
            {
                this._energieUpdate--;

                if (this._energieUpdate <= 0)
                {
                    RoomUser User = this.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this.Username);
                    if (User.IsAsleep == false && this.Hopital == 0)
                    {
                        Random energieUsed = new Random();
                        int energieUsedNumber = energieUsed.Next(1, 3);
                        this.EnergieMalaise(energieUsedNumber);
                    }

                    this.EnergieUpdate = PlusStaticGameSettings.EnergieTimer;
                }
            }
            catch { }
        }

        public GameClient GetClient()
        {
            if (this._client != null)
                return this._client;

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Id);
        }

        public HabboMessenger GetMessenger()
        {
            return Messenger;
        }

        public BadgeComponent GetBadgeComponent()
        {
            return BadgeComponent;
        }

        public InventoryComponent GetInventoryComponent()
        {
            return InventoryComponent;
        }

        public SearchesComponent GetNavigatorSearches()
        {
            return this._navigatorSearches;
        }

        public EffectsComponent Effects()
        {
            return this._fx;
        }

        public ClothingComponent GetClothing()
        {
            return this._clothing;
        }

        public int GetQuestProgress(int p)
        {
            int progress = 0;
            quests.TryGetValue(p, out progress);
            return progress;
        }

        public UserAchievement GetAchievementData(string p)
        {
            UserAchievement achievement = null;
            Achievements.TryGetValue(p, out achievement);
            return achievement;
        }

        public void ChangeName(string Username)
        {
            this.LastNameChange = PlusEnvironment.GetUnixTimestamp();
            this.Username = Username;

            this.SaveKey("username", Username);
            this.SaveKey("last_change", this.LastNameChange.ToString());
        }

        public void SaveKey(string Key, string Value)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET " + Key + " = @value WHERE `id` = '" + this.Id + "' LIMIT 1;");
                dbClient.AddParameter("value", Value);
                dbClient.RunQuery();
            }
        }

        public void PrepareRoom(int Id, string Password)
        {
            if (this.GetClient() == null || this.GetClient().GetHabbo() == null)
                return;

            if (this.GetClient().GetHabbo().CanChangeRoom == false)
                return;

            if (this.GetClient().GetHabbo().InRoom)
            {
                Room OldRoom = null;
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(this.GetClient().GetHabbo().CurrentRoomId, out OldRoom))
                    return;

                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(this.GetClient(), false, false);
            }

            if (this.GetClient().GetHabbo().IsTeleporting && this.GetClient().GetHabbo().TeleportingRoomID != Id)
            {
                this.GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            Room Room = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Id);
            if (Room == null)
            {
                this.GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            if (Room.isCrashed)
            {
                this.GetClient().SendNotification("This room has crashed! :(");
                this.GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            this.GetClient().GetHabbo().CurrentRoomId = Room.RoomId;

            /* if (Room.GetRoomUserManager().userCount >= Room.UsersMax && !this.GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") && this.GetClient().GetHabbo().Id != Room.OwnerId)
            {
                this.GetClient().SendMessage(new CantConnectComposer(1));
                this.GetClient().SendMessage(new CloseConnectionComposer());
                return;
            } */

            if (!this.GetClient().GetHabbo().GetPermissions().HasRight("room_ban_override") && Room.UserIsBanned(this.GetClient().GetHabbo().Id))
            {
                if (Room.HasBanExpired(this.GetClient().GetHabbo().Id))
                    Room.RemoveBan(this.GetClient().GetHabbo().Id);
                else
                {
                    this.GetClient().GetHabbo().RoomAuthOk = false;
                    this.GetClient().SendMessage(new CantConnectComposer(4));
                    this.GetClient().SendMessage(new CloseConnectionComposer());
                    return;
                }
            }

            this.GetClient().SendMessage(new OpenConnectionComposer());
            if (!Room.CheckRights(this.GetClient(), true, true) && !this.GetClient().GetHabbo().IsTeleporting && !this.GetClient().GetHabbo().IsHopping)
            {
                if (Room.Access == RoomAccess.DOORBELL && !this.GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (Room.UserCount > 0)
                    {
                        this.GetClient().SendMessage(new DoorbellComposer(""));
                        Room.SendMessage(new DoorbellComposer(this.GetClient().GetHabbo().Username), true);
                        return;
                    }
                    else
                    {
                        this.GetClient().SendMessage(new FlatAccessDeniedComposer(""));
                        this.GetClient().SendMessage(new CloseConnectionComposer());
                        return;
                    }
                }
                else if (Room.Access == RoomAccess.PASSWORD && !this.GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (Password.ToLower() != Room.Password.ToLower() || String.IsNullOrWhiteSpace(Password))
                    {
                        this.GetClient().SendMessage(new GenericErrorComposer(-100002));
                        this.GetClient().SendMessage(new CloseConnectionComposer());
                        return;
                    }
                }
            }

            if (!EnterRoom(Room))
                this.GetClient().SendMessage(new CloseConnectionComposer());

        }

        public bool EnterRoom(Room Room)
        {
            if (Room == null)
                this.GetClient().SendMessage(new CloseConnectionComposer());

            this.GetClient().SendMessage(new RoomReadyComposer(Room.RoomId, Room.ModelName));
            if (Room.Wallpaper != "0.0")
                this.GetClient().SendMessage(new RoomPropertyComposer("wallpaper", Room.Wallpaper));
            if (Room.Floor != "0.0")
                this.GetClient().SendMessage(new RoomPropertyComposer("floor", Room.Floor));

            this.GetClient().SendMessage(new RoomPropertyComposer("landscape", Room.Landscape));
            this.GetClient().SendMessage(new RoomRatingComposer(Room.Score, !(this.GetClient().GetHabbo().RatedRooms.Contains(Room.RoomId) || Room.OwnerId == this.GetClient().GetHabbo().Id)));


            if (Room.OwnerId != this.Id)
            {
                this.GetClient().GetHabbo().GetStats().RoomVisits += 1;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(this.GetClient(), "ACH_RoomEntry", 1);
            }
            return true;
        }
    }
}