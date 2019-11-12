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
    class AlerteCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 2 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 3)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<message>"; }
        }

        public string Description
        {
            get { return "Envoyer un message au serveur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            string Message = CommandManager.MergeParams(Params, 1);
            if(Message == null || Message == "")
            {
                Session.SendWhisper("Merci d'entrer un message.");
                return;
            }

            PlusEnvironment.GetGame().GetClientManager().sendGouvMsg(Message);
        }
    }
}