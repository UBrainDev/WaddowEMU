using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RouletteCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 16 && Session.GetHabbo().RankId > 1 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Lancer la roulette"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if(PlusEnvironment.RouletteTurning == true)
            {
                Session.SendWhisper("Veuillez patienter, la roulette tourne.");
                return;
            }

            if (PlusEnvironment.RouletteEtat == 0)
            {
                Room CurrentRoom = Session.GetHabbo().CurrentRoom;

                int CountMiser = 0;
                foreach (RoomUser UserInRoom in CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                        continue;

                    if (UserInRoom.participateRoulette == true)
                        CountMiser += 1;
                }

                if(CountMiser == 0)
                {
                    Session.SendWhisper("Veuillez patienter que quelqu'un mise avant de lancer la roulette.");
                    return;
                }

                PlusEnvironment.RouletteEtat = 1;
                PlusEnvironment.RouletteTurning = true;

                Random rouletteNumber = new Random();
                int winNumber = rouletteNumber.Next(0, 37);

                foreach (RoomUser UserInRoom in CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "roulette_casino;spinTo;" + winNumber);
                }
                User.OnChat(User.LastBubble, "* Lance la roulette *", true);

                System.Timers.Timer timer1 = new System.Timers.Timer(9000);
                timer1.Interval = 9000;
                timer1.Elapsed += delegate
                {
                    int[] red = { 32, 19, 21, 25, 34, 27, 36, 30, 23, 5, 16, 1, 14, 9, 18, 7, 12, 3 };

                    foreach (RoomUser UserInRoom in CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                    {
                        if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                            continue;
                        
                        if (UserInRoom.participateRoulette == true)
                        {
                            if (UserInRoom.numberRoulette == 0 && winNumber == 0)
                            {
                                int WinJetons = UserInRoom.miseRoulette * 10;
                                UserInRoom.GetClient().GetHabbo().Casino_Jetons += WinJetons;
                                UserInRoom.GetClient().GetHabbo().updateCasinoJetons();
                                UserInRoom.OnChat(UserInRoom.LastBubble, "* Gagne " + (WinJetons - UserInRoom.miseRoulette) + " jeton(s) à la roulette (sur une mise de " + UserInRoom.miseRoulette + ") *", true);
                            }
                            else if (UserInRoom.numberRoulette == winNumber && UserInRoom.numberRoulette != 0 && winNumber != 0)
                            {
                                int WinJetons = UserInRoom.miseRoulette * 3;
                                UserInRoom.GetClient().GetHabbo().Casino_Jetons += WinJetons;
                                UserInRoom.GetClient().GetHabbo().updateCasinoJetons();
                                UserInRoom.OnChat(UserInRoom.LastBubble, "* Gagne " + (WinJetons - UserInRoom.miseRoulette) + " jeton(s) à la roulette (sur une mise de " + UserInRoom.miseRoulette + ") *", true);
                            }
                            else if (red.Contains(UserInRoom.numberRoulette) && red.Contains(winNumber) && UserInRoom.numberRoulette != 0 && winNumber != 0 || !red.Contains(UserInRoom.numberRoulette) && !red.Contains(winNumber) && UserInRoom.numberRoulette != 0 && winNumber != 0)
                            {
                                int WinJetons = Convert.ToInt32(UserInRoom.miseRoulette * 1.5);
                                UserInRoom.GetClient().GetHabbo().Casino_Jetons += WinJetons;
                                UserInRoom.GetClient().GetHabbo().updateCasinoJetons();
                                UserInRoom.OnChat(UserInRoom.LastBubble, "* Gagne " + (WinJetons - UserInRoom.miseRoulette) + " jeton(s) à la roulette (sur une mise de " + UserInRoom.miseRoulette + ") *", true);
                            }
                            else
                            {
                                UserInRoom.GetClient().SendWhisper("Vous n'avez rien gagné à la roulette.");
                            }

                            UserInRoom.participateRoulette = false;
                            UserInRoom.numberRoulette = 0;
                            UserInRoom.miseRoulette = 0;
                        }
                    }

                    PlusEnvironment.RouletteTurning = false;
                    timer1.Stop();
                };
                timer1.Start();
                return;
            }
            else if(PlusEnvironment.RouletteEtat != 0)
            {
                User.OnChat(User.LastBubble, "* Faites vos jeux *", true);
                PlusEnvironment.RouletteEtat = 0;

                foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "roulette_casino;reset");
                }
                return;
            }
        }
    }
}