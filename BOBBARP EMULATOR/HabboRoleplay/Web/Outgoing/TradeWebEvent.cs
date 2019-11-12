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
    class TradeWebEvent : IWebEvent
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

                #region cancelTrade
                case "cancelTrade":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.isTradingItems == false)
                            return;

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.isTradingUsername);
                        if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            Client.SendWhisper(User.isTradingUsername + " n'est plus dans votre appartement.");
                            User.cancelItemsTrade();
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser == null || TargetUser.isTradingItems == false)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.cancelItemsTrade();
                            return;
                        }

                        User.cancelItemsTrade();
                        TargetUser.cancelItemsTrade();
                        Client.SendWhisper("Vous avez annulé l'échange.");
                        TargetUser.GetClient().SendWhisper(Client.GetHabbo().Username + " a annulé l'échange.");
                    }
                    break;
                #endregion
                #region addItems
                case "addItems":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.isTradingItems == false)
                            return;

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.isTradingUsername);
                        if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            Client.SendWhisper(User.isTradingUsername + " n'est plus dans votre appartement.");
                            User.cancelItemsTrade();
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser == null || TargetUser.isTradingItems == false)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.cancelItemsTrade();
                            return;
                        }

                        if (User.isTradingListItems.Count >= 9)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string itemName = ReceivedData[1];

                        switch (itemName)
                        {
                            #region credits
                            case "credits":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Credits)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("credits"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos crédits de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Credits - Convert.ToInt32(paramater);
                                    User.addItemsTrade("credits", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region coca
                            case "coca":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Coca)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("coca"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos cocas de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("coca") < (TargetClient.GetHabbo().Coca + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("coca") - TargetClient.GetHabbo().Coca) + " cocas supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Coca - Convert.ToInt32(paramater);
                                    User.addItemsTrade("coca", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region fanta
                            case "fanta":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Fanta)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("fanta"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos fantas de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("fanta") < (TargetClient.GetHabbo().Fanta + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("fanta") - TargetClient.GetHabbo().Fanta) + " fantas supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Fanta - Convert.ToInt32(paramater);
                                    User.addItemsTrade("fanta", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region pain
                            case "pain":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Pain)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("pain"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos baguettes de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("pain") < (TargetClient.GetHabbo().Pain + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("pain") - TargetClient.GetHabbo().Pain) + " pains supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Pain - Convert.ToInt32(paramater);
                                    User.addItemsTrade("pain", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region sucette
                            case "sucette":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Sucette)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("sucette"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos sucettes de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("sucette") < (TargetClient.GetHabbo().Sucette + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("sucette") - TargetClient.GetHabbo().Sucette) + " sucettes supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Sucette - Convert.ToInt32(paramater);
                                    User.addItemsTrade("sucette", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region savon
                            case "savon":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Savon)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("savon"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos savons de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("savon") < (TargetClient.GetHabbo().Savon + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("savon") - TargetClient.GetHabbo().Savon) + " savons supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Savon - Convert.ToInt32(paramater);
                                    User.addItemsTrade("savon", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region doliprane
                            case "doliprane":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Doliprane)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("doliprane"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos dolipranes de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().Sac == 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de sac à dos.");
                                        return;
                                    }

                                    if (TargetClient.GetHabbo().getMaxItem("doliprane") < (TargetClient.GetHabbo().Doliprane + Convert.ToInt32(paramater)))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + (TargetClient.GetHabbo().getMaxItem("doliprane") - TargetClient.GetHabbo().Doliprane) + " dolipranes supplémentaires dans son sac à dos.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Doliprane - Convert.ToInt32(paramater);
                                    User.addItemsTrade("doliprane", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region weed
                            case "weed":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Weed)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("weed"))
                                    {
                                        Client.SendWhisper("Veuillez enlever votre weed de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Weed - Convert.ToInt32(paramater);
                                    User.addItemsTrade("weed", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region cigarette
                            case "cigarette":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().PhilipMo)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("cigarette"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos cigarettes de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if ((TargetClient.GetHabbo().PhilipMo + Convert.ToInt32(paramater)) > 60)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        int NumberRestant = 60 - TargetClient.GetHabbo().PhilipMo;
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + NumberRestant + " cigarettes supplémentaires.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().PhilipMo - Convert.ToInt32(paramater);
                                    User.addItemsTrade("cigarette", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region clipper
                            case "clipper":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Convert.ToInt32(Client.GetHabbo().Clipper / 50))
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("clipper"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos clippers de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    if ((TargetClient.GetHabbo().Clipper + (Convert.ToInt32(paramater) * 50) ) > 100)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        int NumberRestant = Convert.ToInt32((100 - TargetClient.GetHabbo().Clipper) / 50);
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " peut stocker maximum " + NumberRestant + " clippers supplémentaires.");
                                        return;
                                    }

                                    int NewNumber = (Client.GetHabbo().Clipper - (Convert.ToInt32(paramater) *50))/50;
                                    User.addItemsTrade("clipper", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region hoverboardBlanc
                            case "hoverboardBlanc":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().WhiteHoverboard == 0 || User.isTradingListItems.ContainsKey("hoverboardBlanc"))
                                        return;

                                    if (TargetClient.GetHabbo().WhiteHoverboard != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un hoverboard blanc.");
                                        return;
                                    }

                                    User.addItemsTrade("hoverboardBlanc", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region porsche911
                            case "porsche911":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Porsche911 == 0 || User.isTradingListItems.ContainsKey("porsche911"))
                                        return;

                                    if (TargetClient.GetHabbo().Porsche911 != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Porsche 911.");
                                        return;
                                    }

                                    User.addItemsTrade("porsche911", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region fiatPunto
                            case "fiatPunto":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().FiatPunto == 0 || User.isTradingListItems.ContainsKey("fiatPunto"))
                                        return;

                                    if (TargetClient.GetHabbo().FiatPunto != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Fiat Punto.");
                                        return;
                                    }

                                    User.addItemsTrade("fiatPunto", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region volkswagenJetta
                            case "volkswagenJetta":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().VolkswagenJetta == 0 || User.isTradingListItems.ContainsKey("volkswagenJetta"))
                                        return;

                                    if (TargetClient.GetHabbo().VolkswagenJetta != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Volkswagen Jetta.");
                                        return;
                                    }

                                    User.addItemsTrade("volkswagenJetta", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region bmwI8
                            case "bmwI8":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().BmwI8 == 0 || User.isTradingListItems.ContainsKey("bmwI8"))
                                        return;

                                    if (TargetClient.GetHabbo().BmwI8 != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une BMW i8.");
                                        return;
                                    }

                                    User.addItemsTrade("bmwI8", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region audiA8
                            case "audiA8":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().AudiA8 == 0 || User.isTradingListItems.ContainsKey("audiA8"))
                                        return;

                                    if (TargetClient.GetHabbo().AudiA8 != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Audi A8.");
                                        return;
                                    }

                                    User.addItemsTrade("audiA8", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region audiA3
                            case "audiA3":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().AudiA3 == 0 || User.isTradingListItems.ContainsKey("audiA3"))
                                        return;

                                    if (TargetClient.GetHabbo().AudiA3 != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Audi A3.");
                                        return;
                                    }

                                    User.addItemsTrade("audiA3", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region batte
                            case "batte":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Batte == 0 || User.isTradingListItems.ContainsKey("batte"))
                                        return;

                                    if (TargetClient.GetHabbo().Batte != 0 || TargetClient.GetHabbo().PoliceCasier.Contains("[BATTE]"))
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une batte.");
                                        return;
                                    }

                                    User.addItemsTrade("batte", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region sabre
                            case "sabre":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Sabre == 0 || User.isTradingListItems.ContainsKey("sabre"))
                                        return;

                                    if (TargetClient.GetHabbo().Sabre != 0 || TargetClient.GetHabbo().PoliceCasier.Contains("[SABRE]"))
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un sabre.");
                                        return;
                                    }

                                    User.addItemsTrade("sabre", paramater, image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region ak47
                            case "ak47":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Ak47 == 0 || User.isTradingListItems.ContainsKey("ak47"))
                                        return;

                                    if (TargetClient.GetHabbo().Ak47 != 0 || TargetClient.GetHabbo().PoliceCasier.Contains("[AK47]"))
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une AK47.");
                                        return;
                                    }

                                    User.addItemsTrade("ak47", Convert.ToString(Client.GetHabbo().AK47_Munitions), image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region uzi
                            case "uzi":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Uzi == 0 || User.isTradingListItems.ContainsKey("uzi"))
                                        return;

                                    if (TargetClient.GetHabbo().Uzi != 0 || TargetClient.GetHabbo().PoliceCasier.Contains("[UZI]"))
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un Uzi.");
                                        return;
                                    }

                                    User.addItemsTrade("uzi", Convert.ToString(Client.GetHabbo().Uzi_Munitions), image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region cocktail
                            case "cocktail":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];

                                    int Amount;
                                    if (!int.TryParse(paramater, out Amount) || Convert.ToInt32(paramater) <= 0 || paramater.StartsWith("0") || Convert.ToInt32(paramater) > Client.GetHabbo().Cocktails)
                                    {
                                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "trade;errorMontantTrade");
                                        return;
                                    }

                                    if (User.isTradingListItems.ContainsKey("cocktail"))
                                    {
                                        Client.SendWhisper("Veuillez enlever vos cocktails molotov de votre proposition pour définir un autre montant.");
                                        return;
                                    }

                                    int NewNumber = Client.GetHabbo().Cocktails - Convert.ToInt32(paramater);
                                    User.addItemsTrade("cocktail", paramater, image, NewNumber, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region gps
                            case "gps":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Gps == 0 || User.isTradingListItems.ContainsKey("gps"))
                                        return;

                                    if (TargetClient.GetHabbo().Gps != 0)
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un GPS.");
                                        return;
                                    }

                                    User.addItemsTrade("gps", "GPS", image, 0, TargetUser);
                                    break;
                                }

                            #endregion;
                            #region telephone
                            case "telephone":
                                {
                                    string paramater = ReceivedData[2];
                                    string image = ReceivedData[3];
                                    if (Client.GetHabbo().Telephone == 0 || User.isTradingListItems.ContainsKey("telephone"))
                                        return;

                                    if (TargetClient.GetHabbo().Telephone != 0 || TargetClient.GetHabbo().PoliceCasier.Contains("[TELEPHONE]"))
                                    {
                                        Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un téléphone.");
                                        return;
                                    }

                                    User.addItemsTrade("telephone", Client.GetHabbo().TelephoneName, image, 0, TargetUser);
                                    break;
                                }

                                #endregion;
                        }
                    }
                    break;
                #endregion
                #region removeItems
                case "removeItems":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.isTradingItems == false)
                            return;

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.isTradingUsername);
                        if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            Client.SendWhisper(User.isTradingUsername + " n'est plus dans votre appartement.");
                            User.cancelItemsTrade();
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser == null || TargetUser.isTradingItems == false)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.cancelItemsTrade();
                            return;
                        }

                        if (User.isTradingListItems.Count >= 9)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string itemName = ReceivedData[1];
                        if (!User.isTradingListItems.ContainsKey(itemName))
                            return;

                        switch (itemName)
                        {
                            #region credits
                            case "credits":
                                {
                                    User.removeItemsTrade("credits", Convert.ToString(Client.GetHabbo().Credits), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region coca
                            case "coca":
                                {
                                    User.removeItemsTrade("coca", Convert.ToString(Client.GetHabbo().Coca), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region fanta
                            case "fanta":
                                {
                                    User.removeItemsTrade("fanta", Convert.ToString(Client.GetHabbo().Fanta), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region pain
                            case "pain":
                                {
                                    User.removeItemsTrade("pain", Convert.ToString(Client.GetHabbo().Pain), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region sucette
                            case "sucette":
                                {
                                    User.removeItemsTrade("sucette", Convert.ToString(Client.GetHabbo().Sucette), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region savon
                            case "savon":
                                {
                                    User.removeItemsTrade("savon", Convert.ToString(Client.GetHabbo().Savon), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region doliprane
                            case "doliprane":
                                {
                                    User.removeItemsTrade("doliprane", Convert.ToString(Client.GetHabbo().Doliprane), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region weed
                            case "weed":
                                {
                                    User.removeItemsTrade("weed", Convert.ToString(Client.GetHabbo().Weed), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region cigarette
                            case "cigarette":
                                {
                                    User.removeItemsTrade("cigarette", Convert.ToString(Client.GetHabbo().PhilipMo), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region clipper
                            case "clipper":
                                {
                                    int NewNumber = Client.GetHabbo().Clipper / 50;
                                    User.removeItemsTrade("clipper", Convert.ToString(NewNumber), TargetUser);
                                    break;
                                }

                            #endregion;
                            #region hoverboardBlanc
                            case "hoverboardBlanc":
                                {
                                    User.removeItemsTrade("hoverboardBlanc", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region porsche911
                            case "porsche911":
                                {
                                    User.removeItemsTrade("porsche911", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region fiatPunto
                            case "fiatPunto":
                                {
                                    User.removeItemsTrade("fiatPunto", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region volkswagenJetta
                            case "volkswagenJetta":
                                {
                                    User.removeItemsTrade("volkswagenJetta", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region bmwI8
                            case "bmwI8":
                                {
                                    User.removeItemsTrade("bmwI8", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region audiA8
                            case "audiA8":
                                {
                                    User.removeItemsTrade("audiA8", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region audiA3
                            case "audiA3":
                                {
                                    User.removeItemsTrade("audiA3", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region batte
                            case "batte":
                                {
                                    User.removeItemsTrade("batte", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region sabre
                            case "sabre":
                                {
                                    User.removeItemsTrade("sabre", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region ak47
                            case "ak47":
                                {
                                    User.removeItemsTrade("ak47", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region uzi
                            case "uzi":
                                {
                                    User.removeItemsTrade("uzi", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region cocktail
                            case "cocktail":
                                {
                                    User.removeItemsTrade("cocktail", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region gps
                            case "gps":
                                {
                                    User.removeItemsTrade("gps", "noParameter", TargetUser);
                                    break;
                                }

                            #endregion;
                            #region telephone
                            case "telephone":
                                {
                                    User.removeItemsTrade("telephone", "noParameter", TargetUser);
                                    break;
                                }
                                #endregion;
                        }
                    }
                    break;
                #endregion
                #region confirmTrade
                case "confirmTrade":
                    {
                        if (Client == null || Client.GetHabbo() == null)
                            return;

                        if(Client.GetHabbo().getCooldown("confirm_trade"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.isTradingItems == false || User.isTradingConfirm)
                            return;

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.isTradingUsername);
                        if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            Client.SendWhisper(User.isTradingUsername + " n'est plus dans votre appartement.");
                            User.cancelItemsTrade();
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser == null || TargetUser.isTradingItems == false)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.cancelItemsTrade();
                            return;
                        }

                        if (TargetUser.isTradingListItems.Count == 0 && User.isTradingListItems.Count == 0)
                        {
                            Client.SendWhisper("Il n'y aucun item à échanger.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("confirm_trade", 5000);
                        if(TargetUser.isTradingConfirm)
                        {
                            TargetUser.valideTrade(User);
                            User.valideTrade(TargetUser);
                            TargetUser.isTradingItems = false;
                            TargetUser.isTradingUsername = null;
                            TargetUser.isTradingListItems.Clear();

                            User.isTradingItems = false;
                            User.isTradingUsername = null;
                            User.isTradingListItems.Clear();
                            Client.SendWhisper("L'échange a bien été effectué.");
                            TargetClient.SendWhisper("L'échange a bien été effectué.");
                            return;
                        }
                        
                        User.confirmTrade(TargetUser);
                    }
                    break;
                    #endregion
            }
        }
    }
}