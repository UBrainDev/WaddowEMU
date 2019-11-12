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
    class CasierWebEvent : IWebEvent
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
                #region deposer
                case "deposer":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        if(Client.GetHabbo().getCooldown("using_casier"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || !User.usingCasier)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string itemName = ReceivedData[1];

                        switch (itemName)
                        {
                            #region weed
                            case "weed":
                                {
                                    string paramater = ReceivedData[2];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Weed)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;errorDepotMontant");
                                        return;
                                    }

                                    Client.GetHabbo().addCooldown("using_casier", 2000);
                                    Client.GetHabbo().Weed -= Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateWeed();
                                    Client.GetHabbo().CasierWeed += Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCasierWeed();
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;refreshNumber;" + Client.GetHabbo().CasierWeed + ";" + Client.GetHabbo().CasierCocktails);
                                    User.OnChat(User.LastBubble, "* Dépose " + paramater + "g de weed dans son casier *", true);
                                    break;
                                }

                            #endregion;
                            #region cocktail
                            case "cocktail":
                                {
                                    string paramater = ReceivedData[2];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Cocktails)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;errorDepotMontant");
                                        return;
                                    }

                                    Client.GetHabbo().addCooldown("using_casier", 2000);
                                    Client.GetHabbo().Cocktails -= Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCocktails();
                                    Client.GetHabbo().CasierCocktails += Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCasierCocktails();
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;refreshNumber;" + Client.GetHabbo().CasierWeed + ";" + Client.GetHabbo().CasierCocktails);
                                    User.OnChat(User.LastBubble, "* Dépose " + paramater + " cocktail(s) molotov dans son casier *", true);
                                    break;
                                }

                                #endregion;
                        }
                    }
                    break;
                #endregion
                #region retirer
                case "retirer":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        if (Client.GetHabbo().getCooldown("using_casier"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || !User.usingCasier)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string itemName = ReceivedData[1];

                        switch (itemName)
                        {
                            #region weed
                            case "weed":
                                {
                                    string paramater = ReceivedData[2];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().CasierWeed)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;errorRetirerMontant");
                                        return;
                                    }

                                    Client.GetHabbo().addCooldown("using_casier", 2000);
                                    Client.GetHabbo().CasierWeed -= Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCasierWeed();
                                    Client.GetHabbo().Weed += Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateWeed();
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;refreshNumber;" + Client.GetHabbo().CasierWeed + ";" + Client.GetHabbo().CasierCocktails);
                                    User.OnChat(User.LastBubble, "* Retire " + paramater + "g de weed de son casier *", true);
                                    break;
                                }

                            #endregion;
                            #region cocktail
                            case "cocktail":
                                {
                                    string paramater = ReceivedData[2];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().CasierCocktails)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;errorRetirerMontant");
                                        return;
                                    }

                                    Client.GetHabbo().addCooldown("using_casier", 2000);
                                    Client.GetHabbo().CasierCocktails -= Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCasierCocktails();
                                    Client.GetHabbo().Cocktails += Convert.ToInt32(paramater);
                                    Client.GetHabbo().updateCocktails();
                                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "casier;refreshNumber;" + Client.GetHabbo().CasierWeed + ";" + Client.GetHabbo().CasierCocktails);
                                    User.OnChat(User.LastBubble, "* Retire " + paramater + " cocktail(s) molotov de son casier *", true);
                                    break;
                                }

                                #endregion;
                        }
                    }
                    break;
                    #endregion
            }
        }
    }
}