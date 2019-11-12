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
    class TaserCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().Username == "ADMIN-Soubes" || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true)
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
            get { return "Taser un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :taser <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().ArmeEquiped != "taser")
            {
                Session.SendWhisper("Vous devez être équipé d'un taser.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous taser vous même.");
                return;
            }

            if (Session.GetHabbo().getCooldown("taser_command"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }


            if (TargetClient.GetHabbo().TravailId == 4 || TargetClient.GetHabbo().TravailId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas taser cet utilisateur.");
                return;
            }

            if (TargetClient.GetHabbo().Prison != 0 || TargetClient.GetHabbo().Hopital == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas taser une personne hospitalisé ou emprisonné.");
                return;
            }

            if (TargetClient.GetHabbo().Menotted == true)
            {
                Session.SendWhisper("Vous ne pouvez pas taser une personne qui est menottée.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetUser.Tased == true)
            {
                Session.SendWhisper("Cet utilisateur est déjà tasé.");
                return;
            }

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                User.OnChat(User.LastBubble, "* Tente de taser " + TargetClient.GetHabbo().Username + " mais n'y parvient pas *", true);
                return;
            }

            Session.GetHabbo().addCooldown("taser_command", 6000);
            if (TargetClient.GetHabbo().Conduit != null)
            {
                TargetClient.GetHabbo().Conduit = null;
            }
            TargetUser.Tased = true;
            User.userTased = TargetClient.GetHabbo().Username;
            User.OnChat(User.LastBubble, "* Tase " + TargetClient.GetHabbo().Username + " *", true);
            TargetClient.GetHabbo().Effects().ApplyEffect(53);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "police;taser");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "police;taser");

            System.Timers.Timer timer2 = new System.Timers.Timer(4000);
            timer2.Interval = 4000;
            timer2.Elapsed += delegate
            {
                if (TargetClient.GetHabbo().Effects().CurrentEffect == 53)
                    TargetClient.GetHabbo().resetEffectEvent();
                TargetUser.Tased = false;
                User.userTased = null;
                timer2.Stop();
            };
            timer2.Start();
        }
    }
}