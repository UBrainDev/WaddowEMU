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
    class PortArmeCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 17 && Session.GetHabbo().Travaille == true)
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
            get { return "Vendre un permis de port d'armes à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :portarme <pseudonyme>");
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
                Session.SendWhisper("Vous ne pouvez pas vendre le permis de port d'armes à " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if (TargetClient.GetHabbo().Permis_arme == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un permis de port d'armes.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Vend un permis de port d'armes à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "portarme:" + PlusEnvironment.getPriceOfItem("Permis port arme") + ":" + PlusEnvironment.getTaxeOfItem("Permis port arme");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>permis de port d'armes</b> pour <b>" + PlusEnvironment.getPriceOfItem("Permis port arme") + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem("Permis port arme") + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem("Permis port arme"));
        }
    }
}