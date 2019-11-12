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
    class ArreterCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Arrêter de travailler."; }
        }

        public bool canExecute(GameClient Session)
        {
            return true;
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().Travaille == false)
            {
                Session.SendWhisper("Vous ne travaillez pas.");
                return;
            }

            if(Session.GetHabbo().AntiAFK !=  0)
            {
                Session.SendWhisper("Veuillez répondre à l'anti AFK.");
                return;
            }

            Session.GetHabbo().stopWork();
        }
    }
}