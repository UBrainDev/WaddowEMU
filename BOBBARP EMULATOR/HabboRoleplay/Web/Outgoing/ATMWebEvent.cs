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
using Plus.HabboHotel.Groups;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class ATMWebEvent : IWebEvent
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

                #region connect
                case "connect":
                    {
                        Socket.Send("distributeur;connect");
                    }
                    break;
                #endregion
                #region disconnect
                case "disconnect":
                    {
                        Socket.Send("distributeur;disconnect");
                    }
                    break;
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

                        if (User.usingATM == false || Client.GetHabbo().Cb == "null")
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == null || ReceivedData[1] != Client.GetHabbo().Cb)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;code");
                            return;
                        }

                        User.canUseCB = true;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;goToHome");
                        return;
                    }
                #endregion
                #region deposer
                case "deposer":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.usingATM == false || User.canUseCB == false)
                            return;

                        if (Client.GetHabbo().getCooldown("atm") == true)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;patienter");
                            return;
                        }

                        if (Client.GetHabbo().CurrentRoomId != 2)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;room");
                            return;
                        }

                        if (PlusEnvironment.GetGame().GetClientManager().userBankWorking() > 0)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;depot_working");
                            return;
                        }

                        int SplitData = Data.IndexOf(',');
                        string Montant = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Montant))
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant");
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Montant, out Amount) || Convert.ToInt32(Montant) <= 0 || Montant.StartsWith("0"))
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant");
                            return;
                        }

                        if (Convert.ToInt32(Montant) > Client.GetHabbo().Credits)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant_depot");
                            return;
                        }

                        if (Convert.ToInt32(Montant) < 40)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;depot_min");
                            return;
                        }

                        if (User.isTradingItems)
                        {
                            Client.SendWhisper("Vous ne pouvez pas déposer de crédits pendant que vous faites un échange");
                            return;
                        }

                        Client.GetHabbo().addCooldown("atm", 2000);
                        decimal depotDecimal = Convert.ToInt32(Montant);
                        int Depot = Convert.ToInt32((depotDecimal / 100m) * 95m);
                        int ForBank = Convert.ToInt32((depotDecimal / 100m) * 5m);
                        Group Banque = null;
                        if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(3, out Banque))
                        {
                            Banque.ChiffreAffaire += ForBank;
                            Banque.updateChiffre();
                        }
                        Client.GetHabbo().Credits -= Convert.ToInt32(Montant);
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                        Client.GetHabbo().Banque += Depot;
                        Client.GetHabbo().updateBanque();
                        User.OnChat(User.LastBubble, "* Dépose " + Montant + " crédits dans son compte bancaire (-" + ForBank + " crédits de taxe par la banque) *", true);
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;success");
                        break;
                    }
                #endregion
                #region retirer
                case "retirer":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.usingATM == false || User.canUseCB == false)
                            return;

                        if (Client.GetHabbo().getCooldown("atm") == true)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;patienter");
                            return;
                        }

                        int SplitData = Data.IndexOf(',');
                        string Montant = Data.Substring(SplitData + 1);

                        if (string.IsNullOrWhiteSpace(Montant))
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant");
                            return;
                        }

                        int Amount;
                        if (!int.TryParse(Montant, out Amount) || Convert.ToInt32(Montant) <= 0 || Montant.StartsWith("0"))
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant");
                            return;
                        }

                        if (Convert.ToInt32(Montant) > Client.GetHabbo().Banque)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;error;montant_banque");
                            return;
                        }

                        if (User.isTradingItems)
                        {
                            Client.SendWhisper("Vous ne pouvez pas déposer de crédits pendant que vous faites un échange");
                            return;
                        }

                        Client.GetHabbo().addCooldown("atm", 2000);
                        User.OnChat(User.LastBubble, "* Retire " + Montant + " crédits de son compte bancaire *", true);
                        Client.GetHabbo().Banque -= Convert.ToInt32(Montant);
                        Client.GetHabbo().updateBanque();
                        Client.GetHabbo().Credits += Convert.ToInt32(Montant);
                        Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "my_stats;" + Client.GetHabbo().Credits + ";" + Client.GetHabbo().Duckets + ";" + Client.GetHabbo().EventPoints);
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "distributeur;success");
                        break;
                    }
                    #endregion
            }
        }
    }
}
