using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class TaxiCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            /*if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18)
                return true;*/

            return false;
        }
        
        public string TypeCommand
        {
            get { return "divers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Utiliser un taxi."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :taxi <appartId>");
                return;
            }

            int num;
            if (!Int32.TryParse(Params[1], out num) || Params[1].StartsWith("0"))
            {
                Session.SendWhisper("L'ID de l'appartement est invalide.");
                return;
            }

            Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(Params[1]));
            if (TargetRoom == null)
            {
                Session.SendWhisper("Impossible de trouver l'appartement [" + Params[1] + "]");
                return;
            }

            if(TargetRoom.Id == PlusEnvironment.Salade)
            {
                Session.SendWhisper("Vous ne pouvez pas vous rendre dans l'appartement [" + Params[1] + "] car une salade y est organisée.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Session.GetHabbo().Menotted || User.Tased)
            {
                Session.SendWhisper("Vous ne pouvez pas appeler un taxi lorsque vous êtes tasé ou menotté.");
                return;
            }

            if (Convert.ToInt32(Params[1]) == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Vous êtes déjà dans cet appartement.");
                return;
            }

            if (TargetRoom.Id == 50 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 80 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 102 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 106 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 105 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 103 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 110 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 107 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 108 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 109 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 111 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 115 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 116 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 120 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 121 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 122 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 123 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 125 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 126 && Session.GetHabbo().Rank != 8 || TargetRoom.Id == 127 && Session.GetHabbo().Rank != 8)
            {
                Session.SendWhisper("Impossible d'aller dans cet appartement.");
                return;
            }
            if (Session.GetHabbo().Rank != 8)
            {
            User.OnChat(User.LastBubble, "* Appel un fdp et se rend à[" + TargetRoom.Id + "] " + TargetRoom.Name + " *", true);
            Session.GetHabbo().CanChangeRoom = true;
            Session.GetHabbo().PrepareRoom(Convert.ToInt32(Params[1]), "");
            }
            else{
            User.OnChat(User.LastBubble, "* Appel un taxi et se rend à[" + TargetRoom.Id + "] " + TargetRoom.Name + " *", true);
            Session.GetHabbo().CanChangeRoom = true;
            Session.GetHabbo().PrepareRoom(Convert.ToInt32(Params[1]), "");
            }
         
    }
}
    }