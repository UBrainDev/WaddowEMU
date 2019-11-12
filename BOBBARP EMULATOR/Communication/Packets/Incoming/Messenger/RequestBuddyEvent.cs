using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Quests;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class RequestBuddyEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Packet.PopString());

            if (Session.GetHabbo().Telephone == 0)
            {
                Session.SendWhisper("Vous ne pouvez pas demander le numéro de " + TargetClient.GetHabbo().Username + " car vous n'avez pas de téléphone.");
                return;
            }

            if (TargetClient == null || TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("Une erreur est survenue.");
                return;
            }

            if (TargetClient.GetHabbo().Telephone == 0)
            {
                Session.SendWhisper("Vous ne pouvez pas demander le numéro de " + TargetClient.GetHabbo().Username + " car il n'a pas de téléphone.");
                return;
            }

            if (Session.GetHabbo().GetMessenger().RequestBuddy(TargetClient.GetHabbo().Username))
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_FRIEND);
        }
    }
}
