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
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class ItemWebEvent : IWebEvent
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
                #region boire
                case "boire":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1)
                            return;
                        
                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == "coca")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":boire coca");
                        }
                        else if (ReceivedData[1] == "fanta")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":boire fanta");
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                #endregion
                #region manger
                case "manger":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == "pain")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":manger baguette");
                        }
                        else if (ReceivedData[1] == "sucette")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":manger sucette");
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                #endregion
                #region medicament
                case "medicament":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == "doliprane")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":medicament doliprane");
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                #endregion
                #region fumer
                case "fumer":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        if (ReceivedData[1] == "weed")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":fumer joint");
                        }
                        else if (ReceivedData[1] == "cigarette")
                        {
                            PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":fumer cigarette");
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                #endregion
                #region conduire
                case "conduire":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        if (Client.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && Client.GetHabbo().TravailId != 4 || Client.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && Client.GetHabbo().TravailId == 4 && Client.GetHabbo().Travaille == false || Client.GetHabbo().CurrentRoomId == 80)
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire de véhicule ici.");
                            return;
                        }

                        if(Client.GetHabbo().EventCount != 0 && Client.GetHabbo().EventDay == 0)
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire pendant que vous réalisez une mission.");
                            return;
                        }

                        if (Client.GetHabbo().footballTeam != null)
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire pendant que vous jouez au foot.");
                            return;
                        }

                        if (PlusEnvironment.Purge == true)
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire pendant la purge.");
                            return;
                        }

                        if (Client.GetHabbo().Menotted == true)
                        {
                            Client.SendWhisper("Vous ne pouvez pas utilisé un véhicule pendant que vous êtes menotté.");
                            return;
                        }

                        string[] ReceivedData = Data.Split(',');
                        string Vehicule = ReceivedData[1];
                        if (Vehicule == null)
                            return;

                        if (User.Item_On != 8387 && Vehicule != "pinkHoverboard" && Vehicule != "blackHoverboard" && Vehicule != "whiteHoverboard")
                        {
                            Client.SendMessage(new RoomNotificationComposer("car_notification", "message", "Vous devez être sur la route pour pouvoir conduire un véhicule."));
                            return;
                        }

                        if (Client.GetHabbo().ArmeEquiped != null && Client.GetHabbo().ArmeEquiped != "taser")
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire lorsque vous êtes équipé d'une arme.");
                            return;
                        }

                        if (User.isDemarreCar == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().Conduit != null && Vehicule != Client.GetHabbo().Conduit)
                        {
                            Client.SendWhisper("Vous conduisez déjà un véhicule.");
                            return;
                        }
                        
                        if (User.isTradingItems)
                        {
                            Client.SendWhisper("Vous ne pouvez pas conduire de véhicule pendant que vous faites un échange.");
                            return;
                        }

                        if (Client.GetHabbo().getCooldown("conduire"))
                        {
                            Client.SendWhisper("Veuillez patienter");
                            return;
                        }

                        string name;
                        string shortname;
                        int enable;
                        if (Vehicule == "audia8")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().AudiA8 == 0)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = true;
                            User.UltraFastWalking = false;
                            shortname = "audia8";
                            name = "sa Audi A8";
                            enable = 800;
                        }
                        else if (Vehicule == "audia3")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().AudiA3 == 0)
                                return;

                            User.FastWalking = false;
                            User.SuperFastWalking = true;
                            User.UltraFastWalking = false;
                            shortname = "audia3";
                            name = "sa Audi A3";
                            enable = 802;
                        }
                        else if (Vehicule == "porsche911")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().Porsche911 == 0)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = true;
                            shortname = "porsche911";
                            name = "sa Porsche 911";
                            enable = 2000;
                        }
                        else if (Vehicule == "fiatpunto")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().FiatPunto == 0)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = true;
                            User.UltraFastWalking = false;
                            shortname = "fiatpunto";
                            name = "sa Fiat Punto";
                            enable = 21;
                        }
                        else if (Vehicule == "volkswagenjetta")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().VolkswagenJetta == 0)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = true;
                            User.UltraFastWalking = false;
                            shortname = "volkswagenjetta";
                            name = "sa Volkswagen Jetta";
                            enable = 69;
                        }
                        else if (Vehicule == "bmwi8")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().BmwI8 == 0)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = true;
                            shortname = "bmwi8";
                            name = "sa BMW i8";
                            enable = 814;
                        }
                        else if (Vehicule == "police")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().TravailId != 4 || Client.GetHabbo().RankId == 1 || Client.GetHabbo().Travaille == false)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = true;
                            shortname = "police";
                            name = "sa voiture de fonction";
                            enable = 19;
                        }
                        else if (Vehicule == "gouvernement")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().TravailId != 18 || Client.GetHabbo().Travaille == false || Client.GetHabbo().RankId == 1)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = true;
                            shortname = "gouvernement";
                            name = "sa voiture de fonction";
                            enable = 510;
                        }
                        else if (Vehicule == "avion")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {

                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().TravailId != 18 || Client.GetHabbo().Travaille == false || Client.GetHabbo().RankId == 7 || Client.GetHabbo().RankId == 8)
                            {
                                return;
                            }

                            User.FastWalking = false;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = true;
                            shortname = "avion";
                            name = "son avion de fonction";
                            enable = 176;
                        }
                        else if (Vehicule == "pinkHoverboard")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().Confirmed == 0)
                            {
                                return;
                            }

                            Client.GetHabbo().addCooldown("conduire", 3000);
                            User.FastWalking = true;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = false;
                            shortname = "pinkHoverboard";
                            name = "hoverboard rose";
                            enable = 505;
                            Client.GetHabbo().Conduit = Vehicule;
                            User.OnChat(User.LastBubble, "* Monte sur son " + name + " *", true);
                            User.GetClient().GetHabbo().Effects().ApplyEffect(enable);
                            return;
                        }
                        else if (Vehicule == "whiteHoverboard")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().WhiteHoverboard == 0)
                                return;

                            Client.GetHabbo().addCooldown("conduire", 3000);
                            User.FastWalking = true;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = false;
                            shortname = "whiteHoverboard";
                            name = "hoverboard blanc";
                            enable = 191;
                            Client.GetHabbo().Conduit = Vehicule;
                            User.OnChat(User.LastBubble, "* Monte sur son " + name + " *", true);
                            User.GetClient().GetHabbo().Effects().ApplyEffect(enable);
                            return;
                        }
                        else if (Vehicule == "blackHoverboard")
                        {
                            if (Client.GetHabbo().Conduit == Vehicule)
                            {
                                Client.GetHabbo().stopConduire();
                                return;
                            }

                            if (Client.GetHabbo().Confirmed == 0)
                            {
                                return;
                            }

                            Client.GetHabbo().addCooldown("conduire", 3000);
                            User.FastWalking = true;
                            User.SuperFastWalking = false;
                            User.UltraFastWalking = false;
                            shortname = "blackHoverboard";
                            name = "hoverboard noir";
                            enable = 504;
                            Client.GetHabbo().Conduit = Vehicule;
                            User.OnChat(User.LastBubble, "* Monte sur son " + name + " *", true);
                            User.GetClient().GetHabbo().Effects().ApplyEffect(enable);
                            return;
                        }
                        else
                        {
                            return;
                        }

                        Client.GetHabbo().addCooldown("conduire", 3000);
                        int AppartId = Client.GetHabbo().CurrentRoomId;
                        User.isDemarreCar = true;
                        Socket.Send("voiture;demarre");
                        User.OnChat(User.LastBubble, "* Met les clefs et démarre " + name + " *", true);
                        User.GetClient().GetHabbo().Effects().ApplyEffect(enable);

                        System.Timers.Timer timer2 = new System.Timers.Timer(2500);
                        timer2.Interval = 2500;
                        timer2.Elapsed += delegate
                       {
                           Client.GetHabbo().Conduit = shortname;
                           User.OnChat(User.LastBubble, "* Commence à conduire " + name + " *", true);

                           User.isDemarreCar = false;
                           timer2.Stop();
                       };
                        timer2.Start();
                    }
                    break;
                #endregion
                #region arme
                case "arme":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Hopital == 1 || Client.GetHabbo().Prison != 0)
                            return;

                        string[] ReceivedData = Data.Split(',');
                        string Arme = ReceivedData[1];
                        PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Client, ":eq " + Arme);
                    }
                    break;
                    #endregion
            }
        }
    }
}
