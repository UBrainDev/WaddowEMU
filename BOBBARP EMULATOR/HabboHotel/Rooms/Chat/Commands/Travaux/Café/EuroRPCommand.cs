using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class EuroRPCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 2 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 5 && Session.GetHabbo().RankId == 3 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Inscrire un civil au tirage EuroRP"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :eurorp <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 2 || Math.Abs(User.X - TargetUser.X) > 2)
            {
                Session.SendWhisper("Vous ne pouvez pas inscrire " + TargetClient.GetHabbo().Username + " au tirage EuroRP car il est trop loin de vous.");
                return;
            }

            if (TargetUser.Transaction != null || TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une transaction en cours, veuillez patienter.");
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `eurorp_participants` WHERE `participant_id` = @userid");
                dbClient.AddParameter("userid", TargetClient.GetHabbo().Id);
                int getLot = dbClient.getInteger();

                if (getLot > 0)
                {
                    if (Session.GetHabbo().Id == TargetClient.GetHabbo().Id)
                    {
                        Session.SendWhisper("Vous êtes déjà inscrit au tirage EuroRP.");
                    }
                    else
                    {
                        Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà inscrit au tirage EuroRP.");
                    }
                    User.makeAction = false;
                    return;
                }
            }
            
            User.OnChat(User.LastBubble, "* Propose à " + TargetClient.GetHabbo().Username + " de s'inscrire au tirage EuroRP *", true);
            TargetUser.Transaction = "eurorp:" + PlusEnvironment.getPriceOfItem("Tirage EuroRP") + ":" + PlusEnvironment.getTaxeOfItem("Tirage EuroRP");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous inscrire au tirage <b>EuroRP</b> pour <b>" + PlusEnvironment.getPriceOfItem("Tirage EuroRP") + " crédits</b> dont <b>" + PlusEnvironment.getTaxeOfItem("Tirage EuroRP") + "</b> qui iront à l'État.;" + PlusEnvironment.getPriceOfItem("Tirage EuroRP"));
        }
    }
}