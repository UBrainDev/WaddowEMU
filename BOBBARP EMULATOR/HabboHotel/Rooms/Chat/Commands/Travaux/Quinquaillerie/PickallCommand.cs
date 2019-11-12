using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class PickallCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().CurrentRoom.OwnerId == Session.GetHabbo().Id || Session.GetHabbo().Rank == 8)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "appart"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Prendre tous les mobiliers de son appartement"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            foreach (Item Item in Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetFloor)
            {
                if(Item.GetBaseItem().SpriteId == 3289)
                {
                    Session.SendWhisper("Vous ne pouvez pas utiliser la commande :pickall car il y a de la weed chez vous.");
                    return;
                }
            }

            Room.GetRoomItemHandler().RemoveItems(Session);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = '0', `user_id` = @UserId WHERE `base_item` != '1100' AND `room_id` = @RoomId");
                dbClient.AddParameter("RoomId", Room.Id);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.RunQuery();
            }

            List<Item> Items = Room.GetRoomItemHandler().GetWallAndFloor.ToList();
            if (Items.Count > 0)
                Session.SendWhisper("Il semblerait qu'il reste des mobiliers !");

            Session.SendMessage(new FurniListUpdateComposer());
        }
    }
}