using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ReseauCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Définir le réseau d'un appartement"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :reseau <chiffre>");
                return;
            }
            
            int Amount;
            if (!int.TryParse(Params[1], out Amount) || Convert.ToInt32(Params[1]) < 0 || Params[1].StartsWith("0") || Convert.ToInt32(Params[1]) > 5)
            {
                Session.SendWhisper("Le chiffre doit être compris entre 0 et 5.");
                return;
            }

            if(Room.Reseau == Convert.ToInt32(Params[1]))
            {
                Session.SendWhisper("Le chiffre est déjà le même que l'actuel.");
                return;
            }

            Session.GetHabbo().CurrentRoom.Reseau = Convert.ToInt32(Params[1]);
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE rooms SET reseau = @reseau WHERE id = @roomId");
                dbClient.AddParameter("reseau", Convert.ToInt32(Params[1]));
                dbClient.AddParameter("roomId", Session.GetHabbo().CurrentRoomId);
                dbClient.RunQuery();
            }
            Session.SendWhisper("Réseau fixé sur " + Convert.ToInt32(Params[1]) + "/5.");

        }
    }
}