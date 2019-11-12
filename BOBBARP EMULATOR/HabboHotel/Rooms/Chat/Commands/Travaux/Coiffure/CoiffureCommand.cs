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
    class CoiffureCommand : IChatCommand
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
            get { return "Coiffer un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :coiffure <pseudonyme>");
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
                Session.SendWhisper("Vous ne pouvez pas vous couper vous même les cheveux.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if(TargetUser.cheveuxPropre == false)
            {
                Session.SendWhisper("Vous devez laver les cheveux de " + TargetClient.GetHabbo().Username + " avant de pouvoir lui faire une coiffure.");
                return;
            }

            if (User.usernameCoiff == TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous coiffez déjà " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            if (TargetUser.usernameCoiff != null)
            {
                Session.SendWhisper("Un coiffeur s'occupe déjà de " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            if (!TargetUser.Statusses.ContainsKey("sit"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être assis devant le mirroir.");
                return;
            }

            if (Math.Abs(User.X - TargetUser.X) != 1 || User.Y != TargetUser.Y)
            {
                Session.SendWhisper("Vous devez être derrière " + TargetClient.GetHabbo().Username + " pour pouvoir lui faire une coiffure.");
                return;
            }

            if(TargetClient.GetHabbo().Coiffure == 0 && TargetClient.GetHabbo().Confirmed == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit payer une coiffure pour pouvoir être coiffé.");
                return;
            }

            if (TargetClient.GetHabbo().Travaille == true)
            {
                TargetClient.GetHabbo().stopWork();
            }

            User.usernameCoiff = TargetClient.GetHabbo().Username;
            TargetUser.usernameCoiff = Session.GetHabbo().Username;
            User.OnChat(User.LastBubble, "* Détermine la coiffure à réaliser pour " + TargetClient.GetHabbo().Username + " *", true);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "coiffure;" + TargetClient.GetHabbo().Gender);
        }
    }
}