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
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Groups;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class LookWebEvent : IWebEvent
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
                #region editLook
                case "editLook":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().EventCount != 0 && Client.GetHabbo().EventDay == 0)
                        {
                            Client.SendWhisper("Vous ne pouvez pas changer de look pendant que vous réalisez une mission.");
                            return;
                        }

                        if (Client.GetHabbo().Travaille == true || Client.GetHabbo().Prison != 0 || Client.GetHabbo().Hopital == 1)
                        {
                            Client.SendWhisper("Vous ne pouvez pas changer de look pendant que vous travaillez, vous êtes emprisonné ou hospitalisé.");
                            return;
                        }

                        if(Client.GetHabbo().getCooldown("changeLook"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string typeItem = ReceivedData[1];
                        string Code = ReceivedData[2];
                        string Color = ReceivedData[3];

                        if (string.IsNullOrWhiteSpace(typeItem) || Code == null || Color == null)
                            return;

                        int num;
                        if (!Int32.TryParse(Code, out num) || !Int32.TryParse(Color, out num))
                            return;
                        
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            if (Convert.ToInt32(Code) != 0 && typeItem != "hd")
                            {
                                dbClient.SetQuery("SELECT COUNT(0) FROM `looks_users` WHERE `user_id` = @userId AND code = @code AND color = @color");
                                dbClient.AddParameter("userId", Client.GetHabbo().Id);
                                dbClient.AddParameter("code", Code);
                                dbClient.AddParameter("color", Color);
                                int countItem = dbClient.getInteger();
                                if (countItem == 0)
                                    return;

                                dbClient.SetQuery("SELECT COUNT(0) FROM `looks` WHERE `type` = @typeItem AND code = @code AND invente = @invente");
                                dbClient.AddParameter("typeItem", typeItem);
                                dbClient.AddParameter("code", Code);
                                dbClient.AddParameter("invente", "1");
                                int countItemOffi = dbClient.getInteger();
                                if (countItemOffi == 0)
                                    return;
                            }
                            else if(typeItem == "hd")
                            {
                                string AllowedVisage;
                                if (Client.GetHabbo().Gender == "f")
                                {
                                    AllowedVisage = ";3105;3106;3104;3100;3099;3098;3097;3096;629;628;627;626;625;620;615;610;605;600;";
                                }
                                else
                                {
                                    AllowedVisage = ";180;185;190;195;200;205;206;207;208;209;3091;3092;3093;3094;3095;3101;3102;3103;";
                                }

                                string newCode = ";" + Code + ";";
                                if (!AllowedVisage.Contains(newCode))
                                    return;
                            }
                        }

                        Client.GetHabbo().addCooldown("changeLook", 2500);
                        if (typeItem == "ha")
                        {
                            if (Convert.ToInt32(Code) == 0)
                            {
                                if (!Client.GetHabbo().Look.Contains("ha-"))
                                {
                                    Client.SendWhisper("Vous ne portez pas de bonnet.");
                                    return;
                                }

                                Client.GetHabbo().removeLookType("ha");
                                User.OnChat(User.LastBubble, "* Enlève son bonnet *", true);
                                return;
                            }

                            if (Client.GetHabbo().Look.Contains("ha-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà ce bonnet.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("ha-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de bonnet *", true);
                        }
                        else if (typeItem == "hd")
                        {
                            if (Client.GetHabbo().Look.Contains("hd-" + Code + "-2"))
                            {
                                Client.SendWhisper("Vous avez déjà ce type de visage.");
                                return;
                            }

                            Client.SendWhisper("Votre type de visage a été modifié.");
                            Client.GetHabbo().changeLookByType("hd-", Code + "-2");
                        }
                        else if (typeItem == "ea")
                        {
                            if (Convert.ToInt32(Code) == 0)
                            {
                                if (!Client.GetHabbo().Look.Contains("ea-"))
                                {
                                    Client.SendWhisper("Vous ne portez pas de lunettes.");
                                    return;
                                }

                                Client.GetHabbo().removeLookType("ea");
                                User.OnChat(User.LastBubble, "* Enlève ses lunettes *", true);
                                return;
                            }

                            if (Client.GetHabbo().Look.Contains("ea-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà ces lunettes.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("ea-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de lunettes *", true);
                        }
                        else if (typeItem == "ch")
                        {
                            if (Client.GetHabbo().Look.Contains("ch-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà ce tee shirt.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("ch-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de tee shirt *", true);
                        }
                        else if (typeItem == "lg")
                        {
                            if (Client.GetHabbo().Look.Contains("lg-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà ce pantalon.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("lg-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de pantalon *", true);
                        }
                        else if (typeItem == "sh")
                        {
                            if (Client.GetHabbo().Look.Contains("sh-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà ces chaussures.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("sh-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de chaussures *", true);
                        }
                        else if (typeItem == "fa")
                        {
                            if (Convert.ToInt32(Code) == 0)
                            {
                                if (!Client.GetHabbo().Look.Contains("fa-"))
                                {
                                    Client.SendWhisper("Vous ne portez pas d'accessoire.");
                                    return;
                                }

                                Client.GetHabbo().removeLookType("fa");
                                User.OnChat(User.LastBubble, "* Enlève son accessoire *", true);
                                return;
                            }

                            if (Client.GetHabbo().Look.Contains("fa-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà cet accessoire.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("fa-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change d'accessoire *", true);
                        }
                        else if (typeItem == "ca")
                        {
                            if (Convert.ToInt32(Code) == 0)
                            {
                                if (!Client.GetHabbo().Look.Contains("ca-"))
                                {
                                    Client.SendWhisper("Vous ne portez pas d'écharpe.");
                                    return;
                                }

                                Client.GetHabbo().removeLookType("ca");
                                User.OnChat(User.LastBubble, "* Enlève son écharpe *", true);
                                return;
                            }

                            if (Client.GetHabbo().Look.Contains("ca-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà cette écharpe.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("ca-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change d'écharpe *", true);
                        }
                        else if (typeItem == "wa")
                        {
                            if (Convert.ToInt32(Code) == 0)
                            {
                                if (!Client.GetHabbo().Look.Contains("wa-"))
                                {
                                    Client.SendWhisper("Vous ne portez pas de ceinture.");
                                    return;
                                }

                                Client.GetHabbo().removeLookType("wa");
                                User.OnChat(User.LastBubble, "* Enlève sa ceinture *", true);
                                return;
                            }

                            if (Client.GetHabbo().Look.Contains("wa-" + Code + "-" + Color))
                            {
                                Client.SendWhisper("Vous portez déjà cette ceinture.");
                                return;
                            }

                            Client.GetHabbo().changeLookByType("wa-", Code + "-" + Color);
                            User.OnChat(User.LastBubble, "* Change de ceinture *", true);
                        }
                    }
                    break;
                #endregion
                #region buylook
                case "buylook":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.isTradingItems)
                        {
                            Client.SendWhisper("Vous ne pouvez pas acheter de vêtements pendant que vous faites un échange.");
                            return;
                        }

                        if (Client.GetHabbo().CurrentRoomId != 6)
                            return;

                        if (Client.GetHabbo().getCooldown("buyLook"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string typeItem = ReceivedData[1];
                        string Code = ReceivedData[2];
                        string Color = ReceivedData[3];

                        if (string.IsNullOrWhiteSpace(typeItem) || Code == null || Color == null || !PlusEnvironment.GetConfig().data["look.color"].Contains(";" + Color + ";"))
                            return;

                        int num;
                        if (!Int32.TryParse(Code, out num) || !Int32.TryParse(Color, out num))
                            return;

                        // ESSAYE MEME LOOK QUANCIEN
                        int PriceItem;
                        int taxeItem;
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `looks_users` WHERE `user_id` = @userId AND code = @code AND color = @color");
                            dbClient.AddParameter("userId", Client.GetHabbo().Id);
                            dbClient.AddParameter("code", Code);
                            dbClient.AddParameter("color", Color);
                            int countItem = dbClient.getInteger();
                            if (countItem > 0)
                            {
                                Client.SendWhisper("Vous avez déjà ce produit.");
                                return;
                            }

                            dbClient.SetQuery("SELECT COUNT(0) FROM `looks` WHERE `type` = @typeItem AND code = @code AND gender = 'u' OR gender = @gender");
                            dbClient.AddParameter("typeItem", typeItem);
                            dbClient.AddParameter("code", Code);
                            dbClient.AddParameter("gender", Client.GetHabbo().Gender);
                            int countItemOffi = dbClient.getInteger();
                            if (countItemOffi == 0)
                                return;

                            dbClient.SetQuery("SELECT price FROM `looks` WHERE code = @code");
                            dbClient.AddParameter("code", Code);
                            PriceItem = dbClient.getInteger();

                            dbClient.SetQuery("SELECT taxe FROM `looks` WHERE code = @code");
                            dbClient.AddParameter("code", Code);
                            taxeItem = dbClient.getInteger();
                        }

                        if(Client.GetHabbo().Credits < PriceItem)
                        {
                            Client.SendWhisper("Il vous faut " + PriceItem + " pour pouvoir payer cet article.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("buyLook", 2000);
                        Client.GetHabbo().addTenu(Convert.ToInt32(Code), Convert.ToInt32(Color));
                        Client.GetHabbo().Credits -= PriceItem;
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);

                        Group Gouvernement = null;
                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                        {
                            Gouvernement.ChiffreAffaire += taxeItem;
                            Gouvernement.updateChiffre();
                        }

                        Group Hem = null;
                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(9, out Hem))
                        {
                            Hem.ChiffreAffaire += PriceItem - taxeItem;
                            Hem.updateChiffre();
                        }

                        if (typeItem == "ha")
                        {
                            User.OnChat(User.LastBubble, "* Achète un bonnet [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "ea")
                        {
                            User.OnChat(User.LastBubble, "* Achète des lunettes [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "ch")
                        {
                            User.OnChat(User.LastBubble, "* Achète un tee shirt [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "undifi")
                        {
                            User.OnChat(User.LastBubble, "* Achète une ceinture [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "lg")
                        {
                            User.OnChat(User.LastBubble, "* Achète un pantalon [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "sh")
                        {
                            User.OnChat(User.LastBubble, "* Achète des chaussures [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "fa")
                        {
                            User.OnChat(User.LastBubble, "* Achète un accessoire [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "ca")
                        {
                            User.OnChat(User.LastBubble, "* Achète une écharpe [-" + PriceItem + " CRÉDITS] *", true);
                        }
                        else if (typeItem == "wa")
                        {
                            User.OnChat(User.LastBubble, "* Achète une ceinture [-" + PriceItem + " CRÉDITS] *", true);
                        }
                    }
                    break;
                    #endregion
            }
        }
    }
}
