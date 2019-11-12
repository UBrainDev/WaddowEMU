using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class EmptyItemsCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "divers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Vider son inventaire"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendNotification("Êtes-vous sûr de vouloir vider votre inventaire ? Tapez \":emptyitems oui\" pour valider votre choix.");
                return;
            }
            else
            {
                if (Params.Length == 2 && Params[1].ToString() == "oui")
                {
                    Session.GetHabbo().GetInventoryComponent().ClearItems();
                    Session.SendWhisper("Votre inventaire a bien été vidé.", 1);
                    return;
                }
                else if (Params.Length == 2 && Params[1].ToString() != "oui")
                {
                    Session.SendWhisper("Pour confirmer, vous devez taper :emptyitems oui", 1);
                    return;
                }
            }
        }
    }
}