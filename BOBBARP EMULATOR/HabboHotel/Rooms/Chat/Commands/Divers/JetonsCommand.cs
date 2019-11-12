using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class JetonsCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 16 && Session.GetHabbo().RankId == 1 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 16 && Session.GetHabbo().RankId == 3 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme> <montant de jetons>"; }
        }

        public string Description
        {
            get { return "Vendre des jetons à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :jetons <pseudonyme> <montant de jetons>");
                return;
            }

            if (Session.GetHabbo().getCooldown("jetons_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
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
            if (Math.Abs(User.Y - TargetUser.Y) > 2 || Math.Abs(User.X - TargetUser.X) > 2)
            {
                Session.SendWhisper("Rapprochez vous de " + TargetClient.GetHabbo().Username + " pour lui vendre des jetons.");
                return;
            }

            int Amount;
            string Montant = Params[2];
            if (!int.TryParse(Montant, out Amount) || Convert.ToInt32(Params[2]) <= 0 || Montant.StartsWith("0"))
            {
                Session.SendWhisper("Le montant de jetons est invalide.");
                return;
            }

            if(Convert.ToInt32(Montant) > 10000)
            {
                Session.SendWhisper("Vous ne pouvez pas vendre plus de 10 000 jetons.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("jetons_command", 3000);
            User.OnChat(User.LastBubble, "* Vend " + Montant + " jeton(s) à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "jetons:" + Montant + ":" + Convert.ToInt32(Montant) * 10;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>" + Montant + " jeton(s)</b> pour <b>" + Convert.ToInt32(Montant) * 10 + " crédits</b>.;0");
        }
    }
}