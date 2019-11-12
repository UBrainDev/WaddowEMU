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
    class FumerCommand : IChatCommand
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
            get { return "Fumer un produit"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :fumer <produit>");
                return;
            }

            if(Session.GetHabbo().getCooldown("fumer"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas fumer pendant que vous faites un échange.");
                return;
            }

            string Produit = Params[1];
            if (Produit == "joint")
            {
                if (Session.GetHabbo().Weed < 2)
                {
                    Session.SendWhisper("Il vous faut 2g de weed pour pouvoir fumer un joint.");
                    return;
                }

                if (Session.GetHabbo().PhilipMo < 1)
                {
                    Session.SendWhisper("Il vous faut une cigarette pour pouvoir rouler.");
                    return;
                }

                if (Session.GetHabbo().Clipper < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de feu.");
                    return;
                }

                Session.GetHabbo().PhilipMo -= 1;
                Session.GetHabbo().updatePhilipMo();
                Session.GetHabbo().Weed -= 2;
                Session.GetHabbo().updateWeed();
                Session.GetHabbo().Clipper -= 1;
                Session.GetHabbo().updateClipper();
            }
            else if (Produit == "cigarette")
            {
                if (Session.GetHabbo().PhilipMo < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de cigarette.");
                    return;
                }

                if (Session.GetHabbo().Clipper < 1)
                {
                    Session.SendWhisper("Vous n'avez pas de feu.");
                    return;
                }

                if(Session.GetHabbo().Sante < 10)
                {
                    Session.SendWhisper("Vous ne pouvez pas fumer de cigarette car votre santé est trop faible.");
                    return;
                }

                Session.GetHabbo().PhilipMo -= 1;
                Session.GetHabbo().updatePhilipMo();
                Session.GetHabbo().Clipper -= 1;
                Session.GetHabbo().updateClipper();
            }
            else
            {
                Session.SendWhisper("Le produit que vous avez rentré est invalide.");
                return;
            }

            Session.GetHabbo().addCooldown("fumer", 10000);
            if (Produit == "joint")
            {
                User.OnChat(User.LastBubble, "* Sort une cigarette de son paquet et vide le tabac dans sa feuille slim *", true);
                User.OnChat(User.LastBubble, "* Ajoute 2g de weed et roule son joint *", true);
                User.OnChat(User.LastBubble, "* Allume son joint avec son clipper et le fume [+50% SANTÉ] *", true);

                if (Session.GetHabbo().Sante > 50)
                {
                    Session.GetHabbo().Sante = 100;
                    Session.GetHabbo().updateSante();

                }
                else
                {
                    Session.GetHabbo().Sante += 50;
                    Session.GetHabbo().updateSante();
                }
                Session.SendMessage(new WhisperComposer(User.VirtualId, "SANTÉ : " + Session.GetHabbo().Energie + "/100", 0, 34));
                Session.GetHabbo().SmokeTimer += 1;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "smoke;start");
            }
            else
            {
                User.OnChat(User.LastBubble, "* Sort une cigarette de son paquet *", true);
                User.OnChat(User.LastBubble, "* Allume sa cigarette avec son clipper et la fume [-2% SANTÉ] *", true);
                if (Session.GetHabbo().Sante > 1)
                {
                    Session.GetHabbo().Sante -= 2;
                    Session.GetHabbo().updateSante();

                }
            }
        }
    }
}