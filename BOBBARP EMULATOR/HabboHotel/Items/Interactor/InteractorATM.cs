using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorATM : IFurniInteractor
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

            if (Session.GetHabbo().Cb == "null")
            {
                Session.SendWhisper("Vous ne pouvez pas utiliser le distributeur car vous n'avez pas de carte bancaire.");
                return;
            }

            if (User.usingATM == true)
            {
                User.canUseCB = false;
                User.usingATM = false;
                User.Frozen = false;
                User.OnChat(User.LastBubble, "* Enlève sa carte bancaire du distributeur *", true);
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "distributeur", "disconnect");
                return;
            }
            else
            {
                User.usingATM = true;
                User.Frozen = true;
                User.OnChat(User.LastBubble, "* Met sa carte bancaire dans le distributeur *", true);
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "distributeur", "connect");
                return;
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}