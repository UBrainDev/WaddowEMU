using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class MutuelleCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 6 && Session.GetHabbo().Travaille == true)
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
            get { return "Savoir le contrat d'un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :mutuelle <pseudonyme>");
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
            if (User.ConnectedMetier == false)
            {
                Session.SendWhisper("Vous ne pouvez pas consulter le contrat de " + TargetClient.GetHabbo().Username + " car vous n'êtes pas connecté au réseau de Harmonie Mutuelle.");
                return;
            }

            if (TargetClient.GetHabbo().Mutuelle == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de contrat chez Harmonie Mutuelle.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            User.OnChat(User.LastBubble, "* Consulte le contrat d'assurance mutuelle de " + TargetClient.GetHabbo().Username + " *", true);
            if (TargetClient.GetHabbo().Mutuelle == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a un contrat d'assurance mutuelle à un taux de 25% expirant le " + TargetClient.GetHabbo().MutuelleDate.Day + "/" + TargetClient.GetHabbo().MutuelleDate.Month + "/" + TargetClient.GetHabbo().MutuelleDate.Year + " à " + TargetClient.GetHabbo().MutuelleDate.Hour + ":" + TargetClient.GetHabbo().MutuelleDate.Minute + ".");
            }
            else if (TargetClient.GetHabbo().Mutuelle == 2)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a un contrat d'assurance mutuelle à un taux de 50% expirant le " + TargetClient.GetHabbo().MutuelleDate.Day + "/" + TargetClient.GetHabbo().MutuelleDate.Month + "/" + TargetClient.GetHabbo().MutuelleDate.Year + " à " + TargetClient.GetHabbo().MutuelleDate.Hour + ":" + TargetClient.GetHabbo().MutuelleDate.Minute + ".");
            }
            else if (TargetClient.GetHabbo().Mutuelle == 3)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a un contrat d'assurance mutuelle à un taux de 75% expirant le " + TargetClient.GetHabbo().MutuelleDate.Day + "/" + TargetClient.GetHabbo().MutuelleDate.Month + "/" + TargetClient.GetHabbo().MutuelleDate.Year + " à " + TargetClient.GetHabbo().MutuelleDate.Hour + ":" + TargetClient.GetHabbo().MutuelleDate.Minute + ".");
            }
            else if (TargetClient.GetHabbo().Mutuelle == 4)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a un contrat d'assurance mutuelle à un taux de 100% expirant le " + TargetClient.GetHabbo().MutuelleDate.Day + "/" + TargetClient.GetHabbo().MutuelleDate.Month + "/" + TargetClient.GetHabbo().MutuelleDate.Year + " à " + TargetClient.GetHabbo().MutuelleDate.Hour + ":" + TargetClient.GetHabbo().MutuelleDate.Minute + ".");
            }
        }
    }
}