using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DeposerCommand : IChatCommand
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
            get { return "<pseudonyme> <montant>"; }
        }

        public string Description
        {
            get { return "Déposer des crédits dans un compte en banque d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :deposer <pseudonyme> <montant>");
                return;
            }

            if (Session.GetHabbo().getCooldown("bank_command"))
            {
                Session.SendWhisper("Veuillez patienter");
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
                Session.SendWhisper("Vous ne pouvez pas déposer des crédits dans le compte en banque de " + TargetClient.GetHabbo().Username + " sans vous connecter au réseau de la Banque Populaire.");
                return;
            }

            int num;
            if (!Int32.TryParse(Params[2], out num) || Params[2].StartsWith("0"))
            {
                Session.SendWhisper("Le montant n'est pas valide.");
                return;
            }

            if (Convert.ToInt32(Params[2]) > TargetClient.GetHabbo().Credits)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas " + Params[2] + " crédit(s) sur lui.");
                return;
            }

            if (Convert.ToInt32(Params[2]) < 40)
            {
                Session.SendWhisper("Le montant de dépot minimum est de 40 crédits.");
                return;
            }

            Session.GetHabbo().addCooldown("bank_command", 2000);
            TargetClient.GetHabbo().Credits -= Convert.ToInt32(Params[2]);
            TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);

            decimal depotDecimal = Convert.ToInt32(Params[2]);
            int Depot = Convert.ToInt32((depotDecimal / 100m) * 95m);
            int ForBank = Convert.ToInt32((depotDecimal / 100m) * 5m);
            Group Banque = null;
            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(3, out Banque))
            {
                Banque.ChiffreAffaire += ForBank;
                Banque.updateChiffre();
            }
            TargetClient.GetHabbo().Banque += Depot;
            TargetClient.GetHabbo().updateBanque();
            User.OnChat(User.LastBubble, "* Dépose " + Params[2] + " crédit(s) dans le compte bancaire de " + TargetClient.GetHabbo().Username + " (-" + ForBank + " crédits de taxe par la banque) *", true);
        }
    }
}