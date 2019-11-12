using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Games;

using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Incoming;

using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUser
    {
        public bool AllowOverride;
        public BotAI BotAI;
        public RoomBot BotData;
        public bool CanWalk;
        public int CarryItemID; //byte
        public int CarryTimer; //byte
        public int ChatSpamCount = 0;
        public int ChatSpamTicks = 16;
        public ItemEffectType CurrentItemEffect;
        public int DanceId;
        public bool FastWalking = false;
        public bool SuperFastWalking = false;
        public bool UltraFastWalking = false;
        public int FreezeCounter;
        public int FreezeLives;
        public bool Freezed;
        public bool Frozen;
        public bool Tased;
        public string userTased;
        public string userFocused = null;
        public bool Disconnect;
        public int GateId;
        public bool makeAction;
        public bool ConnectedMetier;
        public string Transaction;
        public int AppartItem = 0;
        public int Item_On;
        public DateTime lastAction;
        public DateTime lastActionWebEvent;
        public bool WebSocketUse;
        public bool usingATM;
        public bool wantSoin = false;
        public bool canUseCB = false;
        public bool isDemarreCar;
        public bool isMakingCard;
        public string isMakingCardCode = null;
        public int GoalX; //byte
        public int GoalY; //byte
        public int HabboId;
        public int HorseID = 0;
        public int IdleTime; //byte
        public bool InteractingGate;
        public int InternalRoomID;
        public bool canUseFoutain = false;
        public bool isBruling = false;
        public int isBrulingItem = 0;
        public bool IsAsleep;
        public bool IsWalking;
        public int LastBubble = 0;
        public double LastInteraction;
        public Item LastItem = null;
        public int LockedTilesCount;
        public string AnalyseUser;
        public string DuelUser = null;
        public string DuelToken = null;
        public bool makeCreation = false;
        public int creationTimer;
        public string CreationName;
        public bool Immunised = false;
        public string ImmunisedToken;
        public bool HaveTicket;
        public bool connectedToSlot = false;
        public bool isSlot = false;
        public string boissonToken = null;
        public string boissonPrepared = null;
        public bool participateRoulette = false;
        public int numberRoulette = 0;
        public int miseRoulette = 0;
        public string ManVsZombieTeam = null;
        public bool usingCasier = false;
        public bool mainPropre = false;
        public bool isDisconnecting = false;
        public string fouillerUser = null;

        // EVENT
        public bool haveMunition = false;
        public int creationGift = 0;
        public int creationGiftTotal = 0;

        public List<Vector2D> Path = new List<Vector2D>();
        public bool PathRecalcNeeded = false;
        public int PathStep = 1;
        public Pet PetData;

        public int PrevTime;
        public bool RidingHorse = false;
        public int RoomId;
        public int RotBody; //byte
        public int RotHead; //byte

        public bool SetStep;
        public int SetX; //byte
        public int SetY; //byte
        public double SetZ;
        public double SignTime;
        public byte SqState;
        public Dictionary<string, string> Statusses;
        public int TeleDelay; //byte
        public bool TeleportEnabled;
        public bool UpdateNeeded;
        public int VirtualId;
        public string Purchase = "";
        public int isFarmingRock = 0;

        public int X; //byte
        public int Y; //byte
        public double Z;

        public FreezePowerUp banzaiPowerUp;
        public bool isLying = false;
        public bool isSitting = false;
        private GameClient mClient;
        private Room mRoom;
        public bool moonwalkEnabled = false;
        public bool shieldActive;
        public int shieldCounter;
        public TEAM Team;
        public bool FreezeInteracting;
        public int UserId;
        public bool IsJumping;

        public bool isRolling = false;
        public int rollerDelay = 0;

        public int LLPartner = 0;
        public double TimeInRoom = 0;
        public int Capture = 0;
        public int CaptureProgress = 0;
        public bool cheveuxPropre = false;
        public string usernameCoiff = null;
        public int userChangeRank;
        public bool policeFouille = false;

        // ECHANGE
        public bool isTradingItems = false;
        public string isTradingUsername = null;
        public bool isTradingConfirm = false;
        public Dictionary<string, string> isTradingListItems = new Dictionary<string, string>();

        public RoomUser(int HabboId, int RoomId, int VirtualId, Room room)
        {
            this.Freezed = false;
            this.HabboId = HabboId;
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            this.IdleTime = 0;

            this.X = 0;
            this.Y = 0;
            this.Z = 0;
            this.PrevTime = 0;
            this.RotHead = 0;
            this.RotBody = 0;
            this.UpdateNeeded = true;
            this.Statusses = new Dictionary<string, string>();

            this.TeleDelay = -1;
            this.mRoom = room;

            this.AllowOverride = false;
            this.CanWalk = true;


            this.SqState = 3;

            this.InternalRoomID = 0;
            this.CurrentItemEffect = ItemEffectType.NONE;

            this.FreezeLives = 0;
            this.InteractingGate = false;
            this.GateId = 0;
            this.LastInteraction = 0;
            this.LockedTilesCount = 0;

            this.IsJumping = false;
            this.TimeInRoom = 0;
        }


        public Point Coordinate
        {
            get { return new Point(X, Y); }
        }

        public bool IsPet
        {
            get { return (IsBot && BotData.IsPet); }
        }

        public int CurrentEffect
        {
            get { return GetClient().GetHabbo().Effects().CurrentEffect; }
        }


        public bool IsDancing
        {
            get
            {
                if (DanceId >= 1)
                {
                    return true;
                }

                return false;
            }
        }

        public bool NeedsAutokick
        {
            get
            {
                if (IsBot)
                    return false;

                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;

                if (GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || GetRoom().OwnerId == HabboId)
                    return false;

                if (GetRoom().Id == 1649919)
                    return false;

                if (IdleTime >= 7200)
                    return true;

                return false;
            }
        }

        public bool IsTrading
        {
            get
            {
                if (IsBot)
                    return false;

                if (Statusses.ContainsKey("trd"))
                    return true;

                return false;
            }
        }

        public bool IsBot
        {
            get
            {
                if (BotData != null)
                    return true;

                return false;
            }
        }

        public string GetUsername()
        {
            if (IsBot)
                return string.Empty;

            if (GetClient() != null)
            {
                if (GetClient().GetHabbo() != null)
                {
                    return GetClient().GetHabbo().Username;
                }
                else
                    return PlusEnvironment.GetUsernameById(HabboId);

            }
            else
                return PlusEnvironment.GetUsernameById(HabboId);
        }

        public void UnIdle()
        {
            if (!IsBot)
            {
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAFK = 0;
            }

            IdleTime = 0;

            if (IsAsleep)
            {
                this.OnChat(this.LastBubble, "* Se réveille *", true);
                IsAsleep = false;
                GetRoom().SendMessage(new SleepComposer(this, false));
                this.GetClient().GetHabbo().resetEffectEvent();
            }
        }

        public void Dispose()
        {
            Statusses.Clear();
            mRoom = null;
            mClient = null;
        }

        public void Chat(string Message, bool Shout, int colour = 0)
        {
            if (GetRoom() == null)
                return;

            if (!IsBot)
                return;


            if (IsPet)
            {
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowPetSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, 0));
                }
            }
            else
            {
                foreach (RoomUser User in GetRoom().GetRoomUserManager().GetUserList().ToList())
                {
                    if (User == null || User.IsBot)
                        continue;

                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowBotSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, (colour == 0 ? 2 : colour)));
                }
            }
        }

        public void HandleSpamTicks()
        {
            if (ChatSpamTicks >= 0)
            {
                ChatSpamTicks--;

                if (ChatSpamTicks == -1)
                {
                    ChatSpamCount = 0;
                }
            }
        }

        public bool IncrementAndCheckFlood(out int MuteTime)
        {
            MuteTime = 0;

            ChatSpamCount++;
            if (ChatSpamTicks == -1)
                ChatSpamTicks = 8;
            else if (ChatSpamCount >= 6)
            {
                if (GetClient().GetHabbo().GetPermissions().HasRight("events_staff"))
                    MuteTime = 3;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("gold_vip"))
                    MuteTime = 7;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("silver_vip"))
                    MuteTime = 10;
                else
                    MuteTime = 20;

                GetClient().GetHabbo().FloodTime = PlusEnvironment.GetUnixTimestamp() + MuteTime;

                ChatSpamCount = 0;
                return true;
            }
            return false;
        }

        public void OnChat(int Colour, string Message, bool Shout)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || mRoom == null)
                return;

            if (mRoom.GetWired().TriggerEvent(Items.Wired.WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), Message))
                return;


            GetClient().GetHabbo().HasSpoken = true;

            if (mRoom.WordFilterList.Count > 0 && !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {
                Message = mRoom.GetFilter().CheckMessage(Message);
            }

            ServerPacket Packet = null;
            if (Shout)
                Packet = new ShoutComposer(VirtualId, Message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
            else
                Packet = new ChatComposer(VirtualId, Message, PlusEnvironment.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);


            if (GetClient().GetHabbo().TentId > 0)
            {
                mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, Packet);

                Packet = new WhisperComposer(this.VirtualId, "[Tent Chat] " + Message, 0, Colour);

                List<RoomUser> ToNotify = mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                if (ToNotify.Count > 0)
                {
                    foreach (RoomUser user in ToNotify)
                    {
                        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null ||
                            user.GetClient().GetHabbo().TentId == GetClient().GetHabbo().TentId)
                        {
                            continue;
                        }

                        user.GetClient().SendMessage(Packet);
                    }
                }
            }
            else
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetRoomUsers().ToList())
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().MutedUsers.Contains(mClient.GetHabbo().Id))
                        continue;

                    if (mRoom.chatDistance > 0 && Gamemap.TileDistance(this.X, this.Y, User.X, User.Y) > mRoom.chatDistance)
                        continue;
                    
                    User.GetClient().SendMessage(Packet);
                }
            }

            #region Pets/Bots responces
            if (Shout)
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.IsBot)
                        User.BotAI.OnUserShout(this, Message);
                }
            }
            else
            {
                foreach (RoomUser User in mRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (!User.IsBot)
                        continue;

                    if (User.IsBot)
                        User.BotAI.OnUserSay(this, Message);
                }
            }
            #endregion

        }

        public void ClearMovement(bool Update)
        {
            IsWalking = false;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;

            if (Update)
            {
                UpdateNeeded = true;
            }
        }

        public void setDuelWinner()
        {
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(this.DuelUser);
            if (TargetClient != null)
            {
                Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                TargetUser.OnChat(TargetUser.LastBubble, "* Gagne le duel contre " + this.GetClient().GetHabbo().Username + " *", true);
                TargetUser.DuelUser = null;
                TargetUser.DuelToken = null;
            }

            this.DuelUser = null;
            this.DuelToken = null;
        }

        public void valideTrade(RoomUser userTrade)
        {
            Habbo MeTrade = this.GetClient().GetHabbo();
            Habbo OtherTrade = userTrade.GetClient().GetHabbo();

            #region credits
            if (userTrade.isTradingListItems.ContainsKey("credits"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("credits", out montant);

                OtherTrade.Credits -= Convert.ToInt32(montant);
                userTrade.GetClient().SendMessage(new CreditBalanceComposer(OtherTrade.Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(userTrade.GetClient(), "my_stats;" + OtherTrade.Credits + ";" + OtherTrade.Duckets + ";" + OtherTrade.EventPoints);

                MeTrade.Credits += Convert.ToInt32(montant);
                this.GetClient().SendMessage(new CreditBalanceComposer(MeTrade.Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "my_stats;" + MeTrade.Credits + ";" + MeTrade.Duckets + ";" + MeTrade.EventPoints);

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("INSERT INTO echange_logs (item, montant, from_user, to_user) VALUES ('credits', '" + montant + "', '" + OtherTrade.Id + "', '" + MeTrade.Id + "')");
                }
            }
            #endregion
            #region coca
            if (userTrade.isTradingListItems.ContainsKey("coca"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("coca", out montant);

                MeTrade.Coca += Convert.ToInt32(montant);
                MeTrade.updateCoca();
                OtherTrade.Coca -= Convert.ToInt32(montant);
                OtherTrade.updateCoca();
            }
            #endregion
            #region fanta
            if (userTrade.isTradingListItems.ContainsKey("fanta"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("fanta", out montant);

                MeTrade.Fanta += Convert.ToInt32(montant);
                MeTrade.updateFanta();
                OtherTrade.Fanta -= Convert.ToInt32(montant);
                OtherTrade.updateFanta();
            }
            #endregion
            #region pain
            if (userTrade.isTradingListItems.ContainsKey("pain"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("pain", out montant);

                MeTrade.Pain += Convert.ToInt32(montant);
                MeTrade.updatePain();
                OtherTrade.Pain -= Convert.ToInt32(montant);
                OtherTrade.updatePain();
            }
            #endregion
            #region sucette
            if (userTrade.isTradingListItems.ContainsKey("sucette"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("sucette", out montant);

                MeTrade.Sucette += Convert.ToInt32(montant);
                MeTrade.updateSucette();
                OtherTrade.Sucette -= Convert.ToInt32(montant);
                OtherTrade.updateSucette();
            }
            #endregion
            #region savon
            if (userTrade.isTradingListItems.ContainsKey("savon"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("savon", out montant);

                MeTrade.Savon += Convert.ToInt32(montant);
                MeTrade.updateSavon();
                OtherTrade.Savon -= Convert.ToInt32(montant);
                OtherTrade.updateSavon();
            }
            #endregion
            #region doliprane
            if (userTrade.isTradingListItems.ContainsKey("doliprane"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("doliprane", out montant);

                MeTrade.Doliprane += Convert.ToInt32(montant);
                MeTrade.updateDoliprane();
                OtherTrade.Doliprane -= Convert.ToInt32(montant);
                OtherTrade.updateDoliprane();
            }
            #endregion
            #region weed
            if (userTrade.isTradingListItems.ContainsKey("weed"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("weed", out montant);

                MeTrade.Weed += Convert.ToInt32(montant);
                MeTrade.updateWeed();
                OtherTrade.Weed -= Convert.ToInt32(montant);
                OtherTrade.updateWeed();
            }
            #endregion
            #region cigarette
            if (userTrade.isTradingListItems.ContainsKey("cigarette"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("cigarette", out montant);

                MeTrade.PhilipMo += Convert.ToInt32(montant);
                MeTrade.updatePhilipMo();
                OtherTrade.PhilipMo -= Convert.ToInt32(montant);
                OtherTrade.updatePhilipMo();
            }
            #endregion
            #region clipper
            if (userTrade.isTradingListItems.ContainsKey("clipper"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("clipper", out montant);

                MeTrade.Clipper += (Convert.ToInt32(montant) * 50);
                MeTrade.updateClipper();
                OtherTrade.Clipper -= (Convert.ToInt32(montant) * 50);
                OtherTrade.updateClipper();
            }
            #endregion
            #region hoverboardBlanc
            if (userTrade.isTradingListItems.ContainsKey("hoverboardBlanc"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("hoverboardBlanc", out montant);

                MeTrade.WhiteHoverboard = 1;
                MeTrade.updateWhiteHoverboard();
                OtherTrade.WhiteHoverboard = 0;
                OtherTrade.updateWhiteHoverboard();
            }
            #endregion
            #region porsche911
            if (userTrade.isTradingListItems.ContainsKey("porsche911"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("porsche911", out montant);

                MeTrade.Porsche911 = 1;
                MeTrade.updatePorsche911();
                OtherTrade.Porsche911 = 0;
                OtherTrade.updatePorsche911();
            }
            #endregion
            #region fiatPunto
            if (userTrade.isTradingListItems.ContainsKey("fiatPunto"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("fiatPunto", out montant);

                MeTrade.FiatPunto = 1;
                MeTrade.updateFiatPunto();
                OtherTrade.FiatPunto = 0;
                OtherTrade.updateFiatPunto();
            }
            #endregion
            #region volkswagenJetta
            if (userTrade.isTradingListItems.ContainsKey("volkswagenJetta"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("volkswagenJetta", out montant);

                MeTrade.VolkswagenJetta = 1;
                MeTrade.updateVolkswagenJetta();
                OtherTrade.VolkswagenJetta = 0;
                OtherTrade.updateVolkswagenJetta();
            }
            #endregion
            #region bmwI8
            if (userTrade.isTradingListItems.ContainsKey("bmwI8"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("bmwI8", out montant);

                MeTrade.BmwI8 = 1;
                MeTrade.updateBMWI8();
                OtherTrade.BmwI8 = 0;
                OtherTrade.updateBMWI8();
            }
            #endregion
            #region audiA8
            if (userTrade.isTradingListItems.ContainsKey("audiA8"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("audiA8", out montant);

                MeTrade.AudiA8 = 1;
                MeTrade.updateAudiA8();
                OtherTrade.AudiA8 = 0;
                OtherTrade.updateAudiA8();
            }
            #endregion
            #region audiA3
            if (userTrade.isTradingListItems.ContainsKey("audiA3"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("audiA3", out montant);

                MeTrade.AudiA3 = 1;
                MeTrade.updateAudiA3();
                OtherTrade.AudiA3 = 0;
                OtherTrade.updateAudiA3();
            }
            #endregion
            #region batte
            if (userTrade.isTradingListItems.ContainsKey("batte"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("batte", out montant);

                MeTrade.Batte = 1;
                MeTrade.updateBatte();
                OtherTrade.Batte = 0;
                OtherTrade.updateBatte();
            }
            #endregion
            #region sabre
            if (userTrade.isTradingListItems.ContainsKey("sabre"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("sabre", out montant);

                MeTrade.Sabre = 1;
                MeTrade.updateSabre();
                OtherTrade.Sabre = 0;
                OtherTrade.updateSabre();
            }
            #endregion
            #region ak47
            if (userTrade.isTradingListItems.ContainsKey("ak47"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("ak47", out montant);

                MeTrade.Ak47 = 1;
                MeTrade.updateAk47();
                MeTrade.AK47_Munitions += OtherTrade.AK47_Munitions;
                MeTrade.updateAK47Munitions();

                OtherTrade.Ak47 = 0;
                OtherTrade.updateAk47();
                OtherTrade.AK47_Munitions = 0;
                OtherTrade.updateAK47Munitions();
            }
            #endregion
            #region uzi
            if (userTrade.isTradingListItems.ContainsKey("uzi"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("uzi", out montant);

                MeTrade.Uzi = 1;
                MeTrade.updateUzi();
                MeTrade.Uzi_Munitions += OtherTrade.Uzi_Munitions;
                MeTrade.updateUziMunitions();

                OtherTrade.Uzi = 0;
                OtherTrade.updateUzi();
                OtherTrade.Uzi_Munitions = 0;
                OtherTrade.updateUziMunitions();
            }
            #endregion
            #region cocktail
            if (userTrade.isTradingListItems.ContainsKey("cocktail"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("cocktail", out montant);

                MeTrade.Cocktails += Convert.ToInt32(montant);
                MeTrade.updateCocktails();
                OtherTrade.Cocktails -= Convert.ToInt32(montant);
                OtherTrade.updateCocktails();
            }
            #endregion
            #region gps
            if (userTrade.isTradingListItems.ContainsKey("gps"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("gps", out montant);

                MeTrade.Gps = 1;
                MeTrade.updateGps();
                OtherTrade.Gps = 0;
                OtherTrade.updateGps();
            }
            #endregion
            #region telephone
            if (userTrade.isTradingListItems.ContainsKey("telephone"))
            {
                string montant;
                userTrade.isTradingListItems.TryGetValue("telephone", out montant);

                MeTrade.Telephone = 1;
                MeTrade.updateTelephone();
                MeTrade.TelephoneName = OtherTrade.TelephoneName;
                MeTrade.updateTelephoneName();
                if(MeTrade.TelephoneForfait == 0)
                {
                    MeTrade.TelephoneForfaitType = 1;
                    MeTrade.updateForfaitType();
                }

                OtherTrade.Telephone = 0;
                OtherTrade.updateTelephone();
                OtherTrade.TelephoneName = "Aucun";
                OtherTrade.updateTelephoneName();
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("DELETE FROM `messenger_friendships` WHERE `user_one_id` = @userId OR `user_two_id` = @userId");
                    dbClient.AddParameter("userId", OtherTrade.Id);
                    dbClient.RunQuery();
                }

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "telephone;show");
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(userTrade.GetClient(), "telephone;hide");
            }
            #endregion

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;cancelTrade");
            return;
        }

        public void cancelItemsTrade()
        {
            this.isTradingItems = false;
            this.isTradingUsername = null;
            this.isTradingConfirm = false;
            this.isTradingListItems.Clear();
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;cancelTrade");
        }

        public void removeItemsTrade(string Name, string Parameter, RoomUser TargetUser)
        {
            this.isTradingConfirm = false;
            TargetUser.isTradingConfirm = false;
            this.isTradingListItems.Remove(Name);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;removeItems;me;" + Name + ";" + Parameter);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetUser.GetClient(), "trade;removeItems;other;" + Name + ";" + Parameter);
        }

        public void addItemsTrade(string Name, string Parameter, string Image, int Count, RoomUser TargetUser)
        {
            this.isTradingConfirm = false;
            TargetUser.isTradingConfirm = false;
            this.isTradingListItems.Add(Name, Parameter);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;addItems;me;" + Name + ";" + Parameter + ";" + Image + ";" + Count);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetUser.GetClient(), "trade;addItems;other;" + Name + ";" + Parameter + ";" + Image);
        }

        public void confirmTrade(RoomUser TargetUser)
        {
            this.isTradingConfirm = true;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;valideTrade;me");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetUser.GetClient(), "trade;valideTrade;other");
        }

        public void initItemsTrade(RoomUser TargetUser)
        {
            Habbo MeTrade = this.GetClient().GetHabbo();
            Habbo TargetTrade = TargetUser.GetClient().GetHabbo();

            string itemsList = "credits-" + MeTrade.Credits + "/";
            if (MeTrade.Coca > 0)
            {
                itemsList = itemsList + "coca-" + MeTrade.Coca + "/";
            }

            if (MeTrade.Fanta > 0)
            {
                itemsList = itemsList + "fanta-" + MeTrade.Fanta + "/";
            }

            if (MeTrade.Pain > 0)
            {
                itemsList = itemsList + "pain-" + MeTrade.Pain + "/";
            }

            if (MeTrade.Sucette > 0)
            {
                itemsList = itemsList + "sucette-" + MeTrade.Sucette + "/";
            }

            if (MeTrade.Doliprane > 0)
            {
                itemsList = itemsList + "doliprane-" + MeTrade.Doliprane + "/";
            }

            if (MeTrade.Savon > 0)
            {
                itemsList = itemsList + "savon-" + MeTrade.Savon + "/";
            }

            if (MeTrade.Weed > 0)
            {
                itemsList = itemsList + "weed-" + MeTrade.Weed + "/";
            }

            if (MeTrade.PhilipMo > 0)
            {
                itemsList = itemsList + "cigarette-" + MeTrade.PhilipMo + "/";
            }

            int ClipperNumber = MeTrade.Clipper / 50;
            if (ClipperNumber > 0)
            {
                itemsList = itemsList + "clipper-" + ClipperNumber + "/";
            }

            if(MeTrade.WhiteHoverboard == 1)
            {
                itemsList = itemsList + "hoverboardBlanc/";
            }

            if (MeTrade.Porsche911 == 1)
            {
                itemsList = itemsList + "porsche911/";
            }

            if (MeTrade.FiatPunto == 1)
            {
                itemsList = itemsList + "fiatPunto/";
            }

            if (MeTrade.VolkswagenJetta == 1)
            {
                itemsList = itemsList + "volkswagenJetta/";
            }

            if (MeTrade.BmwI8 == 1)
            {
                itemsList = itemsList + "bmwI8/";
            }

            if (MeTrade.AudiA8 == 1)
            {
                itemsList = itemsList + "audiA8/";
            }

            if (MeTrade.AudiA3 == 1)
            {
                itemsList = itemsList + "audiA3/";
            }

            if (MeTrade.Batte == 1)
            {
                itemsList = itemsList + "batte/";
            }

            if (MeTrade.Sabre == 1)
            {
                itemsList = itemsList + "sabre/";
            }

            if (MeTrade.Ak47 == 1)
            {
                itemsList = itemsList + "ak47-" + MeTrade.AK47_Munitions + "/";
            }

            if (MeTrade.Uzi == 1)
            {
                itemsList = itemsList + "uzi-" + MeTrade.Uzi_Munitions + "/";
            }

            if (MeTrade.Cocktails > 0)
            {
                itemsList = itemsList + "cocktail-" + MeTrade.Cocktails + "/";
            }

            if (MeTrade.Telephone != 0)
            {
                itemsList = itemsList + "telephone-" + MeTrade.TelephoneName + "/";
            }

            if (MeTrade.Gps != 0)
            {
                itemsList = itemsList + "gps/";
            }

            if (MeTrade.Travaille)
            {
                MeTrade.stopWork();
                MeTrade.resetAvatarEvent();
            }

            if (MeTrade.Conduit != null)
            {
                MeTrade.stopConduire();
                MeTrade.resetEffectEvent();
            }

            if (MeTrade.ArmeEquiped != null)
            {
                MeTrade.ArmeEquiped = null;
                MeTrade.resetEffectEvent();
            }

            if (TargetTrade.Travaille)
            {
                TargetTrade.stopWork();
                TargetTrade.resetAvatarEvent();
            }

            if (TargetTrade.Conduit != null)
            {
                TargetTrade.stopConduire();
                TargetTrade.resetEffectEvent();
            }

            if (TargetTrade.ArmeEquiped != null)
            {
                TargetTrade.ArmeEquiped = null;
                TargetTrade.resetEffectEvent();
            }
    
            this.isTradingItems = true;
            this.isTradingUsername = TargetTrade.Username;
            this.isTradingConfirm = false;
            this.isTradingListItems.Clear();
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "trade;initTrade;" + TargetTrade.Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + MeTrade.Look + "&head_direction=3&gesture=sml&headonly=1;//habbo.fr/habbo-imaging/avatarimage?figure=" + TargetTrade.Look + "&head_direction=3&gesture=sml&headonly=1;" + itemsList);
        }

        public void MoveToIfCanWalk(Point c)
        {
            if (this.CanWalk == false || !this.IsBot && this.isTradingItems == true)
                return;

            MoveTo(c.X, c.Y);
        }

        public void MoveTo(Point c)
        {
            MoveTo(c.X, c.Y);
        }

        public void MoveTo(int pX, int pY, bool pOverride)
        {
            if (TeleportEnabled)
            {
                UnIdle();
                GetRoom().SendMessage(GetRoom().GetRoomItemHandler().UpdateUserOnRoller(this, new Point(pX, pY), 0, GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)));
                if (Statusses.ContainsKey("sit"))
                    Z -= 0.35;
                UpdateNeeded = true;
                return;
            }

            if ((GetRoom().GetGameMap().SquareHasUsers(pX, pY) && !pOverride) || Frozen)
                return;

            if (!this.IsBot && this.usingCasier)
            {
                this.usingCasier = false;
                this.OnChat(this.LastBubble, "* Ferme son casier avec son cadenas *", true);
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "casier;hide");
            }

            if (!this.IsBot && this.connectedToSlot == true)
            {
                this.connectedToSlot = false;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this.GetClient(), "slot_machine;hide");
            }

            UnIdle();

            GoalX = pX;
            GoalY = pY;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
        }

        public void MoveTo(int pX, int pY)
        {
            MoveTo(pX, pY, false);
        }

        public void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }


        public void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
        }

        public void CarryItem(int Item)
        {
            CarryItemID = Item;

            if (Item > 0)
                CarryTimer = 240;
            else
                CarryTimer = 0;

            GetRoom().SendMessage(new CarryObjectComposer(VirtualId, Item));
        }


        public void SetRot(int Rotation, bool HeadOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking)
            {
                return;
            }

            int diff = RotBody - Rotation;

            RotHead = RotBody;

            if (Statusses.ContainsKey("sit") || HeadOnly)
            {
                if (RotBody == 2 || RotBody == 4)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
                else if (RotBody == 0 || RotBody == 6)
                {
                    if (diff > 0)
                    {
                        RotHead = RotBody - 1;
                    }
                    else if (diff < 0)
                    {
                        RotHead = RotBody + 1;
                    }
                }
            }
            else if (diff <= -2 || diff >= 2)
            {
                RotHead = Rotation;
                RotBody = Rotation;
            }
            else
            {
                RotHead = Rotation;
            }

            UpdateNeeded = true;
        }

        public void SetStatus(string Key, string Value)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses[Key] = Value;
            }
            else
            {
                AddStatus(Key, Value);
            }
        }

        public void AddStatus(string Key, string Value)
        {
            Statusses[Key] = Value;
        }

        public void RemoveStatus(string Key)
        {
            if (Statusses.ContainsKey(Key))
            {
                Statusses.Remove(Key);
            }
        }

        public void ApplyEffect(int effectID)
        {
            if (IsBot)
            {
                this.mRoom.SendMessage(new AvatarEffectComposer(VirtualId, effectID));
                return;
            }

            if (IsBot || GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().Effects() == null)
                return;

            GetClient().GetHabbo().Effects().ApplyEffect(effectID);
        }

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y--;
                }
                else if (RotBody == 2)
                {
                    Sq.X++;
                }
                else if (RotBody == 4)
                {
                    Sq.Y++;
                }
                else if (RotBody == 6)
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.Y++;
                }
                else if (RotBody == 2)
                {
                    Sq.X--;
                }
                else if (RotBody == 4)
                {
                    Sq.Y--;
                }
                else if (RotBody == 6)
                {
                    Sq.X++;
                }
                else
                {
                    Sq.X--;
                }

                return Sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X++;
                }
                else if (RotBody == 2)
                {
                    Sq.Y--;
                }
                else if (RotBody == 4)
                {
                    Sq.X--;
                }
                else if (RotBody == 6)
                {
                    Sq.Y++;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(this.X, this.Y);

                if (RotBody == 0)
                {
                    Sq.X--;
                }
                else if (RotBody == 2)
                {
                    Sq.Y++;
                }
                else if (RotBody == 4)
                {
                    Sq.X++;
                }
                else if (RotBody == 6)
                {
                    Sq.Y--;
                }
                return Sq;
            }
        }


        public GameClient GetClient()
        {
            if (IsBot)
            {
                return null;
            }
            if (mClient == null)
                mClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(HabboId);
            return mClient;
        }

        public Room GetRoom()
        {
            if (mRoom == null)
                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(RoomId, out mRoom))
                    return mRoom;

            return mRoom;
        }
    }

    public enum ItemEffectType
    {
        NONE,
        SWIM,
        SwimLow,
        SwimHalloween,
        Iceskates,
        Normalskates,
        PublicPool,
        //Skateboard?
    }

    public static class ByteToItemEffectEnum
    {
        public static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.NONE;
                case 1:
                    return ItemEffectType.SWIM;
                case 2:
                    return ItemEffectType.Normalskates;
                case 3:
                    return ItemEffectType.Iceskates;
                case 4:
                    return ItemEffectType.SwimLow;
                case 5:
                    return ItemEffectType.SwimHalloween;
                case 6:
                    return ItemEffectType.PublicPool;
                //case 7:
                //return ItemEffectType.Custom;
                default:
                    return ItemEffectType.NONE;
            }
        }
    }

    //0 = none
    //1 = pool
    //2 = normal skates
    //3 = ice skates
}