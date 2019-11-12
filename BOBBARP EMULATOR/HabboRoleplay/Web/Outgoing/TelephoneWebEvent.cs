using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using System.Data;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class TelephoneWebEvent : IWebEvent
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
                // ACTION
                #region show
                case "show":
                    {
                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        Socket.Send("telephone;show");
                    }
                    break;
                #endregion
                #region hide
                case "hide":
                    {
                        Socket.Send("telephone;hide");
                    }
                    break;
                #endregion
                #region open
                case "open":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if(Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        User.UnIdle();
                        if(Client.GetHabbo().TelephoneEteint == true)
                        {
                            Client.GetHabbo().TelephoneEteint = false;
                            User.OnChat(User.LastBubble, "* Sort son  " + Client.GetHabbo().TelephoneName + " et l'allume *", true);
                            Client.GetHabbo().Effects().ApplyEffect(65);
                            return;
                        }

                        User.OnChat(User.LastBubble, "* Sort son " + Client.GetHabbo().TelephoneName + " de sa poche *", true);
                        if (Client.GetHabbo().Conduit == null && Client.GetHabbo().ArmeEquiped == null && User.Immunised == false)
                        {
                            Client.GetHabbo().Effects().ApplyEffect(65);
                        }
                    }
                    break;
                #endregion
                #region eteindre
                case "eteindre":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if (Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().inCallWithUsername != null)
                        {
                            Client.SendWhisper("Veuillez raccrocher pour pouvoir éteindre votre téléphone.");
                            return;
                        }

                        User.UnIdle();
                        if (Client.GetHabbo().Effects().CurrentEffect == 65)
                        {
                            Client.GetHabbo().resetEffectEvent();
                        }
                        Client.GetHabbo().TelephoneEteint = true;
                        User.OnChat(User.LastBubble, "* Éteint son " + Client.GetHabbo().TelephoneName + " et le range dans sa poche *", true);
                    }
                    break;
                #endregion
                #region close
                case "close":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        User.UnIdle();
                        if (Client.GetHabbo().Effects().CurrentEffect == 65)
                        {
                            Client.GetHabbo().resetEffectEvent();
                        }
                        User.OnChat(User.LastBubble, "* Range son " + Client.GetHabbo().TelephoneName + " dans sa poche *", true);
                    }
                    break;
                #endregion

                // APPLICATION
                #region banque
                case "banque":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if(Client.GetHabbo().TelephoneForfaitReset < DateTime.Now)
                        {
                            Client.SendWhisper("Vous devez renouveller votre forfait pour pouvoir ouvrir cette application.");
                            return;
                        }

                        if (Room.Reseau <= 1)
                        {
                            Client.SendWhisper("Vous ne pouvez pas ouvrir cet application car vous ne captez pas de 3G.");
                            return;
                        }

                        Socket.Send("telephone;banque;" + Client.GetHabbo().Banque);
                    }
                    break;
                #endregion
                #region youtube
                case "youtube":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if (Client.GetHabbo().TelephoneForfaitReset < DateTime.Now)
                        {
                            Client.SendWhisper("Vous devez renouveller votre forfait pour pouvoir ouvrir cet application.");
                            return;
                        }

                        if (Room.Reseau <= 1)
                        {
                            Client.SendWhisper("Vous ne pouvez pas ouvrir cet application car vous ne captez pas de 3G.");
                            return;
                        }

                        Socket.Send("telephone;youtube");
                    }
                    break;
                #endregion
                #region bouygues
                case "bouygues":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        string Message = "";
                        if (Client.GetHabbo().TelephoneForfaitType == 1)
                        {
                            Message = "Sensation Base";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 2)
                        {
                            Message = "Sensation";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 3)
                        {
                            Message = "Sensation Plus";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 4)
                        {
                            Message = "Sensation Max";

                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 5)
                        {
                            Message = "Teléphone jetable";
                        }

                        Socket.Send("telephone;bouygues;" + Convert.ToInt32(Client.GetHabbo().TelephoneForfait/60) + ";" + Client.GetHabbo().TelephoneForfaitSms + ";" + Message);
                    }
                    break;
                #endregion
                #region contacts
                case "contacts":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        Socket.Send("telephone;contacts");
                    }
                    break;
                #endregion

                // SYSTEM
                #region sms
                case "sms":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if(Client.GetHabbo().getCooldown("send_message"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                      

                             if (Client.GetHabbo().TelephoneForfaitType != 5 && Client.GetHabbo().TelephoneForfaitSms <= 0)
                        {
                            Client.SendWhisper("Vous n'avez plus de SMS sur votre téléphone jetable. Pour le recharger, achetez un nouveau téléphone.");
                            return;
                        }

                         if (Client.GetHabbo().TelephoneForfaitReset < DateTime.Now && Client.GetHabbo().TelephoneForfaitType < 5)
                        {
                            Client.SendWhisper("Vous devez renouveller votre forfait pour pouvoir envoyer un SMS.");
                            return;
                        }

                        else if (Client.GetHabbo().TelephoneForfaitType == 5 && Client.GetHabbo().TelephoneForfaitSms <= 0)
                        {
                            Client.SendWhisper("Vous devez changer vôtre Téléphone jettable pour envoyer un SMS.");
                            return;
                        }

                        if (Client.GetHabbo().TelephoneForfaitSms <= 0 && !(Client.GetHabbo().TelephoneForfaitType == 5))
                        {
                            Client.SendWhisper("Vous ne pouvez plus envoyer d'SMS car vous avez consommé l'intégralité du montant accordé à votre forfait.");
                            return;
                        }
                        
                        if (Room.Reseau <= 1)
                        {
                            Client.SendWhisper("Vous ne pouvez pas envoyer d'SMS car vous n'avez pas de réseau.");
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string userId = ReceivedData[1];
                        int num;
                        if (!Int32.TryParse(userId, out num) || userId.StartsWith("0"))
                            return;

                        if (Convert.ToInt32(userId) == Client.GetHabbo().Id)
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `id` = @id");
                            dbClient.AddParameter("id", userId);
                            int countUser = dbClient.getInteger();
                            if (countUser == 0)
                                return;

                            dbClient.SetQuery("SELECT * FROM `users` WHERE `id` = @id;");
                            dbClient.AddParameter("id", userId);
                            DataTable userInfo = null;
                            userInfo = dbClient.getTable();

                            foreach (DataRow Row in userInfo.Rows)
                            {
                                if (Convert.ToDateTime(Row["telForfaitReset"]) < DateTime.Now)
                                {
                                    Client.SendWhisper("Vous ne pouvez pas envoyer un SMS à " + Row["username"] + " car il n'a aucun forfait actif.");
                                    return;
                                }

                                MessengerBuddy Buddy = null;
                                if (!Client.GetHabbo().GetMessenger().TryGetFriend(Convert.ToInt32(userId), out Buddy))
                                {
                                    Client.SendWhisper("Vous ne pouvez pas envoyer un SMS à " + Row["username"] + " car vous n'avez pas ou plus son numéro.");
                                    return;
                                }

                                string Message = CommandManager.MergeParamsByVirgule(ReceivedData, 2);
                                Message = Message.Trim();

                                if (string.IsNullOrWhiteSpace(Message))
                                {
                                    Client.SendWhisper("Veuillez rentrer un message.");
                                    return;
                                }

                                if(Message == "%/send_localisation/%" && Client.GetHabbo().InRoom)
                                {
                                    Message = "[" + Client.GetHabbo().CurrentRoomId + "] " + Client.GetHabbo().CurrentRoom.Name + ".";
                                    byte[] encodeToUtf8 = Encoding.Default.GetBytes(Message);
                                    Message = "Je suis actuellement à " + Encoding.UTF8.GetString(encodeToUtf8);
                                }

                                Client.GetHabbo().addCooldown("send_message", 3000);
                                dbClient.SetQuery("INSERT INTO `telephone_sms` (`user_id`, `for_user`, `message`, `date`) VALUES (@user_id, @for_id, @msg, NOW())");
                                dbClient.AddParameter("user_id", Client.GetHabbo().Id);
                                dbClient.AddParameter("for_id", userId);
                                dbClient.AddParameter("msg", Message);
                                dbClient.RunQuery();

                                Client.GetHabbo().TelephoneForfaitSms -= 1;
                                Client.GetHabbo().updateTelephoneForfaitSms();
                                Socket.Send("telephone;reload_sms;" + userId);
                                User.UnIdle();
                                User.OnChat(User.LastBubble, "* Envoie un SMS à " + Row["username"] + " *", true);
                                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Convert.ToString(Row["username"]));
                                if(TargetClient != null && TargetClient.GetHabbo() != null && TargetClient.GetHabbo().Hopital == 0 && TargetClient.GetHabbo().Prison == 0 && TargetClient.GetHabbo().Telephone == 1 && TargetClient.GetHabbo().TelephoneEteint == false)
                                {
                                    Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                                    RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;receive_sms;" + Client.GetHabbo().Id);
                                    TargetUser.OnChat(TargetUser.LastBubble, "* Reçoit un SMS de " + Client.GetHabbo().Username + " *", true);
                                }
                            }
                        }
                    }
                    
                    break;
                #endregion
                #region appel_user
                case "appel_user":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if (Client.GetHabbo().TelephoneForfaitType == 5 && Client.GetHabbo().TelephoneForfait <= 0)
                        {
                            Client.SendWhisper("Vous n'avez plus de crédit téléphonique sur votre téléphone jetable. Pour le recharger, achetez un nouveau téléphone.");
                        }
                        if (Client.GetHabbo().inCallWithUsername != null)
                        {
                            Client.SendWhisper("Vous êtes déjà en appel, veuillez raccrocher pour pouvoir en passer un nouveau.");
                            return;
                        }

                        if (Client.GetHabbo().TelephoneForfaitReset < DateTime.Now && !(Client.GetHabbo().TelephoneForfaitType == 5))
                        {
                            Client.SendWhisper("Vous devez renouveller votre forfait pour pouvoir passer un appel.");
                            return;
                        }

                        if (Client.GetHabbo().TelephoneForfait <= 0 && !(Client.GetHabbo().TelephoneForfaitType == 5))
                        {
                            Client.SendWhisper("Vous n'avez plus de forfait téléphonique.");
                            
                        }
                        if (Client.GetHabbo().TelephoneForfait <= 0 && Client.GetHabbo().TelephoneForfaitType == 5)
                        {
                            Client.SendWhisper("Ton téléphone jetable à expiré... Pour passer un appel, achète un nouveau téléphone.");
                        }

                        if (Client.GetHabbo().isCalling == true)
                        {
                            Client.SendWhisper("Vous passez déjà un appel ou recevez un appel, veuillez patienter avant d'en passer un nouveau.");
                            return;
                        }
                        
                        if (Room.Reseau <= 1)
                        {
                            Client.SendWhisper("Vous ne pouvez pas passer d'appel car vous n'avez pas de réseau.");
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string Username = ReceivedData[1];
                        if(Username == null || Username == "")
                        {
                            Client.SendWhisper("Veuillez rentrer un pseudonyme.");
                            return;
                        }

                        if(Username == "17")
                        {
                            if (Client.GetHabbo().getCooldown("appelpolice"))
                            {
                                Client.SendWhisper("Vous devez patienter 10 minutes entre chaque appel de la Police Nationale.");
                                return;
                            }

                            string UsernamePolice = PlusEnvironment.GetGame().GetClientManager().appelPolice(Client.GetHabbo().Id);
                            if (UsernamePolice == null)
                            {
                                Client.SendWhisper("Aucun employé de la Police Nationale n'est disponible pour le moment.");
                                return;
                            }

                            Username = UsernamePolice;
                        }

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                        if (TargetClient == null || TargetClient.GetHabbo() == null)
                        {
                            Client.SendWhisper("La personne que vous essayez d'appeler n'est pas dans la ville.");
                            return;
                        }

                        if (TargetClient.GetHabbo().Id == Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("Vous ne pouvez pas vous appelez vous même.");
                            return;
                        }

                        if (TargetClient.GetHabbo().Telephone == 0)
                        {
                            Client.SendWhisper("La personne que vous essayer d'appeler n'a pas de téléphone.");
                            return;
                        }

                        if (TargetClient.GetHabbo().TelephoneForfaitReset < DateTime.Now && ReceivedData[1] != "17")
                        {
                            Client.SendWhisper("La personne que vous essayer d'appeler n'a aucun contrat de forfait actif.");
                            return;
                        }

                        MessengerBuddy Buddy = null;
                        if (ReceivedData[1] != "17" && !Client.GetHabbo().GetMessenger().TryGetFriend(TargetClient.GetHabbo().Id, out Buddy))
                        {
                            Client.SendWhisper("Vous ne pouvez pas appeler " + TargetClient.GetHabbo().Username + " car vous n'avez pas son numéro.");
                            return;
                        }

                        Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                        RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

                        if (TargetUser.GetClient().GetHabbo().TelephoneEteint == true || TargetUser.GetClient().GetHabbo().Hopital == 1 || TargetUser.GetClient().GetHabbo().Prison != 0)
                        {
                            Client.SendWhisper("La personne que vous essayer d'appeler a son téléphone éteint.");
                            return;
                        }

                        if (TargetUser.GetClient().GetHabbo().receiveCallUsername != null || TargetUser.GetClient().GetHabbo().isCalling == true)
                        {
                            Client.SendWhisper("La personne que vous essayer d'appeler a déjà un appel entrant.");
                            return;
                        }

                        if (TargetUser.GetClient().GetHabbo().inCallWithUsername != null)
                        {
                            Client.SendWhisper("La personne que vous essayer d'appeler est déjà en appel.");
                            return;
                        }

                        if (TargetRoom.Reseau <= 1)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de réseau.");
                            return;
                        }

                        if (ReceivedData[1] == "17")
                        {
                            Client.GetHabbo().addCooldown("appelpolice", 600);
                            User.OnChat(User.LastBubble, "* Appelle la Police Nationale *", true);
                        }
                        else
                        {
                            User.OnChat(User.LastBubble, "* Appelle " + TargetClient.GetHabbo().Username + " *", true);
                        }
                        TargetUser.OnChat(TargetUser.LastBubble, "* Reçoit un appel de " + Client.GetHabbo().Username + " *", true);

                        Socket.Send("telephone;send_appel;" + TargetClient.GetHabbo().Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + TargetClient.GetHabbo().Look  + "&head_direction=3&gesture=sml&size=l&headonly=1");
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;receive_appel;" + Client.GetHabbo().Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + Client.GetHabbo().Look  + "&head_direction=3&gesture=sml&size=l&headonly=1");

                        Client.GetHabbo().isCalling = true;
                        TargetUser.GetClient().GetHabbo().receiveCallUsername = Client.GetHabbo().Username;
                        Random TokenRand = new Random();
                        int tokenNumber = TokenRand.Next(1600, 2894354);
                        string Token = "APPEL-" + tokenNumber;
                        Client.GetHabbo().callingToken = Token;

                        System.Timers.Timer timer1 = new System.Timers.Timer(15000);
                        timer1.Interval = 30000;
                        timer1.Elapsed += delegate
                        {
                            if (TargetUser.GetClient().GetHabbo().receiveCallUsername == Client.GetHabbo().Username && Client.GetHabbo().callingToken == Token)
                            {
                                Socket.Send("telephone;closeAppel");
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                                Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas répondu.");
                                TargetClient.SendMessage(new WhisperComposer(TargetUser.VirtualId, "Vous avez manqué un appel de " + Client.GetHabbo().Username, 0, 34));
                                Client.GetHabbo().isCalling = false;
                                TargetUser.GetClient().GetHabbo().receiveCallUsername = null;
                            }
                            timer1.Stop();
                        };
                        timer1.Start();
                    }
                    break;
                #endregion
                #region decrocher
                case "decrocher":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().receiveCallUsername == null)
                            return;

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetHabbo().receiveCallUsername);
                        if (TargetClient == null || TargetClient.GetHabbo() == null)
                            return;

                        Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                        RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

                        Client.GetHabbo().receiveCallUsername = null;
                        Client.GetHabbo().inCallWithUsername = TargetClient.GetHabbo().Username;
                        TargetUser.GetClient().GetHabbo().inCallWithUsername = Client.GetHabbo().Username;
                        TargetClient.GetHabbo().timeAppel = 0;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;isAppel");
                        Socket.Send("telephone;isAppel");
                    }
                    break;
                #endregion
                #region raccrocher
                case "raccrocher":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().receiveCallUsername == null && Client.GetHabbo().inCallWithUsername == null)
                            return;

                        if (Client.GetHabbo().receiveCallUsername != null)
                        {
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetHabbo().receiveCallUsername);
                            if (TargetClient != null)
                            {
                                Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                                RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                                TargetClient.SendWhisper(Client.GetHabbo().Username + " n'a pas décroché.");
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                                TargetUser.GetClient().GetHabbo().isCalling = false;
                            }
                        }
                        else if (Client.GetHabbo().inCallWithUsername != null)
                        {
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetHabbo().inCallWithUsername);
                            if (TargetClient != null)
                            {
                                Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                                RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                                TargetClient.SendWhisper(Client.GetHabbo().Username + " a raccroché.");
                                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                                TargetUser.GetClient().GetHabbo().isCalling = false;
                                TargetUser.GetClient().GetHabbo().inCallWithUsername = null;
                            }
                            User.OnChat(User.LastBubble, "* Raccroche à " + Client.GetHabbo().inCallWithUsername + " *", true);
                        }

                        Client.GetHabbo().isCalling = false;
                        Client.GetHabbo().receiveCallUsername = null;
                        Client.GetHabbo().inCallWithUsername = null;
                        Socket.Send("telephone;closeAppel");
                    }
                    break;
                #endregion
                #region closeAppel
                case "closeAppel":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0)
                            return;

                        if (Client.GetHabbo().receiveCallUsername == null && Client.GetHabbo().isCalling == false)
                            return;

                        Socket.Send("telephone;closeAppel");
                    }
                    break;
                #endregion
                #region delete
                case "delete":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if(Client.GetHabbo().getCooldown("delete_contact"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        int Amount;
                        string[] ReceivedData = Data.Split(',');
                        string userId = ReceivedData[1];
                        if (!int.TryParse(userId, out Amount) || Convert.ToInt32(userId) <= 0 || userId.StartsWith("0"))
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            return;
                        }

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (user_one_id = " + userId + " AND user_two_id = " + Client.GetHabbo().Id + ") OR (user_two_id = " + userId + " AND user_one_id = " + Client.GetHabbo().Id + ")");
                            int countUser = dbClient.getInteger();
                            if (countUser == 0)
                                return;

                            dbClient.SetQuery("SELECT * FROM `users` WHERE `id` = @id");
                            dbClient.AddParameter("id", userId);
                            DataRow userContactInfo = null;
                            userContactInfo = dbClient.getRow();

                            if (userContactInfo == null)
                                return;

                            Client.GetHabbo().addCooldown("delete_contact", 2000);
                            Client.GetHabbo().GetMessenger().DestroyFriendship(Convert.ToInt32(userId));
                            User.OnChat(User.LastBubble, "* Supprime " + userContactInfo["username"] + " de ses contacts *", true);
                        }
                        Socket.Send("telephone;contacts");
                    }
                    break;
                #endregion
                #region renouveler
                case "renouveler":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Telephone == 0 || Client.GetHabbo().TelephoneEteint == true)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if(Client.GetHabbo().getCooldown("forfait_renouvel"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().TelephoneForfaitReset > DateTime.Now)
                        {
                            Client.SendWhisper("Vous pourrez renouveller votre forfait à partir du " + Client.GetHabbo().TelephoneForfaitReset.Day + "/" + Client.GetHabbo().TelephoneForfaitReset.Month + "/" + Client.GetHabbo().TelephoneForfaitReset.Year + " à " + Client.GetHabbo().TelephoneForfaitReset.Hour + ":" + Client.GetHabbo().TelephoneForfaitReset.Minute + ".");
                            return;
                        }

                        if (User.isTradingItems)
                        {
                            Client.SendWhisper("Veuillez finir votre échange avant de renouveller votre forfait.");
                            return;
                        }

                        string Message = "";
                        if(Client.GetHabbo().TelephoneForfaitType == 1)
                        {
                            Message = "Sensation Base";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 2)
                        {
                            Message = "Sensation";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 3)
                        {
                            Message = "Sensation Plus";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 4)
                        {
                            Message = "Sensation Max";
                        }
                        else if (Client.GetHabbo().TelephoneForfaitType == 5)
                        {
                            Message = "Teléphone jetable";
                        }
                        Client.GetHabbo().addCooldown("forfait_renouvel", 20000);
                        User.Transaction = "forfait:" + PlusEnvironment.getNameOfItem(Message) + ":" + PlusEnvironment.getTypeOfItem(Message) + ":" + PlusEnvironment.getPriceOfItem(Message) + ":" + PlusEnvironment.getTaxeOfItem(Message);
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "transaction;Souhaitez-vous vraiment vous souscrire au forfait <b>" + PlusEnvironment.getNameOfItem(Message) + "</b> pour <b>" + PlusEnvironment.getPriceOfItem(Message) + " crédits</b> pendant une semaine dont <b>" + PlusEnvironment.getTaxeOfItem(Message) + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem(Message));
                    }
                    break;
                    #endregion
            }
        }
    }
}
