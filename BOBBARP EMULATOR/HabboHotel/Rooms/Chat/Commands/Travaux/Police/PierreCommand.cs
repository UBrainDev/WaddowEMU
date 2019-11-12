using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class PierreCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true)
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
            get { return "Donner les crédits des pierres cassés à un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :pierre <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("pierre"))
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
            if(User.ConnectedMetier == false)
            {
                Session.SendWhisper("Vous devez vous connectez au réseau de la Police Nationale avant de pouvoir consulter combien de pierres a cassé " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 2 || Math.Abs(User.X - TargetUser.X) > 2 || !TargetUser.Statusses.ContainsKey("sit"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être assis devant vous pour que vous puissez lui donner les crédits.");
                return;
            }

            if (TargetClient.GetHabbo().Pierre == 0)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a pas cassé de pierre.");
                return;
            }

            Session.GetHabbo().addCooldown("pierre", 5000);
            TargetUser.Frozen = true;
            User.OnChat(User.LastBubble, "* Consulte le montant en kg de pierre cassé par " + TargetClient.GetHabbo().Username + " *", true);

            System.Timers.Timer timer1 = new System.Timers.Timer(1000);
            timer1.Interval = 1000;
            timer1.Elapsed += delegate
            {
                User.OnChat(User.LastBubble, "* Constate que " + TargetClient.GetHabbo().Username + " a cassé " + TargetClient.GetHabbo().Pierre + "kg de pierre *", true);
                timer1.Stop();
            };
            timer1.Start();

            System.Timers.Timer timer3 = new System.Timers.Timer(4000);
            timer3.Interval = 4000;
            timer3.Elapsed += delegate
            {
                if (TargetClient.GetHabbo().Pierre != 0)
                {
                    int Credit = Convert.ToInt32(TargetClient.GetHabbo().Pierre * 10);
                    TargetClient.GetHabbo().Pierre = 0;
                    TargetClient.GetHabbo().updatePierre();
                    TargetClient.GetHabbo().Credits += Credit;
                    TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
                    User.OnChat(User.LastBubble, "* Donne l'équivalence du travail effectué soit " + Credit + " crédits à " + TargetClient.GetHabbo().Username + " *", true);
                }

                TargetUser.Frozen = false;
                timer3.Stop();
            };
            timer3.Start();
        }
    }
}