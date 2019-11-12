using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class QuestionCommand : IChatCommand
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
            get { return "<question>"; }
        }

        public string Description
        {
            get { return "Définir la question d'un quizz"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (PlusEnvironment.Quizz == 0)
            {
                Session.SendWhisper("Aucun quizz n'est organisé.");
                return;
            }

            if (PlusEnvironment.QuizzReponse == null)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer de question car vous n'avez pas défini de réponse.");
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            if (Message == null || Message == "")
            {
                Session.SendWhisper("Merci d'entrer une question.");
                return;
            }

            if(Message.Contains(";"))
            {
                Session.SendWhisper("Votre question ne peut pas contenir le caractère ;");
                return;
            }

            byte[] encodeToUtf8 = Encoding.Default.GetBytes(Message);
            Message = Encoding.UTF8.GetString(encodeToUtf8);

            foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
            {
                if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                    continue;

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "quizz;newQuestion;" + Message);
            }

        }
    }
}