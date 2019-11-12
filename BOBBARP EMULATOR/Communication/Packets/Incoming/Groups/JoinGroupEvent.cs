using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class JoinGroupEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom || Session.GetHabbo().Hopital == 1 || Session.GetHabbo().Prison != 0)
                return;

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Packet.PopInt(), out Group))
                return;

            if (Group.IsMember(Session.GetHabbo().Id) || Group.IsAdmin(Session.GetHabbo().Id) || (Group.HasRequest(Session.GetHabbo().Id) && Group.GroupType == GroupType.PRIVATE))
                return;
            
            if (Session.GetHabbo().TravailId != 1)
            {
                Session.SendWhisper("Vous ne pouvez pas postuler car vous avez déjà un métier.");
                return;
            }

            if (Session.GetHabbo().Carte == 0)
            {
                Session.SendWhisper("Vous devez réaliser votre carte d'identité pour pouvoir postuler dans des entreprises.");
                return;
            }

            if (Session.GetHabbo().CurrentRoomId != Group.RoomId)
            {
                Room WorkRoom = PlusEnvironment.GetGame().GetRoomManager().LoadRoom(Group.RoomId);
                Session.SendWhisper("Vous devez vous rendre à [" + WorkRoom.Id + "] " + WorkRoom.Name + " pour pouvoir postuler dans cette entreprise.");
                return;
            }

            if (Group.GroupType == GroupType.PRIVATE)
            {
                Session.SendWhisper("Impossible de postuler dans ce travail.");
                return;
            }

            Group.AddMember(Session.GetHabbo().Id);

            Room Room = Session.GetHabbo().CurrentRoom;
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Group.GroupType == GroupType.LOCKED)
            {
                User.OnChat(User.LastBubble, "* Fait une demande d'emploi auprès de l'entreprise " + Group.Name + " *", true);
                List<GameClient> GroupAdmins = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                foreach (GameClient Client in GroupAdmins)
                {
                    Client.SendMessage(new GroupMembershipRequestedComposer(Group.Id, Session.GetHabbo(), 3));
                }

                Session.SendMessage(new GroupInfoComposer(Group, Session));
            }
            else
            {
                User.OnChat(User.LastBubble, "* Commence à travailler pour " + Group.Name + " *", true);
                Session.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id)));
                Session.SendMessage(new GroupInfoComposer(Group, Session));
                Session.GetHabbo().setFavoriteGroup(Group.Id);
            }
        }
    }
}
