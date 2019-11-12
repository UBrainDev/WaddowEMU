using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;


using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RechercherCommand : IChatCommand
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
            get { return "<pseudonyme> <niveau>"; }
        }

        public string Description
        {
            get { return "Ajouter/supprimer un civil de la liste des personnes recherchées"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :rechercher <pseudonyme> <niveau>");
                return;
            }

            if (Session.GetHabbo().getCooldown("rechercher"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            string Username = Params[1];

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Username);

            if (Habbo == null)
            {
                Session.SendWhisper("Impossible d'ajouter " + Username + " à la liste des personnes recherchées pour le moment...");
                return;
            }

            if (Params[2] == "up")
            {
                if (!Habbo.checkIfWanted())
                {
                    Session.SendWhisper(Habbo.Username + " n'est pas sur la liste des personnes recherchées.");
                    return;
                }

                if (Habbo.checkStarWanted() == 5)
                {
                    Session.SendWhisper(Habbo.Username + " est déjà recherché à un niveau 5/5.");
                    return;
                }

                RoomUser User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                Session.GetHabbo().addCooldown("rechercher", 2000);
                User2.OnChat(User2.LastBubble, "* Augmente le niveau de recherche de " + Habbo.Username + " à "+ Convert.ToString(Habbo.checkStarWanted() + 1) + "/5 *", true);
                if (Habbo.InRoom)
                {
                    Habbo.GetClient().SendWhisper("Vous êtes désormais recherché à un niveau de " + Convert.ToString(Habbo.checkStarWanted() + 1) + "/5.");
                }
                Habbo.WantedLevelToggle(true);
                return;
            }

            if (Params[2] == "down")
            {
                if (!Habbo.checkIfWanted())
                {
                    Session.SendWhisper(Habbo.Username + " n'est pas sur la liste des personnes recherchées.");
                    return;
                }

                if (Habbo.checkStarWanted() == 1)
                {
                    Session.SendWhisper(Habbo.Username + " est déjà recherché à un niveau 1/1.");
                    return;
                }

                RoomUser User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                Session.GetHabbo().addCooldown("rechercher", 2000);
                User2.OnChat(User2.LastBubble, "* Baisse le niveau de recherche de " + Habbo.Username + " à " + Convert.ToString(Habbo.checkStarWanted() - 1) + "/5 *", true);
                if (Habbo.InRoom)
                {
                    Habbo.GetClient().SendWhisper("Vous êtes désormais recherché à un niveau de " + Convert.ToString(Habbo.checkStarWanted() - 1) + "/5.");
                }
                Habbo.WantedLevelToggle(false);
                return;
            }

            if (Habbo.Prison != 0)
            {
                Session.SendWhisper("Vous ne pouvez pas ajouter une personne emprisonné à la liste des personnes recherchées.");
                return;
            }

            if (Habbo.Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("Vous ne pouvez pas vous ajouter vous même à la liste des personnes recherchées.");
                return;
            }

            if (Habbo.TravailId == 4 || Habbo.TravailId == 18)
            {
                Session.SendWhisper("Vous ne pouvez pas ajouter cet personne à la liste des personnes recherchées.");
                return;
            }

            int Level;
            if (!int.TryParse(Params[2], out Level) || Convert.ToInt32(Params[2]) < 1 || Convert.ToInt32(Params[2]) > 5 || Params[2].StartsWith("0"))
            {
                Session.SendWhisper("Le niveau de recherche est invalide.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            Session.GetHabbo().addCooldown("rechercher", 3000);
            if (!Habbo.checkIfWanted())
            {
                User.OnChat(User.LastBubble, "* Ajoute " + Habbo.Username + " à la liste des personnes recherchées avec un niveau de recherche de " + Level + "/5 *", true);
                Habbo.WantedToggle(true, Level);
                if(Habbo.InRoom)
                {
                    Habbo.GetClient().SendWhisper("Vous êtes désormais sur la liste des personnes recherchées.");
                }
                PlusEnvironment.GetGame().GetClientManager().sendPoliceRadio(Session.GetHabbo().Username + " a ajouté " + Habbo.Username + " à la liste des personnes recherchées.");
                PlusEnvironment.GetGame().GetClientManager().sendLastActionMsg("<span class=\"red\">" + Habbo.Username + "</span> est maintenant recherché");
                return;
            }
            else
            {
                User.OnChat(User.LastBubble, "* Enlève " + Habbo.Username + " de la liste des personnes recherchées *", true);
                Habbo.WantedToggle(false, 0);
                if (Habbo.InRoom)
                {
                    Habbo.GetClient().SendWhisper("Vous avez été enlevé de la liste des personnes recherchées.");
                }
                PlusEnvironment.GetGame().GetClientManager().sendPoliceRadio(Session.GetHabbo().Username + " a enlevé " + Habbo.Username + " de la liste des personnes recherchées.");
                return;
            }
        }
    }
}