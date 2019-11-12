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
using Plus.HabboHotel.Groups;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class CoiffureWebEvent : IWebEvent
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
                #region newCoiffure
                case "newCoiffure":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (User.usernameCoiff == null)
                            return;

                        if (Client.GetHabbo().TravailId != 15 || Client.GetHabbo().Travaille == false)
                            return;

                        if(User.makeAction == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.usernameCoiff);
                        if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Client.GetHabbo().CurrentRoom)
                        {
                            User.usernameCoiff = null;
                            return;
                        }

                        RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        if (TargetUser == null)
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.usernameCoiff = null;
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string Name = ReceivedData[1];
                        string Color = ReceivedData[2];

                        if (TargetClient.GetHabbo().checkCoiff("hr-" + Name + "-"+ Color) == true)
                        {
                            Client.SendWhisper(TargetClient.GetHabbo().Username +  " a déjà cette coiffure.");
                            User.usernameCoiff = null;
                            TargetUser.usernameCoiff = null;
                            return;
                        }

                        int Amount;
                        if (Name == null || Color == null || !int.TryParse(Name, out Amount) || !int.TryParse(Color, out Amount) || !PlusEnvironment.GetConfig().data["head.color"].Contains(";" + Color + ";"))
                        {
                            Client.SendWhisper("Une erreur est survenue.");
                            User.usernameCoiff = null;
                            TargetUser.usernameCoiff = null;
                            return;
                        }

                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("SELECT COUNT(0) FROM `coiffures` WHERE `name` = @name AND `type` = @type");
                            dbClient.AddParameter("name", Name);
                            dbClient.AddParameter("type", TargetClient.GetHabbo().Gender);
                            int countCoiff = dbClient.getInteger();
                            if (countCoiff == 0)
                            {
                                Client.SendWhisper("Une erreur est survenue.");
                                User.usernameCoiff = null;
                                TargetUser.usernameCoiff = null;
                                return;
                            }
                        }

                        User.makeAction = true;
                        User.OnChat(User.LastBubble, "* Réalise la coiffure souhaitée par " + TargetClient.GetHabbo().Username + " *", true);

                        System.Timers.Timer timer1 = new System.Timers.Timer(5000);
                        timer1.Interval = 2000;
                        timer1.Elapsed += delegate
                        {
                            if (TargetClient != null && TargetClient.GetHabbo().CurrentRoomId == Client.GetHabbo().CurrentRoomId)
                            {
                                User.OnChat(User.LastBubble, "* Coupe les cheveux de " + TargetClient.GetHabbo().Username + " *", true);
                            }
                            else
                            {
                                User.usernameCoiff = null;
                                User.makeAction = false;
                            }
                            timer1.Stop();
                        };
                        timer1.Start();

                        System.Timers.Timer timer2 = new System.Timers.Timer(10000);
                        timer2.Interval = 10000;
                        timer2.Elapsed += delegate
                        {
                            if (TargetClient != null && TargetClient.GetHabbo().CurrentRoomId == Client.GetHabbo().CurrentRoomId)
                            {
                                User.OnChat(User.LastBubble, "* Fini de réaliser la coupe de " + TargetClient.GetHabbo().Username + " *", true);
                                TargetClient.GetHabbo().changeLookByType("hr-", Name + "-" + Color);
                                if (TargetClient.GetHabbo().Confirmed == 0)
                                {
                                    TargetClient.GetHabbo().Coiffure -= 1;
                                    TargetClient.GetHabbo().updatecoiffure();
                                }
                                else
                                {
                                    int Prix;
                                    int Taxe;

                                    if (TargetClient.GetHabbo().Gender == "f")
                                    {
                                        Prix = PlusEnvironment.getPriceOfItem("Coiffure Femme");
                                        Taxe = PlusEnvironment.getTaxeOfItem("Coiffure Femme");
                                    }
                                    else
                                    {
                                        Prix = PlusEnvironment.getPriceOfItem("Coiffure Homme");
                                        Taxe = PlusEnvironment.getTaxeOfItem("Coiffure Homme");
                                    }

                                    Group HairSalon = null;
                                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(15, out HairSalon))
                                    {
                                        HairSalon.ChiffreAffaire += Convert.ToInt32(Prix - Taxe);
                                        HairSalon.updateChiffre();
                                    }
                                }
                                User.makeAction = false;
                                User.usernameCoiff = null;
                                TargetUser.usernameCoiff = null;
                            }
                            else
                            {
                                User.usernameCoiff = null;
                            }
                            timer2.Stop();
                        };
                        timer2.Start();


                        return;
                    }
                #endregion
            }
        }
    }
}
