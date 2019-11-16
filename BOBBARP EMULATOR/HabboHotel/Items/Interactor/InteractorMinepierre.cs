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
    public class InteractorMinepierre : IFurniInteractor
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
                    Session.SendWhisper("Veuillez patienter avant de miner à nouveau.");
                    return;
                }

                Session.GetHabbo().addCooldown("mine_pierre", 3000);
                Item.InteractingUser = Session.GetHabbo().Id;
                User.CanWalk = false;
                User.ClearMovement(true);
                User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                Item.RequestUpdate(2, true);

                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                int Energie = 50;
                int NumberEnergie = 100 - Energie;

                Random rand = new Random();
                int myrandom = rand.Next(100); 
                System.Console.WriteLine(myrandom);

                int recompense = 0;

                if (myrandom < 75)
                {
                    recompense = 0;
                }else if(myrandom <= 82.5){
                    recompense = 1;
                }else if(myrandom <= 95){
                    recompense = 2;
                }else if(myrandom <= 97.5){
                    recompense = 3;
                }else if(myrandom <= 98.75){
                    recompense = 5;
                }else if(myrandom <= 99.5){
                    recompense = 15;
                }else if(myrandom <= 100){
                    recompense = 30;
                }

                Session.GetHabbo().Credits += recompense;

                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);

                if (recompense >= 1)
                {
                    User.OnChat(User.LastBubble, "* Mine une pierre et y trouve " + recompense + " credits *", true);
                }
                else
                {
                    User.OnChat(User.LastBubble, "* Mine une pierre mais n'y trouve aucun crédit... *", true);
                }
                
                Session.GetHabbo().Energie -= 1;
                
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}