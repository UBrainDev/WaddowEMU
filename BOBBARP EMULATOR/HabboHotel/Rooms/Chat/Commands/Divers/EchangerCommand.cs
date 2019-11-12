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
    class EchangerCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "items"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Échanger ses items avec un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :echanger <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("echanger"))
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

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas échanger avec vous même.");
                return;
            } 

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetUser.IsBot)
            {
                Session.SendWhisper("Vous ne pouvez pas échanger avec cet utilisateur.");
                return;
            }

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                Session.SendWhisper("Vous devez être à coté de " + TargetClient.GetHabbo().Username + " pour pouvoir échanger avec lui.");
                return;
            }

            if (Session.GetHabbo().Hopital != 0 || Session.GetHabbo().Prison != 0)
            {
                Session.SendWhisper("Vous ne pouvez pas faire d'échange lorsque vous êtes hospitalisé ou emprisonné.");
                return;
            }

            if (TargetClient.GetHabbo().Hopital != 0 || TargetClient.GetHabbo().Prison != 0)
            {
                Session.SendWhisper("Vous ne pouvez pas faire d'échange avec un civil hospitalisé ou emprisonné.");
                return;
            }

            if (Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO") || PlusEnvironment.SaladeAttente == false && PlusEnvironment.Salade == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer d'échange dans un ghetto ou pendant une salade.");
                return;
            }

            if (User.Tased || Session.GetHabbo().Menotted == true)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer d'échanger lorsque vous êtes tasé ou menotté.");
                return;
            }

            if (TargetUser.Tased || TargetClient.GetHabbo().Menotted == true)
            {
                Session.SendWhisper("Vous ne pouvez pas lancer d'échanger avec un civil tasé ou menotté.");
                return;
            }

            if (User.usingCasier || User.usingATM)
            {
                Session.SendWhisper("Vous ne pouvez pas faire d'échange pour le moment.");
                return;
            }

            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous êtes déjà en train d'échanger avec quelqu'un.");
                return;
            }

            if (TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà en train d'échanger avec quelqu'un.");
                return;
            }

            if(TargetUser.usingCasier || TargetUser.usingATM)
            {
                Session.SendWhisper("Vous ne pouvez pas faire d'échange avec " + TargetClient.GetHabbo().Username + " pour le moment.");
                return;
            }

            if(TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a une proposition en cours, veuillez patienter.");
                return;
            }

            if (TargetUser.isTradingItems)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " a déjà un échange en cours, veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("echanger", 10000);
            User.OnChat(User.LastBubble, "* Propose à " + TargetClient.GetHabbo().Username +" de faire un échange *", true);
            TargetUser.Transaction = "echange_items:" + Session.GetHabbo().Username;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite échanger avec vous.;0");
        }
    }
}