using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using System.Data;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class BindowsWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;


            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                // ACTION
                #region open
                case "open":
                    {
                        Socket.Send("bindows;open");
                    }
                    break;
                #endregion
            }
        }
    }
}
