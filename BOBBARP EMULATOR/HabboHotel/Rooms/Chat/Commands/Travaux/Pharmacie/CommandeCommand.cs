using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class CommandeCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 10 && Session.GetHabbo().Travaille == true)
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
            get { return "Prendre la commande d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Params.Length == 1)
            {
                if (Session.GetHabbo().Commande == null)
                {
                    Session.SendWhisper("Vous n'avez aucune commande en cours.");
                    return;
                }

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().Commande);
                if (TargetClient.GetHabbo() != null && TargetClient.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
                {
                    RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                    TargetClient.GetHabbo().Commande = null;
                    TargetUser.Purchase = "";
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                }

                User.Purchase = "";
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                User.CarryItem(0);
                Session.GetHabbo().Commande = null;
                User.OnChat(User.LastBubble, "* Arrête de prendre la commande de " + Session.GetHabbo().Commande + " *", true);
            }
            else
            {
                string Username = Params[1];
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
                if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                {
                    Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                    return;
                }

                if (Session.GetHabbo().Commande != null)
                {
                    Session.SendWhisper("Vous vous occupez déjà d'une commande.");
                    return;
                }

                if(TargetClient.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper(TargetClient.GetHabbo().Sac + " doit posséder un sac pour stocker les produits que vous allez lui vendre.");
                    return;
                }

                if (TargetClient.GetHabbo().Commande != null)
                {
                    Session.SendWhisper("Un pharmacien s'occupe déjà de la commande de " + TargetClient.GetHabbo().Username + ".");
                    return;
                }

                RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                if (TargetUser.Transaction != null || TargetUser.isTradingItems)
                {
                    Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                    return;
                }

                Session.GetHabbo().Commande = TargetClient.GetHabbo().Username;
                TargetClient.GetHabbo().Commande = Session.GetHabbo().Username;
                User.Purchase = "";
                TargetUser.Purchase = "";
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                User.CarryItem(1011);
                User.OnChat(User.LastBubble, "* Prend la commande de " + TargetClient.GetHabbo().Username + " *", true);
            }
        }
    }
}