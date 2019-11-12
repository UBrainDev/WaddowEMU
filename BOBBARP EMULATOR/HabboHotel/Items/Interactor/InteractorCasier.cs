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
    public class InteractorCasier : IFurniInteractor
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
            if (User.usingCasier)
                return;

            if(User.Tased || Session.GetHabbo().Menotted == true)
            {
                Session.SendWhisper("Vous ne pouvez pas ouvrir votre casier pendant que vous immobilisé.");
                return;
            }

            if(Session.GetHabbo().getCooldown("open_casier"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if(User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas ouvrir votre casier pendant que vous faites un échange.");
                return;
            }

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y) && Item.GetBaseItem().SpriteId != 4614)
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            Session.GetHabbo().addCooldown("open_casier", 5000);
            User.usingCasier = true;
            User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);
            User.OnChat(User.LastBubble, "* Entre le code de son cadenas et ouvre son casier *", true);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "casier;open;" + Session.GetHabbo().CasierWeed + ";" + Session.GetHabbo().CasierCocktails);
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}