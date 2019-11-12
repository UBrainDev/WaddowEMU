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
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class GagnantCommand : IChatCommand
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
            get { return "Donner le gain du tirage EuroRP au gagnant"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :gagnant <pseudonyme>");
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
                Session.SendWhisper("Vous ne pouvez pas donner le gain EuroRP à " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if (Session.GetHabbo().getCooldown("gagnant_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("gagnant_command", 5000);
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `eurorp` WHERE `last_winner` = @userid AND lot = 0");
                dbClient.AddParameter("userid", TargetClient.GetHabbo().Id);
                int getLot = dbClient.getInteger();

                if (getLot == 0)
                {
                    if (Session.GetHabbo().Id == TargetClient.GetHabbo().Id)
                    {
                        Session.SendWhisper("Vous n'avez pas gagné au tirage EuroRP ou vous avez déjà reçu votre gain.");
                        return;
                    }
                    else
                    {
                        Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas gagné au tirage EuroRP ou a déjà reçu son gain.");
                        return;
                    }
                }

                dbClient.SetQuery("SELECT `last_montant` FROM `eurorp`");
                int EuroRPWin = dbClient.getInteger();
                dbClient.RunQuery("UPDATE `eurorp` SET lot = '1'");
                Group Cafe = null;
                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(5, out Cafe))
                {
                    Cafe.ChiffreAffaire -= EuroRPWin;
                    Cafe.updateChiffre();
                }
                User.OnChat(User.LastBubble, "* Donne le gain du tirage EuroRP à " + TargetClient.GetHabbo().Username + " *", true);
                TargetUser.OnChat(TargetUser.LastBubble, "* Reçoit le gain [+" + EuroRPWin + " CRÉDITS] *", true);
                TargetClient.GetHabbo().Credits += EuroRPWin;
                TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
                return;
            }
        }
    }
}