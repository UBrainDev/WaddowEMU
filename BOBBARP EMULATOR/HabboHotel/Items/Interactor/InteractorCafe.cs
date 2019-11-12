using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Pathfinding;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorCafe : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

            if (Session.GetHabbo().TravailId != 5 || Session.GetHabbo().RankId == 2 || Session.GetHabbo().Travaille == false)
                return;

            if (Session.GetHabbo().getCooldown("prepare_cafe"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            // CAFÉ
            if (Item.GetBaseItem().SpriteId == 7871)
            {
                if(User.boissonPrepared == "cafe")
                {
                    Session.SendWhisper("Vous avez déjà un café de prêt.");
                    return;
                }

                Session.GetHabbo().addCooldown("prepare_cafe", 7000);
                User.Frozen = true;

                Random TokenRand = new Random();
                int tokenNumber = TokenRand.Next(1600, 2894354);
                string Token = "CAFE-" + tokenNumber;
                User.boissonToken = Token;

                User.OnChat(User.LastBubble, "* Commence à préparer un café *", true);
                System.Timers.Timer timer1 = new System.Timers.Timer(6000);
                timer1.Interval = 6000;
                timer1.Elapsed += delegate
                {
                    User.CarryItem(41);
                    User.boissonPrepared = "cafe";
                    User.OnChat(User.LastBubble, "* Fini de prépare son café *", true);
                    User.Frozen = false;
                    timer1.Stop();
                };
                timer1.Start();

                System.Timers.Timer timer2 = new System.Timers.Timer(60000);
                timer2.Interval = 60000;
                timer2.Elapsed += delegate
                {
                    if(User.boissonPrepared == "cafe" && User.boissonToken == Token)
                    {
                        User.CarryItem(0);
                        User.boissonPrepared = null;
                        Session.SendWhisper("Votre café a été jeté car il est froid.");
                    }
                    timer2.Stop();
                };
                timer2.Start();

                return;
            }

            // CAPUCCINO
            if (Item.GetBaseItem().SpriteId == 3202)
            {
                if (User.boissonPrepared == "cappuccino")
                {
                    Session.SendWhisper("Vous avez déjà un cappuccino de prêt.");
                    return;
                }

                Session.GetHabbo().addCooldown("prepare_cafe", 7000);
                User.Frozen = true;

                Random TokenRand = new Random();
                int tokenNumber = TokenRand.Next(1600, 2894354);
                string Token = "CAPPUCCINO-" + tokenNumber;
                User.boissonToken = Token;

                User.OnChat(User.LastBubble, "* Commence à préparer un cappuccino *", true);
                System.Timers.Timer timer1 = new System.Timers.Timer(6000);
                timer1.Interval = 6000;
                timer1.Elapsed += delegate
                {
                    User.CarryItem(0);
                    User.CarryItem(41);
                    User.boissonPrepared = "cappuccino";
                    User.OnChat(User.LastBubble, "* Fini de prépare son cappuccino *", true);
                    User.Frozen = false;
                    timer1.Stop();
                };
                timer1.Start();

                System.Timers.Timer timer2 = new System.Timers.Timer(60000);
                timer2.Interval = 60000;
                timer2.Elapsed += delegate
                {
                    if (User.boissonPrepared == "cappuccino" && User.boissonToken == Token)
                    {
                        User.boissonPrepared = null;
                        Session.SendWhisper("Votre cappuccino a été jeté car il est froid.");
                    }
                    timer2.Stop();
                };
                timer2.Start();

                return;
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}