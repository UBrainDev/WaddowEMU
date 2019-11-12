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
    class BingoCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 2 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 3 && Session.GetHabbo().Travaille == true)
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
            get { return "Vendre un bingo à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :bingo <pseudonyme>");
                return;
            }

            if(Session.GetHabbo().getCooldown("bingo_command"))
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
                Session.SendWhisper("Vous ne pouvez pas vendre un ticket de Bingo à " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("bingo_command", 5000);
            User.OnChat(User.LastBubble, "* Vend un ticket de Bingo à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "bingo:" + PlusEnvironment.getPriceOfItem("Ticket de Bingo") + ":" + PlusEnvironment.getTaxeOfItem("Ticket de Bingo");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>ticket de Bingo</b> pour <b>" + PlusEnvironment.getPriceOfItem("Ticket de Bingo") + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem("Ticket de Bingo") + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem("Ticket de Bingo"));
        }
    }
}