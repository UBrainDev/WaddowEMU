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
    class BoireCommand : IChatCommand
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
            get { return "Boire un produit"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :boire <produit>");
                return;
            }

            if (Session.GetHabbo().Energie > 99)
            {
                Session.SendWhisper("Vous n'avez pas besoin de boire car votre énergie est au maximum.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if(User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas boire pendant que vous faites un échange.");
                return;
            }

            string Produit = Params[1];
            int Energie;
            string Name;
            if (Produit == "coca" || Produit == "coca-cola")
            {
                if (Session.GetHabbo().Coca < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de coca.");
                    return;
                }

                Session.GetHabbo().Coca -= 1;
                Session.GetHabbo().updateCoca();
                Energie = 25;
                Name = "un coca";
            }
            else if (Produit == "fanta")
            {
                if (Session.GetHabbo().Fanta < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de fanta.");
                    return;
                }

                Session.GetHabbo().Fanta -= 1;
                Session.GetHabbo().updateFanta();
                Energie = 25;
                Name = "un fanta";
            }
            else
            {
                Session.SendWhisper("Le produit que vous avez rentré est invalide.");
                return;
            }
            
            int NumberEnergie = 100 - Energie;
            User.OnChat(User.LastBubble, "* Boit "+ Name + " [+"+ Energie + "% ÉNERGIE] *", true);
            if (Session.GetHabbo().Energie >= NumberEnergie)
            {
                Session.GetHabbo().Energie = 100;
                Session.GetHabbo().updateEnergie();
                
            }
            else
            {
                Session.GetHabbo().Energie += Energie;
                Session.GetHabbo().updateEnergie();
            }

            Session.SendMessage(new WhisperComposer(User.VirtualId, "ÉNERGIE : "+ Session.GetHabbo().Energie  + "/100", 0, 34));
        }
    }
}