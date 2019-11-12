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
    class LeverCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 7 && Session.GetHabbo().Travaille == true)
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
            get { return "Lever un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :lever <pseudonyme>");
                return;
            }

            string Username = Params[1];
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoomId != Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (!TargetUser.Statusses.ContainsKey("lay"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être couché pour pouvoir le lever.");
                return;
            }

            string PushDirection = "down";
            if (TargetClient.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId && (Math.Abs(User.X - TargetUser.X) < 3 && Math.Abs(User.Y - TargetUser.Y) < 3))
            {
                if (User.RotBody == 0)
                    PushDirection = "up";
                if (User.RotBody == 2)
                    PushDirection = "right";
                if (User.RotBody == 4)
                    PushDirection = "down";
                if (User.RotBody == 6)
                    PushDirection = "left";

                if (PushDirection == "up")
                    TargetUser.MoveTo(User.X, User.Y - 1);

                if (PushDirection == "right")
                    TargetUser.MoveTo(User.X + 1, User.Y);

                if (PushDirection == "down")
                    TargetUser.MoveTo(User.X, User.Y + 1);

                if (PushDirection == "left")
                    TargetUser.MoveTo(User.X - 1, User.Y);

                User.OnChat(User.LastBubble, "* Lève " + TargetClient.GetHabbo().Username + " *", true);
                return;
            }
            else
            {
                Session.SendWhisper("Veuillez vous rapprocher de " + TargetClient.GetHabbo().Username + " pour pouvoir le lever.");
                return;
            }
        }
    }
}