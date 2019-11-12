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
    class AffairesCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 6 && Session.GetHabbo().Travaille == true)
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
            get { return "Rendre les affaires à un civil"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :affaires <pseudonyme>");
                return;
            }

            if (Session.GetHabbo().getCooldown("affaires"))
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
                Session.SendWhisper("Vous devez vous connectez au réseau de la Police Nationale avant de pouvoir consulter les affaires stockés de " + TargetClient.GetHabbo().Username + ".");
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (Math.Abs(User.Y - TargetUser.Y) > 2 || Math.Abs(User.X - TargetUser.X) > 2 || !TargetUser.Statusses.ContainsKey("sit"))
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " doit être assis devant vous pour que vous puissez lui rendre ces affaires.");
                return;
            }

            if (TargetClient.GetHabbo().PoliceCasier == null || TargetClient.GetHabbo().PoliceCasier == "")
            {
                Session.SendWhisper(TargetClient.GetHabbo().Username + " n'a aucune affaire à lui dans les casiers de la police.");
                return;
            }

            Session.GetHabbo().addCooldown("affaires", 5000);
            TargetUser.Frozen = true;
            User.OnChat(User.LastBubble, "* Consulte les affaires de " + TargetClient.GetHabbo().Username + " stockés dans nos casiers *", true);

            System.Timers.Timer timer1 = new System.Timers.Timer(3000);
            timer1.Interval = 3000;
            timer1.Elapsed += delegate
            {
                User.OnChat(User.LastBubble, "* Rend les affaires personnelles de " + TargetClient.GetHabbo().Username + " *", true);
                if (TargetClient.GetHabbo().PoliceCasier.Contains("[TELEPHONE]"))
                {
                    TargetClient.GetHabbo().Telephone = 1;
                    TargetClient.GetHabbo().updateTelephone();
                    TargetClient.GetHabbo().PoliceCasier = TargetClient.GetHabbo().PoliceCasier.Replace("[TELEPHONE]", "");
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "telephone", "show");
                }

                if (TargetClient.GetHabbo().Permis_arme == 1)
                {
                    if (TargetClient.GetHabbo().PoliceCasier.Contains("[AK47]"))
                    {
                        TargetClient.GetHabbo().Ak47 = 1;
                        TargetClient.GetHabbo().updateAk47();
                        TargetClient.GetHabbo().PoliceCasier = TargetClient.GetHabbo().PoliceCasier.Replace("[AK47]", "");
                    }

                    if (TargetClient.GetHabbo().PoliceCasier.Contains("[UZI]"))
                    {
                        TargetClient.GetHabbo().Uzi = 1;
                        TargetClient.GetHabbo().updateUzi();
                        TargetClient.GetHabbo().PoliceCasier = TargetClient.GetHabbo().PoliceCasier.Replace("[UZI]", "");
                    }

                    if (TargetClient.GetHabbo().PoliceCasier.Contains("[SABRE]"))
                    {
                        TargetClient.GetHabbo().Sabre = 1;
                        TargetClient.GetHabbo().updateSabre();
                        TargetClient.GetHabbo().PoliceCasier = TargetClient.GetHabbo().PoliceCasier.Replace("[SABRE]", "");
                    }

                    if (TargetClient.GetHabbo().PoliceCasier.Contains("[BATTE]"))
                    {
                        TargetClient.GetHabbo().Batte = 1;
                        TargetClient.GetHabbo().updateBatte();
                        TargetClient.GetHabbo().PoliceCasier = TargetClient.GetHabbo().PoliceCasier.Replace("[BATTE]", "");
                    }
                }
                else if(TargetClient.GetHabbo().PoliceCasier.Contains("[AK47]") || TargetClient.GetHabbo().PoliceCasier.Contains("[UZI]") || TargetClient.GetHabbo().PoliceCasier.Contains("[SABRE]") || TargetClient.GetHabbo().PoliceCasier.Contains("[BATTE]"))
                {
                    Session.SendWhisper(TargetClient.GetHabbo().Username + " doit avoir un permis de port d'armes pour récupérer ses armes stockées par la Police Nationale.");
                }
                TargetClient.GetHabbo().updatePoliceCasier();
                TargetUser.Frozen = false;
                timer1.Stop();
            };
            timer1.Start();
        }
    }
}