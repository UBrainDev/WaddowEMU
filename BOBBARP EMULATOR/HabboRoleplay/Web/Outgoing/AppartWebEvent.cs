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

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class AppartWebEvent : IWebEvent
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

                #region setId
                case "setId":
                    {
                        if (Client.GetHabbo().Rank != 8)
                            return;

                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.AppartItem == 0)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string appartId = ReceivedData[1];

                        int num;
                        if (string.IsNullOrWhiteSpace(appartId) || !Int32.TryParse(appartId, out num) || !Int32.TryParse(appartId, out num))
                        {
                            Client.SendWhisper("L'ID de l'appartement est invalide.");
                            return;
                        }

                        Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(appartId));
                        if (TargetRoom == null)
                        {
                            Client.SendWhisper("L'ID de l'appartement est invalide.");
                            return;
                        }

                        if (TargetRoom.Loyer == 0)
                        {
                            Client.SendWhisper("L'appartement n'est pas en location.");
                            return;
                        }

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `orpi_items_apparts` WHERE `id_appart` = @idAppart");
                            dbClient.AddParameter("idAppart", appartId);
                            int appartCount = dbClient.getInteger();
                            if (appartCount > 0)
                            {
                                Client.SendWhisper("Cet appartement est déjà enregistré.");
                                return;
                            }

                            dbClient.SetQuery("INSERT INTO `orpi_items_apparts` (id_item, id_appart) VALUES (@idItem, @idAppart)");
                            dbClient.AddParameter("idItem", User.AppartItem);
                            dbClient.AddParameter("idAppart", TargetRoom.Id);
                            dbClient.RunQuery();
                            Client.SendWhisper("L'appartement a bien été défini.");
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "appart;setAppart;" + TargetRoom.Id + ";" + TargetRoom.OwnerName + ";" + TargetRoom.Loyer_Date);
                            return;
                        }
                    }
                    break;
                #endregion

                // ORPI
                #region orpiLogin
                case "orpiLogin":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.ConnectedMetier == false || Client.GetHabbo().TravailId != 21 || Client.GetHabbo().Travaille == false)
                            return;

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `rooms` WHERE `owner` != '3' AND `loyer` = '1'");
                            int appartLouedCount = dbClient.getInteger();

                            dbClient.SetQuery("SELECT COUNT(0) FROM `rooms` WHERE `owner` = '3' AND `loyer` = '1'");
                            int appartALoued = dbClient.getInteger();

                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "ordinateur;orpi;home;" + appartLouedCount + ";" + appartALoued);
                            return;
                        }
                    }
                    break;
                #endregion

                #region louerAppart
                case "louerAppart":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if(Client.GetHabbo().getCooldown("louer_appart"))
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().TravailId != 21 || Client.GetHabbo().Travaille == false || User.ConnectedMetier == false)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string appartId = ReceivedData[1];
                        string username = ReceivedData[2];

                        int num;
                        if (!Int32.TryParse(appartId, out num) || !Int32.TryParse(appartId, out num))
                        {

                            Client.SendWhisper("Une erreur est survenue.");
                            return;
                        }

                        Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(appartId));
                        if (TargetRoom == null)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            return;
                        }

                        if (TargetRoom.Loyer == 0 || TargetRoom.OwnerId != 3)
                        {
                            Client.SendWhisper("L'appartement n'est plus à louer.");
                            return;
                        }

                        if (string.IsNullOrEmpty(username))
                        {

                            Client.SendWhisper("Veuillez rentrer un pseudonyme.");
                            return;
                        }

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
                        if(TargetClient == null || TargetClient.GetHabbo().CurrentRoomId != Client.GetHabbo().CurrentRoomId)
                        {
                            Client.SendWhisper("Impossible de trouver " + username + " dans cet appartement.");
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if(TargetUser.Transaction != null || TargetUser.isTradingItems)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                            return;
                        }

                        Client.GetHabbo().addCooldown("louer_appart", 5000);
                        User.OnChat(User.LastBubble, "* Fait un contrat de location à " + TargetClient.GetHabbo().Username + " *", true);

                        int TargetRoomPrice = TargetRoom.Prix_Vente;
                        decimal TargetRoomPriceDecimal = Convert.ToDecimal(TargetRoomPrice);
                        int TargetRoomTaxe = Convert.ToInt32((TargetRoomPriceDecimal / 100m) * 15m);

                        TargetUser.Transaction = "location:" + TargetRoom.Id + ":" + TargetRoomPrice + ":" + TargetRoomTaxe;
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Client.GetHabbo().Username + "</b> souhaite vous faire un <b>contrat de location</b> pour l'appartement <b>[" + TargetRoom.Id + "] " + TargetRoom.Name + "</b> pour <b>" + TargetRoomPrice + " crédits</b> dont <b>" + TargetRoomTaxe + "</b> pour la taxe d'habitation.;" + TargetRoomPrice);
                    }
                    break;
                    #endregion
            }
        }
    }
}
