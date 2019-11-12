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
    class SummonCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Convoquer un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :summon <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null)
            {
                Session.SendWhisper("Impossible de convoquer " + Username + " car il est introuvable.");
                return;
            }

            if(TargetClient.GetHabbo().CurrentRoomId == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est dans votre appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Prison != 0 || TargetClient.GetHabbo().Hopital != 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " ne peut pas être convoqué car il purge une peine de prison ou il est hospitalisé.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.OnChat(User.LastBubble, "* Convoque " + TargetClient.GetHabbo().Username + " *", true);
            TargetClient.GetHabbo().CanChangeRoom = true;
            TargetClient.GetHabbo().PrepareRoom(Session.GetHabbo().CurrentRoomId, "");
        }
    }
}