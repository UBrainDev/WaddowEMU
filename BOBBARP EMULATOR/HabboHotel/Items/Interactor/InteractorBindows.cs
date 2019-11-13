using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorBindows : IFurniInteractor
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
                return;

            if (User.makeAction == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }
            
                User.makeAction = true;
                if (User.ConnectedMetier == false)
                {
                    User.Frozen = true;
                    User.OnChat(User.LastBubble, "* Demarre Bindows *", true);
                
                System.Timers.Timer timer1 = new System.Timers.Timer(2000);
                timer1.Interval = 2000;
                timer1.Elapsed += delegate
                {
                        User.OnChat(User.LastBubble, "* Se connecte à sa session *", true);
                        timer1.Stop();
                    };
                    timer1.Start();

                    System.Timers.Timer timer2 = new System.Timers.Timer(3500);
                    timer2.Interval = 3500;
                    timer2.Elapsed += delegate
                    {
                        Session.SendWhisper("Ouverture de votre session en cours...");
                        timer2.Stop();
                    };
                    timer2.Start();
                }
                else
                {
                    User.OnChat(User.LastBubble, "* Se déconnecte de sa session Bindows *", true);
                    
                    System.Timers.Timer timer1 = new System.Timers.Timer(3000);
                    timer1.Interval = 3000;
                    timer1.Elapsed += delegate
                    {
                        User.OnChat(User.LastBubble, "* Éteint Bindows *", true);
                        timer1.Stop();
                    };
                    timer1.Start();

                    System.Timers.Timer timer2 = new System.Timers.Timer(4000);
                    timer2.Interval = 4000;
                    timer2.Elapsed += delegate
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(2, true);

                        Session.SendWhisper("Votre session Bindows est à présent fermée.");
                        
                        User.ConnectedMetier = false;
                        User.makeAction = false;
                        User.Frozen = false;
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