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
    class RecruterCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 || Session.GetHabbo().Username == "ADMIN-UBrain" || Session.GetHabbo().Username == "ADMIN-Soubes" )
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
            get { return "Recruter un civil dans une entreprise"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {

            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :recruter <pseudonyme> <travailID>");
                return;
            }

            if (Session.GetHabbo().getCooldown("recruter_command") == true)
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

            if(TargetClient.GetHabbo().TravailId != 1)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un travail.");
                return;
            }

            int Amount;
            string TravailId = Params[2];
            if (!int.TryParse(TravailId, out Amount) || Convert.ToInt32(Params[2]) <= 0 || TravailId.StartsWith("0"))
            {
                Session.SendWhisper("L'ID du travail est invalide.");
                return;
            }

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Convert.ToInt32(TravailId), out Group))
            {
                Session.SendWhisper("L'ID du travail est invalide.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Convert.ToInt32(TravailId) != 4)
            {
                Session.SendWhisper("Le ministre de l'intérieur peut s'occuper seulement de la Police Nationale.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4 && Convert.ToInt32(TravailId) != 7 && Convert.ToInt32(TravailId) != 10 && Convert.ToInt32(TravailId) != 6 && Convert.ToInt32(TravailId) != 10 && Convert.ToInt32(TravailId) != 10)
            {
                Session.SendWhisper("Le ministre de la santé peut s'occuper seulement de l'Hôpital, de la Pharmacie et de la Mutuelle.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && Convert.ToInt32(TravailId) == 4 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && Convert.ToInt32(TravailId) == 7 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && Convert.ToInt32(TravailId) == 6 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && Convert.ToInt32(TravailId) == 10)
            {
                Session.SendWhisper("Le ministre du travail ne peut pas s'occuper de la Police Nationale, de l'Hôpital, de la Pharmacie ou de la Mutuelle.");
                return;
            }

            if (Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 5 && Convert.ToInt32(TravailId) == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas recruter des civils dans le gouvernement.");
                return;
            }

            GroupRank NewRank = null;
            PlusEnvironment.GetGame().getGroupRankManager().TryGetRank(Convert.ToInt32(TravailId), 1, out NewRank);
            if (NewRank == null)
            {
                Session.SendWhisper("Une erreur est survenue.");
                return;
            }

            Session.GetHabbo().addCooldown("recruter_command", 2000);
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            Group.AddMemberByForce(TargetClient.GetHabbo().Id);
            TargetClient.GetHabbo().setFavoriteGroup(Group.Id);
            User.OnChat(User.LastBubble, "* Recrute " + TargetClient.GetHabbo().Username + " en tant que " + NewRank.Name + " dans l'entreprise " + Group.Name + " *", true);
        }
    }
}