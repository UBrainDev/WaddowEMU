using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Users;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class RemoveGroupMemberEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int GroupId = Packet.PopInt();
            int UserId = Packet.PopInt();

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out Group))
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            if (!Group.IsMember(Session.GetHabbo().Id))
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (UserId != Session.GetHabbo().Id)
            {
                if (!Group.IsAdmin(Session.GetHabbo().Id))
                    return;

                if(Session.GetHabbo().Travaille == false)
                {
                    Session.SendWhisper("Veuillez travailler pour virer un employé.");
                    return;
                }

                Habbo Habbo = PlusEnvironment.GetHabboById(UserId);
                if (Habbo == null)
                {
                    Session.SendWhisper("Impossible de virer cet utilisateur pour le moment.");
                    return;
                }

                if (!Group.IsMember(Habbo.Id) || Habbo.TravailId == 1)
                    return;

                if(Group.IsAdmin(Habbo.Id))
                {
                    Session.SendWhisper("Vous ne pouvez pas virer un directeur.");
                    return;
                }

                User.OnChat(User.LastBubble, "* Vire " + Habbo.Username + " de son travail *", true);
                if (Habbo.InRoom)
                {
                    Room TargetRoom = Habbo.CurrentRoom;
                    RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(Habbo.Id);
                    if (Habbo.Travaille == true)
                    {
                        TargetUser.GetClient().GetHabbo().stopWork();
                    }
                    TargetUser.GetClient().SendWhisper(Session.GetHabbo().Username + " vous a viré de votre travail.");
                    Habbo.GetClient().SendMessage(new GroupInfoComposer(Group, Session));

                    Habbo.setFavoriteGroup(1);
                    Group GroupBase2 = null;
                    PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(1, out GroupBase2);
                    GroupBase2.AddMemberList(Habbo.Id);
                }

                Group.sendToChomage(Habbo.Id);
                return;
            }

            if (Session.GetHabbo().Travaille == true)
            {
                Session.SendWhisper("Vous devez arrêter de travailler pour pouvoir démissionner.");
                return;
            }

            if (Session.GetHabbo().TravailId == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas démissionner car vous n'avez pas de travail.");
                return;
            }

            User.OnChat(User.LastBubble, "* Démissionne de son travail actuel *", true);
            Group.sendToChomage(Session.GetHabbo().Id);
            Session.SendMessage(new GroupInfoComposer(Group, Session));
            Session.GetHabbo().setFavoriteGroup(1);
            Group GroupBase = null;
            PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(1, out GroupBase);
            GroupBase.AddMemberList(Session.GetHabbo().Id);
        }
    }
}