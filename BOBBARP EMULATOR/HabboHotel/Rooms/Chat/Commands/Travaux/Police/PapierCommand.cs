using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class PapierCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().Travaille == true && Session.GetHabbo().CurrentRoomId == 48)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Faire les papiers d'identité d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :papier <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown(""))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 3 || Math.Abs(User.X - TargetUser.X) > 3 || !TargetUser.Statusses.ContainsKey("sit"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être assis devant vous pour que vous puissez lui faire ses papiers.");
                return;
            }

            if (TargetClient.GetHabbo().Carte == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà sa carte d'identité.");
                return;
            }

            Session.GetHabbo().addCooldown("papiers", 9500);
            TargetUser.Frozen = true;
            User.OnChat(User.LastBubble, "* Fait la carte d'identité de " + TargetClient.GetHabbo().Username + " *", true);
            System.Timers.Timer timer2 = new System.Timers.Timer(2000);
            timer2.Interval = 2000;
            timer2.Elapsed += delegate
            {
                if (TargetClient != null && Session.GetHabbo().Travaille == true && Session.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().Carte == 0)
                {
                    User.OnChat(User.LastBubble, "* Prend les informations personnelles de " + TargetClient.GetHabbo().Username + " comme son nom, son adresse... *", true);
                }
                timer2.Stop();
            };
            timer2.Start();

            System.Timers.Timer timer3 = new System.Timers.Timer(5000);
            timer3.Interval = 5000;
            timer3.Elapsed += delegate
            {
                if (TargetClient != null && Session.GetHabbo().Travaille == true && Session.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().Carte == 0)
                {
                    User.OnChat(User.LastBubble, "* Prend les empreintes de " + TargetClient.GetHabbo().Username + " *", true);
                }
                timer3.Stop();
            };
            timer3.Start();

            System.Timers.Timer timer4 = new System.Timers.Timer(9000);
            timer4.Interval = 8000;
            timer4.Elapsed += delegate
            {
                if (TargetClient != null && Session.GetHabbo().Travaille == true && Session.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().CurrentRoomId == 48 && TargetClient.GetHabbo().Carte == 0)
                {
                    User.OnChat(User.LastBubble, "* Fini de réaliser la carte d'identité de " + TargetClient.GetHabbo().Username + " *", true);
                    TargetClient.GetHabbo().Carte = 1;
                    TargetClient.GetHabbo().updateCarte();
                    TargetUser.Frozen = false;
                }
                timer4.Stop();
            };
            timer4.Start();
        }
    }
}