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
    class AnalyserCommand : IChatCommand
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
            get { return "Analyser un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :analyser <pseudonyme>");
                return;
            }

            if(Session.GetHabbo().getCooldown("analyser_command"))
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

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper("Vous ne pouvez pas analyser " + TargetClient.GetHabbo().Username + " car il est trop loin de vous.");
                return;
            }

            if (TargetClient.GetHabbo().Hopital == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'est pas hospitalisé.");
                return;
            }

            if(TargetUser.wantSoin == false && TargetClient.GetHabbo().Mutuelle != 4)
            {
                if (TargetUser.Transaction != null)
                {
                    Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà une proposition en cours, veuillez patienter.");
                    return;
                }

                Session.GetHabbo().addCooldown("analyser_command", 2000);
                User.OnChat(User.LastBubble, "* Propose à " + TargetClient.GetHabbo().Username + " de recevoir des soins payant *", true);
                int PrixSoin = Convert.ToInt32(PlusEnvironment.GetConfig().data["price.soin"]);

                if (TargetClient.GetHabbo().Mutuelle == 1)
                {
                    PrixSoin  = (PrixSoin * 75) / 100;
                }
                else if (TargetClient.GetHabbo().Mutuelle == 2)
                {
                    PrixSoin = (PrixSoin * 50) / 100;
                }
                else if(TargetClient.GetHabbo().Mutuelle == 3)
                {
                    PrixSoin = (PrixSoin * 25) / 100;
                }

                TargetUser.Transaction = "soins";
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous <b>soigner</b> pour <b>" + PrixSoin + " crédits</b>.;0");
                return;
            }

            if (User.AnalyseUser == TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous avez déjà analysé cet utilisateur.");
                return;
            }

            Session.GetHabbo().addCooldown("analyser_command", 2000);
            User.AnalyseUser = TargetClient.GetHabbo().Username;
            User.OnChat(User.LastBubble, "* Analyse " + TargetClient.GetHabbo().Username + " *", true);
            if (TargetClient.GetHabbo().Energie == 0)
            {
                User.OnChat(User.LastBubble, "* Constate que " + TargetClient.GetHabbo().Username + " manque d'énergie *", true);
            }
            else if (TargetClient.GetHabbo().Sante != 100)
            {
                User.OnChat(User.LastBubble, "* Constate les blessures de " + TargetClient.GetHabbo().Username + " *", true);
            }
            else
            {
                User.OnChat(User.LastBubble, "* Constate la baisse de tension de " + TargetClient.GetHabbo().Username + " *", true);
            }
        }
    }
}