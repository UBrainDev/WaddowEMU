using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GroupsRank;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class VirerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 || Session.GetHabbo().Username == "ADMIN-UBrain" || Session.GetHabbo().Username == "ADMIN-Soubes")
            {
                return true;
            }

            return false;
        }

        public string TypeCommand
        {
            get { return "directeur"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Virer un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {

            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :virer <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("virer_command") == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            string Username = Params[1];
            int idUser;
            int groupId;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @username LIMIT 1");
                dbClient.AddParameter("username", Username);
                idUser = dbClient.getInteger();

                if(idUser == 0)
                {
                    Session.SendWhisper("Impossible de trouver cet utilisateur.");
                    return;
                }

                dbClient.SetQuery("SELECT `group_id` FROM `group_memberships` WHERE `user_id` = @userId LIMIT 1");
                dbClient.AddParameter("userId", idUser);
                groupId = dbClient.getInteger();
            }

          /*  if (idUser == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous virer vous même.");
                return;
            }*/

            if (groupId == 1)
            {
                Session.SendWhisper(Username + " n'a pas de travail.");
                return;
            }

            bool CanChangeRank = false;
            if (Session.GetHabbo().Username == "ADMIN-UBrain" || Session.GetHabbo().Username == "ADMIN-Soubes")
            {
                CanChangeRank = true;
            }

            if (CanChangeRank == false && groupId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas virer des employés du gouvernement.");
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group))
            {
                Session.SendWhisper("Une erreur est survenue.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && groupId != 4)
            {
                Session.SendWhisper("Le ministre de l'intérieur peut s'occuper seulement de la Police Nationale.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 && groupId != 7 && groupId != 6 && groupId != 10)
            {
                Session.SendWhisper("Le ministre de la santé peut s'occuper seulement de l'Hôpital, de la Pharmacie et de la Mutuelle.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && groupId == 4 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && groupId == 7 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && groupId == 10 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && groupId == 6)
            {
                Session.SendWhisper("Le ministre du travail ne peut pas s'occuper de la Police Nationale, de l'Hôpital, de la Pharmacie ou de la Mutuelle.");
                return;
            }

            Session.GetHabbo().addCooldown("virer_command", 2000);
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            Group.sendToChomage(idUser);
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient != null)
            {
                TargetClient.SendMessage(new GroupInfoComposer(Group, Session));
                TargetClient.GetHabbo().setFavoriteGroup(1);

                if(TargetClient.GetHabbo().Travaille)
                {
                    TargetClient.GetHabbo().Travaille = false;
                    TargetClient.GetHabbo().resetAvatarEvent();
                    TargetClient.SendWhisper(Session.GetHabbo().Username + " vous a viré de votre entreprise.");
                }
            }

            Group GroupBase = null;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(1, out GroupBase);
            GroupBase.AddMemberList(idUser);
            User.OnChat(User.LastBubble, "* Vire " + Username + " de son travail *", true);
        }
    }
}