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
    class DeclineGroupMembershipEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            if (Session == null || !Session.GetHabbo().InRoom)
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
                Session.SendWhisper("Veuillez travailler pour refuser la demande d'emploi de cet utilisateur.");
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
            if (Habbo != null && Habbo.InRoom)
            {
                Habbo.GetClient().SendWhisper(Session.GetHabbo().Username + " a refusé votre demande d'emploi auprès de l'entreprise " + Group.Name + ".");
            }

            Group.HandleRequest(UserId, false);
            Session.SendMessage(new UnknownGroupComposer(Group.Id, UserId));
        }
    }
}