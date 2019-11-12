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
    class MangerCommand : IChatCommand
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
            get { return "Manger un produit"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :manger <produit>");
                return;
            }

            if (Session.GetHabbo().Energie > 99)
            {
                Session.SendWhisper("Vous n'avez pas besoin de manger car votre énergie est au maximum.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas manger pendant que vous faites un échange.");
                return;
            }

            string Produit = Params[1];
            int Energie;
            string Name;
            if (Produit == "baguette")
            {
                if (Session.GetHabbo().Pain < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de baguette.");
                    return;
                }

                Session.GetHabbo().Pain -= 1;
                Session.GetHabbo().updatePain();
                Energie = 50;
                Name = "une baguette";
            }
            else if (Produit == "sucette")
            {
                if (Session.GetHabbo().Sucette < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de sucette.");
                    return;
                }

                Session.GetHabbo().Sucette -= 1;
                Session.GetHabbo().updateSucette();
                Energie = 10;
                Name = "une sucette";
            }
            else
            {
                Session.SendWhisper("Le produit que vous avez rentré est invalide.");
                return;
            }
            
            int NumberEnergie = 100 - Energie;
            User.OnChat(User.LastBubble, "* Mange "+ Name + " [+"+ Energie + "% ÉNERGIE] *", true);
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