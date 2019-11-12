using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using System.Text.RegularExpressions;
using Plus.HabboHotel.Rooms;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class PanierWebEvent : IWebEvent
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

                #region send
                case "send":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        Socket.Send("panier;" + User.Purchase + ";" + Convert.ToString(Client.GetHabbo().getPriceOfPanier()) + ";" + Client.GetHabbo().CurrentRoomId);
                    }
                    break;
                #endregion
                #region remove
                case "remove":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.Transaction == "panier")
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == "eau")
                        {
                            if (!User.Purchase.Contains("eau"))
                                return;

                            var regex = new Regex(Regex.Escape("eau-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);

                            User.Purchase = newPanier;
                            User.OnChat(User.LastBubble, "* Retire une bouteille d'eau de son panier *", true);
                        }
                        else if (ReceivedData[1] == "coca")
                        {
                            if (!User.Purchase.Contains("coca"))
                                return;

                            var regex = new Regex(Regex.Escape("coca-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);

                            User.Purchase = newPanier;
                            User.OnChat(User.LastBubble, "* Retire un coca de son panier *", true);
                        }
                        else if (ReceivedData[1] == "fanta")
                        {
                            if (!User.Purchase.Contains("fanta"))
                                return;

                            var regex = new Regex(Regex.Escape("fanta-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);

                            User.Purchase = newPanier;
                            User.OnChat(User.LastBubble, "* Retire un fanta de son panier *", true);
                        }
                        else if (ReceivedData[1] == "sucette")
                        {
                            if (!User.Purchase.Contains("sucette"))
                                return;

                            var regex = new Regex(Regex.Escape("sucette-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);

                            User.Purchase = newPanier;
                            User.OnChat(User.LastBubble, "* Retire une sucette de son panier *", true);
                        }
                        else if (ReceivedData[1] == "pain")
                        {
                            if (!User.Purchase.Contains("pain"))
                                return;

                            var regex = new Regex(Regex.Escape("pain-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);

                            User.Purchase = newPanier;
                            User.OnChat(User.LastBubble, "* Retire un pain de son panier *", true);
                        }
                        else if (ReceivedData[1] == "savon")
                        {
                            if (Client.GetHabbo().TravailId != 10)
                                return;

                            if (Client.GetHabbo().Travaille != true)
                                return;

                            if (Client.GetHabbo().Commande == null)
                                return;

                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetHabbo().Commande);
                            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                            {
                                Client.GetHabbo().Commande = null;
                                User.Purchase = "";
                                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "panier", "send");
                                return;
                            }

                            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                            if (TargetUser.Transaction != null)
                                return;

                            if (!User.Purchase.Contains("savon"))
                                return;

                            var regex = new Regex(Regex.Escape("savon-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);
                            
                            User.Purchase = newPanier;
                            TargetUser.Purchase = User.Purchase;
                            User.OnChat(User.LastBubble, "* Retire un savon de la commande de " + TargetClient.GetHabbo().Username + " *", true);
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                        }
                        else if (ReceivedData[1] == "doliprane")
                        {
                            if (Client.GetHabbo().TravailId != 10)
                                return;

                            if (Client.GetHabbo().Travaille != true)
                                return;

                            if (Client.GetHabbo().Commande == null)
                                return;

                            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Client.GetHabbo().Commande);
                            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                            {
                                Client.GetHabbo().Commande = null;
                                User.Purchase = "";
                                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "panier", "send");
                                return;
                            }

                            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                            if (TargetUser.Transaction != null)
                                return;

                            if (!User.Purchase.Contains("doliprane"))
                                return;

                            var regex = new Regex(Regex.Escape("doliprane-"));
                            var newPanier = regex.Replace(User.Purchase, "", 1);
                            
                            User.Purchase = newPanier;
                            TargetUser.Purchase = User.Purchase;
                            User.OnChat(User.LastBubble, "* Retire un doliprane de la commande de " + TargetClient.GetHabbo().Username + " *", true);
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                        }
                        Socket.Send("panier;" + User.Purchase + ";" + Convert.ToString(Client.GetHabbo().getPriceOfPanier()) +";" + Client.GetHabbo().CurrentRoomId);
                    }
                    break;
                    #endregion
            }
        }
    }
}
