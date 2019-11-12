using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RestartCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Username == "ADMIN-Soubes" || Session.GetHabbo().Username == "ADMIN-Soubes") ;
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Rédémarrer le serveur."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if(PlusEnvironment.restart == true)
            {
                PlusEnvironment.restart = false;
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le redémarrage du serveur a été annulé.");
                return;
            }
            
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            PlusEnvironment.restart = true;
            User.OnChat(User.LastBubble, "* Effectue un redémarrage du serveur *", true);
            PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Petit redem de l'ému . Faites pas chier c pour une maj soyez content .Le serveur va redémarrer dans 2 minutes.");

            System.Timers.Timer timer1 = new System.Timers.Timer(60000);
            timer1.Interval = 60000;
            timer1.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 1 minute.");
                }
                timer1.Stop();
            };
            timer1.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer(90000);
            timer2.Interval = 90000;
            timer2.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 30 secondes.");
                }
                timer2.Stop();
            };
            timer2.Start();

            System.Timers.Timer timer3 = new System.Timers.Timer(100000);
            timer3.Interval = 100000;
            timer3.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 20 secondes.");
                }
                timer3.Stop();
            };
            timer3.Start();

            System.Timers.Timer timer4 = new System.Timers.Timer(110000);
            timer4.Interval = 110000;
            timer4.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 10 secondes.");
                }
                timer4.Stop();
            };
            timer4.Start();

            System.Timers.Timer timer5 = new System.Timers.Timer(115000);
            timer5.Interval = 115000;
            timer5.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 5 secondes.");
                }
                timer5.Stop();
            };
            timer5.Start();

            System.Timers.Timer timer6 = new System.Timers.Timer(116000);
            timer6.Interval = 116000;
            timer6.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 4 secondes.");
                }
                timer6.Stop();
            };
            timer6.Start();

            System.Timers.Timer timer7 = new System.Timers.Timer(117000);
            timer7.Interval = 117000;
            timer7.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 3 secondes.");
                }
                timer7.Stop();
            };
            timer7.Start();

            System.Timers.Timer timer8 = new System.Timers.Timer(118000);
            timer8.Interval = 118000;
            timer8.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 2 secondes.");
                }
                timer8.Stop();
            };
            timer8.Start();

            System.Timers.Timer timer9 = new System.Timers.Timer(119000);
            timer9.Interval = 119000;
            timer9.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Le serveur va redémarrer dans 1 seconde.");
                }
                timer9.Stop();
            };
            timer9.Start();

            System.Timers.Timer Reboot = new System.Timers.Timer(119000);
            Reboot.Interval = 119000;
            Reboot.Elapsed += delegate
            {
                if (PlusEnvironment.restart == true)
                {
                    PlusEnvironment.restartServeur();
                }
                Reboot.Stop();
            };
            Reboot.Start();
        }
    }
}