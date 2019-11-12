using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using System.Text.RegularExpressions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using System.Data;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class GangWebEvent : IWebEvent
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

                #region toggle
                case "toggle":
                    {
                        Socket.Send("gang;toggle;" + Client.GetHabbo().Gang + ";" + Client.GetHabbo().getNameOfGang());
                    }
                    break;
                #endregion
                #region create
                case "create":
                    {
                        if (Client.GetHabbo().Gang != 0)
                            return;

                        if (Client.GetHabbo().Credits < 500)
                        {
                            Client.SendWhisper("Il vous faut 500 crédits pour créer votre gang.");
                            return;
                        }

                        int SplitData = Data.IndexOf(',');
                        string nameOFGang = Data.Substring(SplitData + 1);
                        nameOFGang = nameOFGang.Trim();

                        if (nameOFGang.Length < 3)
                        {
                            Client.SendWhisper("Le nom de votre gang doit faire minimum 3 caractères.");
                            return;
                        }

                        if (nameOFGang.Length > 20)
                        {
                            Client.SendWhisper("Le nom de votre gang doit faire maximum 20 caractères.");
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(nameOFGang))
                        {
                            Client.SendWhisper("Le nom de votre gang est invalide.");
                            return;
                        }

                        var withoutSpecial = new string(nameOFGang.Where(c => Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsWhiteSpace(c)).ToArray());
                        if (nameOFGang != withoutSpecial)
                        {
                            Client.SendWhisper("Le nom de votre gang contient des caractères non autorisés.");
                            return;
                        }

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `gang` WHERE `name` = @name");
                            dbClient.AddParameter("name", nameOFGang);
                            int GangCount = dbClient.getInteger();
                            if (GangCount > 0)
                            {
                                Client.SendWhisper("Le nom de votre gang est déjà utilisé.");
                                return;
                            }
                        }

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if(User.isTradingItems)
                        {
                            Client.SendWhisper("Vous ne pouvez pas créer de gang pendant que vous faites un échange.");
                            return;
                        }

                        Client.GetHabbo().Credits -= 500;
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                        Client.GetHabbo().createGang(nameOFGang);
                        User.OnChat(User.LastBubble, "* Forme le gang " + nameOFGang + " [-500 CRÉDITS] *", true);

                        Socket.Send("gang;goToGang;" + Client.GetHabbo().getNameOfGang());
                        break;
                    }
                #endregion
                #region promouvoir
                case "promouvoir":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().getCooldown("gang") == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if(Client.GetHabbo().GangRank == 1 || Client.GetHabbo().GangRank == 2)
                            return;

                        int SplitData = Data.IndexOf(',');
                        string Username = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Username))
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT id, gang, gang_rank FROM `users` WHERE username = @username LIMIT 1");
                            dbClient.AddParameter("username", Username);
                            DataRow getUser = dbClient.getRow();

                            if (getUser["id"] == null)
                                return;

                            if (Convert.ToInt32(getUser["id"]) == Client.GetHabbo().Id)
                                return;

                            if (Convert.ToInt32(getUser["gang"]) != Client.GetHabbo().Gang)
                                return;

                            if (Convert.ToInt32(getUser["gang_rank"]) == 3 && Client.GetHabbo().GangRank == 3 || Convert.ToInt32(getUser["gang_rank"]) == 4)
                                return;

                            Client.GetHabbo().addCooldown("gang", 2000);
                            int newRank = 0;
                            if(Convert.ToInt32(getUser["gang_rank"]) == 1)
                            {
                                User.OnChat(User.LastBubble, "* Promouvoit " + Username + " en tant que Membre de son gang *", true);
                                newRank = 2;
                            }
                            else if (Convert.ToInt32(getUser["gang_rank"]) == 2)
                            {
                                User.OnChat(User.LastBubble, "* Promouvoit " + Username + " en tant que Sous chef de son gang *", true);
                                newRank = 3;
                            }
                            else if (Convert.ToInt32(getUser["gang_rank"]) == 3)
                            {
                                User.OnChat(User.LastBubble, "* Promouvoit " + Username + " en tant que Chef de son gang *", true);
                                newRank = 4;
                            }

                            dbClient.SetQuery("UPDATE `users` SET `gang_rank` = @newrank WHERE `id` = @userId LIMIT 1");
                            dbClient.AddParameter("newrank", newRank);
                            dbClient.AddParameter("userId", Convert.ToInt32(getUser["id"]));
                            dbClient.RunQuery();

                            PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a promu " + Username + ".");
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                            if(TargetClient != null)
                            {
                                TargetClient.GetHabbo().GangRank = newRank;
                            }
                        }

                        Socket.Send("gang;goToGang;" + Client.GetHabbo().getNameOfGang());
                        break;
                    }
                #endregion
                #region retrograder
                case "retrograder":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().getCooldown("gang") == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if (Client.GetHabbo().GangRank == 1 || Client.GetHabbo().GangRank == 2)
                            return;

                        int SplitData = Data.IndexOf(',');
                        string Username = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Username))
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT id, gang, gang_rank FROM `users` WHERE username = @username LIMIT 1");
                            dbClient.AddParameter("username", Username);
                            DataRow getUser = dbClient.getRow();

                            if (getUser["id"] == null)
                                return;

                            if (Convert.ToInt32(getUser["id"]) == Client.GetHabbo().Id)
                                return;

                            if (Convert.ToInt32(getUser["gang"]) != Client.GetHabbo().Gang)
                                return;

                            if (Convert.ToInt32(getUser["gang_rank"]) == 4 && Client.GetHabbo().GangRank == 3 || Convert.ToInt32(getUser["gang_rank"]) == 1)
                                return;

                            if (Convert.ToInt32(getUser["gang_rank"]) == 4 && Client.GetHabbo().GangRank == 4 && Client.GetHabbo().getOwnerOfGang() != Client.GetHabbo().Id)
                                return;

                            Client.GetHabbo().addCooldown("gang", 2000);

                            int newRank = 0;
                            if (Convert.ToInt32(getUser["gang_rank"]) == 2)
                            {
                                User.OnChat(User.LastBubble, "* Rétrograde " + Username + " en tant que Arrivant de son gang *", true);
                                newRank = 1;
                            }
                            else if (Convert.ToInt32(getUser["gang_rank"]) == 3)
                            {
                                User.OnChat(User.LastBubble, "* Rétrograde " + Username + " en tant que Membre chef de son gang *", true);
                                newRank = 2;
                            }
                            else if (Convert.ToInt32(getUser["gang_rank"]) == 4)
                            {
                                User.OnChat(User.LastBubble, "* Rétrograde " + Username + " en tant que Sous chef de son gang *", true);
                                newRank = 3;
                            }

                            dbClient.SetQuery("UPDATE `users` SET `gang_rank` = @newrank WHERE `id` = @userId LIMIT 1");
                            dbClient.AddParameter("newrank", newRank);
                            dbClient.AddParameter("userId", Convert.ToInt32(getUser["id"]));
                            dbClient.RunQuery();

                            PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a rétrogradé " + Username + ".");
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                            if (TargetClient != null)
                            {
                                TargetClient.GetHabbo().GangRank = newRank;
                            }
                        }

                        Socket.Send("gang;goToGang;" + Client.GetHabbo().getNameOfGang());
                        break;
                    }
                #endregion
                #region virer
                case "virer":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if (Client.GetHabbo().GangRank == 1 || Client.GetHabbo().GangRank == 2 || Client.GetHabbo().GangRank == 3)
                            return;

                        int SplitData = Data.IndexOf(',');
                        string Username = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Username))
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT id, gang, gang_rank FROM `users` WHERE username = @username LIMIT 1");
                            dbClient.AddParameter("username", Username);
                            DataRow getUser = dbClient.getRow();

                            if (getUser["id"] == null)
                                return;

                            if (Convert.ToInt32(getUser["id"]) == Client.GetHabbo().Id)
                                return;

                            if (Convert.ToInt32(getUser["gang"]) != Client.GetHabbo().Gang)
                                return;

                            if (Convert.ToInt32(getUser["gang_rank"]) == 4 && Client.GetHabbo().GangRank == 4 && Client.GetHabbo().getOwnerOfGang() != Client.GetHabbo().Id)
                                return;

                            dbClient.SetQuery("UPDATE `users` SET `gang` = '0', `gang_rank` = '0' WHERE `id` = @userId LIMIT 1");
                            dbClient.AddParameter("userId", Convert.ToInt32(getUser["id"]));
                            dbClient.RunQuery();
                            User.OnChat(User.LastBubble, "* Vire " + Username + " de son gang *", true);

                            PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a viré " + Username + " du gang.");
                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                            if (TargetClient != null)
                            {
                                TargetClient.GetHabbo().Gang = 0;
                                TargetClient.GetHabbo().GangRank = 0;
                            }
                        }

                        Socket.Send("gang;goToGang;" + Client.GetHabbo().getNameOfGang());
                        break;
                    }
                #endregion
                #region owner
                case "owner":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if (Client.GetHabbo().getOwnerOfGang() != Client.GetHabbo().Id)
                            return;

                        int SplitData = Data.IndexOf(',');
                        string Username = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Username))
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT id, gang, gang_rank FROM `users` WHERE username = @username LIMIT 1");
                            dbClient.AddParameter("username", Username);
                            DataRow getUser = dbClient.getRow();

                            if (getUser["id"] == null)
                                return;

                            if (Convert.ToInt32(getUser["id"]) == Client.GetHabbo().Id)
                                return;

                            if (Convert.ToInt32(getUser["gang"]) != Client.GetHabbo().Gang)
                                return;

                            if (Convert.ToInt32(getUser["gang_rank"]) != 4)
                                return;

                            dbClient.SetQuery("UPDATE `gang` SET `owner` = @newOwner WHERE `id` = @gangId LIMIT 1");
                            dbClient.AddParameter("newOwner", Convert.ToInt32(getUser["id"]));
                            dbClient.AddParameter("gangId", Client.GetHabbo().Gang);
                            dbClient.RunQuery();
                            User.OnChat(User.LastBubble, "* Donne la propriété du gang à " + Username + " *", true);

                            PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a donné la propriété du gang à " + Username + ".");
                        }

                        Socket.Send("gang;goToGang;" + Client.GetHabbo().getNameOfGang());
                        break;
                    }
                #endregion
                #region inviter
                case "inviter":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().getCooldown("gang") == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }
                        
                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if (Client.GetHabbo().GangRank == 1)
                            return;

                        int SplitData = Data.IndexOf(',');
                        string Username = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Username))
                        {
                            Client.SendWhisper("Veuillez rentrer un pseudonyme.");
                            return;
                        }

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                        if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            Client.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                            return;
                        }

                        if (TargetClient.GetHabbo().Gang == Client.GetHabbo().Gang)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username + " est déjà dans votre gang.");
                            return;
                        }

                        if (Client.GetHabbo().CurrentRoomId == PlusEnvironment.Salade)
                        {
                            Client.SendWhisper("Vous ne pouvez pas inviter un civil dans votre gang pendant une salade.");
                            return;
                        }

                        if (TargetClient.GetHabbo().Gang != 0)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un gang.");
                            return;
                        }

                        if (Client.GetHabbo().countUserOfGang() > 24)
                        {
                            Client.SendWhisper("Votre gang a déjà un effectif de 25 membres.");
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser.Transaction != null)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une invitation en cours, veuillez patienter.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("gang", 2000);
                        PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a invité " + TargetClient.GetHabbo().Username + " dans le gang.");
                        User.OnChat(User.LastBubble, "* Invite " + TargetClient.GetHabbo().Username + " dans son gang *", true);
                        TargetUser.Transaction = "gang:" + Client.GetHabbo().Gang;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Client.GetHabbo().Username + "</b> vous invite à rejoindre son gang <b>" + Client.GetHabbo().getNameOfGang() + "</b>.;0");
                        break;
                    }
                #endregion
                #region leave
                case "leave":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Gang == 0)
                            return;

                        if (Client.GetHabbo().getOwnerOfGang() == Client.GetHabbo().Id && Client.GetHabbo().countUserOfGang() > 1)
                        {
                            Client.SendWhisper("Vous devez donner la propriété de votre gang à quelqu'un avant de pouvoir le quitter.");
                            return;
                        }

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            if(Client.GetHabbo().countUserOfGang() == 1)
                            {
                                dbClient.SetQuery("DELETE FROM `gang` WHERE `id` = @gang_id");
                                dbClient.AddParameter("gang_id", Client.GetHabbo().Gang);
                                dbClient.RunQuery();

                                dbClient.SetQuery("UPDATE rooms SET capture = 0 WHERE `capture` = @gang_capture");
                                dbClient.AddParameter("gang_capture", Client.GetHabbo().Gang);
                                dbClient.RunQuery();
                            }

                            PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Client.GetHabbo().Gang, Client.GetHabbo().Username + " a quitté le gang.");
                            Client.GetHabbo().Gang = 0;
                            Client.GetHabbo().updateGang();
                            Client.GetHabbo().GangRank = 0;
                            Client.GetHabbo().updateGangRank();
                            User.OnChat(User.LastBubble, "* Quitte son gang *", true);
                        }

                        Socket.Send("gang;toggle");
                        break;
                    }
                    #endregion
            }
        }
    }
}