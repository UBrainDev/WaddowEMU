using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Bobba.Web
{
    class WebSockets
    {
        public WebEventManager()
        {
            string IP = PlusEnvironment.GetConfig().data["ws.tcp.bindip"];
            int Port = int.Parse(PlusEnvironment.GetConfig().data["ws.tcp.port"]);

            this._webSocketServer = new WebSocketServer("ws://" + IP + ":" + Port);
            this._webSockets = new ConcurrentDictionary<IWebSocketConnection, WebSocketUser>();
            this._webEvents = new ConcurrentDictionary<string, IWebEvent>();
            this.RegisterIncoming();
            this.RegisterOutgoing();
        }
    }
}
