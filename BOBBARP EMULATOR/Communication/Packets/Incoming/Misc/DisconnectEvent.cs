using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Misc
{
    class DisconnectEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            RoomUser User = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            
            #region resetTased
            if (User.Tased == true)
            {
                Session.GetHabbo().Prison = 1;
                Session.GetHabbo().updatePrison();
                Session.GetHabbo().takeBien();
                Session.GetHabbo().PrisonCount += 1;
                Session.GetHabbo().updatePrisonCount();
                Session.GetHabbo().Timer = 20;
                Session.GetHabbo().updateTimer();
            }
            #endregion
            #region resetMenotted
            if (Session.GetHabbo().Menotted == true)
            {
                if (PlusEnvironment.GetGame().GetClientManager().getPoliceMenotte(Session.GetHabbo().Username) != null)
                {
                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(PlusEnvironment.GetGame().GetClientManager().getPoliceMenotte(Session.GetHabbo().Username));
                    if (TargetClient.GetHabbo() != null)
                    {
                        TargetClient.GetHabbo().MenottedUsername = null;
                    }
                }

                Session.GetHabbo().Prison = 1;
                Session.GetHabbo().updatePrison();
                Session.GetHabbo().takeBien();
                Session.GetHabbo().PrisonCount += 1;
                Session.GetHabbo().updatePrisonCount();
                Session.GetHabbo().Timer = 30;
                Session.GetHabbo().updateTimer();
            }

            if(Session.GetHabbo().MenottedUsername != null)
            {
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().MenottedUsername);
                if (TargetClient != null && TargetClient.GetHabbo() != null)
                {
                    TargetClient.GetHabbo().Menotted = false;
                    TargetClient.GetHabbo().resetEffectEvent();
                }
            }
            #endregion
            #region resetAppel
            if (Session.GetHabbo().inCallWithUsername != null)
            {
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().inCallWithUsername);
                if(TargetClient != null)
                {
                    TargetClient.GetHabbo().isCalling = false;
                    TargetClient.GetHabbo().inCallWithUsername = null;
                    TargetClient.SendWhisper(Session.GetHabbo().Username + " a raccroché.");
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                }
            }
            #endregion
            #region resetFootball
            if (Session.GetHabbo().footballTeam != null)
            {
                Session.GetHabbo().footballTeam = null;
            }
            #endregion
            #region checkIfWinSalade
            if (Session.GetHabbo().CurrentRoomId == PlusEnvironment.Salade)
            {
                PlusEnvironment.GetGame().GetClientManager().checkIfWinSalade();
            }
            #endregion

            // Session.Disconnect();
        }
    }
}
