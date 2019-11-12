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
    class TabacCommand : IChatCommand
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
            get { return "Vendre un paquet de cigarette à un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :tabac <pseudonyme>");
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
                Session.SendWhisper("Vous ne pouvez pas vendre un paquet de Philip Morris à " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if ((TargetClient.GetHabbo().PhilipMo + 20) > 60)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà 3 paquets de Philip Morris.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Vend un paquet de Philip Morris à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "tabac:" + PlusEnvironment.getPriceOfItem("Philip Morris") + ":" + PlusEnvironment.getTaxeOfItem("Philip Morris");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>paquet de Philip Morris</b> pour <b>" + PlusEnvironment.getPriceOfItem("Philip Morris") + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem("Philip Morris") + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem("Philip Morris"));
        }
    }
}