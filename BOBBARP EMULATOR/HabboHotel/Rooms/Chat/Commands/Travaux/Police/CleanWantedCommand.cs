using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;


using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class CleanWantedCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
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
            get { return "Vider la liste des personnes recherchées"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().getCooldown("clean_rechercher"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            Session.GetHabbo().addCooldown("clean_rechercher", 3000);
            using (IQueryAdapter mysqlConnect = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                mysqlConnect.RunQuery("TRUNCATE TABLE users_wanted");
            }
            User.OnChat(User.LastBubble, "* Vide la liste des personnes recherchées *", true);
            PlusEnvironment.GetGame().GetClientManager().sendPoliceRadio(Session.GetHabbo().Username + " a vidé la liste des personnes recherchées.");
        }
    }
}