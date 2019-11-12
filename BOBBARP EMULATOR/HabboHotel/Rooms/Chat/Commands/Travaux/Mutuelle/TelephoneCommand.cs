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
    class TelephoneCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 12 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme> <nom du telephone>"; }
        }

        public string Description
        {
            get { return "Vendre un téléphone à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :telephone <pseudonyme> <nom du téléphone>");
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
                Session.SendWhisper("Vous ne pouvez pas vendre de téléphone car vous n'êtes pas connecté au réseau de Bouygues Telecom.");
                return;
            }

            string Message = CommandManager.MergeParams(Params, 2);
            if (!PlusEnvironment.checkIfItemExist(Message, "tel"))
            {
                Session.SendWhisper("Ce téléphone n'existe pas.");
                return;
            }

            if (TargetClient.GetHabbo().PoliceCasier.Contains("[TELEPHONE]"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit rendre son téléphone situé au commissariat avant de pouvoir changer de téléphone.");
                return;
            }

            if(TargetClient.GetHabbo().TelephoneName == Message)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà le " + PlusEnvironment.getNameOfItem(Message) + ".");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Vend un "+ PlusEnvironment.getNameOfItem(Message) + " à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "telephone:" + PlusEnvironment.getNameOfItem(Message) + ":" + PlusEnvironment.getPriceOfItem(Message) + ":" + PlusEnvironment.getTaxeOfItem(Message);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un <b>" + PlusEnvironment.getNameOfItem(Message) + "</b> pour <b>" + PlusEnvironment.getPriceOfItem(Message) + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem(Message) + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem(Message));
        }
    }
}