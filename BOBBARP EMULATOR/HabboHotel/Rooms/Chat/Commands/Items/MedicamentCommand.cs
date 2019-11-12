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
    class MedicamentCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Hopital == 0)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "items"; }
        }

        public string Parameters
        {
            get { return "<produit>"; }
        }

        public string Description
        {
            get { return "Prendre un médicament"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :manger <produit>");
                return;
            }

            if (Session.GetHabbo().Sante > 99)
            {
                Session.SendWhisper("Vous n'avez pas besoin de prendre un médicament car votre santé est au maximum.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas prendre de médicament pendant que vous faites un échange.");
                return;
            }

            if (Session.GetHabbo().getCooldown("medicament"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            string Produit = Params[1];
            int Sante;
            string Name;
            if (Produit == "doliprane")
            {
                if (Session.GetHabbo().Doliprane < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de doliprane.");
                    return;
                }

                Session.GetHabbo().Doliprane -= 1;
                Session.GetHabbo().updateDoliprane();
                Sante = 50;
                Name = "un doliprane";
            }
            else
            {
                Session.SendWhisper("Le produit que vous avez rentré est invalide.");
                return;
            }

            Session.GetHabbo().addCooldown("medicament", 6000);
            int NumberSante = 100 - Sante;
            User.OnChat(User.LastBubble, "* Prend "+ Name + " [+"+ Sante + "% SANTÉ] *", true);
            if (Session.GetHabbo().Sante >= NumberSante)
            {
                Session.GetHabbo().Sante = 100;
                Session.GetHabbo().updateSante();
                
            }
            else
            {
                Session.GetHabbo().Sante += Sante;
                Session.GetHabbo().updateSante();
            }

            Session.SendMessage(new WhisperComposer(User.VirtualId, "SANTÉ : "+ Session.GetHabbo().Sante  + "/100", 0, 34));
        }
    }
}