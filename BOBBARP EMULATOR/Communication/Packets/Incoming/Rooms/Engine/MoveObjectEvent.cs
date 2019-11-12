using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Quests;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;



namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class MoveObjectEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int ItemId = Packet.PopInt();
            if (ItemId == 0)
                return;

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            Item Item;
            if (Room.Group != null)
            {
                if (!Room.CheckRights(Session, false, true))
                {
                    Item = Room.GetRoomItemHandler().GetItem(ItemId);
                    if (Item == null)
                        return;

                    Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                    return;
                }
            }
            else
            {
                if (!Room.CheckRights(Session))
                {
                    return;
                }
            }

            Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item == null)
                return;

            if (Item.GetBaseItem().SpriteId == 2693 && Session.GetHabbo().Rank != 8)
            {
                Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                Session.SendWhisper("Vous ne pouvez pas déplacer la porte de votre appartement.");
                return;
            }

            if (Item.GetBaseItem().SpriteId == 5273 && Room.GetRoomItemHandler().CheckIfItemOnCase(3289, Item.GetX, Item.GetY))
            {
                Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                Session.SendWhisper("Vous ne pouvez pas déplacer cet ampoule car elle permet le développement d'une plante.");
                return;
            }


            int lastX = Item.GetX;
            int lastY = Item.GetY;
            int x = Packet.PopInt();
            int y = Packet.PopInt();
            int Rotation = Packet.PopInt();

            if (Room.GetRoomItemHandler().CheckIfItemOnCase(3289, x, y) && Item.GetBaseItem().SpriteId != 5273 || Room.GetRoomItemHandler().CheckIfItemOnCase(7858, x, y) && Item.GetBaseItem().SpriteId != 5273)
            {
                Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                return;
            }

            if (Item.GetBaseItem().SpriteId == 3289 || Item.GetBaseItem().SpriteId == 5273 && Room.GetRoomItemHandler().CheckIfItemOnCase(5273, x, y) || Item.GetBaseItem().SpriteId == 7858 && Room.GetRoomItemHandler().CheckIfItemOnCase(7858, x, y))
            {
                Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                return;
            }

            if (Room.GetRoomItemHandler().SetFloorItem(Session, Item, x, y, Rotation, false, false, true))
            {
                Item WeedPlant = Room.GetRoomItemHandler().getItemOnCase(3289, lastX, lastY);
                Item Lumiere = Room.GetRoomItemHandler().getItemOnCase(5273, lastX, lastY);

                if (Item.GetBaseItem().SpriteId == 7858 && WeedPlant != null)
                {
                    Room.GetRoomItemHandler().SetFloorItem(Session, WeedPlant, x, y, 0, false, false, true);
                }

                if (Item.GetBaseItem().SpriteId == 7858 && Lumiere != null)
                {
                    Room.GetRoomItemHandler().SetFloorItem(Session, Lumiere, x, y, 0, false, false, true);
                }

                Room.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
            }
        }
    }
}