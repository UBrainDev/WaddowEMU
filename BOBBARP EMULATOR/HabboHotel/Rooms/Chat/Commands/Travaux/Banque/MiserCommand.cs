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
    class MiserCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().CurrentRoomId == 46)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "divers"; }
        }

        public string Parameters
        {
            get { return "<chiffre> <montant>"; }
        }

        public string Description
        {
            get { return "Miser à la roulette"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :miser <chiffre> <montant>");
                return;
            }

            if (PlusEnvironment.RouletteEtat != 0 || PlusEnvironment.RouletteTurning == true)
            {
                Session.SendWhisper("Vous ne pouvez pas miser pour le moment.");
                return;
            }

            if (Session.GetHabbo().getCooldown("miser_command"))
            {
                Session.SendWhisper("Veuillez patienter");
                return;
            }

            if (Session.GetHabbo().TravailId == 16 && Session.GetHabbo().Travaille == true)
            {
                Session.SendWhisper("Vous ne pouvez pas miser pendant que vous travaillez.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User.participateRoulette == true)
            {
                Session.SendWhisper("Vous avez déjà miser.");
                return;
            }

            if (PlusEnvironment.GetGame().GetClientManager().userCroupierWorking() == 0)
            {
                Session.SendWhisper("Vous ne pouvez pas miser car aucun croupier ne travaille.");
                return;
            }

            if (User.Item_On != 3800)
            {
                Session.SendWhisper("Vous devez être à une table pour pouvoir miser.");
                return;
            }

            int num;
            if (!Int32.TryParse(Params[1], out num) || Convert.ToInt32(Params[1]) < 0 || Convert.ToInt32(Params[1]) > 36 || Params[1].StartsWith("0") && Params[1].Length > 1)
            {
                Session.SendWhisper("Le chiffre n'est pas valide.");
                return;
            }

            if (!Int32.TryParse(Params[2], out num) || Params[2].StartsWith("0") || Convert.ToInt32(Params[2]) <= 0)
            {
                Session.SendWhisper("Le montant de jetons n'est pas valide.");
                return;
            }

            if (Convert.ToInt32(Params[2]) > 100)
            {
                Session.SendWhisper("Vous ne pouvez pas miser plus de 100 jetons.");
                return;
            }

            if (Session.GetHabbo().Casino_Jetons == 0)
            {
                Session.SendWhisper("Vous n'avez plus de jetons.");
                return;
            }

            if (Convert.ToInt32(Params[2]) > Session.GetHabbo().Casino_Jetons)
            {
                Session.SendWhisper("Vous avez seulement " + Session.GetHabbo().Casino_Jetons + " jetons.");
                return;
            }

            Session.GetHabbo().addCooldown("miser_command", 3000);
            User.participateRoulette = true;
            Session.GetHabbo().Casino_Jetons -= Convert.ToInt32(Params[2]);
            Session.GetHabbo().updateCasinoJetons();
            User.numberRoulette = Convert.ToInt32(Params[1]);
            User.miseRoulette = Convert.ToInt32(Params[2]);
            User.OnChat(User.LastBubble, "* Mise " + Params[2] + " jeton(s) sur le chiffre " + Params[1] + " *", true);
        }
    }
}