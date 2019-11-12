using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class MusiqueCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 2)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return "<ID Musique Youtube>"; }
        }

        public string Description
        {
            get { return "Mettre de la musique dans un appartement."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :musique <ID Musique Youtube>");
                return;
            }

            if(Params[1] == "stop")
            {
                foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "musique;stop");
                }
                
                Session.SendWhisper("Musique arrêtée.");
                return;
            }

            if (Params[1].Contains(";"))
            {
                Session.SendWhisper("L'ID de la musique est invalide.");
                return;
            }

            System.Net.WebClient wc = new System.Net.WebClient();
            try
            {
                byte[] raw = wc.DownloadData("https://www.youtube.com/oembed?format=json&url=https://www.youtube.com/watch?v=" + Params[1]);
                foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                {
                    if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null)
                        continue;

                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "musique;start;" + Params[1]);
                }

                Session.SendWhisper("Musique démarrée.");
                return;
            }
            catch
            {
                Session.SendWhisper("L'ID de la musique est invalide.");
                return;
            }
        }
    }
}