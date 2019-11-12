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
    class EmpCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true)
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
            get { return "Emprisonner un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :emp <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient.GetHabbo() == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Menotted == false)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être menotter avant de pouvoir l'emprisonner.");
                return;
            }

            if (Session.GetHabbo().MenottedUsername != TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous ne pouvez pas emprisonné " + TargetClient.GetHabbo().Username + " car ce n'est pas vous qui l'avez menotter.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous emprisonner vous même.");
                return;
            }

            if (Session.GetHabbo().CurrentRoomId != 20)
            {
                Session.SendWhisper("Vous devez être à l'Hôtel de Police pour pouvoir enfermé un civil.");
                return;
            }

            if (!TargetClient.GetHabbo().checkIfWanted())
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être sur la liste des personnes recherchées avant de pouvoir l'emprisonner.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 3 || Math.Abs(User.X - TargetUser.X) > 3)
            {
                Session.SendWhisper("Vous devez être à coté de " + TargetClient.GetHabbo().Username + " pour pouvoir l'emprisonner.");
                return;
            }

            if (TargetUser.policeFouille == false)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit déposer ses affaires personnelles avant de pouvoir être enfermé.");
                return;
            }

            int Minutes = 5;
            if(TargetClient.GetHabbo().checkStarWanted() == 1)
            {
                Minutes = 5;
            }
            else if (TargetClient.GetHabbo().checkStarWanted() == 2)
            {
                Minutes = 10;
            }
            else if (TargetClient.GetHabbo().checkStarWanted() == 3)
            {
                Minutes = 15;
            }
            else if (TargetClient.GetHabbo().checkStarWanted() == 4)
            {
                Minutes = 20;
            }
            else if (TargetClient.GetHabbo().checkStarWanted() == 5)
            {
                Minutes = 30;
            }

            Session.GetHabbo().MenottedUsername = null;
            TargetClient.GetHabbo().Menotted = false;
            TargetClient.GetHabbo().resetEffectEvent();
            TargetClient.GetHabbo().updatePrisonEtat(TargetUser, Minutes, 1);
            TargetClient.GetHabbo().PrisonCount += 1;
            TargetClient.GetHabbo().updatePrisonCount();
            Session.GetHabbo().insertLastAction("A emprisonné " + TargetClient.GetHabbo().Username + " pendant " + Minutes + " minute(s).");
            PlusEnvironment.GetGame().GetClientManager().sendLastActionMsg("<span class=\"blue\">" + Session.GetHabbo().Username + "</span> a emprisonné <span class=\"red\">" + TargetClient.GetHabbo().Username + "</span> pendant <span>" + Minutes + "</span> minute(s)");
            User.OnChat(User.LastBubble, "* Emprisonne " + TargetClient.GetHabbo().Username + " pendant " + Minutes + " minute(s) * ", true);
            TargetClient.SendMessage(new RoomNotificationComposer("prison_alert", "message", Session.GetHabbo().Username + " vous emprisonné pendant " + Minutes + " minute(s)."));
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