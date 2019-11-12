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
    class LaverCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 15 && Session.GetHabbo().Travaille == true)
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
            get { return "Laver les cheveux d'un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :laver <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if(TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous laver vous même les cheveux.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetClient.GetHabbo().Confirmed == 0 && TargetClient.GetHabbo().Coiffure == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas acheté de coiffure.");
                return;
            }

            if (TargetUser.cheveuxPropre == true)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà les cheveux propres.");
                return;
            }

            if (User.usernameCoiff == TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous lavez déjà la tête de " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            if (TargetUser.usernameCoiff != null)
            {
                Session.SendWhisper("Un coiffeur s'occupe déjà de " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            if (!TargetUser.Statusses.ContainsKey("sit"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être assis devant le lavabo.");
                return;
            }

            if (Math.Abs(User.X - TargetUser.X) != 2 || User.Y != TargetUser.Y)
            {
                Session.SendWhisper("Vous devez être derrière le lavabo de " + TargetClient.GetHabbo().Username + " pour pouvoir lui laver les cheveux.");
                return;
            }

            if (TargetClient.GetHabbo().Travaille == true)
            {
                TargetClient.GetHabbo().stopWork();
            }

            User.usernameCoiff = TargetClient.GetHabbo().Username;
            TargetUser.usernameCoiff = Session.GetHabbo().Username;
            User.OnChat(User.LastBubble, "* Penche la tête de " + TargetClient.GetHabbo().Username + " sur le lavabo *", true);
        }
    }
}