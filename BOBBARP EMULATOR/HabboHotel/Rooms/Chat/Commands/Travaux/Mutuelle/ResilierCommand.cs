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
    class ResilierCommand : IChatCommand
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
            get { return "Résilier le contrat d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :resilier <pseudonyme>");
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
                Session.SendWhisper("Vous ne pouvez pas résilier le contrat de " + TargetClient.GetHabbo().Username + " car vous n'êtes pas connecté au réseau de Harmonie Mutuelle.");
                return;
            }

            if (TargetClient.GetHabbo().Mutuelle == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de contrat chez Harmonie Mutuelle.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Propose à " + TargetClient.GetHabbo().Username + " de résilier son contrat mutuelle *", true);
            TargetUser.Transaction = "resilier_mutuelle";
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> vous propose de résilier votre <b>contrat d'assurance mutuelle</b> qui a pour date d'expiration le <b>" + TargetClient.GetHabbo().MutuelleDate.Day + "/" + TargetClient.GetHabbo().MutuelleDate.Month + "/" + TargetClient.GetHabbo().MutuelleDate.Year + " à " + TargetClient.GetHabbo().MutuelleDate.Hour + ":" + TargetClient.GetHabbo().MutuelleDate.Minute + "</b>.;0");
        }
    }
}