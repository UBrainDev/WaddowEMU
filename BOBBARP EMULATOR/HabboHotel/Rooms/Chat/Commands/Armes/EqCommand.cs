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
    class EqCommand : IChatCommand
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
            get { return "<arme>"; }
        }

        public string Description
        {
            get { return "S'équiper d'une arme."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :eq <arme>");
                return;
            }

            string Arme = Params[1];
            if (!Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO") && PlusEnvironment.Purge == false && Arme != "taser" && PlusEnvironment.Salade != Session.GetHabbo().CurrentRoomId)
            {
                Session.SendWhisper("Vous ne pouvez pas vous équiper d'une arme ici.");
                return;
            }

            if (PlusEnvironment.Salade == Session.GetHabbo().CurrentRoomId && Session.GetHabbo().Rank == 8)
            {
                Session.SendWhisper("Les staffs ne peuvent pas s'équipe d'une arme pendant la salade.");
                return;
            }

            if (Session.GetHabbo().getCooldown("eq_command") == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (Session.GetHabbo().ArmeEquiped == Arme)
            {
                User.OnChat(User.LastBubble, "* Se déséquipe de son arme *", true);
                Session.GetHabbo().ArmeEquiped = null;
                Session.GetHabbo().resetEffectEvent();
                return;
            }

            if (Session.GetHabbo().Recharge == true)
            {
                Session.SendWhisper("Vous ne pouvez pas vous équiper d'une arme pendant que vous rechargez.");
                return;
            }
            
            if (Session.GetHabbo().Conduit != null && Arme != "taser" || User.isDemarreCar == true && Arme != "taser")
            {
                Session.SendWhisper("Vous ne pouvez pas vous équiper d'une arme pendant que vous conduisez.");
                return;
            }
            
            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas vous équiper d'une arme pendant que vous faites un échange.");
                return;
            }

            string Name;
            int Enable;
            int TempsRecharge;
            int Chargeur = 0;

            Session.GetHabbo().addCooldown("eq_command", 3000);
            if (Arme == "batte")
            {
                if (Session.GetHabbo().Batte == 0)
                {
                    Session.SendWhisper("Vous n'avez pas de batte de baseball.");
                    return;
                }

                TempsRecharge = 0;
                Enable = 591;
                Name = "S'équipe d'une batte de baseball";
            }
            else if (Arme == "taser")
            {
                if (Session.GetHabbo().TravailId != 4 || Session.GetHabbo().Travaille == false)
                    return;

                TempsRecharge = 0;
                Enable = 592;
                Name = "S'équipe d'un taser";
            }
            else if (Arme == "sabre")
            {
                if (Session.GetHabbo().Sabre == 0)
                {
                    Session.SendWhisper("Vous n'avez pas de sabre.");
                    return;
                }

                TempsRecharge = 0;
                Enable = 162;
                Name = "S'équipe d'un sabre";
            }
            else if (Arme == "cocktail")
            {
                if (Session.GetHabbo().Cocktails == 0)
                {
                    Session.SendWhisper("Vous n'avez pas de cocktail molotov.");
                    return;
                }

                if(User.DuelUser != null)
                {
                    Session.SendWhisper("Vous ne pouvez pas vous équiper d'un cocktail molotov pendant un duel.");
                    return;
                }

                TempsRecharge = 0;
                Enable = 1005;
                Name = "S'équipe d'un cocktail molotov";
            }
            else if (Arme == "ak47")
            {
                if (Session.GetHabbo().Ak47 == 0)
                {
                    Session.SendWhisper("Vous n'avez pas d'AK47.");
                    return;
                }

                if (Session.GetHabbo().CurrentRoomId == PlusEnvironment.Salade)
                {
                    Session.SendWhisper("Vous ne pouvez pas vous équipe d'arme à feu pendant la salade.");
                    return;
                }

                if (Session.GetHabbo().AK47_Munitions == 0)
                {
                    Session.SendWhisper("Vous n'avez plus de munitions pour votre AK47.");
                    return;
                }

                TempsRecharge = 2500;
                Enable = 583;
                Name = "S'équipe d'une AK47";
                if(Session.GetHabbo().AK47_Munitions > 4)
                {
                    Chargeur = 5;
                }
                else
                {
                    Chargeur = Session.GetHabbo().AK47_Munitions;
                }
            }
            else if (Arme == "uzi")
            {
                if (Session.GetHabbo().Uzi == 0)
                {
                    Session.SendWhisper("Vous n'avez pas d'Uzi.");
                    return;
                }

                if (Session.GetHabbo().CurrentRoomId == PlusEnvironment.Salade)
                {
                    Session.SendWhisper("Vous ne pouvez pas vous équipe d'arme à feu pendant la salade.");
                    return;
                }

                if (Session.GetHabbo().Uzi_Munitions == 0)
                {
                    Session.SendWhisper("Vous n'avez plus de munitions pour votre Uzi.");
                    return;
                }

                TempsRecharge = 3000;
                Enable = 580;
                Name = "S'équipe d'un Uzi";
                if (Session.GetHabbo().Uzi_Munitions > 6)
                {
                    Chargeur = 7;
                }
                else
                {
                    Chargeur = Session.GetHabbo().Uzi_Munitions;
                }
            }
            else
            {
                Session.SendWhisper("L'arme que vous avez indiqué est invalide.");
                return;
            }

            if (Session.GetHabbo().Conduit == null)
            {
                Session.GetHabbo().Effects().ApplyEffect(Enable);
            }
            User.OnChat(User.LastBubble, "* " + Name + " *", true);
            Session.GetHabbo().ArmeEquiped = Arme;
            if (TempsRecharge > 0)
            {
                Session.GetHabbo().Recharge = true;
                if(Arme == "ak47")
                {
                    User.OnChat(User.LastBubble, "* Recharge sa AK47 *", true);
                }
                else if (Arme == "uzi")
                {
                    User.OnChat(User.LastBubble, "* Recharge son Uzi *", true);
                }

                System.Timers.Timer timer2 = new System.Timers.Timer(TempsRecharge);
                timer2.Interval = TempsRecharge;
                timer2.Elapsed += delegate
                {
                    if (Arme == "ak47")
                    {
                        User.OnChat(User.LastBubble, "* Fini de recharger sa AK47 *", true);
                    }
                    else if (Arme == "uzi")
                    {
                        User.OnChat(User.LastBubble, "* Fini de recharger son Uzi *", true);
                    }
                    Session.GetHabbo().Chargeur = Chargeur;
                    Session.GetHabbo().Recharge = false;
                    timer2.Stop();
                };
                timer2.Start();
            }
        }
    }
}