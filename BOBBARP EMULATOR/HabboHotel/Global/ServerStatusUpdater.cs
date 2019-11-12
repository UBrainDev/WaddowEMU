using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using log4net;
using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Global
{
    public class ServerStatusUpdater : IDisposable
    {
        private static ILog log = LogManager.GetLogger("Mango.Global.ServerUpdater");

        private const int UPDATE_IN_SECS = 30;

        private Timer _timer;
        
        public ServerStatusUpdater()
        {
        }

        public void Init()
        {
            this._timer = new Timer(new TimerCallback(this.OnTick), null, TimeSpan.FromSeconds(UPDATE_IN_SECS), TimeSpan.FromSeconds(UPDATE_IN_SECS));

            Console.Title = "Waddow Emulator - 0 civils en ligne - 0 appartements actifs - 0 jour(s) 0 heure(s) . Nous sommes basés sur BOBBARP Emulateur V2.";

            log.Info("Server Status Updater has been started.");
        }

        public void OnTick(object Obj)
        {
            this.UpdateOnlineUsers();
        }

        private void UpdateOnlineUsers()
        {
            TimeSpan Uptime = DateTime.Now - PlusEnvironment.ServerStarted;

            int UsersOnline = Convert.ToInt32(PlusEnvironment.GetGame().GetClientManager().Count);
            int RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;

            Console.Title = "Waddow Emulator - " + UsersOnline + " civils en ligne - " + RoomCount + " appartements actifs - " + Uptime.Days + " jour(s), " + Uptime.Hours + " heure(s), " + Uptime.Minutes + " minute(s)\n Nous sommes basés sur BOBBARP Emulateur V2.";

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @usersOnline, `loaded_rooms` = @loadedRoomsInt LIMIT 1;");
                dbClient.AddParameter("usersOnline", UsersOnline);
                dbClient.AddParameter("loadedRoomsInt", RoomCount);
                dbClient.RunQuery();
            }
        }


        public void Dispose()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
            }

            this._timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
