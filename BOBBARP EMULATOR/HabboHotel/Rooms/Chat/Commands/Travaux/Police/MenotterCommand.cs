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
    class MenotterCommand : IChatCommand
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
            get { return "Menotter un utilisateur"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :menotter <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("menotter_command"))
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
                Session.SendWhisper("Vous ne pouvez pas vous menotter vous même.");
                return;
            }


            if (TargetClient.GetHabbo().TravailId == 4 || TargetClient.GetHabbo().TravailId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas menotter cet utilisateur.");
                return;
            }

            if (TargetClient.GetHabbo().Prison != 0 || TargetClient.GetHabbo().Hopital == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas menotter un prisonnier ou une personne hospitalisé.");
                return;
            }

            if(Session.GetHabbo().MenottedUsername != null && Session.GetHabbo().MenottedUsername != TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper("Vous ne pouvez pas menotter " + TargetClient.GetHabbo().Username + " car vous menottez déjà un civil.");
                return;
            }

            if(TargetClient.GetHabbo().Menotted == true && Session.GetHabbo().MenottedUsername != TargetClient.GetHabbo().Username)
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " est déjà menotté.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 1 || Math.Abs(User.X - TargetUser.X) > 1)
            {
                User.OnChat(User.LastBubble, "* Sort ses menottes *", true);
                User.OnChat(User.LastBubble, "* Tente de menotter " + TargetClient.GetHabbo().Username + " mais n'y parvient pas *", true);
                User.OnChat(User.LastBubble, "* Range ses menottes *", true);
                return;
            }

            if (TargetClient.GetHabbo().Menotted == true)
            {
                if (Session.GetHabbo().MenottedUsername != TargetClient.GetHabbo().Username)
                {
                    Session.SendWhisper("Vous ne pouvez pas enlever les menottes de " + TargetClient.GetHabbo().Username + " car vous n'avez pas les clefs.");
                    return;
                }

                TargetUser.FastWalking = false;
                TargetUser.SuperFastWalking = false;
                TargetUser.UltraFastWalking = false;
                User.OnChat(User.LastBubble, "* Enlève les menottes de " + TargetClient.GetHabbo().Username + " *", true);
                TargetClient.GetHabbo().Effects().ApplyEffect(0);
                User.OnChat(User.LastBubble, "* Récupère ses menottes *", true);
                TargetClient.GetHabbo().Menotted = false;
                Session.GetHabbo().MenottedUsername = null;
                return;
            }
            else
            {
                User.OnChat(User.LastBubble, "* Sort ses menottes *", true);
                User.OnChat(User.LastBubble, "* Tente de menotter " + TargetClient.GetHabbo().Username + " *", true);
                Session.GetHabbo().addCooldown("menotter_command", 2500);
                System.Timers.Timer timer1 = new System.Timers.Timer(2000);
                timer1.Interval = 2000;
                timer1.Elapsed += delegate
                {
                    if ((Math.Abs(User.Y - TargetUser.Y) < 2 || Math.Abs(User.X - TargetUser.X) < 2) && Session.GetHabbo().CurrentRoom == TargetClient.GetHabbo().CurrentRoom)
                    {
                        TargetClient.GetHabbo().Menotted = true;
                        Session.GetHabbo().MenottedUsername = TargetClient.GetHabbo().Username;
                        if (TargetClient.GetHabbo().Conduit != null)
                        {
                            TargetClient.GetHabbo().Conduit = null;
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "voiture;stop");
                        }

                        if(TargetUser.isTradingItems)
                        {
                            GameClient ClientTrading = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.isTradingUsername);
                            if (ClientTrading != null)
                            {
                                RoomUser UserTrading = User.GetClient().GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(ClientTrading.GetHabbo().Id);
                                ClientTrading.SendWhisper("L'échange a été annulé.");
                                UserTrading.cancelItemsTrade();
                            }
                            TargetUser.cancelItemsTrade();
                        }

                        if (TargetUser.ConnectedMetier == true)
                        {
                            TargetUser.ConnectedMetier = false;
                        }

                        if (TargetClient.GetHabbo().ArmeEquiped != null)
                        {
                            TargetClient.GetHabbo().ArmeEquiped = null;
                            TargetClient.GetHabbo().resetEffectEvent();
                            TargetUser.OnChat(TargetUser.LastBubble, "* Se déséquipe de son arme *", true);
                        }

                        if (TargetUser.Tased == true)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "police;detaser");
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "police;detaser");
                            TargetUser.Tased = false;
                            User.userTased = null;
                        }

                        User.OnChat(User.LastBubble, "* Parvient à menotter " + TargetClient.GetHabbo().Username + " *", true);
                        TargetClient.GetHabbo().Effects().ApplyEffect(590);
                    }
                    else
                    {
                        User.OnChat(User.LastBubble, "* Ne parvient pas à le menotter *", true);
                        User.OnChat(User.LastBubble, "* Range ses menottes *", true);
                    }
                    timer1.Stop();
                };
                timer1.Start();
            }
        }
    }
}