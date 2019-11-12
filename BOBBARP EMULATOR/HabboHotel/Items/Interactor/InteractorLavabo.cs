using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorLavabo : IFurniInteractor
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

            if (Session.GetHabbo().TravailId != 7 || Session.GetHabbo().Travaille == false)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            if(Session.GetHabbo().getCooldown("lavage_main"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if (User.mainPropre)
            {
                Session.SendWhisper("Vous avez déjà les mains propres.");
                return;
            }

            Session.GetHabbo().addCooldown("lavage_main", 3500);
            User.Frozen = true;
            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            User.OnChat(User.LastBubble, "* Ouvre l'eau du lavabo *", true);

            System.Timers.Timer timer1 = new System.Timers.Timer(1000);
            timer1.Interval = 1500;
            timer1.Elapsed += delegate
            {
                User.OnChat(User.LastBubble, "* Se rince les mains *", true);
                timer1.Stop();
            };
            timer1.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer(4000);
            timer2.Interval = 2500;
            timer2.Elapsed += delegate
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                User.mainPropre = true;
                User.OnChat(User.LastBubble, "* Fini de se rincer les mains  *", true);
                User.Frozen = false;
                timer2.Stop();
            };
            timer2.Start();
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}