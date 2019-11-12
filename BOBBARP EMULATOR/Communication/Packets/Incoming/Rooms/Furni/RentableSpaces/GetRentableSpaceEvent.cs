using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using System.Data;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni.RentableSpaces
{
    class GetRentableSpaceEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            RoomUser User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            // APPART
            int itemId = Packet.PopInt();

            DataRow Row = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `orpi_items_apparts` WHERE `id_item` = @idItem LIMIT 1");
                dbClient.AddParameter("idItem", itemId);
                Row = dbClient.getRow();
            }

            if (Row == null && Session.GetHabbo().Rank != 8)
                return;

            if (Row == null && Session.GetHabbo().Rank == 8)
            {
                User.AppartItem = itemId;
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "appart;setId");
                return;
            }

            if (Row != null)
            {
                Room TargetRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Convert.ToInt32(Row["id_appart"]));
                if (TargetRoom == null || TargetRoom.Loyer == 0)
                {
                    Session.SendWhisper("Une erreur est survenue.");
                    return;
                }

                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "appart;setAppart;" + TargetRoom.Id + ";" + TargetRoom.OwnerName + ";" + TargetRoom.Loyer_Date);
                return;
            }
        }
    }
}
