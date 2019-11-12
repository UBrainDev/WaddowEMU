using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ReponseCommand : IChatCommand
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
            get { return "<reponse>"; }
        }

        public string Description
        {
            get { return "Définir la réponse d'une question d'un quizz"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if(PlusEnvironment.Quizz == 0)
            {
                Session.SendWhisper("Aucun quizz n'est organisé.");
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            if (Message == null || Message == "")
            {
                Session.SendWhisper("Merci d'entrer une réponse.");
                return;
            }

            PlusEnvironment.QuizzReponse = Message.ToLower();
            Session.SendWhisper("Réponse défini comme \"" + Message + "\".");
        }
    }
}