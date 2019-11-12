using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class SaladeCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 2)
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
            get { return "Démarrer une salade."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if(PlusEnvironment.SaladeAttente == true || PlusEnvironment.Salade != 0)
            {
                Session.SendWhisper("Une salade est déjà organisée.");
                return;
            }

            if(Session.GetHabbo().CurrentRoomId == 1 || Session.GetHabbo().CurrentRoomId == 3 || Session.GetHabbo().CurrentRoomId == 18 || Session.GetHabbo().CurrentRoomId == 20)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer de salade dans le centre ville, dans l'hôpital ou dans la prison et sa cours.");
                return;
            }

            if (Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO"))
            {
                Session.SendWhisper("Vous ne pouvez pas organiser une salade dans un ghetto.");
                return;
            }

            if(PlusEnvironment.Purge == true || PlusEnvironment.ManVsZombie != 0 || PlusEnvironment.ManVsZombieLoading == true)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer une salade car un autre événement est en cours.");
                return;
            }

            PlusEnvironment.SaladeAttente = true;
            int AppartId = Session.GetHabbo().CurrentRoomId;
            Room AppartSalade = Session.GetHabbo().CurrentRoom;

            PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 5 minutes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
            PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Vous ne perdrez pas de crédits pendant la salade.");
            System.Timers.Timer timer2 = new System.Timers.Timer(120000);
            timer2.Interval = 120000;
            timer2.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 3 minutes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer2.Stop();
            };
            timer2.Start();
            
            System.Timers.Timer timer3 = new System.Timers.Timer(180000);
            timer3.Interval = 180000;
            timer3.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 2 minutes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer3.Stop();
            };
            timer3.Start();

            System.Timers.Timer timer4 = new System.Timers.Timer(240000);
            timer4.Interval = 240000;
            timer4.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 1 minute dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer4.Stop();
            };
            timer4.Start();

            System.Timers.Timer timer5 = new System.Timers.Timer(270000);
            timer5.Interval = 270000;
            timer5.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 30 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer5.Stop();
            };
            timer5.Start();

            System.Timers.Timer timer6 = new System.Timers.Timer(280000);
            timer6.Interval = 280000;
            timer6.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 20 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer6.Stop();
            };
            timer6.Start();

            System.Timers.Timer timer7 = new System.Timers.Timer(290000);
            timer7.Interval = 290000;
            timer7.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 10 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer7.Stop();
            };
            timer7.Start();

            System.Timers.Timer timer8 = new System.Timers.Timer(295000);
            timer8.Interval = 295000;
            timer8.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 5 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer8.Stop();
            };
            timer8.Start();

            System.Timers.Timer timer9 = new System.Timers.Timer(296000);
            timer9.Interval = 296000;
            timer9.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 4 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer9.Stop();
            };
            timer9.Start();

            System.Timers.Timer timer10 = new System.Timers.Timer(297000);
            timer10.Interval = 297000;
            timer10.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 3 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer10.Stop();
            };
            timer10.Start();

            System.Timers.Timer timer11 = new System.Timers.Timer(298000);
            timer11.Interval = 298000;
            timer11.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 2 secondes dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer11.Stop();
            };
            timer11.Start();

            System.Timers.Timer timer12 = new System.Timers.Timer(299000);
            timer12.Interval = 299000;
            timer12.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade va commencer dans 1 seconde dans l'appartement [" + AppartSalade.Id + "] " + AppartSalade.Name + ".");
                timer12.Stop();
            };
            timer12.Start();

            System.Timers.Timer timer13 = new System.Timers.Timer(300000);
            timer13.Interval = 300000;
            timer13.Elapsed += delegate
            {
                PlusEnvironment.Salade = AppartId;
                PlusEnvironment.SaladeAttente = false;
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La salade a démarrée.");
                timer13.Stop();
            };
            timer13.Start();
        }
    }
}