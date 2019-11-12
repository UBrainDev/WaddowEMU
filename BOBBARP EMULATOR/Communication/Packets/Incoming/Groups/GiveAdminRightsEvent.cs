using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.Cache;



namespace Plus.Communication.Packets.Incoming.Groups
{
    class GiveAdminRightsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
                return;

            if (!Group.IsMember(UserId) || !Group.IsAdmin(Session.GetHabbo().Id) || !Session.GetHabbo().InRoom)
                return;

            if (Session.GetHabbo().Travaille == false)
            {
                Session.SendWhisper("Veuillez travailler pour modifier le rang d'un utilisateur.");
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendWhisper("Vous ne pouvez pas modifier le rang de cet employé pour le moment.");
                return;
            }

            Room Room = Session.GetHabbo().CurrentRoom;
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.userChangeRank = Habbo.Id;

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "directeur;show;" + Habbo.Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + Habbo.Look + "&head_direction=2&gesture=sml&size=l;" + Habbo.RankInfo.Name);
        }
    }
}
