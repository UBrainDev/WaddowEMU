using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class CelluleCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 4 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId > 1 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "travail"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme> <minutes>"; }
        }

        public string Description
        {
            get { return "Emprisonner un utilisateur dans une cellule d'isolement"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :celulle <pseudonyme> <temps>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous emprisonner vous même.");
                return;
            }
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            int Amount;
            string Minutes = Params[2];
            if (!int.TryParse(Minutes, out Amount) || Convert.ToInt32(Params[2]) <= 0 || Minutes.StartsWith("0"))
            {
                Session.SendWhisper("Le temps d'emprisonnement indiqué est invalide.");
                return;
            }

            if (Convert.ToInt32(Minutes) > 1000)
            {
                Session.SendWhisper("Vous ne pouvez pas enfermer un utilisateur plus de 1000 minutes.");
                return;
            }

            if(TargetClient.GetHabbo().Prison != 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà emprisonné.");
                return;
            }

            if(TargetClient.GetHabbo().Conduit != null)
            {
                TargetClient.GetHabbo().stopConduire();
            }
            TargetClient.GetHabbo().takeBien();
            TargetClient.GetHabbo().updatePrisonEtat(TargetUser, Convert.ToInt32(Minutes), 2);
            TargetClient.GetHabbo().PrisonCount += 1;
            TargetClient.GetHabbo().updatePrisonCount();
            Session.GetHabbo().insertLastAction("A emprisonné " + TargetClient.GetHabbo().Username + " dans une cellule d'isolement pendant " + Minutes + " minute(s).");
            PlusEnvironment.GetGame().GetClientManager().sendLastActionMsg("<span class=\"blue\">" + Session.GetHabbo().Username + "</span> a emprisonné <span class=\"red\">" + TargetClient.GetHabbo().Username + "</span> dans une cellule d'isolement pendant <span>" + Minutes + "</span> minute(s)");
            User.OnChat(User.LastBubble, "* Emprisonne " + TargetClient.GetHabbo().Username + " dans une cellule d'isolement pendant " + Minutes + " minute(s) * ", true);
            TargetClient.SendMessage(new RoomNotificationComposer("prison_alert", "message", Session.GetHabbo().Username + " vous emprisonné dans une cellule d'isolement pendant " + Minutes + " minute(s)."));
            if (TargetClient.GetHabbo().checkIfWanted())
            {
                TargetClient.GetHabbo().WantedToggle(false, 0);
                PlusEnvironment.GetGame().GetClientManager().sendPoliceRadio(TargetClient.GetHabbo().Username + " a été retrouvé et emprisonné.");
            }

            Group Gouvernement = null;
            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
            {
                Gouvernement.ChiffreAffaire += 100;
                Gouvernement.updateChiffre();
            }
        }
    }
}