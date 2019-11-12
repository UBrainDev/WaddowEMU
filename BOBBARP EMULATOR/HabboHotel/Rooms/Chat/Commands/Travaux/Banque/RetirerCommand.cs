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
    class RetirerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 3 && Session.GetHabbo().Travaille == true)
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
            get { return "Retirer des crédits d'un compte en banque d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :retirer <pseudonyme> <montant>");
                return;
            }

            if (Session.GetHabbo().getCooldown("bank_command"))
            {
                Session.SendWhisper("Veuillez patienter");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User.ConnectedMetier == false)
            {
                Session.SendWhisper("Vous ne pouvez pas retirer des crédits du compte en banque de " + TargetClient.GetHabbo().Username + " sans vous connecter au réseau de la Banque Populaire.");
                return;
            }

            int num;
            if (!Int32.TryParse(Params[2], out num) || Params[2].StartsWith("0"))
            {
                Session.SendWhisper("Le montant n'est pas valide.");
                return;
            }

            if (Convert.ToInt32(Params[2]) > TargetClient.GetHabbo().Banque)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas " + Params[2] + " crédit(s) dans son compte en banque.");
                return;
            }

            Session.GetHabbo().addCooldown("bank_command", 2000);
            TargetClient.GetHabbo().Banque -= Convert.ToInt32(Params[2]);
            TargetClient.GetHabbo().updateBanque();
            TargetClient.GetHabbo().Credits += Convert.ToInt32(Params[2]);
            TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
            User.OnChat(User.LastBubble, "* Retire " + Params[2] + " crédit(s) du compte en banque de " + TargetClient.GetHabbo().Username + " et lui donne *", true);
        }
    }
}