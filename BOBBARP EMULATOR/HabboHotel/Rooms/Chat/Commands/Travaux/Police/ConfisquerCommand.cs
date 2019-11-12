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
    class ConfisquerCommand : IChatCommand
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
            get { return "Confisquer la weed à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :confisquer <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (Session.GetHabbo().getCooldown("confisquer_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if(User.fouillerUser != TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous devez d'abord fouiller " + TargetClient.GetHabbo().Username + " avant de pouvoir confisquer sa weed.");
                return;
            }

            if (TargetClient.GetHabbo().Prison != 0 || TargetClient.GetHabbo().Hopital == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas confisquer la weed d'une personne hospitalisé ou emprisonné.");
                return;
            }
            
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetClient.GetHabbo().Menotted != true && TargetUser.Tased != true)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être immobilisé avant de pouvoir confisquer sa weed.");
                return;
            }

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être à coté de vous pour que vous puissiez confisquer sa weed.");
                return;
            }

            if (TargetClient.GetHabbo().Weed == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de weed.");
                return;
            }

            Session.GetHabbo().addCooldown("confisquer_command", 3000);
            User.OnChat(User.LastBubble, "* Confisque la weed de " + TargetClient.GetHabbo().Username + " et lui donne une amende *", true);
            TargetClient.GetHabbo().takeWeed();
            if (TargetClient.GetHabbo().Permis_arme == 0 && (TargetClient.GetHabbo().Sabre == 1 || TargetClient.GetHabbo().Batte == 1 || TargetClient.GetHabbo().Ak47 == 1 || TargetClient.GetHabbo().Uzi == 1))
            {
                Session.SendWhisper("Pour pouvoir confisquer les armes de " + TargetClient.GetHabbo().Username + ", vous devez l'escorter jusqu'à l'hôtel de police.");
            }
        }
    }
}