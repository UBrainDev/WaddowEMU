using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class TransactionWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;
            

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                #region accepter
                case "accepter":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.Transaction == null)
                        {
                            Client.SendWhisper("Vous n'avez aucune transaction en cours.");
                            return;
                        }
                        
                        #region gang
                        if (User.Transaction.StartsWith("gang"))
                        { 
                            if (Client.GetHabbo().Gang == 0)
                            {
                                string[] gangId = User.Transaction.Split(':');
                                Client.GetHabbo().Gang = Convert.ToInt32(gangId[1]);
                                Client.GetHabbo().updateGang();
                                Client.GetHabbo().GangRank = 1;
                                Client.GetHabbo().updateGangRank();
                                User.OnChat(User.LastBubble, "* Accepte de rejoindre le gang " + Client.GetHabbo().getNameOfGang() + " *", true);
                            }
                        }
                        #endregion
                        #region echange_items
                        if (User.Transaction.StartsWith("echange_items"))
                        {
                            string[] tradeInvitUser = User.Transaction.Split(':');

                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(tradeInvitUser[1]);
                            if(TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                            {
                                User.Transaction = null;
                                Client.SendWhisper("Impossible de lancer l'échange avec " + tradeInvitUser[1] + " car il n'est plus dans votre appartement.");
                                return;
                            }

                            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
                            {
                                User.Transaction = null;
                                Client.SendWhisper("Impossible de lancer l'échange avec " + tradeInvitUser[1] + " car il est trop loin de vous.");
                                return;
                            }

                            User.initItemsTrade(TargetUser);
                            TargetUser.initItemsTrade(User);
                            User.OnChat(User.LastBubble, "* Accepte d'échanger avec " + TargetClient.GetHabbo().Username + " *", true);
                        }
                        #endregion
                        #region duel
                        if (User.Transaction.StartsWith("duel"))
                        {
                            string[] duelData = User.Transaction.Split(':');
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(duelData[1]);
                            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                            {
                                Client.SendWhisper(duelData[1] + " n'est plus dans votre appartement.");
                            }
                            else
                            {
                                RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                                if (TargetUser.DuelUser != null)
                                {
                                    Client.SendWhisper(duelData[1] + " est déjà en duel.");
                                }
                                else
                                {
                                    Random TokenRand = new Random();
                                    int tokenNumber = TokenRand.Next(1600, 2894354);
                                    string Token = "DUEL-" + tokenNumber;

                                    TargetUser.DuelUser = Client.GetHabbo().Username;
                                    User.DuelUser = TargetClient.GetHabbo().Username;
                                    User.DuelToken = Token;
                                    User.OnChat(User.LastBubble, "* Accepte de prendre " + TargetClient.GetHabbo().Username + " en duel *", true);
                                    TargetClient.SendWhisper("Le duel contre " + Client.GetHabbo().Username + " a commencé, vous avez deux minutes pour le tuer.");
                                    Client.SendWhisper("Le duel contre " + TargetClient.GetHabbo().Username + " a commencé, vous avez deux minutes pour le tuer.");

                                    if (Client.GetHabbo().ArmeEquiped == "cocktail")
                                    {
                                        Client.GetHabbo().ArmeEquiped = null;
                                        Client.GetHabbo().resetEffectEvent();
                                    }

                                    if (TargetClient.GetHabbo().ArmeEquiped == "cocktail")
                                    {
                                        TargetClient.GetHabbo().ArmeEquiped = null;
                                        TargetClient.GetHabbo().resetEffectEvent();
                                    }

                                    System.Timers.Timer timer1 = new System.Timers.Timer(120000);
                                    timer1.Interval = 120000;
                                    timer1.Elapsed += delegate
                                    {
                                        if (User != null && User.DuelUser == duelData[1] && User.DuelToken == Token)
                                        {
                                            User.DuelUser = null;
                                            User.DuelToken = null;
                                            Client.SendWhisper("Le duel est terminé.");

                                            if (TargetUser != null)
                                            {
                                                TargetUser.DuelUser = null;
                                                TargetUser.GetClient().SendWhisper("Le duel est terminé.");
                                            }
                                        }

                                        if (User == null && TargetUser.DuelUser != null)
                                        {
                                            TargetUser.DuelUser = null;
                                            TargetUser.GetClient().SendWhisper("Le duel est terminé.");
                                        }
                                        timer1.Stop();
                                    };
                                    timer1.Start();
                                }
                            }
                        }
                        #endregion
                        #region soins
                        if (User.Transaction.StartsWith("soins"))
                        {
                            if (Client.GetHabbo().Hopital == 1)
                            {
                                User.wantSoin = true;
                                User.OnChat(User.LastBubble, "* Accepte de recevoir les soins *", true);
                            }
                            else
                            {
                                Client.SendWhisper("Vous n'êtes plus hospitalisé.");
                            }
                        }
                        #endregion
                        #region resilier_mutuelle
                        // Resilier Mutuelle
                        if (User.Transaction == "resilier_mutuelle")
                        {
                            Client.GetHabbo().Mutuelle = 0;
                            Client.GetHabbo().updateMutuelle();
                            User.OnChat(User.LastBubble, "* Accepte de résilier son contrat d'assurance mutuelle  *", true);
                        }
                        #endregion
                        #region renouveller_appart
                        if (User.Transaction.StartsWith("renouveller_appart"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] locationData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(locationData[2]))
                                    {
                                        Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(locationData[1]));
                                        if (TargetRoom == null)
                                        {
                                            Client.SendWhisper("Une erreur est survenue.");
                                        }
                                        else
                                        {
                                            if (TargetRoom.OwnerId != Client.GetHabbo().Id)
                                            {
                                                Client.SendWhisper("L'appartement n'est plus à vous.");
                                            }
                                            else
                                            {
                                                Client.GetHabbo().Banque -= Convert.ToInt32(locationData[2]);
                                                Client.GetHabbo().updateBanque();

                                                Group Orpi = null;
                                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(21, out Orpi))
                                                {
                                                    Orpi.ChiffreAffaire += Convert.ToInt32(locationData[2]) - Convert.ToInt32(locationData[3]);
                                                    Orpi.updateChiffre();
                                                }

                                                Group Gouvernement = null;
                                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                                {
                                                    Gouvernement.ChiffreAffaire += Convert.ToInt32(locationData[3]);
                                                    Gouvernement.updateChiffre();
                                                }

                                                DateTime Expiration = TargetRoom.Loyer_Date.AddDays(7);
                                                TargetRoom.locationRoom(Client, Expiration);
                                                User.OnChat(User.LastBubble, "* Renouvelle son contrat de location par carte bancaire pour l'appartement [" + TargetRoom.Id + "] " + TargetRoom.Name + " [-" + Convert.ToInt32(locationData[2]) + " CRÉDITS] *", true);
                                                Client.SendWhisper("Vous avez renouveler votre contrat de location jusqu'au " + Expiration.Day + "/" + Expiration.Month + ".");
                                            }
                                        }

                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(locationData[2]) + " crédits dans votre compte en banque pour renouveler le contrat de location."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(locationData[2]))
                                {
                                    Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(locationData[1]));
                                    if (TargetRoom == null)
                                    {
                                        Client.SendWhisper("Une erreur est survenue.");
                                    }
                                    else
                                    {
                                        if (TargetRoom.OwnerId != Client.GetHabbo().Id)
                                        {
                                            Client.SendWhisper("L'appartement n'est plus à vous.");
                                        }
                                        else
                                        {
                                            Client.GetHabbo().Credits -= Convert.ToInt32(locationData[2]);
                                            Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                            Group Orpi = null;
                                            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(21, out Orpi))
                                            {
                                                Orpi.ChiffreAffaire += Convert.ToInt32(locationData[2]) - Convert.ToInt32(locationData[3]);
                                                Orpi.updateChiffre();
                                            }

                                            Group Gouvernement = null;
                                            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                            {
                                                Gouvernement.ChiffreAffaire += Convert.ToInt32(locationData[3]);
                                                Gouvernement.updateChiffre();
                                            }

                                            DateTime Expiration = TargetRoom.Loyer_Date.AddDays(7);
                                            TargetRoom.locationRoom(Client, Expiration);
                                            User.OnChat(User.LastBubble, "* Renouvelle son contrat de location par carte bancaire pour l'appartement [" + TargetRoom.Id + "] " + TargetRoom.Name + " [-" + Convert.ToInt32(locationData[2]) + " CRÉDITS] *", true);
                                            Client.SendWhisper("Vous avez renouveler votre contrat de location jusqu'au " + Expiration.Day + "/" + Expiration.Month + ".");
                                        }
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(locationData[2]) + " crédits sur vous pour renouveler le contrat de location."));
                                }
                            }
                        }
                        #endregion
                        #region location
                        if (User.Transaction.StartsWith("location"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] locationData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(locationData[2]))
                                    {
                                        Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(locationData[1]));
                                        if (TargetRoom == null)
                                        {
                                            Client.SendWhisper("Une erreur est survenue.");
                                        }
                                        else
                                        {
                                            if (TargetRoom.Loyer == 0 || TargetRoom.OwnerId != 3)
                                            {
                                                Client.SendWhisper("L'appartement n'est plus à louer.");
                                            }
                                            else
                                            {
                                                Client.GetHabbo().Banque -= Convert.ToInt32(locationData[2]);
                                                Client.GetHabbo().updateBanque();

                                                Group Orpi = null;
                                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(21, out Orpi))
                                                {
                                                    Orpi.ChiffreAffaire += Convert.ToInt32(locationData[2]) - Convert.ToInt32(locationData[3]);
                                                    Orpi.updateChiffre();
                                                }

                                                Group Gouvernement = null;
                                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                                {
                                                    Gouvernement.ChiffreAffaire += Convert.ToInt32(locationData[3]);
                                                    Gouvernement.updateChiffre();
                                                }

                                                DateTime Expiration = DateTime.Now.AddDays(7);
                                                TargetRoom.locationRoom(Client, Expiration);
                                                User.OnChat(User.LastBubble, "* Fait un contrat de location d'une semaine par carte bancaire pour l'appartement [" + TargetRoom.Id + "] " + TargetRoom.Name + " [-" + Convert.ToInt32(locationData[2]) + " CRÉDITS] *", true);
                                            }
                                        }

                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(locationData[2]) + " crédits dans votre compte en banque pour payer le contrat de location."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(locationData[2]))
                                {
                                    Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(locationData[1]));
                                    if (TargetRoom == null)
                                    {
                                        Client.SendWhisper("Une erreur est survenue.");
                                    }
                                    else
                                    {
                                        if (TargetRoom.Loyer == 0 || TargetRoom.OwnerId != 3)
                                        {
                                            Client.SendWhisper("L'appartement n'est plus à louer.");
                                        }
                                        else
                                        {
                                            Client.GetHabbo().Credits -= Convert.ToInt32(locationData[2]);
                                            Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                            Group Orpi = null;
                                            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(21, out Orpi))
                                            {
                                                Orpi.ChiffreAffaire += Convert.ToInt32(locationData[2]) - Convert.ToInt32(locationData[3]);
                                                Orpi.updateChiffre();
                                            }

                                            Group Gouvernement = null;
                                            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                            {
                                                Gouvernement.ChiffreAffaire += Convert.ToInt32(locationData[3]);
                                                Gouvernement.updateChiffre();
                                            }

                                            DateTime Expiration = DateTime.Now.AddDays(7);
                                            TargetRoom.locationRoom(Client, Expiration);
                                            User.OnChat(User.LastBubble, "* Fait un contrat de location d'une semaine pour l'appartement [" + TargetRoom.Id + "] " + TargetRoom.Name + " [-" + Convert.ToInt32(locationData[2]) + " CRÉDITS] *", true);
                                        }
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(locationData[2]) + " crédits sur vous pour payer le contrat de location."));
                                }
                            }
                        }
                        #endregion
                        #region coiffure
                        if (User.Transaction.StartsWith("coiffure"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] coiffureData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(coiffureData[1]))
                                    {
                                        Group HairSalon = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(15, out HairSalon))
                                        {
                                            HairSalon.ChiffreAffaire += Convert.ToInt32(coiffureData[1]) - Convert.ToInt32(coiffureData[2]);
                                            HairSalon.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(coiffureData[2]);
                                            Gouvernement.updateChiffre();
                                        }
                                        Client.GetHabbo().Banque -= Convert.ToInt32(coiffureData[1]);
                                        Client.GetHabbo().updateBanque();
                                        Client.GetHabbo().Coiffure += 1;
                                        Client.GetHabbo().updatecoiffure();
                                        User.OnChat(User.LastBubble, "* Achète un bon de coiffure par carte bancaire [-" + Convert.ToInt32(coiffureData[1]) + " CRÉDITS] *", true);
                                       
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(coiffureData[1]) + " crédits dans votre compte en banque pour payer un bon de coiffure."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(coiffureData[1]))
                                {
                                    Group HairSalon = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(15, out HairSalon))
                                    {
                                        HairSalon.ChiffreAffaire += Convert.ToInt32(coiffureData[1]) - Convert.ToInt32(coiffureData[2]);
                                        HairSalon.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(coiffureData[2]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Credits -= Convert.ToInt32(coiffureData[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    Client.GetHabbo().Coiffure += 1;
                                    Client.GetHabbo().updatecoiffure();
                                    User.OnChat(User.LastBubble, "* Achète un bon de coiffure [-" + Convert.ToInt32(coiffureData[1]) + " CRÉDITS] *", true);
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(coiffureData[1]) + " crédits sur vous pour payer un bon de coiffure."));
                                }
                            }
                        }
                        #endregion
                        #region Téléphone
                        if (User.Transaction.StartsWith("telephone"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] telephoneData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(telephoneData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(telephoneData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète le " + telephoneData[1] + " avec sa carte bancaire [-" + telephoneData[2] + " CRÉDITS] *", true);
                                        Group Bougyues = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(12, out Bougyues))
                                        {
                                            Bougyues.ChiffreAffaire += Convert.ToInt32(telephoneData[2]) - Convert.ToInt32(telephoneData[3]);
                                            Bougyues.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(telephoneData[3]);
                                            Gouvernement.updateChiffre();
                                        }
                                        Client.GetHabbo().Telephone = 1;
                                        Client.GetHabbo().updateTelephone();
                                        Client.GetHabbo().TelephoneName = telephoneData[1];
                                        Client.GetHabbo().updateTelephoneName();
                                        Client.GetHabbo().TelephoneForfaitType = 1;
                                        Client.GetHabbo().updateForfaitType();
                                        Client.GetHabbo().TelephoneForfait = 600;
                                        Client.GetHabbo().updateForfaitTel();
                                        Client.GetHabbo().TelephoneForfaitSms = 50;
                                        Client.GetHabbo().updateForfaitSms();
                                        Client.GetHabbo().TelephoneForfaitReset = DateTime.Now.AddDays(7);
                                        Client.GetHabbo().updateForfaitReset();
                                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "telephone", "show");
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + telephoneData[2] + " crédits dans votre compte en banque pour payer le " + telephoneData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(telephoneData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(telephoneData[2]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète le " + telephoneData[1] + " [-" + telephoneData[2] + " CRÉDITS] *", true);
                                    Group Bougyues = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(12, out Bougyues))
                                    {
                                        Bougyues.ChiffreAffaire += Convert.ToInt32(telephoneData[2]) - Convert.ToInt32(telephoneData[3]);
                                        Bougyues.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(telephoneData[3]);
                                        Gouvernement.updateChiffre();
                                    }
                                    Client.GetHabbo().Telephone = 1;
                                    Client.GetHabbo().updateTelephone();
                                    Client.GetHabbo().TelephoneName = telephoneData[1];
                                    Client.GetHabbo().updateTelephoneName();
                                    Client.GetHabbo().TelephoneForfaitType = 1;
                                    Client.GetHabbo().updateForfaitType();
                                    Client.GetHabbo().TelephoneForfait = 600;
                                    Client.GetHabbo().updateForfaitTel();
                                    Client.GetHabbo().TelephoneForfaitSms = 50;
                                    Client.GetHabbo().updateForfaitSms();
                                    Client.GetHabbo().TelephoneForfaitReset = DateTime.Now.AddDays(7);
                                    Client.GetHabbo().updateForfaitReset();
                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "telephone", "show");
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + telephoneData[2] + " crédits sur vous pour payer le " + telephoneData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region GPS
                        if (User.Transaction.StartsWith("gps"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] gpsData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(gpsData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(gpsData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète le " + gpsData[1] + " avec sa carte bancaire [-" + gpsData[2] + " CRÉDITS] *", true);
                                        Group Quincaillerie = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(14, out Quincaillerie))
                                        {
                                            Quincaillerie.ChiffreAffaire += Convert.ToInt32(gpsData[2]) - Convert.ToInt32(gpsData[3]);
                                            Quincaillerie.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(gpsData[3]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().Gps = 1;
                                        Client.GetHabbo().updateGps();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + gpsData[2] + " crédits dans votre compte en banque pour payer le " + gpsData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(gpsData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(gpsData[2]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète le " + gpsData[1] + " [-" + gpsData[2] + " CRÉDITS] *", true);
                                    Group Quincaillerie = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(14, out Quincaillerie))
                                    {
                                        Quincaillerie.ChiffreAffaire += Convert.ToInt32(gpsData[2]) - Convert.ToInt32(gpsData[3]);
                                        Quincaillerie.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(gpsData[3]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Gps = 1;
                                    Client.GetHabbo().updateGps();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + gpsData[2] + " crédits sur vous pour payer le " + gpsData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Jetons
                        if (User.Transaction.StartsWith("jetons"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] jetonsData = User.Transaction.Split(':');

                            if (Client.GetHabbo().Credits >= Convert.ToInt32(jetonsData[2]))
                            {
                                Client.GetHabbo().Credits -= Convert.ToInt32(jetonsData[2]);
                                Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                User.OnChat(User.LastBubble, "* Achète " + jetonsData[1] + " jeton(s) [-" + jetonsData[2] + " CRÉDITS] *", true);
                                Group Casino = null;
                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(16, out Casino))
                                {
                                    Casino.ChiffreAffaire += Convert.ToInt32(jetonsData[2]);
                                    Casino.updateChiffre();
                                }

                                Client.GetHabbo().Casino_Jetons += Convert.ToInt32(jetonsData[1]);
                                Client.GetHabbo().updateCasinoJetons();
                            }
                            else
                            {
                                Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + jetonsData[2] + " crédits sur vous pour payer " + jetonsData[1] + " jeton(s)."));
                            }
                        }
                        #endregion
                        #region Sac
                        if (User.Transaction.StartsWith("sac"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] sacData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(sacData[3]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(sacData[3]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète le sac " + sacData[1] + " avec sa carte bancaire [-" + sacData[3] + " CRÉDITS] *", true);
                                        Group Quincaillerie = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(14, out Quincaillerie))
                                        {
                                            Quincaillerie.ChiffreAffaire += Convert.ToInt32(sacData[3]) - Convert.ToInt32(sacData[4]);
                                            Quincaillerie.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(sacData[4]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().Sac = Convert.ToInt32(sacData[2]);
                                        Client.GetHabbo().updateSac();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + sacData[3] + " crédits dans votre compte en banque pour payer le sac " + sacData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(sacData[3]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(sacData[3]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète le sac " + sacData[1] + " avec sa carte bancaire [-" + sacData[3] + " CRÉDITS] *", true);
                                    Group Quincaillerie = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(14, out Quincaillerie))
                                    {
                                        Quincaillerie.ChiffreAffaire += Convert.ToInt32(sacData[3]) - Convert.ToInt32(sacData[4]);
                                        Quincaillerie.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(sacData[4]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Sac = Convert.ToInt32(sacData[2]);
                                    Client.GetHabbo().updateSac();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + sacData[3] + " crédits sur vous pour payer le sac " + sacData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Arme
                        if (User.Transaction.StartsWith("arme"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] telephoneData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(telephoneData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(telephoneData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un(e) " + telephoneData[1] + " avec sa carte bancaire [-" + telephoneData[2] + " CRÉDITS] *", true);
                                        Group Armurerie = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                        {
                                            Armurerie.ChiffreAffaire += Convert.ToInt32(telephoneData[2]) - Convert.ToInt32(telephoneData[3]);
                                            Armurerie.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(telephoneData[3]);
                                            Gouvernement.updateChiffre();
                                        }

                                        if(telephoneData[1] == "Uzi")
                                        {
                                            Client.GetHabbo().Uzi = 1;
                                            Client.GetHabbo().updateUzi();
                                            Client.GetHabbo().Uzi_Munitions += 100;
                                            Client.GetHabbo().updateUziMunitions();
                                        }
                                        else if (telephoneData[1] == "Sabre")
                                        {
                                            Client.GetHabbo().Sabre = 1;
                                            Client.GetHabbo().updateSabre();
                                        }
                                        else if (telephoneData[1] == "Batte")
                                        {
                                            Client.GetHabbo().Batte = 1;
                                            Client.GetHabbo().updateBatte();
                                        }
                                        else if (telephoneData[1] == "Ak47")
                                        {
                                            Client.GetHabbo().Ak47 = 1;
                                            Client.GetHabbo().updateAk47();
                                            Client.GetHabbo().AK47_Munitions += 100;
                                            Client.GetHabbo().updateAK47Munitions();
                                        }
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + telephoneData[2] + " crédits dans votre compte en banque pour payer un(e) " + telephoneData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(telephoneData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(telephoneData[2]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète un(e) " + telephoneData[1] + " [-" + telephoneData[2] + " CRÉDITS] *", true);
                                    Group Armurerie = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                    {
                                        Armurerie.ChiffreAffaire += Convert.ToInt32(telephoneData[2]) - Convert.ToInt32(telephoneData[3]);
                                        Armurerie.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(telephoneData[3]);
                                        Gouvernement.updateChiffre();
                                    }

                                    if (telephoneData[1] == "Uzi")
                                    {
                                        Client.GetHabbo().Uzi = 1;
                                        Client.GetHabbo().updateUzi();
                                        Client.GetHabbo().Uzi_Munitions += 100;
                                        Client.GetHabbo().updateUziMunitions();
                                    }
                                    else if (telephoneData[1] == "Sabre")
                                    {
                                        Client.GetHabbo().Sabre = 1;
                                        Client.GetHabbo().updateSabre();
                                    }
                                    else if (telephoneData[1] == "Batte")
                                    {
                                        Client.GetHabbo().Batte = 1;
                                        Client.GetHabbo().updateBatte();
                                    }
                                    else if (telephoneData[1] == "Ak47")
                                    {
                                        Client.GetHabbo().Ak47 = 1;
                                        Client.GetHabbo().updateAk47();
                                        Client.GetHabbo().AK47_Munitions += 100;
                                        Client.GetHabbo().updateAK47Munitions();
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + telephoneData[2] + " crédits sur vous pour payer un(e) " + telephoneData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Voiture
                        if (User.Transaction.StartsWith("voiture"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] voitureData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(voitureData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(voitureData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un(e) " + voitureData[1] + " avec sa carte bancaire [-" + voitureData[2] + " CRÉDITS] *", true);
                                        Group Toyota = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(11, out Toyota))
                                        {
                                            Toyota.ChiffreAffaire += Convert.ToInt32(voitureData[2]) - Convert.ToInt32(voitureData[3]);
                                            Toyota.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(voitureData[3]);
                                            Gouvernement.updateChiffre();
                                        }

                                        if (voitureData[1] == "Audi A8")
                                        {
                                            Client.GetHabbo().AudiA8 = 1;
                                            Client.GetHabbo().updateAudiA8();
                                        }
                                        else if (voitureData[1] == "Porsche 911")
                                        {
                                            Client.GetHabbo().Porsche911 = 1;
                                            Client.GetHabbo().updatePorsche911();
                                        }
                                        else if (voitureData[1] == "Fiat Punto")
                                        {
                                            Client.GetHabbo().FiatPunto = 1;
                                            Client.GetHabbo().updateFiatPunto();
                                        }
                                        else if (voitureData[1] == "Volkswagen Jetta")
                                        {
                                            Client.GetHabbo().VolkswagenJetta = 1;
                                            Client.GetHabbo().updateVolkswagenJetta();
                                        }
                                        else if (voitureData[1] == "BMW i8")
                                        {
                                            Client.GetHabbo().BmwI8 = 1;
                                            Client.GetHabbo().updateBMWI8();
                                        }
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + voitureData[2] + " crédits dans votre compte en banque pour payer un(e) " + voitureData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(voitureData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(voitureData[2]);
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    User.OnChat(User.LastBubble, "* Achète un(e) " + voitureData[1] + " [-" + voitureData[2] + " CRÉDITS] *", true);
                                    Group Toyota = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(11, out Toyota))
                                    {
                                        Toyota.ChiffreAffaire += Convert.ToInt32(voitureData[2]) - Convert.ToInt32(voitureData[3]);
                                        Toyota.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(voitureData[3]);
                                        Gouvernement.updateChiffre();
                                    }

                                    if (voitureData[1] == "Audi A8")
                                    {
                                        Client.GetHabbo().AudiA8 = 1;
                                        Client.GetHabbo().updateAudiA8();
                                    }
                                    else if (voitureData[1] == "Porsche 911")
                                    {
                                        Client.GetHabbo().Porsche911 = 1;
                                        Client.GetHabbo().updatePorsche911();
                                    }
                                    else if (voitureData[1] == "Fiat Punto")
                                    {
                                        Client.GetHabbo().FiatPunto = 1;
                                        Client.GetHabbo().updateFiatPunto();
                                    }
                                    else if (voitureData[1] == "Volkswagen Jetta")
                                    {
                                        Client.GetHabbo().VolkswagenJetta = 1;
                                        Client.GetHabbo().updateVolkswagenJetta();
                                    }
                                    else if (voitureData[1] == "BMW i8")
                                    {
                                        Client.GetHabbo().BmwI8 = 1;
                                        Client.GetHabbo().updateBMWI8();
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + voitureData[2] + " crédits sur vous pour payer un(e) " + voitureData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Munitions
                        if (User.Transaction.StartsWith("munitions"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] munitionsData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(munitionsData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(munitionsData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète 100 munitions " + munitionsData[1] + " avec sa carte bancaire [-" + munitionsData[2] + " CRÉDITS] *", true);
                                        Group Armurerie = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                        {
                                            Armurerie.ChiffreAffaire += Convert.ToInt32(munitionsData[2]) - Convert.ToInt32(munitionsData[3]);
                                            Armurerie.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(munitionsData[3]);
                                            Gouvernement.updateChiffre();
                                        }

                                        if (munitionsData[1] == "pour Ak47")
                                        {
                                            Client.GetHabbo().AK47_Munitions += 100;
                                            Client.GetHabbo().updateAK47Munitions();
                                        }
                                        else if (munitionsData[1] == "pour Uzi")
                                        {
                                            Client.GetHabbo().Uzi_Munitions += 100;
                                            Client.GetHabbo().updateUziMunitions();
                                        }
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + munitionsData[2] + " crédits dans votre compte en banque pour payer 100 munitions " + munitionsData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(munitionsData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(munitionsData[2]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète 100 munitions " + munitionsData[1] + "  [-" + munitionsData[2] + " CRÉDITS] *", true);
                                    Group Armurerie = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                    {
                                        Armurerie.ChiffreAffaire += Convert.ToInt32(munitionsData[2]) - Convert.ToInt32(munitionsData[3]);
                                        Armurerie.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(munitionsData[3]);
                                        Gouvernement.updateChiffre();
                                    }

                                    if (munitionsData[1] == "pour Ak47")
                                    {
                                        Client.GetHabbo().AK47_Munitions += 100;
                                        Client.GetHabbo().updateAK47Munitions();
                                    }
                                    else if (munitionsData[1] == "pour Uzi")
                                    {
                                        Client.GetHabbo().Uzi_Munitions += 100;
                                        Client.GetHabbo().updateUziMunitions();
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + munitionsData[2] + " crédits sur vous pour payer 100 munitions " + munitionsData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Forfait
                        // FORFAIT
                        if (User.Transaction.StartsWith("forfait"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] forfaitData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(forfaitData[3]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(forfaitData[3]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Se souscrit pour une semaine au forfait " + forfaitData[1] + " [-" + forfaitData[3] + " CRÉDITS] *", true);

                                        Group Bougyues = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(12, out Bougyues))
                                        {
                                            Bougyues.ChiffreAffaire += Convert.ToInt32(forfaitData[3]) - Convert.ToInt32(forfaitData[4]);
                                            Bougyues.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(forfaitData[4]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().TelephoneForfaitType = Convert.ToInt32(forfaitData[2]);
                                        Client.GetHabbo().updateForfaitType();

                                        if (Convert.ToInt32(forfaitData[2]) == 1)
                                        {
                                            Client.GetHabbo().TelephoneForfait = 600;
                                            Client.GetHabbo().TelephoneForfaitSms = 50;
                                        }
                                        else if (Convert.ToInt32(forfaitData[2]) == 2)
                                        {
                                            Client.GetHabbo().TelephoneForfait = 1800;
                                            Client.GetHabbo().TelephoneForfaitSms = 300;
                                        }
                                        else if (Convert.ToInt32(forfaitData[2]) == 3)
                                        {
                                            Client.GetHabbo().TelephoneForfait = 3600;
                                            Client.GetHabbo().TelephoneForfaitSms = 600;
                                        }
                                        else if (Convert.ToInt32(forfaitData[2]) == 4)
                                        {
                                            Client.GetHabbo().TelephoneForfait = 7200;
                                            Client.GetHabbo().TelephoneForfaitSms = 1000;
                                        }
                                        Client.GetHabbo().updateForfaitTel();
                                        Client.GetHabbo().updateForfaitSms();
                                        Client.GetHabbo().TelephoneForfaitReset = DateTime.Now.AddDays(7);
                                        Client.GetHabbo().updateForfaitReset();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + forfaitData[3] + " crédits dans votre compte en banque pour payer le forfait " + forfaitData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(forfaitData[3]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(forfaitData[3]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Se souscrit pour une semaine au forfait " + forfaitData[1] + " [-" + forfaitData[3] + " CRÉDITS] *", true);

                                    Group Bougyues = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(12, out Bougyues))
                                    {
                                        Bougyues.ChiffreAffaire += Convert.ToInt32(forfaitData[3]) - Convert.ToInt32(forfaitData[4]);
                                        Bougyues.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(forfaitData[4]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().TelephoneForfaitType = Convert.ToInt32(forfaitData[2]);
                                    Client.GetHabbo().updateForfaitType();

                                    if (Convert.ToInt32(forfaitData[2]) == 1)
                                    {
                                        Client.GetHabbo().TelephoneForfait = 600;
                                        Client.GetHabbo().TelephoneForfaitSms = 50;
                                    }
                                    else if (Convert.ToInt32(forfaitData[2]) == 2)
                                    {
                                        Client.GetHabbo().TelephoneForfait = 1800;
                                        Client.GetHabbo().TelephoneForfaitSms = 300;
                                    }
                                    else if (Convert.ToInt32(forfaitData[2]) == 3)
                                    {
                                        Client.GetHabbo().TelephoneForfait = 3600;
                                        Client.GetHabbo().TelephoneForfaitSms = 600;
                                    }
                                    else if (Convert.ToInt32(forfaitData[2]) == 4)
                                    {
                                        Client.GetHabbo().TelephoneForfait = 7200;
                                        Client.GetHabbo().TelephoneForfaitSms = 1000;
                                    }
                                    Client.GetHabbo().updateForfaitTel();
                                    Client.GetHabbo().updateForfaitSms();
                                    Client.GetHabbo().TelephoneForfaitReset = DateTime.Now.AddDays(7);
                                    Client.GetHabbo().updateForfaitReset();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + forfaitData[3] + " crédits sur vous pour payer le forfait " + forfaitData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Mutuelle
                        if (User.Transaction.StartsWith("mutuelle"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] mutuelleData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(mutuelleData[3]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(mutuelleData[3]);
                                        Client.GetHabbo().updateBanque();
                                        if (Convert.ToInt32(mutuelleData[1]) == 1)
                                        {
                                            User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 25% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                        }
                                        else if (Convert.ToInt32(mutuelleData[1]) == 2)
                                        {
                                            User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 50% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                        }
                                        else if (Convert.ToInt32(mutuelleData[1]) == 3)
                                        {
                                            User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 75% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                        }
                                        else if (Convert.ToInt32(mutuelleData[1]) == 4)
                                        {
                                            User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 100% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                        }

                                        Group Mutuelle = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(6, out Mutuelle))
                                        {
                                            Mutuelle.ChiffreAffaire += Convert.ToInt32(mutuelleData[3]) - Convert.ToInt32(mutuelleData[4]);
                                            Mutuelle.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(mutuelleData[4]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().Mutuelle = Convert.ToInt32(mutuelleData[1]);
                                        Client.GetHabbo().updateMutuelle();
                                        Client.GetHabbo().MutuelleDate = DateTime.Now.AddDays(Convert.ToInt32(mutuelleData[2]));
                                        Client.GetHabbo().updateMutuelleDate();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + mutuelleData[3] + " crédits dans votre compte en banque pour payer le contrat."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(mutuelleData[3]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(mutuelleData[3]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    if (Convert.ToInt32(mutuelleData[1]) == 1)
                                    {
                                        User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 25% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                    }
                                    else if (Convert.ToInt32(mutuelleData[1]) == 2)
                                    {
                                        User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 50% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                    }
                                    else if (Convert.ToInt32(mutuelleData[1]) == 3)
                                    {
                                        User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 75% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                    }
                                    else if (Convert.ToInt32(mutuelleData[1]) == 4)
                                    {
                                        User.OnChat(User.LastBubble, "* Se souscrit à un contrat d'assurance maladie à un taux de 100% pour " + mutuelleData[2] + " jour(s) [-" + mutuelleData[3] + " CRÉDITS] *", true);
                                    }

                                    Group Mutuelle = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(6, out Mutuelle))
                                    {
                                        Mutuelle.ChiffreAffaire += Convert.ToInt32(mutuelleData[3]) - Convert.ToInt32(mutuelleData[4]);
                                        Mutuelle.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(mutuelleData[4]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Mutuelle = Convert.ToInt32(mutuelleData[1]);
                                    Client.GetHabbo().updateMutuelle();
                                    Client.GetHabbo().MutuelleDate = DateTime.Now.AddDays(Convert.ToInt32(mutuelleData[2]));
                                    Client.GetHabbo().updateMutuelleDate();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Convert.ToInt32(mutuelleData[3]) + " crédits sur vous pour payer le contrat."));
                                }
                            }
                        }
                        #endregion
                        #region Boisson
                        if (User.Transaction.StartsWith("boisson"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] BoissonData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(BoissonData[2]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(BoissonData[2]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un " + BoissonData[1] + " avec sa carte bancaire [-" + BoissonData[2] + " CRÉDITS] *", true);
                                        Group Cafe = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                        {
                                            Cafe.ChiffreAffaire += Convert.ToInt32(BoissonData[2]) - Convert.ToInt32(BoissonData[3]);
                                            Cafe.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(BoissonData[3]);
                                            Gouvernement.updateChiffre();
                                        }

                                        User.CarryItem(41);
                                        User.OnChat(User.LastBubble, "* Boit le " + BoissonData[1] + " [+30% ÉNERGIE] *", true);
                                        if (Client.GetHabbo().Energie >= 70)
                                        {
                                            Client.GetHabbo().Energie = 100;
                                            Client.GetHabbo().updateEnergie();

                                        }
                                        else
                                        {
                                            Client.GetHabbo().Energie += 30;
                                            Client.GetHabbo().updateEnergie();
                                        }
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + BoissonData[2] + " crédits dans votre compte en banque pour payer un " + BoissonData[1] + "."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(BoissonData[2]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(BoissonData[2]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                    User.OnChat(User.LastBubble, "* Achète un " + BoissonData[1] + " [-" + BoissonData[2] + " CRÉDITS] *", true);
                                    Group Cafe = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                    {
                                        Cafe.ChiffreAffaire += Convert.ToInt32(BoissonData[2]) - Convert.ToInt32(BoissonData[3]);
                                        Cafe.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(BoissonData[3]);
                                        Gouvernement.updateChiffre();
                                    }

                                    User.CarryItem(41);
                                    User.OnChat(User.LastBubble, "* Boit le " + BoissonData[1] + " [+30% ÉNERGIE] *", true);
                                    if (Client.GetHabbo().Energie >= 70)
                                    {
                                        Client.GetHabbo().Energie = 100;
                                        Client.GetHabbo().updateEnergie();

                                    }
                                    else
                                    {
                                        Client.GetHabbo().Energie += 30;
                                        Client.GetHabbo().updateEnergie();
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + BoissonData[2] + " crédits sur vous pour payer un " + BoissonData[1] + "."));
                                }
                            }
                        }
                        #endregion
                        #region Bingo
                        if (User.Transaction.StartsWith("bingo"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] BingoData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(BingoData[1]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(BingoData[1]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un ticket de Bingo avec sa carte bancaire [-" + BingoData[1] + " CRÉDITS] *", true);
                                        Group Cafe = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                        {
                                            Cafe.ChiffreAffaire += Convert.ToInt32(BingoData[1]) - Convert.ToInt32(BingoData[2]);
                                            Cafe.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(BingoData[2]);
                                            Gouvernement.updateChiffre();
                                        }

                                        User.OnChat(User.LastBubble, "* Gratte le ticket *", true);
                                        Random bingoGrattage = new Random();
                                        int bingoGrattageNumber = bingoGrattage.Next(1, 11);

                                        System.Timers.Timer timer2 = new System.Timers.Timer(5000);
                                        timer2.Interval = 5000;
                                        timer2.Elapsed += delegate
                                        {
                                            if (bingoGrattageNumber == 1 || bingoGrattageNumber == 3)
                                            {
                                                User.OnChat(User.LastBubble, "* Gagne 5 crédits sur le ticket de Bingo *", true);
                                                Client.GetHabbo().Credits += 5;
                                                Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                                Cafe.ChiffreAffaire -= 5;
                                                Cafe.updateChiffre();
                                            }
                                            else if (bingoGrattageNumber == 4)
                                            {
                                                User.OnChat(User.LastBubble, "* Gagne 15 crédits sur le ticket de Bingo *", true);
                                                Client.GetHabbo().Credits += 15;
                                                Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                                Cafe.ChiffreAffaire -= 15;
                                                Cafe.updateChiffre();
                                            }
                                            else if (bingoGrattageNumber == 5 || bingoGrattageNumber == 6)
                                            {
                                                User.OnChat(User.LastBubble, "* Gagne 10 crédits sur le ticket de Bingo *", true);
                                                Client.GetHabbo().Credits += 10;
                                                Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                                Cafe.ChiffreAffaire -= 10;
                                                Cafe.updateChiffre();
                                            }
                                            else
                                            {
                                                User.OnChat(User.LastBubble, "* Ne gagne rien *", true);
                                            }
                                            timer2.Stop();
                                        };
                                        timer2.Start();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + BingoData[1] + " crédits dans votre compte en banque pour payer le ticket de Bingo."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(BingoData[1]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(BingoData[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète un ticket de Bingo en espèces [-" + BingoData[1] + " CRÉDITS] *", true);
                                    Group Cafe = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                    {
                                        Cafe.ChiffreAffaire += Convert.ToInt32(BingoData[1]) - Convert.ToInt32(BingoData[2]);
                                        Cafe.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(BingoData[2]);
                                        Gouvernement.updateChiffre();
                                    }

                                    User.OnChat(User.LastBubble, "* Gratte le ticket *", true);
                                    Random bingoGrattage = new Random();
                                    int bingoGrattageNumber = bingoGrattage.Next(1, 11);

                                    System.Timers.Timer timer2 = new System.Timers.Timer(5000);
                                    timer2.Interval = 5000;
                                    timer2.Elapsed += delegate
                                    {
                                        if (bingoGrattageNumber == 1 || bingoGrattageNumber == 3)
                                        {
                                            User.OnChat(User.LastBubble, "* Gagne 5 crédits sur le ticket de Bingo *", true);
                                            Client.GetHabbo().Credits += 5;
                                            Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                            Cafe.ChiffreAffaire -= 5;
                                                Cafe.updateChiffre();
                                        }
                                        else if (bingoGrattageNumber == 4)
                                        {
                                            User.OnChat(User.LastBubble, "* Gagne 15 crédits sur le ticket de Bingo *", true);
                                            Client.GetHabbo().Credits += 15;
                                            Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                            Cafe.ChiffreAffaire -= 15;
                                                Cafe.updateChiffre();
                                        }
                                        else if (bingoGrattageNumber == 5 || bingoGrattageNumber == 6)
                                        {
                                            User.OnChat(User.LastBubble, "* Gagne 10 crédits sur le ticket de Bingo *", true);
                                            Client.GetHabbo().Credits += 10;
                                            Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                                            Cafe.ChiffreAffaire -= 10;
                                            Cafe.updateChiffre();
                                        }
                                        else
                                        {
                                            User.OnChat(User.LastBubble, "* Ne gagne rien *", true);
                                        }
                                        timer2.Stop();
                                    };
                                    timer2.Start();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + BingoData[1] + " crédits sur vous pour payer le ticket de Bingo."));
                                }
                            }
                        }
                        #endregion
                        #region Tabac
                        if (User.Transaction.StartsWith("tabac"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] TabacData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(TabacData[1]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(TabacData[1]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un paquet de Philip Morris avec sa carte bancaire [-" + TabacData[1] + " CRÉDITS] *", true);
                                        Group Cafe = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                        {
                                            Cafe.ChiffreAffaire += Convert.ToInt32(TabacData[1]) - Convert.ToInt32(TabacData[2]);
                                            Cafe.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(TabacData[2]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().PhilipMo += 20;
                                        Client.GetHabbo().updatePhilipMo();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + TabacData[1] + " crédits dans votre compte en banque pour payer un paquet de Philip Morris."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(TabacData[1]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(TabacData[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète un paquet de Philip Morris en espèces [-" + TabacData[1] + " CRÉDITS] *", true);
                                    Group Cafe = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                    {
                                        Cafe.ChiffreAffaire += Convert.ToInt32(TabacData[1]) - Convert.ToInt32(TabacData[2]);
                                        Cafe.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(TabacData[2]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().PhilipMo += 20;
                                    Client.GetHabbo().updatePhilipMo();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + TabacData[1] + " crédits sur vous pour payer un paquet de Philip Morris."));
                                }
                            }
                        }
                        #endregion
                        #region Clipper
                        if (User.Transaction.StartsWith("clipper"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] TabacData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(TabacData[1]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(TabacData[1]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un clipper par carte bancaire [-" + TabacData[1] + " CRÉDITS] *", true);
                                        Group Cafe = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                        {
                                            Cafe.ChiffreAffaire += Convert.ToInt32(TabacData[1]) - Convert.ToInt32(TabacData[2]);
                                            Cafe.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(TabacData[2]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().Clipper += 50;
                                        Client.GetHabbo().updateClipper();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + TabacData[1] + " crédits dans votre compte en banque pour acheter un clipper."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(TabacData[1]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(TabacData[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète un clipper en espèces [-" + TabacData[1] + " CRÉDITS] *", true);
                                    Group Cafe = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                    {
                                        Cafe.ChiffreAffaire += Convert.ToInt32(TabacData[1]) - Convert.ToInt32(TabacData[2]);
                                        Cafe.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(TabacData[2]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Clipper += 50;
                                    Client.GetHabbo().updateClipper();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + TabacData[1] + " crédits sur vous pour acheter un clipper."));
                                }
                            }
                        }
                        #endregion
                        #region Port d'armes
                        if (User.Transaction.StartsWith("portarme"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] PortArmeData = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(PortArmeData[1]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(PortArmeData[1]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* Achète un permis de port d'armes avec sa carte bancaire [-" + PortArmeData[1] + " CRÉDITS] *", true);
                                        Group Armurerie = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                        {
                                            Armurerie.ChiffreAffaire += Convert.ToInt32(PortArmeData[1]) - Convert.ToInt32(PortArmeData[2]);
                                            Armurerie.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(PortArmeData[2]);
                                            Gouvernement.updateChiffre();
                                        }

                                        Client.GetHabbo().Permis_arme = 1;
                                        Client.GetHabbo().updatePermisArme();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + PortArmeData[1] + " crédits dans votre compte en banque pour payer le permis de port d'armes."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(PortArmeData[1]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(PortArmeData[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* Achète un permis de port d'armes [-" + PortArmeData[1] + " CRÉDITS] *", true);
                                    Group Armurerie = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(17, out Armurerie))
                                    {
                                        Armurerie.ChiffreAffaire += Convert.ToInt32(PortArmeData[1]) - Convert.ToInt32(PortArmeData[2]);
                                        Armurerie.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(PortArmeData[2]);
                                        Gouvernement.updateChiffre();
                                    }

                                    Client.GetHabbo().Permis_arme = 1;
                                    Client.GetHabbo().updatePermisArme();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + PortArmeData[1] + " crédits sur vous pour payer le permis de port d'armes."));
                                }
                            }
                        }
                        #endregion
                        #region EuroRP
                        if (User.Transaction.StartsWith("eurorp"))
                        {
                            string[] ReceivedData = Data.Split(',');
                            string[] EuroRP = User.Transaction.Split(':');

                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Convert.ToInt32(EuroRP[1]))
                                    {
                                        Client.GetHabbo().Banque -= Convert.ToInt32(EuroRP[1]);
                                        Client.GetHabbo().updateBanque();
                                        User.OnChat(User.LastBubble, "* S'inscrit au tirage EuroRP par carte bancaire [-" + EuroRP[1] + " CRÉDITS] *", true);
                                        Group Cafe = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                        {
                                            Cafe.ChiffreAffaire += Convert.ToInt32(EuroRP[1]) - Convert.ToInt32(EuroRP[2]);
                                            Cafe.updateChiffre();
                                        }

                                        Group Gouvernement = null;
                                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                        {
                                            Gouvernement.ChiffreAffaire += Convert.ToInt32(EuroRP[2]);
                                            Gouvernement.updateChiffre();
                                        }

                                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                        {
                                            dbClient.RunQuery("INSERT INTO eurorp_participants (participant_id) VALUES ('" + Client.GetHabbo().Id + "')");
                                            dbClient.SetQuery("UPDATE `eurorp` SET `montant` = montant + @montant");
                                            dbClient.AddParameter("montant", Convert.ToInt32(EuroRP[1]) - Convert.ToInt32(EuroRP[2]));
                                            dbClient.RunQuery();
                                        }
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + EuroRP[1] + " crédits dans votre compte en banque pour vous inscrire au tirage EuroRP."));
                                    }
                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Convert.ToInt32(EuroRP[1]))
                                {
                                    Client.GetHabbo().Credits -= Convert.ToInt32(EuroRP[1]);
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    User.OnChat(User.LastBubble, "* S'inscrit au tirage EuroRP [-" + EuroRP[1] + " CRÉDITS] *", true);
                                    Group Cafe = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                                    {
                                        Cafe.ChiffreAffaire += Convert.ToInt32(EuroRP[1]) - Convert.ToInt32(EuroRP[2]);
                                        Cafe.updateChiffre();
                                    }

                                    Group Gouvernement = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                    {
                                        Gouvernement.ChiffreAffaire += Convert.ToInt32(EuroRP[2]);
                                        Gouvernement.updateChiffre();
                                    }
                                    
                                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                    {
                                        dbClient.RunQuery("INSERT INTO eurorp_participants (participant_id) VALUES ('" + Client.GetHabbo().Id + "')");
                                        dbClient.SetQuery("UPDATE `eurorp` SET `montant` = montant + @montant");
                                        dbClient.AddParameter("montant", Convert.ToInt32(EuroRP[1]) - Convert.ToInt32(EuroRP[2]));
                                        dbClient.RunQuery();
                                    }
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + EuroRP[1] + " crédits sur vous pour vous inscrire au tirage EuroRP."));
                                }
                            }
                        }
                        #endregion
                        #region panier
                        // PANIER
                        if (User.Transaction == "panier")
                        {
                            string[] ReceivedData = Data.Split(',');
                            if (ReceivedData[1] == "cb")
                            {
                                if (User.canUseCB == false)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'avez pas rentré le bon code.");
                                }
                                else
                                {
                                    if (Client.GetHabbo().Banque >= Client.GetHabbo().getPriceOfPanier())
                                    {
                                        Client.GetHabbo().Banque -= Client.GetHabbo().getPriceOfPanier();
                                        Client.GetHabbo().updateBanque();
                                        if (User.Purchase.Contains("savon") || User.Purchase.Contains("doliprane"))
                                        {
                                            User.OnChat(User.LastBubble, "* Paye le montant de sa commande par carte bancaire [-" + Client.GetHabbo().getPriceOfPanier() + " CRÉDITS] * ", true);
                                        }
                                        else
                                        {
                                            User.OnChat(User.LastBubble, "* Paye le montant du panier par carte bancaire [-" + Client.GetHabbo().getPriceOfPanier() + " CRÉDITS] * ", true);
                                        }
                                        Client.GetHabbo().validerPanier();
                                    }
                                    else
                                    {
                                        Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Client.GetHabbo().getPriceOfPanier() + " crédits dans votre compte en banque pour payer le montant."));
                                    }

                                    User.canUseCB = false;
                                }
                            }
                            else
                            {
                                if (Client.GetHabbo().Credits >= Client.GetHabbo().getPriceOfPanier())
                                {
                                    Client.GetHabbo().Credits -= Client.GetHabbo().getPriceOfPanier();
                                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                    if (User.Purchase.Contains("savon") || User.Purchase.Contains("doliprane"))
                                    {
                                        User.OnChat(User.LastBubble, "* Paye le montant de sa commande en espèces [-" + Client.GetHabbo().getPriceOfPanier() + " CRÉDITS] * ", true);
                                    }
                                    else
                                    {
                                        User.OnChat(User.LastBubble, "* Paye le montant de son panier en espèces [-" + Client.GetHabbo().getPriceOfPanier() + " CRÉDITS] * ", true);
                                    }
                                    Client.GetHabbo().validerPanier();
                                }
                                else
                                {
                                    Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut " + Client.GetHabbo().getPriceOfPanier() + " crédits sur vous pour payer le montant."));
                                }
                            }

                            if (Client.GetHabbo().Commande != null)
                            {
                                Client.GetHabbo().Commande = null;
                            }

                            User.Purchase = "";
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "panier", "send");
                        }
                        #endregion
                        #region Port d'armes
                        if (User.Transaction.StartsWith("ticketTheatre"))
                        {
                            if (Client.GetHabbo().Credits >= 10)
                            {
                                Client.GetHabbo().Credits -= 10;
                                Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                                User.OnChat(User.LastBubble, "* Achète un ticket d'entrée au théâtre [-10 CRÉDITS] *", true);
                                Group Gouvernement = null;
                                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                                {
                                    Gouvernement.ChiffreAffaire += 10;
                                    Gouvernement.updateChiffre();
                                }

                                User.HaveTicket = true;
                            }
                            else
                            {
                                Client.SendMessage(new RoomNotificationComposer("credits_warning", "message", "Il vous faut 10 crédits sur vous pour payer le ticket d'entrée au théâtre."));
                            }
                        }
                        #endregion

                        User.Transaction = null;
                        return;
                    }
                #endregion
                #region refuser
                case "refuser":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.Transaction == null)
                        {
                            Client.SendWhisper("Vous n'avez aucune transaction en cours.");
                            return;
                        }

                        if (User.Transaction.StartsWith("bingo"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le ticket de Bingo *", true);
                        }

                        if (User.Transaction.StartsWith("tabac"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le paquet de cigarettes *", true);
                        }

                        if (User.Transaction.StartsWith("clipper"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le clipper *", true);
                        }

                        if (User.Transaction.StartsWith("sac"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le ticket de sac *", true);
                        }

                        if (User.Transaction.StartsWith("telephone"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le téléphone *", true);
                        }

                        if (User.Transaction.StartsWith("gps"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le GPS *", true);
                        }

                        if (User.Transaction.StartsWith("portarme"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le permis de port d'armes *", true);
                        }

                        if (User.Transaction.StartsWith("arme"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter l'arme *", true);
                        }

                        if (User.Transaction.StartsWith("location"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse le contrat de location *", true);
                        }

                        if (User.Transaction.StartsWith("boisson"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter la boisson *", true);
                        }

                        if (User.Transaction.StartsWith("renouveller_appart"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse de renouveler son contrat de location *", true);
                        }

                        if (User.Transaction.StartsWith("voiture"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter la voiture *", true);
                        }

                        if (User.Transaction.StartsWith("munitions"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter les munitions *", true);
                        }

                        if (User.Transaction.StartsWith("echange_items"))
                        {
                            string[] tradeInvitUser = User.Transaction.Split(':');
                            User.OnChat(User.LastBubble, "* Refuse l'échange " + tradeInvitUser[1] + " *", true);
                        }

                        if (User.Transaction.StartsWith("forfait"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse de se souscrire au forfait *", true);
                        }

                        if (User.Transaction == "coiffure")
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter la coiffure *", true);
                        }

                        if (User.Transaction == "soins")
                        {
                            if (Client.GetHabbo().Hopital == 1)
                            {
                                User.OnChat(User.LastBubble, "* Refuse de recevoir les soins *", true);
                            }
                            else
                            {
                                Client.SendWhisper("Vous n'êtes plus hospitalisé.");
                            }

                            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                            {
                                foreach (Item item in Room.GetRoomItemHandler().GetFloor)
                                {
                                    if (item.GetBaseItem().SpriteId == 2945)
                                    {
                                        Room.GetGameMap().TeleportToItem(User, item);
                                        Room.GetRoomUserManager().UpdateUserStatusses();
                                    }
                                }
                            }
                        }

                        if (User.Transaction.StartsWith("eurorp"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse de s'inscrire au tirage EuroRP *", true);
                        }

                        if (User.Transaction.StartsWith("gang"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse de rejoindre le gang *", true);
                        }

                        if (User.Transaction == "panier")
                        {
                            User.Purchase = "";
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "panier", "send");
                            User.OnChat(User.LastBubble, "* Refuse de payer son panier *", true);
                        }

                        if (User.Transaction.StartsWith("mutuelle"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse le contrat de mutuelle *", true);
                        }

                        if (User.Transaction.StartsWith("duel"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse le duel *", true);
                        }

                        if (User.Transaction.StartsWith("jetons"))
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter les jetons *", true);
                        }

                        if (User.Transaction == "resilier_mutuelle")
                        {
                            User.OnChat(User.LastBubble, "* Refuse de résilier son contrat d'assurance mutuelle  *", true);
                        }

                        if (User.Transaction == "ticketTheatre")
                        {
                            User.OnChat(User.LastBubble, "* Refuse d'acheter le ticket d'entrée au théâtre *", true);
                        }


                        User.Transaction = null;
                        return;
                    }
                #endregion
                #region checkIfGetCB
                case "checkIfGetCB":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Cb == "null")
                        {
                            Client.SendWhisper("Vous ne pouvez pas payer par carte bancaire car vous n'en avez pas.");
                            return;
                        }

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "transaction_code;gotocode");
                        return;
                    }
                #endregion
                #region checkCode
                case "checkCode":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Cb == "null")
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == null || ReceivedData[1] != Client.GetHabbo().Cb)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "transaction_code;error");
                            return;
                        }

                        User.canUseCB = true;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "transaction_code;close");
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "transaction", "accepter,cb");
                        return;
                    }
                #endregion
            }
        }
    }
}
