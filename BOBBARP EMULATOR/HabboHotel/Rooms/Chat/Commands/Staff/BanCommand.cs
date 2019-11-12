using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;


using Plus.HabboHotel.Moderation;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class BanCommand : IChatCommand
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
            get { return "<pseudonyme> <heures ou perm> <raison>"; }
        }

        public string Description
        {
            get { return "Bannir un utilisateur"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :ban <pseudonyme> <heures ou perm> <raison>");
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("Impossible de trouver cet utilisateur.");
                return;
            }

            if (Habbo.Rank != 1 && Habbo.Username != "ADMIN-UBrain")
            {
                Session.SendWhisper("Impossible de bannir cet utilisateur.");
                return;
            }

            Double Expire = 0;
            string Hours = Params[2];
            int Amount;
            if (!int.TryParse(Hours, out Amount) || Convert.ToInt32(Hours) <= 0)
            {
                Session.SendWhisper("L'heure indiquée est invalide.");
                return;
            }

            if (String.IsNullOrEmpty(Hours) || Hours == "perm")
                Expire = PlusEnvironment.GetUnixTimestamp() + 78892200;
            else
                Expire = (PlusEnvironment.GetUnixTimestamp() + (Convert.ToDouble(Hours) * 3600));

            string Reason = null;
            if (Params.Length >= 4)
                Reason = CommandManager.MergeParams(Params, 3);
            else
                Reason = "Aucune raison spécifiée.";

            string Username = Habbo.Username;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `user_info` SET `bans` = `bans` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
            }

            PlusEnvironment.GetGame().GetModerationManager().BanUser(Session.GetHabbo().Username, ModerationBanType.USERNAME, Habbo.Username, Reason, Expire);

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient != null)
                TargetClient.Disconnect();

            PlusEnvironment.GetGame().GetClientManager().sendStaffMsg("Le compte " + Username + " a été banni par " + Session.GetHabbo().Username + " pendant " + Hours + " heure(s) pour la raison suivante : " + Reason);
        }
    }
}