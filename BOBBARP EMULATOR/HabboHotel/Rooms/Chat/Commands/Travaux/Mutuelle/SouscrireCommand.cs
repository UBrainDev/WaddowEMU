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
    class SouscrireCommand : IChatCommand
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
            get { return "Souscrire un civil à un contrat de mutuelle"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 4)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :souscrire <pseudonyme> <type_mutuelle> <temps en jour>");
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
                Session.SendWhisper("Vous ne pouvez pas souscrire " + TargetClient.GetHabbo().Username + " à une mutuelle car vous n'êtes pas connecté au réseau de Harmonie Mutuelle.");
                return;
            }

            int num;
            if (!Int32.TryParse(Params[2], out num) || Params[2].StartsWith("0") || Convert.ToInt32(Params[2]) < 1 || Convert.ToInt32(Params[2]) > 4)
            {
                Session.SendWhisper("Le type de mutuelle est invalide.");
                return;
            }

            if (!Int32.TryParse(Params[3], out num) || Params[3].StartsWith("0") || Convert.ToInt32(Params[3]) < 1)
            {
                Session.SendWhisper("La durée en jour est invalide.");
                return;
            }

            if (Convert.ToInt32(Params[3]) < 7)
            {
                Session.SendWhisper("La durée de souscription doit être de 7 jours minimum.");
                return;
            }

            if (Convert.ToInt32(Params[3]) > 31)
            {
                Session.SendWhisper("La durée de souscription ne peut pas excéder 31 jours.");
                return;
            }

            if (TargetClient.GetHabbo().Mutuelle != 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un contrat.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            int taux = 0;
            int prix = 0;
            int taxe = 0;

            if (Convert.ToInt32(Params[2]) == 1)
            {
                taux = 25;
                prix = PlusEnvironment.getPriceOfItem("Mutuelle 25") * Convert.ToInt32(Params[3]);
                taxe = PlusEnvironment.getTaxeOfItem("Mutuelle 25") * Convert.ToInt32(Params[3]);
            }
            else if (Convert.ToInt32(Params[2]) == 2)
            {
                taux = 50;
                prix = PlusEnvironment.getPriceOfItem("Mutuelle 50") * Convert.ToInt32(Params[3]);
                taxe = PlusEnvironment.getTaxeOfItem("Mutuelle 50") * Convert.ToInt32(Params[3]);
            }
            if (Convert.ToInt32(Params[2]) == 3)
            {
                taux = 75;
                prix = PlusEnvironment.getPriceOfItem("Mutuelle 75") * Convert.ToInt32(Params[3]);
                taxe = PlusEnvironment.getTaxeOfItem("Mutuelle 75") * Convert.ToInt32(Params[3]);
            }
            if (Convert.ToInt32(Params[2]) == 4)
            {
                taux = 100;
                prix = PlusEnvironment.getPriceOfItem("Mutuelle 100") * Convert.ToInt32(Params[3]);
                taxe = PlusEnvironment.getTaxeOfItem("Mutuelle 100") * Convert.ToInt32(Params[3]);
            }

            User.OnChat(User.LastBubble, "* Souscrit à " + TargetClient.GetHabbo().Username + " un contrat d'assurance mutuelle à un taux de " + taux + "% pour " + Params[3] + " jour(s) *", true);
            TargetUser.Transaction = "mutuelle:" + Params[2] + ":" + Params[3] + ":" + prix + ":" + taxe;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> vous propose un <b>contrat d'assurance mutuelle</b> à un taux de <b>" + taux + "%</b> valide pendant <b>" + Params[3] + " jour(s)</b> pour <b>" + prix + " crédits</b> dont <b>" + taxe + "</b> qui iront à l'État.;" + prix);
        }
    }
}