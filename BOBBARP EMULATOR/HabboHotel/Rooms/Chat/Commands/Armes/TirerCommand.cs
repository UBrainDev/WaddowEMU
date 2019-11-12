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
    class TirerCommand : IChatCommand
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
            get { return "Tirer sur un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :tirer <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().ArmeEquiped == null || Session.GetHabbo().ArmeEquiped == "batte" || Session.GetHabbo().ArmeEquiped == "sabre")
            {
                Session.SendWhisper("Vous devez vous équiper d'une arme à feu pour pouvoir tirer sur un utilisateur.");
                return;
            }

            if (Session.GetHabbo().Hopital == 1)
                return;

            if(Session.GetHabbo().CurrentRoomId == 3 || Session.GetHabbo().CurrentRoomId == 18 || Session.GetHabbo().CurrentRoomId == 20)
            {
                Session.SendWhisper("Vous ne pouvez pas utiliser vos armes ici.");
                return;
            }

            if (Session.GetHabbo().ArmeEquiped != null && !Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && PlusEnvironment.Purge == false)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer en dehors du ghetto.");
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
                Session.SendWhisper("Vous ne pouvez pas tirer pendant que vous rechargez votre arme.");
                return;
            }

            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer sur vous même.");
                return;
            }

            if (TargetClient.GetHabbo().Hopital == 1)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Session.GetHabbo().Menotted == true || User.Tased == true)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer lorsque vous êtes immobilisé.");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (User.Immunised == true)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer sur un civil lorsque vous êtes immunisé.");
                return;
            }

            if (TargetUser.Immunised == true)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer sur " + TargetClient.GetHabbo().Username + " car il est immunisé.");
                return;
            }

            if (User.DuelUser != null && TargetClient.GetHabbo().Username != User.DuelUser)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer sur " + TargetClient.GetHabbo().Username + " car vous êtes en duel contre " + User.DuelUser + ".");
                return;
            }

            if (TargetUser.DuelUser != null && TargetUser.DuelUser != Session.GetHabbo().Username)
            {
                Session.SendWhisper("Vous ne pouvez pas tirer sur " + TargetClient.GetHabbo().Username + " car il est en duel.");
                return;
            }

            int Range;
            int DegatMin;
            int DegatMax;
            string Name;
            int Chargeur;

            if (Session.GetHabbo().ArmeEquiped == "ak47")
            {
                if (Session.GetHabbo().AK47_Munitions == 0)
                {
                    Session.SendWhisper("Vous n'avez plus de munitions pour votre AK47.");
                    return;
                }

                if (Session.GetHabbo().AK47_Munitions > 4)
                {
                    Chargeur = 5;
                }
                else
                {
                    Chargeur = Session.GetHabbo().AK47_Munitions;
                }

                if (Session.GetHabbo().Chargeur < 1)
                {
                    Session.GetHabbo().Recharge = true;
                    User.OnChat(User.LastBubble, "* Recharge sa AK47 *", true);
                    System.Timers.Timer timer2 = new System.Timers.Timer(2500);
                    timer2.Interval = 2500;
                    timer2.Elapsed += delegate
                    {
                        User.OnChat(User.LastBubble, "* Fini de recharger sa AK47 *", true);
                        Session.GetHabbo().Chargeur = Chargeur;
                        Session.GetHabbo().Recharge = false;
                        timer2.Stop();
                    };
                    timer2.Start();
                    return;
                }

                Range = 10;
                DegatMin = 11;
                DegatMax = 16;
                Name = "sa AK47";
                Session.GetHabbo().AK47_Munitions -= 1;
                Session.GetHabbo().updateAK47Munitions();
            }
            else if (Session.GetHabbo().ArmeEquiped == "uzi")
            {
                if (Session.GetHabbo().Uzi_Munitions == 0)
                {
                    Session.SendWhisper("Vous n'avez plus de munitions pour votre Uzi.");
                    return;
                }


                if (Session.GetHabbo().Uzi_Munitions > 6)
                {
                    Chargeur = 7;
                }
                else
                {
                    Chargeur = Session.GetHabbo().Uzi_Munitions;
                }

                if (Session.GetHabbo().Chargeur < 1)
                {
                    Session.GetHabbo().Recharge = true;
                    User.OnChat(User.LastBubble, "* Recharge son Uzi *", true);
                    System.Timers.Timer timer2 = new System.Timers.Timer(3000);
                    timer2.Interval = 3000;
                    timer2.Elapsed += delegate
                    {
                        User.OnChat(User.LastBubble, "* Fini de recharger son Uzi *", true);
                        Session.GetHabbo().Chargeur = Chargeur;
                        Session.GetHabbo().Recharge = false;
                        timer2.Stop();
                    };
                    timer2.Start();
                    return;
                }

                Range = 10;
                DegatMin = 15;
                DegatMax = 10;
                Name = "son Uzi";
                Session.GetHabbo().Uzi_Munitions -= 1;
                Session.GetHabbo().updateUziMunitions();
            }
            else
            {
                return;
            }
            
            if (Math.Abs(User.Y - TargetUser.Y) > Range || Math.Abs(User.X - TargetUser.X) > Range || User.X != TargetUser.X && User.Y != TargetUser.Y)
            {
                User.OnChat(User.LastBubble, "* Tire sur " + TargetClient.GetHabbo().Username + " avec " + Name + " mais ne le touche pas [-2% ÉNERGIE] *", true);
                Session.GetHabbo().Chargeur = Session.GetHabbo().Chargeur - 1;
                Session.GetHabbo().EnergieMalaise(2);
                return;
            }

            Random degatTaked = new Random();
            int degatTakedNumber = degatTaked.Next(DegatMin, DegatMax);
            if(TargetClient.GetHabbo().Sante > degatTakedNumber)
            {
                User.OnChat(User.LastBubble, "* Tire sur " + TargetClient.GetHabbo().Username + " avec " + Name + " lui causant " + degatTakedNumber + " points de dégats [-2% ÉNERGIE] *", true);
                TargetClient.GetHabbo().Sante -= degatTakedNumber;
                TargetClient.GetHabbo().updateSante();
                TargetUser.OnChat(TargetUser.LastBubble, "[SANTÉ] " + TargetClient.GetHabbo().Sante + "/100", true);
            }
            else
            {
                TargetClient.GetHabbo().Hopital = 1;
                int Voler = 0;
                if (TargetClient.GetHabbo().Rank == 1 && PlusEnvironment.Purge == false && Session.GetHabbo().Hopital == 0)
                {
                    decimal decimalCredits = Convert.ToInt32(TargetClient.GetHabbo().Credits);
                    Voler = Convert.ToInt32((decimalCredits / 100m) * 50m);
                }
                User.OnChat(User.LastBubble, "* Tire sur " + TargetClient.GetHabbo().Username + " avec " + Name + " et le tue [-2% ÉNERGIE] *", true);
                TargetUser.OnChat(TargetUser.LastBubble, "* Meurt *", true);
                if(Voler > 0)
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
                if(PlusEnvironment.Purge == true && Session.GetHabbo().Gang != 0)
                {
                    PlusEnvironment.updateGangPurgeKill(Session.GetHabbo().Gang);
                }

                if (User.DuelUser != null)
                {
                    User.DuelUser = null;
                    User.DuelToken = null;
                    User.OnChat(TargetUser.LastBubble, "* Gagne son duel *", true);
                }
                PlusEnvironment.GetGame().GetClientManager().sendLastActionMsg("<span class=\"blue\">" + Session.GetHabbo().Username + "</span> a tué <span class=\"red\">" + TargetClient.GetHabbo().Username + "</span> avec " + Name);
                Session.GetHabbo().insertLastAction("A tué " + TargetClient.GetHabbo().Username + " avec " + Name + ".");
                TargetUser.GetClient().SendNotification(Session.GetHabbo().Username + " vous a tué avec " + Name + ", les ambulanciers ont réussi à vous réanimer. Vous pourrez sortir dans 10 minutes.");
            }
            Session.GetHabbo().Chargeur = Session.GetHabbo().Chargeur - 1;
            Session.GetHabbo().EnergieMalaise(2);
        }
    }
}