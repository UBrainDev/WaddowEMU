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
    public class InteractorTacos : IFurniInteractor
    {
        /*public void OnPlace(GameClient Session, Item Item)
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

                if (Session.GetHabbo().getCooldown("manger_distributor"))
                {
                    Session.SendWhisper("Veuillez patienter.");
                    return;
                }

                if (Session.GetHabbo().Energie > 99)
                {
                    Session.SendWhisper("Vous n'avez pas besoin de manger car votre énergie est au maximum.");
                    return;
                }

                if (Session.GetHabbo().Credits < 4)
                {
                    Session.SendWhisper("Il vous faut 6 crédits pour pouvoir acheter un tacos.");
                    return;
                }

                Session.GetHabbo().addCooldown("manger_distributor", 2000);
                Item.InteractingUser = Session.GetHabbo().Id;
                User.CanWalk = false;
                User.ClearMovement(true);
                User.SetRot(Rotation.Calculate(User.X, User.Y, Item.GetX, Item.GetY), false);

                Item.RequestUpdate(2, true);

                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                int Energie = 50;
                int NumberEnergie = 100 - Energie;
                Session.GetHabbo().Credits -= 4;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
                User.OnChat(User.LastBubble, "* Achète un tacos et le mange [+" + Energie + "% ÉNERGIE, -4 CRÉDITS] *", true);
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

                Session.SendMessage(new WhisperComposer(User.VirtualId, "ÉNERGIE : " + Session.GetHabbo().Energie + "/100", 0, 34));
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }*/
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

                if (myrandom >= 50)
                {
                    recompense = 3;
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