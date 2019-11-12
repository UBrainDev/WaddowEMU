using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class LoyerCommand : IChatCommand
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
            get { return "Mettre en location un appartement"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if(Room.Loyer == 1 && Room.OwnerId != 3)
            {
                Session.SendWhisper("Cet appartement est déjà en location.");
                return;
            }

            if (Room.getNameByModel() == "Appartement inconnu")
            {
                Session.SendWhisper("Impossible de mettre cet appartement en vente car le modèle n'est pas défini.");
                return;
            }

            if (Room.getPriceByModel() == 0)
            {
                Session.SendWhisper("Impossible de mettre cet appartement en vente car le prix n'est pas défini.");
                return;
            }

            Room.setLocationRoom(Room.getPriceByModel());
        }
    }
}