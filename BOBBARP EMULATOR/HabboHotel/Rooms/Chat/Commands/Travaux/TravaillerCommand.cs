using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class TravaillerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
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
            get { return "Commencer à travailler"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session.GetHabbo().Travaille == true)
            {
                Session.SendWhisper("Vous travaillez déjà.");
                return;
            }

            if(Session.GetHabbo().getCooldown("travailler_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }
            
            if (Session.GetHabbo().TravailId == 1)
            {
                Session.SendWhisper("Vous n'avez aucun travail, rendez-vous chez une entreprise pour postuler.");
                return;
            }

            if (Session.GetHabbo().RankInfo == null || Session.GetHabbo().TravailInfo == null)
            {
                Session.SendWhisper("Il y a un problème avec votre métier.");
                return;
            }

            if (Session.GetHabbo().RankInfo.WorkEverywhere == 0 && Session.GetHabbo().CurrentRoomId != Session.GetHabbo().TravailInfo.RoomId)
            {
                Session.SendWhisper("Vous ne travaillez pas ici.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Session.GetHabbo().TravailInfo.ChiffreAffaire < Session.GetHabbo().RankInfo.Salaire && Session.GetHabbo().TravailInfo.PayedByEtat == 0 && Session.GetHabbo().TravailInfo.Usine == 0)
            {
                Session.GetHabbo().GetClient().SendWhisper("Vous ne pouvez pas travailler car votre entreprise est en faillite.");
                return;
            }

            if(User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas travailler pendant que vous faites un échange.");
                return;
            }

            if (Session.GetHabbo().EventCount != 0 && Session.GetHabbo().EventDay == 0)
            {
                Session.SendWhisper("Vous ne pouvez pas travailler lorsque vous réalisez des missions.");
                return;
            }

            if (PlusEnvironment.usersSuspendus.ContainsKey(Session.GetHabbo().Username))
            {
                Session.SendWhisper("Vous ne pouvez pas travailler un de vos supérieurs vous a suspendu.");
                return;
            }
            
            if(User.usernameCoiff != null)
            {
                Session.SendWhisper("Vous ne pouvez pas travailler pendant que vous vous faites coiffer.");
                return;
            }

            if (Session.GetHabbo().footballTeam != null)
            {
                Session.SendWhisper("Vous ne pouvez pas travailler pendant que vous jouez au football.");
                return;
            }

            Session.GetHabbo().addCooldown("travailler_command", 5000);
            Session.GetHabbo().updateAvatarEvent(Session.GetHabbo().RankInfo.Look_F, Session.GetHabbo().RankInfo.Look_H, "[TRAVAILLE] " + Session.GetHabbo().RankInfo.Name + ", " + Session.GetHabbo().TravailInfo.Name);
            if (Session.GetHabbo().Timer < 1)
            {
                Session.GetHabbo().Timer = 10;
                Session.GetHabbo().updateTimer();
            }

            Session.GetHabbo().Travaille = true;
            
            User.OnChat(User.LastBubble, "* Commence à travailler *", true);
        }
    }
}