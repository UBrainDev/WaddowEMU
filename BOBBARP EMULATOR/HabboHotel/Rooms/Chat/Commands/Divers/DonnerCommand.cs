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
    class DonnerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "divers"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme> <montant>"; }
        }

        public string Description
        {
            get { return "Donner des crédits à un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :donner <pseudonyme> <montant>");
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
                Session.SendWhisper("Vous ne pouvez pas donner des crédits à vous même.");
                return;
            } 

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if(User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas donner de crédits pendant un échange.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas donner des crédits à "+ TargetClient.GetHabbo().Username + " car il est en échange.");
                return;
            }

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper("Vous devez être à coté de " + TargetClient.GetHabbo().Username + " pour pouvoir lui donner des crédits.");
                return;
            }

            if (Session.GetHabbo().getCooldown("donner") == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            int Amount;
            string Montant = Params[2];
            if (!int.TryParse(Montant, out Amount) || Convert.ToInt32(Params[2]) <= 0 || Montant.StartsWith("0"))
            {
                Session.SendWhisper("Le montant est invalide.");
                return;
            }

            if(Convert.ToInt32(Montant) > Session.GetHabbo().Credits)
            {
                Session.SendWhisper("Vous n'avez pas " + Montant + " crédits sur vous.");
                return;
            }

            if(TargetClient.GetHabbo().LastIp == Session.GetHabbo().LastIp)
            {
                Session.SendWhisper("Vous ne pouvez pas donner des crédits à " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            if (Session.GetHabbo() != null)
            {
                Session.GetHabbo().addCooldown("donner", 20000);
                Session.GetHabbo().Credits = Session.GetHabbo().Credits - Convert.ToInt32(Montant);
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
                TargetClient.GetHabbo().Credits = TargetClient.GetHabbo().Credits + Convert.ToInt32(Montant);
                TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
                User.OnChat(User.LastBubble, "* Donne " + Montant + " crédits à " + TargetClient.GetHabbo().Username + " *", true);
                Room.AddChatlog(Session.GetHabbo().Id, "* Donne " + Montant + " crédits à " + TargetClient.GetHabbo().Username + " *");
                if (Convert.ToInt32(Montant) >= 2500)
                {
                    PlusEnvironment.GetGame().GetClientManager().sendStaffMsg(Session.GetHabbo().Username + " a donné " + Convert.ToInt32(Montant) + " crédits à "+ TargetClient.GetHabbo().Username + " dans l'appartement [" + Session.GetHabbo().CurrentRoomId + "].");
                }
            }
        }
    }
}