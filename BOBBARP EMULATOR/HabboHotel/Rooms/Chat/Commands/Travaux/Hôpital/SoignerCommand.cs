using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class SoignerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 7 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 4)
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
            get { return "Soigner un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :soigner <pseudonyme>");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper("Vous ne pouvez pas soigner " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if (TargetClient.GetHabbo().Hopital == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'est pas hospitalisé.");
                return;
            }

            if (User.AnalyseUser != TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous devez analyser " + TargetClient.GetHabbo().Username + " avant de pouvoir le soigner.");
                return;
            }

            if (!User.mainPropre)
            {
                Session.SendWhisper("Vous devez vous laver les mains avant de pouvoir soigner un civil.");
                return;
            }

            if (TargetClient.GetHabbo().Energie == 0)
            {
                User.OnChat(User.LastBubble, "* Donne à " + TargetClient.GetHabbo().Username + " une barre de chocolat *", true);
                TargetUser.OnChat(TargetUser.LastBubble, "* Mange la barre de chocolat [+50% ÉNERGIE] *", true);
                TargetClient.GetHabbo().Energie = 50;
                TargetClient.GetHabbo().updateEnergie();
                TargetClient.SendMessage(new WhisperComposer(TargetUser.VirtualId, "ÉNERGIE : " + TargetClient.GetHabbo().Energie + "/100", 0, 34));
                TargetClient.GetHabbo().endHopital(TargetClient, 1);
            }
            else if (TargetClient.GetHabbo().Sante != 100)
            {
                User.OnChat(User.LastBubble, "* Constate les blessures de " + TargetClient.GetHabbo().Username + " *", true);
                if (TargetClient.GetHabbo().Sante > 49)
                {
                    User.OnChat(User.LastBubble, "* Fait des points de suture sur les plaies de " + TargetClient.GetHabbo().Username + " *", true);
                    TargetUser.OnChat(TargetUser.LastBubble, "* Reçoit les soins [+50% SANTÉ] *", true);
                    TargetClient.GetHabbo().Sante = 100;
                    TargetClient.GetHabbo().updateSante();
                    TargetClient.GetHabbo().endHopital(TargetClient, 1);
                }
                else
                {
                    User.OnChat(User.LastBubble, "* Donne à " + TargetClient.GetHabbo().Username + " des antibiotiques et soigne les blessures les plus profondes *", true);
                    TargetUser.OnChat(TargetUser.LastBubble, "* Reçoit les soins [+50% SANTÉ] *", true);
                    TargetClient.GetHabbo().Sante = TargetClient.GetHabbo().Sante + 50;
                    TargetClient.GetHabbo().updateSante();
                }
            }
            else
            {
                User.OnChat(User.LastBubble, "* Soigne " + TargetClient.GetHabbo().Username + " *", true);
                TargetClient.GetHabbo().endHopital(TargetClient, 1);
            }
        }
    }
}