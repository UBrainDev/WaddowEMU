using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class PurgeCommand : IChatCommand
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
            get { return "Démarrer/arrêter la purge."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (PlusEnvironment.PurgeLoading == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if (PlusEnvironment.Salade != 0 || PlusEnvironment.SaladeAttente == true || PlusEnvironment.ManVsZombie != 0 || PlusEnvironment.ManVsZombieLoading == false)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer une purge car un autre événement est en cours.");
                return;
            }

            if (PlusEnvironment.Purge == true)
            {
                PlusEnvironment.gangWinPurge();
                PlusEnvironment.Purge = false;
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge est terminée.");
                PlusEnvironment.GetGame().GetClientManager().endPurge();
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            PlusEnvironment.PurgeLoading = true;
            User.OnChat(User.LastBubble, "* Annonce le début de la purge dans 1 minute *", true);
            PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 1 minute.");
            PlusEnvironment.GetGame().GetClientManager().PurgeSound(false);
            // Room.GetRoomUserManager().displayBotByForce(Session, 1, 20, 3, 12, 2, "Je viens défendre cette zone !");
            //Room.GetRoomUserManager().displayBotByForce(Session, 1, 18, 19, 24, 2, "Je viens défendre cette zone !");
            //Room.GetRoomUserManager().displayBotByForce(Session, 1, 3, 13, 23, 2, "Je viens défendre cette zone !");

            System.Timers.Timer timer2 = new System.Timers.Timer(30000);
            timer2.Interval = 30000;
            timer2.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 30 secondes.");
                timer2.Stop();
            };
            timer2.Start();

            System.Timers.Timer timer3 = new System.Timers.Timer(40000);
            timer3.Interval = 40000;
            timer3.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 20 secondes.");

                timer3.Stop();
            };
            timer3.Start();

            System.Timers.Timer timer4 = new System.Timers.Timer(50000);
            timer4.Interval = 50000;
            timer4.Elapsed += delegate
            {

                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 10 secondes.");

                timer4.Stop();
            };
            timer4.Start();

            System.Timers.Timer timer5 = new System.Timers.Timer(55000);
            timer5.Interval = 55000;
            timer5.Elapsed += delegate
            {

                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 5 secondes.");

                timer5.Stop();
            };
            timer5.Start();

            System.Timers.Timer timer6 = new System.Timers.Timer(56000);
            timer6.Interval = 56000;
            timer6.Elapsed += delegate
            {

                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 4 secondes.");

                timer6.Stop();
            };
            timer6.Start();

            System.Timers.Timer timer7 = new System.Timers.Timer(57000);
            timer7.Interval = 57000;
            timer7.Elapsed += delegate
            {

                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 3 secondes.");

                timer7.Stop();
            };
            timer7.Start();

            System.Timers.Timer timer8 = new System.Timers.Timer(58000);
            timer8.Interval = 58000;
            timer8.Elapsed += delegate
            {

                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 2 secondes.");

                timer8.Stop();
            };
            timer8.Start();

            System.Timers.Timer timer9 = new System.Timers.Timer(59000);
            timer9.Interval = 59000;
            timer9.Elapsed += delegate
            {
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge va démarrer dans 1 seconde.");
                timer9.Stop();
            };
            timer9.Start();

            System.Timers.Timer Purge = new System.Timers.Timer(60000);
            Purge.Interval = 60000;
            Purge.Elapsed += delegate
            {
                PlusEnvironment.PurgeLoading = false;
                PlusEnvironment.Purge = true;
                PlusEnvironment.GetGame().GetClientManager().sendServerMessage("La purge a démarré.");
                PlusEnvironment.GetGame().GetClientManager().PurgeSound(true);
                Purge.Stop();
            };
            Purge.Start();
        }
    }
}