using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DuelCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "arme"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Prendre en duel un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :duel <pseudonyme>");
                return;
            }

            if(Session.GetHabbo().getCooldown("duel_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if (!Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO"))
            {
                Session.SendWhisper("Vous ne pouvez pas prendre un civil en duel dans cet appartement.");
                return;
            }

            if (PlusEnvironment.Purge == true)
            {
                Session.SendWhisper("Vous ne pouvez pas prendre un civil en duel pendant la purge.");
                return;
            }

            if (PlusEnvironment.Purge == true)
            {
                Session.SendWhisper("Vous ne pouvez pas prendre un civil en duel pendant la purge.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if(TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas faire de duel avec vous même.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if(TargetUser.DuelUser != null)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà en duel.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une proposition, veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("duel_command", 20000);
            User.OnChat(User.LastBubble, "* Propose un duel à " + TargetClient.GetHabbo().Username + " *", true);
            TargetUser.Transaction = "duel:" + Session.GetHabbo().Username;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous <b>prendre en duel</b>.;0");
        }
    }
}