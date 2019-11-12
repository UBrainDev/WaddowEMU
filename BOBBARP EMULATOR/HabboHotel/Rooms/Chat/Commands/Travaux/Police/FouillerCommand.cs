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
    class FouillerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true)
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
            get { return "Fouiller un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :fouiller <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous fouiller vous même.");
                return;
            }

            if (Session.GetHabbo().getCooldown("fouiller_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }


            if (TargetClient.GetHabbo().TravailId == 4 || TargetClient.GetHabbo().TravailId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas fouiller ce civil.");
                return;
            }

            if (TargetClient.GetHabbo().Prison != 0 || TargetClient.GetHabbo().Hopital == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas fouiller une personne hospitalisé ou emprisonné.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetClient.GetHabbo().Menotted != true && TargetUser.Tased != true)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être immobilisé avant de pouvoir le fouiller.");
                return;
            }
            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être à coté de vous pour pouvoir le fouiller.");
                return;
            }

            Session.GetHabbo().addCooldown("fouiller_command", 3000);
            User.fouillerUser = TargetClient.GetHabbo().Username;
            User.OnChat(User.LastBubble, "* Fouille " + TargetClient.GetHabbo().Username + " *", true);
            StringBuilder FouillerInfo = new StringBuilder();
            FouillerInfo.Append("Biens illégaux retrouvés sur " + TargetClient.GetHabbo().Username + " :\r\r");
            if (TargetClient.GetHabbo().Permis_arme == 0)
            {
                if (TargetClient.GetHabbo().Sabre == 1)
                {
                    FouillerInfo.Append("Sabre\r");
                }

                if (TargetClient.GetHabbo().Batte == 1)
                {
                    FouillerInfo.Append("Batte\r");
                }

                if (TargetClient.GetHabbo().Ak47 == 1)
                {
                    FouillerInfo.Append("Ak47\r");
                }

                if (TargetClient.GetHabbo().Uzi == 1)
                {
                    FouillerInfo.Append("Uzi\r");
                }
            }

            if (TargetClient.GetHabbo().Weed > 0)
            {
                FouillerInfo.Append("Weed : " + TargetClient.GetHabbo().Weed + "g");
            }

            Session.SendNotification(FouillerInfo.ToString());
        }
    }
}