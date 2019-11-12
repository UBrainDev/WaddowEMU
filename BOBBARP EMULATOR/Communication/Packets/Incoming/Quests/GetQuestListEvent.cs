using System;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.Communication.Packets.Incoming;

namespace Plus.Communication.Packets.Incoming.Quests
{
    public class GetGangInfoEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            // GANG
            // PlusEnvironment.GetGame().GetQuestManager().GetList(Session, null);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "gang", "toggle");
        }
    }
}