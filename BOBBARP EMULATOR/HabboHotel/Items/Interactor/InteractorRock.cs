using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorRock : IFurniInteractor
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

            if (Session.GetHabbo().Prison == 0)
            {
                Session.SendWhisper("Il faut être emprisonné pour pouvoir casser des pierres.");
                return;
            }

            if(Session.GetHabbo().getCooldown("casser_pierre"))
            {
                Session.SendWhisper("Veuillez patienter");
                return;
            }

            if (User.isFarmingRock > 0)
            {
                User.isFarmingRock = 0;
                User.OnChat(User.LastBubble, "* Arrête de casser la pierre *", true);
                Session.GetHabbo().resetEffectEvent();
                return;
            }
            else
            {
                Session.GetHabbo().addCooldown("casser_pierre", 5000);
                User.isFarmingRock = 5;
                User.OnChat(User.LastBubble, "* Commence à casser la pierre *", true);
                Session.GetHabbo().resetEffectEvent();
                return;
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}