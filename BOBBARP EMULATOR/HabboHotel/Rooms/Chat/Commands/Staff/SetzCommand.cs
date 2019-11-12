using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class SetzCommand : IChatCommand
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
            get { return "<nombre>"; }
        }

        public string Description
        {
            get { return "Définir à quelle hauteur placer un mobis"; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :setz <nombre>");
                return;
            }

            if (!double.TryParse(Params[1], out Session.GetHabbo().StackHeight) || Convert.ToInt32(Params[1]) > 100 || Convert.ToInt32(Params[2]) < 0 || Params[2].StartsWith("0") && Params[2].Length > 1)
                Session.SendWhisper("Le paramètre doit être compris entre 0 et 100.");
            return;
        }
    }
}
