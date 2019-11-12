using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorDouche : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User.X == Item.GetX && User.Y == Item.GetY)
            {
                if (Session.GetHabbo().Hygiene > 99)
                {
                    Session.SendWhisper("Vous n'avez pas besoin de vous laver car votre hygiène est au maximum.");
                    return;
                }

                if (Session.GetHabbo().Savon <= 0)
                {
                    Session.SendWhisper("Il vous faut un savon pour pouvoir vous laver.");
                    return;
                }

                if (Session.GetHabbo().getCooldown("douche_interactor"))
                {
                    Session.SendWhisper("Veuillez patienter.");
                    return;
                }

                Session.GetHabbo().addCooldown("douche_interactor", 10000);
                User.Frozen = true;
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                Item.RequestUpdate(2, true);
                string NewLook = "";
                if (Session.GetHabbo().Gender == "f")
                {
                    NewLook = Session.GetHabbo().changeLook(Session.GetHabbo().Look, "hr-515-33.ch-3197-92.lg-3198-92.hd-3100-1370");
                }
                else
                {
                    NewLook = Session.GetHabbo().changeLook(Session.GetHabbo().Look, "hr-3163-45.ch-3203-1408.lg-3136-92.hd-180-1379");
                }
                Session.GetHabbo().Look = NewLook;
                Session.SendMessage(new UserChangeComposer(User, true));
                Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(User, false));

                User.OnChat(User.LastBubble, "* Ouvre l'eau de la douche *", true);

                System.Timers.Timer timer1 = new System.Timers.Timer(2000);
                timer1.Interval = 2000;
                timer1.Elapsed += delegate
                {
                    User.OnChat(User.LastBubble, "* Se frotte avec un savon *", true);
                    Session.GetHabbo().Savon = Session.GetHabbo().Savon - 1;
                    Session.GetHabbo().updateSavon();
                    timer1.Stop();
                };
                timer1.Start();

                System.Timers.Timer timer2 = new System.Timers.Timer(5000);
                timer2.Interval = 5000;
                timer2.Elapsed += delegate
                {
                    Item.ExtraData = "0";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(2, true);
                    User.OnChat(User.LastBubble, "* Fini de prendre sa douche *", true);
                    Session.GetHabbo().Hygiene = 100;
                    Session.GetHabbo().updateHygiene();
                    Session.GetHabbo().resetEffectEvent();
                    User.Frozen = false;
                    Session.SendMessage(new WhisperComposer(User.VirtualId, "HYGIÈNE : 100/100 - SAVONS RESTANTS : " + Session.GetHabbo().Savon, 0, 34));
                    Session.GetHabbo().resetAvatarEvent();
                    timer2.Stop();
                };
                timer2.Start();
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}