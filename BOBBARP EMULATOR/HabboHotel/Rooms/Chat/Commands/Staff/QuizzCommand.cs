using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class QuizzCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 2)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Démarrer un quizz."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (PlusEnvironment.Quizz != 0)
            {
                Session.SendWhisper("Un quizz est déjà organisé.");
                return;
            }

            PlusEnvironment.Quizz = Session.GetHabbo().CurrentRoomId;
            foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
            {
                if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "quizz;refreshClassement;" + UserInRoom.GetClient().GetHabbo().Quizz_Points);
            }
            
            PlusEnvironment.GetGame().GetClientManager().sendServerMessage("Un quizz va débuter dans l'appartement [" + Session.GetHabbo().CurrentRoom.Id + "] " + Session.GetHabbo().CurrentRoom.Name + ".");
        }
    }
}