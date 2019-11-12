using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Incoming;
using System;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    public class ActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom || Session.GetHabbo().Prison != 0)
                return;

            if(Session.GetHabbo().getCooldown("action_event"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            int Action = Packet.PopInt();

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (Room.RoomMuted == true || Session.GetHabbo().Hopital == 1)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Session.GetHabbo().CurrentRoomId == 55 && User.HaveTicket == false && Session.GetHabbo().TravailId != 18 && Session.GetHabbo().TravailId != 4)
                return;

            if (User.DanceId > 0)
                User.DanceId = 0;

            if (Session.GetHabbo().Effects().CurrentEffect > 0)
                Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

            User.UnIdle();
            Room.SendMessage(new ActionComposer(User.VirtualId, Action));

            Session.GetHabbo().addCooldown("action_event", 2000);
            if(Action == 1)
            {
                User.OnChat(User.LastBubble, "* Salut *", true);
            }
            else if (Action == 2)
            {
                User.OnChat(User.LastBubble, "* Fait un bisous *", true);
            }
            else if (Action == 3)
            {
                User.OnChat(User.LastBubble, "* Rigole *", true);
            }
            else if (Action == 5) // idle
            {
                if (User.GetClient().GetHabbo().Travaille == true)
                {
                    User.GetClient().GetHabbo().stopWork();
                }

                if (User.isFarmingRock > 0)
                {
                    User.isFarmingRock = 0;
                }
                User.OnChat(User.LastBubble, "* S'absente *", true);
                User.IsAsleep = true;
                Room.SendMessage(new SleepComposer(User, true));
            }

            System.Timers.Timer timer2 = new System.Timers.Timer(1900);
            timer2.Interval = 1900;
            timer2.Elapsed += delegate
            {
                Session.GetHabbo().resetEffectEvent();
                timer2.Stop();
            };
            timer2.Start();
        }
    }
}