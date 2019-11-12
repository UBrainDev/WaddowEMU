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
    public class InteractorCreation : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session.GetHabbo() == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

            bool CanMakeCreation = false;
            if (Session.GetHabbo().TravailInfo.Usine == 1 && Session.GetHabbo().Travaille == true)
                CanMakeCreation = true;

            if (!CanMakeCreation)
                return;

            if (Session.GetHabbo().getCooldown("make_creation"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if (User.makeCreation == true)
            {
                Session.SendWhisper("Vous réalisez déjà " + User.CreationName + ".");
                return;
            }

            if (Session.GetHabbo().TravailId == 19)
            {
                Session.GetHabbo().addCooldown("make_creation", 2000);
                User.makeCreation = true;
                Random Creation = new Random();
                int CreationItem = Creation.Next(1, 3);
                string Name;
                if(CreationItem == 2)
                {
                    Name = "un savon";
                }
                else
                {
                    Name = "un doliprane";
                }
                User.creationTimer = 15;
                User.CreationName = Name;
                User.OnChat(User.LastBubble, "* Commence à réaliser " + Name+ " *", true);
                return;
            }
            else if (Session.GetHabbo().TravailId == 20)
            {
                Session.GetHabbo().addCooldown("make_creation", 2000);
                User.makeCreation = true;
                Random Creation = new Random();
                int CreationItem = Creation.Next(1, 7);
                string Name;
                if (CreationItem == 2)
                {
                    Name = "une Ak47";
                }
                else if (CreationItem == 3)
                {
                    Name = "un Uzi";
                }
                else if (CreationItem == 4)
                {
                    Name = "un sabre";
                }
                else if (CreationItem == 5)
                {
                    Name = "une batte";
                }
                else if (CreationItem == 6)
                {
                    Name = "des munitions pour Uzi";
                }
                else
                {
                    Name = "des munitions pour Ak47";
                }
                User.creationTimer = 15;
                User.CreationName = Name;
                User.OnChat(User.LastBubble, "* Commence à réaliser " + Name + " *", true);
                return;
            }
            else
                return;
            
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}