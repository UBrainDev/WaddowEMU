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
    class ForfaitCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 12 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "forfait"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme> <nom du forfait>"; }
        }

        public string Description
        {
            get { return "Souscrire un forfait à un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :telephone <pseudonyme> <nom du forfait>");
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
                Session.SendWhisper("Vous ne pouvez pas souscrire un forfait à un civil car vous n'êtes pas connecté au réseau de Bouygues Telecom.");
                return;
            }

            if(TargetClient.GetHabbo().Telephone == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de téléphone ou il est stocké au commissariat.");
                return;
            }

            string Message = CommandManager.MergeParams(Params, 2);
            if (!PlusEnvironment.checkIfItemExist(Message, "forfait"))
            {
                Session.SendWhisper("Ce forfait n'existe pas.");
                return;
            }

            if (TargetClient.GetHabbo().TelephoneForfaitType == PlusEnvironment.getTypeOfItem(Message))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà le forfait " + PlusEnvironment.getNameOfItem(Message) + ".");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Propose à " + TargetClient.GetHabbo().Username + " de se souscrire au forfait " + PlusEnvironment.getNameOfItem(Message) + " *", true);
            TargetUser.Transaction = "forfait:" + PlusEnvironment.getNameOfItem(Message) + ":" + PlusEnvironment.getTypeOfItem(Message) + ":" + PlusEnvironment.getPriceOfItem(Message) + ":" + PlusEnvironment.getTaxeOfItem(Message);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous souscrire au forfait <b>" + PlusEnvironment.getNameOfItem(Message) + "</b> pour <b>" + PlusEnvironment.getPriceOfItem(Message) + " crédits</b> par semaine dont <b>" + PlusEnvironment.getTaxeOfItem(Message) + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem(Message));
        }
    }
}