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
    class PlanterCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "arme"; }
        }

        public string Parameters
        {
            get { return "<pseudonyme>"; }
        }

        public string Description
        {
            get { return "Planter un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :planter <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().ArmeEquiped != "sabre")
            {
                Session.SendWhisper("Vous devez vous équiper d'une sabre pour pouvoir planter un utilisateur.");
                return;
            }

            if (Session.GetHabbo().Hopital == 1)
                return;

            if (Session.GetHabbo().CurrentRoomId == 3 || Session.GetHabbo().CurrentRoomId == 18 || Session.GetHabbo().CurrentRoomId == 20)
            {
                Session.SendWhisper("Vous ne pouvez pas utiliser vos armes ici.");
                return;
            }

            if (Session.GetHabbo().ArmeEquiped != null && !Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && PlusEnvironment.Salade != Session.GetHabbo().CurrentRoomId && PlusEnvironment.Purge == false)
            {
                Session.SendWhisper("Vous ne pouvez pas planter en dehors du ghetto.");
                return;
            }

            string Username = Params[1];
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Username);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("Impossible de trouver " + Username + " dans cet appartement.");
                return;
            }

            if (Session.GetHabbo().Recharge == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous plantez vous même.");
                return;
            }

            if (TargetClient.GetHabbo().Hopital == 1)
                return;
            
            if(TargetClient.GetHabbo().Rank == 8 && PlusEnvironment.Salade == Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Impossible de planter cet utilisateur pendant la salade.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Session.GetHabbo().Menotted == true || User.Tased == true)
            {
                Session.SendWhisper("Vous ne pouvez pas planter lorsque vous êtes immobilisé.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (User.Immunised == true)
            {
                Session.SendWhisper("Vous ne pouvez pas planter un civil lorsque vous êtes immunisé.");
                return;
            }

            if (TargetUser.Immunised == true)
            {
                Session.SendWhisper("Vous ne pouvez pas planter " + TargetClient.GetHabbo().Username + " car il est immunisé.");
                return;
            }

            if (User.DuelUser != null && TargetClient.GetHabbo().Username != User.DuelUser)
            {
                Session.SendWhisper("Vous ne pouvez pas planter " + TargetClient.GetHabbo().Username + " car vous êtes en duel contre " + User.DuelUser + ".");
                return;
            }

            if (TargetUser.DuelUser != null && TargetUser.DuelUser != Session.GetHabbo().Username)
            {
                Session.SendWhisper("Vous ne pouvez pas planter " + TargetClient.GetHabbo().Username + " car il est en duel.");
                return;
            }

            int Range;
            int DegatMin;
            int DegatMax;
            string Name;

            if (Session.GetHabbo().ArmeEquiped == "sabre")
            {
                Range = 1;
                DegatMin = 12;
                DegatMax = 21;
                Name = "son sabre";
            }
            else
            {
                return;
            }
            
            if (Math.Abs(User.Y - TargetUser.Y) > Range || Math.Abs(User.X - TargetUser.X) > Range)
            {
                User.OnChat(User.LastBubble, "* Tente de planter " + TargetClient.GetHabbo().Username + " avec " + Name + " mais ne le touche pas [-2% ÉNERGIE] *", true);
                Session.GetHabbo().Chargeur = Session.GetHabbo().Chargeur - 1;
                Session.GetHabbo().EnergieMalaise(2);
                return;
            }

            Random degatTaked = new Random();
            int degatTakedNumber = degatTaked.Next(DegatMin, DegatMax);
            if(TargetClient.GetHabbo().Sante > degatTakedNumber)
            {
                User.OnChat(User.LastBubble, "* Plante " + TargetClient.GetHabbo().Username + " avec " + Name + " lui causant " + degatTakedNumber + " points de dégats [-2% ÉNERGIE] *", true);
                TargetClient.GetHabbo().Sante -= degatTakedNumber;
                TargetClient.GetHabbo().updateSante();
                TargetUser.OnChat(TargetUser.LastBubble, "[SANTÉ] " + TargetClient.GetHabbo().Sante + "/100", true);
            }
            else
            {
                TargetClient.GetHabbo().Hopital = 1;
                int Voler = 0;
                if (TargetClient.GetHabbo().Rank == 1 && PlusEnvironment.Purge == false && Session.GetHabbo().Hopital == 0 && PlusEnvironment.Salade != Session.GetHabbo().CurrentRoomId)
                {
                    decimal decimalCredits = Convert.ToInt32(TargetClient.GetHabbo().Credits);
                    Voler = Convert.ToInt32((decimalCredits / 100m) * 50m);
                }
                User.OnChat(User.LastBubble, "* Plante " + TargetClient.GetHabbo().Username + " avec " + Name + " et le tue [-2% ÉNERGIE] *", true);
                TargetUser.OnChat(TargetUser.LastBubble, "* Meurt *", true);
                if (Voler > 0)
                {
                    User.OnChat(User.LastBubble, "* Récupère " + Voler + " crédits sur le cadavre de " + TargetClient.GetHabbo().Username + " *", true);
                    TargetClient.GetHabbo().Credits -= Voler;
                    TargetClient.SendMessage(new CreditBalanceComposer(TargetClient.GetHabbo().Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "my_stats;" + TargetClient.GetHabbo().Credits + ";" + TargetClient.GetHabbo().Duckets + ";" + TargetClient.GetHabbo().EventPoints);
                    Session.GetHabbo().Credits += Voler;
                    Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
                }
                TargetClient.GetHabbo().updateHopitalEtat(TargetUser, 10);
                TargetClient.GetHabbo().Sante = 0;
                TargetClient.GetHabbo().updateSante();
                Session.GetHabbo().Kills += 1;
                Session.GetHabbo().updateKill();
                Session.GetHabbo().updateGangKill();
                Session.GetHabbo().updateXpGang(10);
                TargetClient.GetHabbo().Morts += 1;
                TargetClient.GetHabbo().updateMort();
                TargetClient.GetHabbo().updateGangMort();
                if (PlusEnvironment.Purge == true && Session.GetHabbo().Gang != 0)
                {
                    PlusEnvironment.updateGangPurgeKill(Session.GetHabbo().Gang);
                }
                else if (PlusEnvironment.Salade == Session.GetHabbo().CurrentRoomId)
                {
                    PlusEnvironment.GetGame().GetClientManager().checkIfWinSalade();
                }

                if (User.DuelUser != null)
                {
                    User.DuelUser = null;
                    User.DuelToken = null;
                    User.OnChat(TargetUser.LastBubble, "* Gagne son duel *", true);
                }
                Session.GetHabbo().insertLastAction("A tué " + TargetClient.GetHabbo().Username + " avec " + Name + ".");
                PlusEnvironment.GetGame().GetClientManager().sendLastActionMsg("<span class=\"blue\">" + Session.GetHabbo().Username + "</span> a tué <span class=\"red\">" + TargetClient.GetHabbo().Username + "</span> avec " + Name);
                TargetUser.GetClient().SendNotification(Session.GetHabbo().Username + " vous a tué avec " + Name + ", les ambulanciers ont réussi à vous réanimer. Vous pourrez sortir dans 10 minutes.");
            }
        }
    }
}