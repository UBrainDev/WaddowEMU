using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorRobinet : IFurniInteractor
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
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            if (Session.GetHabbo().TravailId != 15 || Session.GetHabbo().Travaille == false)
                return;

            if (User.usernameCoiff == null)
            {
                Session.SendWhisper("Vous devez d'abord utilisé la commande :laver <pseudonyme>.");
                return;
            }

            if (User.makeAction == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(User.usernameCoiff);
            if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
            {
                User.usernameCoiff = null;
                return;
            }

            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Item.RequestUpdate(2, true);
            User.makeAction = true;
            User.OnChat(User.LastBubble, "* Mouille les cheveux de " + User.usernameCoiff + " *", true);

            System.Timers.Timer timer1 = new System.Timers.Timer(10000);
            timer1.Interval = 10000;
            timer1.Elapsed += delegate
            {
                User.OnChat(User.LastBubble, "* Met du shampoing sur les cheveux de " + User.usernameCoiff + " *", true);
                timer1.Stop();
            };
            timer1.Start();

            System.Timers.Timer timer2 = new System.Timers.Timer(15000);
            timer2.Interval = 15000;
            timer2.Elapsed += delegate
            {
                User.OnChat(User.LastBubble, "* Rince les cheveux de " + User.usernameCoiff + "  *", true);
                timer2.Stop();
            };
            timer2.Start();

            System.Timers.Timer timer3 = new System.Timers.Timer(20000);
            timer3.Interval = 20000;
            timer3.Elapsed += delegate
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                Item.RequestUpdate(2, true);
                Room Room = Session.GetHabbo().CurrentRoom;
                RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                if(TargetUser != null)
                {
                    TargetUser.usernameCoiff = null;
                    TargetUser.cheveuxPropre = true;
                }
                User.usernameCoiff = null;
                User.makeAction = false;
                User.OnChat(User.LastBubble, "* Fini de laver les cheveux " + User.usernameCoiff + "  *", true);
                timer3.Stop();
            };
            timer3.Start();
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}