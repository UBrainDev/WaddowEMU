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

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class FoutainWebEvent : IWebEvent
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
                #region recuperer
                case "recuperer":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || !User.canUseFoutain)
                            return;

                        if (Client.GetHabbo().getCooldown("foutain_webevent"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (PlusEnvironment.Fontaine == 0)
                        {
                            Client.SendWhisper("Il n'y a pas de pièce dans la fontaine.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("foutain_webevent", 3000);
                        int FontaineCredit = PlusEnvironment.Fontaine;
                        PlusEnvironment.Fontaine = 0;
                        User.OnChat(User.LastBubble, "* Récupère " + FontaineCredit + " crédits dans la fontaine *", true);
                        Client.GetHabbo().Credits += FontaineCredit;
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                    }
                    break;
                #endregion
                #region jeter
                case "jeter":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || !User.canUseFoutain)
                            return;

                        if(Client.GetHabbo().getCooldown("foutain_webevent"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if(Client.GetHabbo().Credits < 5)
                        {
                            Client.SendWhisper("Il vous faut 5 crédits pour pouvoir jeter une pièce.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("foutain_webevent", 3000);
                        Client.GetHabbo().Credits -= 5;
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                        PlusEnvironment.Fontaine += 5;
                        User.OnChat(User.LastBubble, "* Jette une pièce de 5 crédits dans la fontaine et fait un voeux *", true);
                    }
                    break;
                #endregion
            }
        }
    }
}
