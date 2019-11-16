using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using log4net;

using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Users.Process
{
    sealed class ProcessComponent
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.Users.Process.ProcessComponent");

        /// <summary>
        /// Player to update, handle, change etc.
        /// </summary>
        private Habbo _player = null;

        /// <summary>
        /// ThreadPooled Timer.
        /// </summary>
        private Timer _timer = null;
        private Timer _workTimer = null;
        private Timer _blurTimer = null;
        private Timer _energyTimer = null;
        private Timer _hygieneTimer = null;
        private Timer _hopitalTimer = null;
        private Timer _prisonTimer = null;
        private Timer _captureTimer = null;
        private Timer _brulingTimer = null;
        private Timer _appelTimer = null;

        /// <summary>
        /// Prevents the timer from overlapping itself.
        /// </summary>
        private bool _timerRunning = false;

        /// <summary>
        /// Checks if the timer is lagging behind (server can't keep up).
        /// </summary>
        private bool _timerLagging = false;

        /// <summary>
        /// Enable/Disable the timer WITHOUT disabling the timer itself.
        /// </summary>
        private bool _disabled = false;

        /// <summary>
        /// Used for disposing the ProcessComponent safely.
        /// </summary>
        private AutoResetEvent _resetEvent = new AutoResetEvent(true);

        /// <summary>
        /// How often the timer should execute.
        /// </summary>
        private static int _runtimeInSec = 60;

        /// <summary>
        /// Default.
        /// </summary>
        public ProcessComponent()
        {
        }

        /// <summary>
        /// Initializes the ProcessComponent.
        /// </summary>
        /// <param name="Player">Player.</param>
        public bool Init(Habbo Player)
        {
            if (Player == null)
                return false;
            else if (this._player != null)
                return false;

            this._player = Player;
            this._timer = new Timer(new TimerCallback(Run), null, _runtimeInSec * 1000, _runtimeInSec * 1000);
            this._workTimer = new Timer(new TimerCallback(WorkTimer), null, 60000, 60000);
            this._hopitalTimer = new Timer(new TimerCallback(HopitalTimer), null, 60000, 60000);
            this._prisonTimer = new Timer(new TimerCallback(PrisonTimer), null, 60000, 60000);
            this._blurTimer = new Timer(new TimerCallback(BlurTimer), null, 60000, 60000);
            this._energyTimer = new Timer(new TimerCallback(EnergyTimer), null, 240000, 240000);
            this._hygieneTimer = new Timer(new TimerCallback(HygieneTimer), null, 300000, 300000);
            this._captureTimer = new Timer(new TimerCallback(CaptureTimer), null, 2000, 2000);
            this._brulingTimer = new Timer(new TimerCallback(BrulingTimer), null, 1000, 1000);
            this._appelTimer = new Timer(new TimerCallback(AppelTimer), null, 1000, 1000);
            return true;
        }

        public void WorkTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null)
                    return;

                if (this._player.Travaille == false)
                    return;

                if (this._player.Prison != 0 || this._player.Hopital == 1)
                    return;

                if (User.makeCreation == true)
                {
                    if (User.creationTimer <= 1)
                    {
                        User.makeCreation = false;
                        User.OnChat(User.LastBubble, "* Fini de réaliser " + User.CreationName + " *", true);
                        this._player.Creations += 1;
                        this._player.updateCreations();
                        if (this._player.Creations == 25)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 25 créations, vous recevrez désormais 2 crédits en plus sur votre salaire.");
                        }
                        else if (this._player.Creations == 50)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 50 créations, vous recevrez désormais 3 crédits en plus sur votre salaire.");
                        }
                        else if (this._player.Creations == 75)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 75 créations, vous recevrez désormais 5 crédits en plus sur votre salaire.");
                        }
                        else if (this._player.Creations == 100)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 100 créations, vous recevrez désormais 7 crédits en plus sur votre salaire.");
                        }
                        else if (this._player.Creations == 150)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 150 créations, vous recevrez désormais 10 crédits en plus sur votre salaire.");
                        }
                        else if (this._player.Creations == 200)
                        {
                            this._player.GetClient().SendWhisper("Vous avez réalisé 200 créations, vous recevrez désormais 15 crédits en plus sur votre salaire.");
                        }
                    }
                    else
                    {
                        User.creationTimer -= 1;
                        this._player.GetClient().SendWhisper("Il vous reste " + User.creationTimer + " minute(s) avant de finir votre création.");
                    }
                }

                if (this._player.Timer <= 1)
                {
                    int Salaire = this._player.RankInfo.Salaire;
                    if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 199)
                    {
                        Salaire += 15;
                    }
                    else if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 149)
                    {
                        Salaire += 10;
                    }
                    else if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 99)
                    {
                        Salaire += 7;
                    }
                    else if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 74)
                    {
                        Salaire += 5;
                    }
                    else if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 49)
                    {
                        Salaire += 3;
                    }
                    else if (this._player.TravailInfo.Usine == 1 && this._player.Creations > 24)
                    {
                        Salaire += 2;
                    }

                    if (this._player.TravailInfo.ChiffreAffaire < Salaire && this._player.TravailInfo.PayedByEtat == 0 && this._player.TravailInfo.Usine == 0)
                    {
                        this._player.GetClient().SendWhisper("Votre entreprise ne peut pas vous payer car elle est en faillite.");
                        this._player.stopWork();
                        return;
                    }

                    int TaxeValeur;
                    int Taxe;
                    int SalaireRecu;
                    decimal SalaireDecimal = Convert.ToDecimal(Salaire);
                    if (this._player.Confirmed == 1)
                    {
                        SalaireRecu = Convert.ToInt32((SalaireDecimal / 100m) * 95m);
                        Taxe = Convert.ToInt32((SalaireDecimal / 100m) * 5m);
                        TaxeValeur = 5;
                    }
                    else
                    {
                        SalaireRecu = Convert.ToInt32((SalaireDecimal / 100m) * 85m);
                        Taxe = Convert.ToInt32((SalaireDecimal / 100m) * 15m);
                        TaxeValeur = 15;
                    }

                    User.OnChat(User.LastBubble, "* Reçoit sa paye *", true);
                    this._player.GetClient().SendNotification("Vous avez reçu votre salaire de " + Salaire + " crédits, l'état vous a taxé de " + TaxeValeur + "% soit " + Taxe + " crédits. Le prochain vous sera versé dans 10 minutes.");
                    Group Gouvernement = null;
                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                    {
                        Gouvernement.ChiffreAffaire += Taxe;
                        Gouvernement.updateChiffre();
                    }
                    if (this._player.TravailInfo.Usine == 0)
                    {
                        if (this._player.TravailInfo.PayedByEtat == 0)
                        {
                            this._player.TravailInfo.ChiffreAffaire -= Salaire;
                            this._player.TravailInfo.updateChiffre();
                        }
                        else
                        {
                            Gouvernement.ChiffreAffaire -= Salaire;
                            Gouvernement.updateChiffre();
                        }
                    }
                    this._player.Credits += SalaireRecu;
                    this._player.GetClient().SendMessage(new CreditBalanceComposer(this._player.Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "my_stats;" + this._player.GetClient().GetHabbo().Credits + ";" + this._player.GetClient().GetHabbo().Duckets + ";" + this._player.GetClient().GetHabbo().EventPoints);
                    this._player.Timer = 10;
                    this._player.updateTimer();
                    return;
                }
                else
                {
                    this._player.Timer = this._player.Timer - 1;
                    this._player.updateTimer();

                    if (this._player.Timer == 6)
                    {
                        Random trashRecompense = new Random();
                        int number1 = trashRecompense.Next(1, 10);
                        int number2 = trashRecompense.Next(1, 10);
                        this._player.AntiAFK = number1 + number2;
                        this._player.AntiAFKAttempt = 0;
                        this._player.GetClient().SendWhisper("[ANTI AFK] Combien font " + number1 + " + " + number2 + " ?");
                    }
                    else if (this._player.Timer == 5 && this._player.AntiAFK != 0)
                    {
                        this._player.GetClient().SendWhisper("[ANTI AFK] Il vous reste 1 minute pour répondre à l'anti AFK.");
                    }
                    else if (this._player.Timer == 4 && this._player.AntiAFK != 0)
                    {
                        this._player.Timer = 7;
                        this._player.updateTimer();
                        this._player.AntiAFK = 0;
                        this._player.GetClient().SendWhisper("[ANTI AFK] Vous n'avez pas répondu à l'anti AFK.");
                        this._player.stopWork();
                        return;
                    }

                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `worktime` FROM `users` WHERE id = @id LIMIT 1");
                        dbClient.AddParameter("id", this._player.Id);
                        string sworktime = dbClient.getString();
                        int worktime = Convert.ToInt32(sworktime);

                        dbClient.SetQuery("UPDATE users SET worktime = @wrktme WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("wrktme", (worktime + 1));
                        dbClient.AddParameter("id", this._player.Id);
                        dbClient.RunQuery();
                    }
                    this._player.GetClient().SendWhisper("Il vous reste " + this._player.Timer + " minute(s) avant de recevoir votre paye.");
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[WorkTimer] Une erreur est survenue.");
                Console.WriteLine(e.ToString());
            }
        }
        public void HopitalTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                if (this._player.Hopital == 0)
                    return;

                if (this._player.Timer <= 1)
                {
                    this._player.Timer = 0;
                    this._player.updateTimer();
                    if (this._player.Energie == 0)
                    {
                        this._player.Energie = 25;
                        this._player.updateEnergie();
                    }
                    if (this._player.Sante == 0)
                    {
                        this._player.Sante = 40;
                        this._player.updateSante();
                    }
                    RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                    this._player.endHopital(User.GetClient(), 0);
                    return;
                }
                else
                {
                    this._player.Timer = this._player.Timer - 1;
                    this._player.updateTimer();
                    this._player.GetClient().SendWhisper("Il vous reste " + this._player.Timer + " minute(s) avant de pouvoir quitter l'hôpital.");
                    return;
                }
            }
            catch
            {
                Console.WriteLine("[HopitalTimer] Une erreur est survenue.");
            }
        }
        public void BlurTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null)
                    return;

                if (this._player.SmokeTimer > 0)
                {
                    this._player.SmokeTimer -= 1;
                    if (this._player.SmokeTimer == 0)
                    {
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "smoke;stop");
                    }
                }

                if (this._player.Blur == 0)
                    return;

                this._player.Blur -= 1;
                this._player.updateBlur();
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "blur;" + this._player.Blur);
            }
            catch
            {
                Console.WriteLine("[BlurTimer] Une erreur est survenue.");
            }
        }
        public void PrisonTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                if (this._player.Prison == 0)
                    return;

                if (this._player.Timer <= 1)
                {
                    this._player.EndPrison();
                    return;
                }
                else
                {
                    this._player.Timer = this._player.Timer - 1;
                    this._player.updateTimer();
                    this._player.GetClient().SendWhisper("Il vous reste " + this._player.Timer + " minute(s) avant de pouvoir quitter la prison.");
                    return;
                }
            }
            catch
            {
                Console.WriteLine("[PrisonTimer] Une erreur est survenue.");
            }
        }
        public void EnergyTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                if (this._player.Hopital == 1)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null || User.IsAsleep == true)
                    return;

                Random energieUsed = new Random();
                int energieUsedNumber;
                if (this._player.Travaille == true)
                {
                    energieUsedNumber = energieUsed.Next(1, 4);
                }
                else
                {
                    energieUsedNumber = energieUsed.Next(1, 3);
                }

                if (User.usernameCoiff == null)
                {
                    this._player.EnergieMalaise(energieUsedNumber);
                }

                if (this._player.Energie < 5)
                {
                    this._player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, "Attention, votre énergie est très faible !", 0, 34));
                }
                return;
            }
            catch
            {
                Console.WriteLine("[EnergyTimer] Une erreur est survenue.");
            }
        }
        public void HygieneTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                if (this._player.Hopital == 1)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null)
                    return;

                Random HygieneUsed = new Random();
                int HygieneNumber = HygieneUsed.Next(1, 3);
                if (this._player.Hygiene != 0)
                {
                    if (this._player.Hygiene <= 3)
                    {
                        this._player.Hygiene = 0;
                        this._player.updateHygiene();
                    }
                    else
                    {
                        this._player.Hygiene -= HygieneNumber;
                        this._player.updateHygiene();
                    }
                }
                else
                {
                    if (this._player.Sante <= 3 && User.usernameCoiff == null && this._player.Prison == 0)
                    {
                        this._player.MaladieHygiene();
                    }
                    else if (this._player.Sante > 3)
                    {
                        this._player.Sante -= HygieneNumber;
                        this._player.updateSante();
                    }
                }

                return;
            }
            catch
            {
                Console.WriteLine("[HygieneTimer] Une erreur est survenue.");
            }
        }
        public void CaptureTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null || User.Capture == 0 || this._player.Gang == 0)
                    return;

                if (User.CaptureProgress >= 98)
                {
                    User.Capture = 0;
                    this._player.CurrentRoom.Capture = this._player.Gang;
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE rooms SET capture = @capture WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("capture", this._player.Gang);
                        dbClient.AddParameter("id", this._player.CurrentRoom.Id);
                        dbClient.RunQuery();
                    }
                    this._player.updateXpGang(50);
                    User.OnChat(User.LastBubble, "* Fini de capturer cette zone *", true);
                    PlusEnvironment.GetGame().GetClientManager().sendGangMsg(this._player.Gang, this._player.Username + " a capturé la zone : [" + this._player.CurrentRoom.Id + "] " + this._player.CurrentRoom.Name + " .");
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "capture;hide");
                    return;
                }
                else
                {
                    User.CaptureProgress = User.CaptureProgress + 2;
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "capture;update;" + User.CaptureProgress);
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[CaptureTimer] Une erreur est survenue.");
            }
        }
        public void BrulingTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null || User.isBruling == false || User.IsBot || this._player.Hopital != 0 || User.Immunised == true)
                    return;

                if (this._player.Sante > 5)
                {
                    this._player.Sante -= 5;
                    this._player.updateSante();
                    this._player.GetClient().SendWhisper("Vous êtes en train de perdre de la vie [" + this._player.Sante + "/100]");
                }
                else
                {
                    User.OnChat(User.LastBubble, "* Meurt brûlé *", true);

                    string Username = null;
                    foreach (Item item in this._player.CurrentRoom.GetRoomItemHandler().GetFloor)
                    {
                        if (item.Id == User.isBrulingItem)
                        {
                            Username = item.Username;
                            break;
                        }
                    }

                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);

                    if (TargetClient == null)
                    {
                        User.GetClient().SendNotification("Vous êtes mort suite à des brûlure d'un cocktail molotov, les ambulanciers ont réussi à vous réanimer. Vous pourrez sortir dans 10 minutes.");
                    }
                    else if (TargetClient.GetHabbo().Username == this._player.Username)
                    {
                        User.GetClient().SendNotification("Vous vous êtes suicidé avec votre cocktail molotov, les ambulanciers ont réussi à vous réanimer. Vous pourrez sortir dans 10 minutes.");
                    }
                    else
                    {
                        User.GetClient().SendNotification(TargetClient.GetHabbo().Username + " vous avez tué avec un cocktail molotov, les ambulanciers ont réussi à vous réanimer. Vous pourrez sortir dans 10 minutes.");
                        TargetClient.GetHabbo().Kills += 1;
                        TargetClient.GetHabbo().updateKill();
                        TargetClient.GetHabbo().updateGangKill();
                        TargetClient.GetHabbo().updateXpGang(10);
                        TargetClient.SendWhisper("Vous avez tué " + this._player.Username + " avec votre cocktail molotov.");
                    }

                    this._player.updateHopitalEtat(User, 10);
                    this._player.Sante = 0;
                    this._player.updateSante();
                    this._player.Morts += 1;
                    this._player.updateMort();
                    this._player.updateGangMort();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[BrulingTimer] Une erreur est survenue.");
            }
        }
        public void AppelTimer(object State)
        {
            try
            {
                if (this._player == null || !this._player.InRoom)
                    return;

                RoomUser User = this._player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(this._player.Username);
                if (User == null || this._player.isCalling == false || this._player.inCallWithUsername == null)
                    return;

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(this._player.inCallWithUsername);

                if (this._player.TelephoneForfait < 1)
                {
                    this._player.isCalling = false;
                    if (TargetClient != null)
                    {
                        TargetClient.GetHabbo().inCallWithUsername = null;
                        TargetClient.SendWhisper(this._player.Username + " n'a plus de forfait téléphonique.");
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                    }
                    this._player.GetClient().SendWhisper("Vous n'avez plus de forfait téléphonique.");
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "telephone;closeAppel");
                    this._player.inCallWithUsername = null;
                    this._player.TelephoneForfait = 0;
                    this._player.updateForfaitTel();
                    return;
                }

                this._player.timeAppel = this._player.timeAppel + 1;
                int totalSeconds = this._player.timeAppel;
                int seconds = totalSeconds % 60;
                int minutes = totalSeconds / 60;
                string time = minutes.ToString("D2") + ":" + seconds.ToString("D2");
                if (TargetClient != null)
                {
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;timeAppel;" + time);
                }
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(this._player.GetClient(), "telephone;timeAppel;" + time);
                this._player.TelephoneForfait = this._player.TelephoneForfait - 1;
                this._player.updateForfaitTel();
                return;

            }
            catch (Exception e)
            {
                Console.WriteLine("[AppelTimer] Une erreur est survenue.");
            }
        }
        /// <summary>
        /// Called for each time the timer ticks.
        /// </summary>
        /// <param name="State"></param>
        public void Run(object State)
        {
            try
            {
                if (this._disabled)
                    return;

                if (this._timerRunning)
                {
                    this._timerLagging = true;
                    log.Warn("<Player " + this._player.Id + "> Server can't keep up, Player timer is lagging behind.");
                    return;
                }

                this._resetEvent.Reset();

                // BEGIN CODE

                #region Muted Checks
                if (this._player.TimeMuted > 0)
                    this._player.TimeMuted -= 60;
                #endregion

                #region Console Checks
                if (this._player.MessengerSpamTime > 0)
                    this._player.MessengerSpamTime -= 60;
                if (this._player.MessengerSpamTime <= 0)
                    this._player.MessengerSpamCount = 0;
                #endregion

                this._player.TimeAFK += 1;

                #region Respect checking
                if (this._player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
                {
                    this._player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '" + (this._player.Rank == 1 && this._player.VIPRank == 0 ? 10 : this._player.VIPRank == 1 ? 15 : 20) + "', `dailyPetRespectPoints` = '" + (this._player.Rank == 1 && this._player.VIPRank == 0 ? 10 : this._player.VIPRank == 1 ? 15 : 20) + "', `respectsTimestamp` = '" + DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + this._player.Id + "' LIMIT 1");
                    }

                    this._player.GetStats().DailyRespectPoints = (this._player.Rank == 1 && this._player.VIPRank == 0 ? 10 : this._player.VIPRank == 1 ? 15 : 20);
                    this._player.GetStats().DailyPetRespectPoints = (this._player.Rank == 1 && this._player.VIPRank == 0 ? 10 : this._player.VIPRank == 1 ? 15 : 20);

                    if (this._player.GetClient() != null)
                    {
                        this._player.GetClient().SendMessage(new UserObjectComposer(this._player));
                    }
                }
                #endregion

                #region Reset Scripting Warnings
                if (this._player.GiftPurchasingWarnings < 15)
                    this._player.GiftPurchasingWarnings = 0;

                if (this._player.MottoUpdateWarnings < 15)
                    this._player.MottoUpdateWarnings = 0;

                if (this._player.ClothingUpdateWarnings < 15)
                    this._player.ClothingUpdateWarnings = 0;
                #endregion


                if (this._player.GetClient() != null)
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(this._player.GetClient(), "ACH_AllTimeHotelPresence", 1);

                // this._player.EnergieTimer();
                // this._player.TimerOneMinute();
                this._player.Effects().CheckEffectExpiry(this._player);

                // END CODE

                // Reset the values
                this._timerRunning = false;
                this._timerLagging = false;

                this._resetEvent.Set();
            }
            catch { }
        }

        /// <summary>
        /// Stops the timer and disposes everything.
        /// </summary>
        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                this._resetEvent.WaitOne(TimeSpan.FromMinutes(5));
            }
            catch { } // give up

            // Set the timer to disabled
            this._disabled = true;

            // Dispose the timer to disable it.
            try
            {
                if (this._timer != null)
                    this._timer.Dispose();

                if (this._workTimer != null)
                    this._timer.Dispose();

                if (this._blurTimer != null)
                    this._blurTimer.Dispose();

                if (this._hopitalTimer != null)
                    this._hopitalTimer.Dispose();

                if (this._prisonTimer != null)
                    this._prisonTimer.Dispose();

                if (this._energyTimer != null)
                    this._energyTimer.Dispose();

                if (this._hygieneTimer != null)
                    this._hygieneTimer.Dispose();

                if (this._captureTimer != null)
                    this._captureTimer.Dispose();

                if (this._brulingTimer != null)
                    this._brulingTimer.Dispose();

                if (this._appelTimer != null)
                    this._appelTimer.Dispose();
            }
            catch { }

            // Remove reference to the timers.
            this._timer = null;
            this._workTimer = null;
            this._blurTimer = null;
            this._hopitalTimer = null;
            this._prisonTimer = null;
            this._energyTimer = null;
            this._hygieneTimer = null;
            this._captureTimer = null;
            this._brulingTimer = null;
            this._appelTimer = null;

            // Null the player so we don't reference it here anymore
            this._player = null;
        }
    }
}