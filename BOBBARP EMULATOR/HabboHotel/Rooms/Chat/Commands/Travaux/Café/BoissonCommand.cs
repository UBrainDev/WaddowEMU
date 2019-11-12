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
    class BoissonCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 1 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 3 && Session.GetHabbo().Travaille == true)
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
            get { return "Vendre une boisson préparé à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :boisson <pseudonyme>");
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

            if(User.boissonPrepared == null)
            {
                Session.SendWhisper("Vous n'avez aucune boisson de préparé.");
                return;
            }

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper("Vous ne pouvez pas vendre de boisson à " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            string Name = "";
            int Price = 0;
            int Taxe = 0;

            if(User.boissonPrepared == "cafe")
            {
                Name = PlusEnvironment.getNameOfItem("Café").ToLower();
                Price = PlusEnvironment.getPriceOfItem("Café");
                Taxe = PlusEnvironment.getTaxeOfItem("Café");
            }
            else if (User.boissonPrepared == "cappuccino")
            {
                Name = PlusEnvironment.getNameOfItem("Cappuccino").ToLower();
                Price = PlusEnvironment.getPriceOfItem("Cappuccino");
                Taxe = PlusEnvironment.getTaxeOfItem("Cappuccino");
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("boissonPrepared", 5000);
            User.boissonPrepared = null;
            User.CarryItem(0);
            User.OnChat(User.LastBubble, "* Vend un " + Name + " à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "boisson:" + Name + ":" + Price + ":" + Taxe;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>" + Name + "</b> pour <b>" + Price + " crédits</b> dont <b>" + Taxe + "</b> qui iront à l'État.;" + Price);
        }
    }
}