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

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class PromouvoirCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 || Session.GetHabbo().Username == "ADMIN-UBrain" || Session.GetHabbo().Username == "ADMIN-Soubes")
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
            get { return "Promouvoir un employé dans une entreprise"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :promouvoir <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("promouvoir_command") == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if(TargetClient.GetHabbo().TravailId == 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas de travail.");
                return;
            }

            bool CanChangeRank = false;
            if (Session.GetHabbo().Username == "ADMIN-UBrain" || Session.GetHabbo().Username == "ADMIN-Soubes")
            {
                CanChangeRank = true;
            }

            if (CanChangeRank == false && TargetClient.GetHabbo().TravailId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas promouvoir des employés du gouvernement.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && TargetClient.GetHabbo().TravailId != 4 && CanChangeRank == false)
            {
                Session.SendWhisper("Le ministre de l'intérieur peut s'occuper seulement de la Police Nationale.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 && TargetClient.GetHabbo().TravailId != 7 && TargetClient.GetHabbo().TravailId != 6 && TargetClient.GetHabbo().TravailId != 10 && CanChangeRank == false)
            {
                Session.SendWhisper("Le ministre de la santé peut s'occuper seulement de l'Hôpital, de la Pharmacie et de la Mutuelle.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && TargetClient.GetHabbo().TravailId == 4 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && TargetClient.GetHabbo().TravailId == 7 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && TargetClient.GetHabbo().TravailId == 10 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && TargetClient.GetHabbo().TravailId == 6)
            {
                Session.SendWhisper("Le ministre du travail ne peut pas s'occuper de la Police Nationale, de l'Hôpital, de la Pharmacie ou de la Mutuelle.");
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(TargetClient.GetHabbo().TravailId, out Group))
            {
                Session.SendWhisper("Une erreur est survenue.");
                return;
            }

            GroupRank NewRank = null;
            PlusEnvironment.GetGame().getGroupRankManager().TryGetRank(Convert.ToInt32(TargetClient.GetHabbo().TravailId), TargetClient.GetHabbo().RankId + 1, out NewRank);
            if (NewRank == null)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà le rang le plus haut.");
                return;
            }

            Session.GetHabbo().addCooldown("promouvoir_command", 2000);
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            Group.updateRank(TargetClient.GetHabbo().Id);
            if (NewRank.Rank == 2)
            {
                Group.MakeAdmin(TargetClient.GetHabbo().Id);
            }
            User.OnChat(User.LastBubble, "* Promouvoit " + TargetClient.GetHabbo().Username + " en tant que " + NewRank.Name + " dans l'entreprise " + Group.Name +  " *", true);
        }
    }
}