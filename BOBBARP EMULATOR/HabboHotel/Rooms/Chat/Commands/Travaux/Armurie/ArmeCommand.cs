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
    class ArmeCommand : IChatCommand
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
            get { return "<pseudonyme> <nom de l'arme>"; }
        }

        public string Description
        {
            get { return "Vendre une arme à un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :arme <pseudonyme> <nom de l'arme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if(TargetClient.GetHabbo().Sac == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit posséder un sac pour que vous puissez lui vendre une arme.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            string Message = CommandManager.MergeParams(Params, 2);
            if (!PlusEnvironment.checkIfItemExist(Message, "arme"))
            {
                Session.SendWhisper("Cette arme n'existe pas.");
                return;
            }

            if (Message.ToLower() == "ak47" && TargetClient.GetHabbo().PoliceCasier.Contains("[AK47]") || Message.ToLower() == "ak47" && TargetClient.GetHabbo().Ak47 == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une Ak47.");
                return;
            }

            if (Message.ToLower() == "sabre" && TargetClient.GetHabbo().PoliceCasier.Contains("[SABRE]") || Message.ToLower() == "sabre" && TargetClient.GetHabbo().Sabre == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un sabre.");
                return;
            }

            if (Message.ToLower() == "uzi" && TargetClient.GetHabbo().PoliceCasier.Contains("[UZI]") || Message.ToLower() == "uzi" && TargetClient.GetHabbo().Uzi == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un Uzi.");
                return;
            }

            if (Message.ToLower() == "batte" && TargetClient.GetHabbo().PoliceCasier.Contains("[BATTE]") || Message.ToLower() == "batte" && TargetClient.GetHabbo().Batte == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une batte.");
                return;
            }

            if (TargetClient.GetHabbo().getMaxItem("arme") <= TargetClient.GetHabbo().GetNumberOfArmes())
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " ne peut pas stocker plus de " + TargetClient.GetHabbo().getMaxItem("arme") + " dans son sac.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            User.OnChat(User.LastBubble, "* Vend un(e) "+ PlusEnvironment.getNameOfItem(Message) + " à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "arme:" + PlusEnvironment.getNameOfItem(Message) + ":" + PlusEnvironment.getPriceOfItem(Message) + ":" + PlusEnvironment.getTaxeOfItem(Message);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous vendre un(e) <b>" + PlusEnvironment.getNameOfItem(Message) + "</b> pour <b>" + PlusEnvironment.getPriceOfItem(Message) + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem(Message) + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem(Message));
        }
    }
}