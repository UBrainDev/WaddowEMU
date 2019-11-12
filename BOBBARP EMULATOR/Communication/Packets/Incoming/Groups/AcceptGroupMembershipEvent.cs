using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;


using Plus.HabboHotel.Users;
using Plus.HabboHotel.Cache;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class AcceptGroupMembershipEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            if (Session == null || !Session.GetHabbo().InRoom || Session.GetHabbo().Hopital == 1 || Session.GetHabbo().Prison != 0)
                return;

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
                return;

            if (!Group.IsAdmin(Session.GetHabbo().Id))
                return;

            if (!Group.HasRequest(UserId))
                return;

            if (Session.GetHabbo().Travaille == false)
            {
                Session.SendWhisper("Veuillez travailler pour accepter la demande d'emploi de cet utilisateur.");
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo == null)
            {
                Session.SendWhisper("Impossible de valider la demande pour le moment.");
                return;
            }

            if(Habbo.TravailId != 1)
            {
                Session.SendWhisper(Habbo.Username + " a déjà un travail.");
                Group.HandleRequest(UserId, false);
                return;
            }

            Room Room = Session.GetHabbo().CurrentRoom;
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.OnChat(User.LastBubble, "* Accepte la demande d'emploi de " + Habbo.Username + " *", true);

            Group.HandleRequest(UserId, true);
            Session.SendMessage(new GroupMemberUpdatedComposer(GroupId, Habbo, 4));
            if (Habbo.InRoom)
            {
                Room TargetRoom = Habbo.CurrentRoom;
                RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Habbo.Id);
                Habbo.setFavoriteGroup(GroupId);
                TargetUser.OnChat(TargetUser.LastBubble, "* Commence à travailler pour " + Group.Name + " *", true);
            }
        }
    }
}