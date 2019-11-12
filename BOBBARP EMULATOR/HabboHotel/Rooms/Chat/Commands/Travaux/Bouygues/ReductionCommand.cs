/*using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ReductionCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 12 || Session.GetHabbo().Travaille == true || Client.GetHabbo().RankId == 1)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<produit> <prix>"; }
        }

        public string Description
        {
            get { return "Faire une réduction sur un produit . EN PARLER AVANT AVEC UN MINISTRE"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().TravailId != 12 || Session.GetHabbo().Travaille == false)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :reduction <produit> <montant>");
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
            if (User.ConnectedMetier == false)
            {
                Session.SendWhisper("Vous devez vous connecter au réseau de Hair Salon avant de pouvoir vendre des bons de coiffure.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            if (TargetClient.GetHabbo().Confirmed == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas besoin d'acheter une coiffure car il est civil confirmé.");
                return;
            }

            if (TargetClient.GetHabbo().Coiffure > 9)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà 10 coiffures à dépenser.");
                return;
            }

            int Prix;
            int Taxe;
            if (TargetClient.GetHabbo().Gender == "f")
            {
                Prix = PlusEnvironment.getPriceOfItem("Coiffure Femme");
                Taxe = PlusEnvironment.getTaxeOfItem("Coiffure Femme");
            }
            else
            {
                Prix = PlusEnvironment.getPriceOfItem("Coiffure Homme");
                Taxe = PlusEnvironment.getTaxeOfItem("Coiffure Homme");
            }
            User.OnChat(User.LastBubble, "* Vend un bon de coiffure à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "coiffure:" + Prix + ":" + Taxe;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>bon de coiffure</b> pour <b>" + Prix + " crédits</b> dont <b>" + Taxe + "</b> qui iront à l'État.;" + Prix);
        }
    }
}*/