using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ConvertirCommand : IChatCommand
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
            get { return "Convertir les jetons d'un utilisateur en crédits"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :convertir <pseudonyme> <montant de jetons>");
                return;
            }

            if (Session.GetHabbo().getCooldown("convertir_command"))
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
                Session.SendWhisper("Rapprochez vous de " + TargetClient.GetHabbo().Username + " pour lui convertir ses jetons.");
                return;
            }

            if(TargetUser.participateRoulette == true)
            {
                Session.SendWhisper("Vous ne pouvez pas convertir les jetons de " + TargetClient.GetHabbo().Username + " pendant qu'il participe à la roulette.");
                return;
            }

            if (TargetUser.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas convertir les jetons de "+ TargetClient.GetHabbo().Username + " pour le moment car il fait un échange.");
                return;
            }

            int Amount;
            string Montant = Params[2];
            if (!int.TryParse(Montant, out Amount) || Convert.ToInt32(Params[2]) <= 0 || Montant.StartsWith("0"))
            {
                Session.SendWhisper("Le montant de jetons est invalide.");
                return;
            }

            if (Convert.ToInt32(Montant) > TargetClient.GetHabbo().Casino_Jetons)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username +  " a seulement " + TargetClient.GetHabbo().Casino_Jetons + " jeton(s).");
                return;
            }

            Session.GetHabbo().addCooldown("convertir_command", 3000);
            TargetClient.GetHabbo().Casino_Jetons -= Convert.ToInt32(Montant);
            TargetClient.GetHabbo().updateCasinoJetons();
            Group Casino = null;
            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(16, out Casino))
            {
                Casino.ChiffreAffaire -= Convert.ToInt32(Montant) * 10;
                Casino.updateChiffre();
            }
            TargetClient.GetHabbo().Credits += Convert.ToInt32(Montant) * 10;
            TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
            User.OnChat(User.LastBubble, "* Prend "+ Convert.ToInt32(Montant) + " jeton(s) à " + TargetClient.GetHabbo().Username + " et lui donne " + Convert.ToInt32(Montant) * 10 + " crédits *", true);
        }
    }
}