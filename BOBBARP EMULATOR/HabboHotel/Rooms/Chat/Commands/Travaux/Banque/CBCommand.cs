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
    class CBCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 3 && Session.GetHabbo().Travaille == true)
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
            get { return "Réaliser la carte bancaire d'un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :cb <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("bank_command"))
            {
                Session.SendWhisper("Veuillez patienter");
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
                Session.SendWhisper("Vous ne pouvez pas réaliser la carte bancaire de " + TargetClient.GetHabbo().Username + " sans vous connecter au réseau de la Banque Populaire.");
                return;
            }

            if(TargetClient.GetHabbo().Cb != "null")
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une carte bancaire.");
                return;
            }
            
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser.isMakingCard == true)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà en train de faire sa carte.");
                return;
            }

            Session.GetHabbo().addCooldown("bank_command", 2000);
            TargetUser.Frozen = true;
            TargetUser.isMakingCard = true;
            User.OnChat(User.LastBubble, "* Réalise la carte bancaire de " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.OnChat(TargetUser.LastBubble, "* Choisit son code *", true);
            TargetClient.SendWhisper("Merci de rentrer le code de votre carte bancaire.");
        }
    }
}