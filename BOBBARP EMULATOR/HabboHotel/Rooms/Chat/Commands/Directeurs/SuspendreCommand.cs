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
    class SuspendreCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "directeur"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Suspendre un employé dans une entreprise"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :suspendre <pseudonyme>");
                return;
            }

            if(Session.GetHabbo().RankInfo.Rank == 0)
            {
                Session.SendWhisper("Seul les superviseurs et directeurs peuvent suspendre un employé.");
                return;
            }

            if (Session.GetHabbo().Travaille == false)
            {
                Session.SendWhisper("Veuillez travailler pour suspendre un employé.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if(TargetClient.GetHabbo().TravailId != Session.GetHabbo().TravailId)
            {
                Session.SendWhisper("Vous ne pouvez pas suspendre " + TargetClient.GetHabbo().Username + " car il ne travaille pas pour vous.");
                return;
            }

            if ((Session.GetHabbo().RankInfo.Rank == 1 && TargetClient.GetHabbo().RankInfo.Rank > 0) || (Session.GetHabbo().RankInfo.Rank == 2 && TargetClient.GetHabbo().RankInfo.Rank == 2))
            {
                Session.SendWhisper("Vous ne pouvez pas suspendre cet utilisateur.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous suspendre vous même.");
                return;
            }

            if(PlusEnvironment.usersSuspendus.ContainsKey(TargetClient.GetHabbo().Username))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà suspendu de travail.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            PlusEnvironment.suspendreUser(TargetClient.GetHabbo().Username);
            if(TargetClient.GetHabbo().Travaille == true)
            {
                TargetClient.GetHabbo().Travaille = false;
                TargetClient.GetHabbo().resetAvatarEvent();
            }
            User.OnChat(User.LastBubble, "* Suspend " + TargetClient.GetHabbo().Username + " de travailler pendant 5 minutes *", true);
        }
    }
}