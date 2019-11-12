using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class DisconnectCommand :IChatCommand
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
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Déconnecter un utilisateur"; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :disconnect <pseudonyme>");
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Impossible de trouver cet utilisateur.");
                return;
            }

            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank && Session.GetHabbo().Username != "ADMIN-UBrain")
            {
                Session.SendWhisper("Vous ne pouvez pas déconnecter cet utilisateur.");
                return;
            }

            TargetClient.GetConnection().Dispose();
        }
    }
}
