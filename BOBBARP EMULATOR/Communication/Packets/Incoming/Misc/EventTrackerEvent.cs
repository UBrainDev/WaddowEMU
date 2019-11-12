using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Incoming.Misc
{
    class EventTrackerEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            // ABOUT 
            if(Convert.ToString(Packet).Contains("client.toolbar.clicked"))
            {
                if (Convert.ToString(Packet).Contains("NAVIGATOR"))
                {
                    if (Session.GetHabbo().Rank == 8)
                        return;

                    TimeSpan Uptime = DateTime.Now - PlusEnvironment.ServerStarted;
                    int OnlineUsers = PlusEnvironment.GetGame().GetClientManager().Count;
                    int RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "about;BETA;" + Uptime.Days + " jour(s), " + Uptime.Hours + " heure(s) et " + Uptime.Minutes + " minute(s);" + OnlineUsers + ";" + RoomCount);
                }
            }
        }
    }
}
