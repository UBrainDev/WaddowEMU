using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Pathfinding;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorGoulag : IFurniInteractor
    {
        
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Item.ExtraData != "1" && Item.GetBaseItem().VendingIds.Count >= 1 && Item.InteractingUser == 0 && Session != null)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User.IsTrading)
                    return;

                if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
                {
                    User.MoveToIfCanWalk(Item.SquareInFront);
                    return;
                }

                User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                if (Session.GetHabbo().getCooldown("mine_pierre"))
                {
                    Session.SendWhisper("Veuillez patienter avant de retourner au goulag.");
                    return;
                }

                Session.GetHabbo().addCooldown("mine_pierre", 50000);
                Item.InteractingUser = Session.GetHabbo().Id;
                User.CanWalk = false;
                User.ClearMovement(true);
                User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                Item.RequestUpdate(2, true);

                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                int Energie = 50;
                int NumberEnergie = 100 - Energie;

                User.OnChat(User.LastBubble, "* Se rend au goulag *", true);


                System.Timers.Timer timer2 = new System.Timers.Timer(3500);
                timer2.Interval = 3500;
                timer2.Elapsed += delegate
                {
                    User.OnChat(User.LastBubble, "* Travaille bien pour satisfaire Lénine *", true);

                    System.Timers.Timer timer33 = new System.Timers.Timer(3500);
                    timer33.Interval = 3500;
                    timer33.Elapsed += delegate
                    {
                        User.OnChat(User.LastBubble, "* Sors du goulag et gagne 50c *", true);
                    };
                };

            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}