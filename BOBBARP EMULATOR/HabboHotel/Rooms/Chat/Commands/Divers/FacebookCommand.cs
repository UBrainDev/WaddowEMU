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
    class FacebookCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Facebook == 0)
                return true;

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
            get { return "Nous rejoindre sur Facebook"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().Facebook = 1;
            Session.GetHabbo().updateFacebook();
            Session.SendWhisper("Vous venez de gagner 20 crédits.");
            Session.GetHabbo().Credits += 20;
            Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "facebook;");
            return;
        }
    }
}