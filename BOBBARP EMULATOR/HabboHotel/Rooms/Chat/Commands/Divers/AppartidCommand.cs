using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class AppartidCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "appart"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Savoir l'ID de l'appartement actuel"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            Session.SendWhisper("Vous êtes dans l'appartement [" + Session.GetHabbo().CurrentRoom.Id +"]");
            return;
        }
    }
}