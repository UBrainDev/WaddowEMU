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
    class LacrymoCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8)
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
            get { return "Gazer un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :lacrymo <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas utiliser du gaz lacrymogène sur vous même.");
                return;
            }

            if (TargetClient.GetHabbo().Blur > 8)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà reçu une dose conséquente de gaz lacrymogène dans les yeux.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 2 || Math.Abs(User.X - TargetUser.X) > 2)
            {
                User.OnChat(User.LastBubble, "* Sort sa bombe de gaz lacrymogène *", true);
                User.OnChat(User.LastBubble, "* Tente de gazer les yeux de " + TargetClient.GetHabbo().Username + " mais n'y parvient pas *", true);
                User.OnChat(User.LastBubble, "* Range sa bombe *", true);
                return;
            }

            User.OnChat(User.LastBubble, "* Sort sa bombe de gaz lacrymogène *", true);
            User.OnChat(User.LastBubble, "* Tente de gazer les yeux de " + TargetClient.GetHabbo().Username + " *", true);
            System.Timers.Timer timer1 = new System.Timers.Timer(2000);
            timer1.Interval = 2000;
            timer1.Elapsed += delegate
            {
                if (Math.Abs(User.Y - TargetUser.Y) < 3 || Math.Abs(User.X - TargetUser.X) < 3 && Session.GetHabbo().CurrentRoom == TargetClient.GetHabbo().CurrentRoom)
                {
                    User.OnChat(User.LastBubble, "* Parvient à gazer " + TargetClient.GetHabbo().Username + " *", true);
                    TargetClient.GetHabbo().Blur = TargetClient.GetHabbo().Blur + 2;
                    TargetClient.GetHabbo().updateBlur();
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "blur;" + TargetClient.GetHabbo().Blur);
                }
                else
                {
                    User.OnChat(User.LastBubble, "* Ne parvient pas à le gazer *", true);
                    User.OnChat(User.LastBubble, "* Range sa bombe *", true);
                }
                timer1.Stop();
            };
            timer1.Start();
        }
    }
}