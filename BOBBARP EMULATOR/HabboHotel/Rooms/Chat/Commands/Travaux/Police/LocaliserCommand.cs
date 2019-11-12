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
    class LocaliserCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().Travaille == true)
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
            get { return "Localiser un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :localiser <pseudonyme>");
                return;
            }

            if(Session.GetHabbo().getCooldown("localiser_command"))
            {
                Session.SendWhisper("Veuillez patienter avant de retenter une localisation.");
                return;
            }

            Session.GetHabbo().addCooldown("localiser_command", 30000);
            string Username = Params[1];
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.OnChat(User.LastBubble, "* Tente de localiser " + Username + " *", true);
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || !TargetClient.GetHabbo().InRoom || TargetClient.GetHabbo().Telephone == 0 || TargetClient.GetHabbo().TelephoneEteint == true || TargetClient.GetHabbo().Rank == 8 && Session.GetHabbo().Rank == 1)
            {
                Session.SendWhisper("Impossible de localiser " + Username + ".");
                return;
            }

            if(TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous savez très bien où vous êtes...");
                return;
            }

            Session.SendWhisper(TargetClient.GetHabbo().Username + " se situe à ["+ TargetClient.GetHabbo().CurrentRoom.Id +"] " + TargetClient.GetHabbo().CurrentRoom.Name + ".");
            return;
        }
    }
}