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
    class IndiceCommande : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().EventCount == 7 && Session.GetHabbo().Rank == 8)
                return true;

            return false;
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
            get { return "Revoir l'indice"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "eventMission;alert;Indice;https://i.imgur.com/zbUNVD7.gif;Document texte;Voici le plan :<br /><i>Quand le Père Noël sera de tourné au centre ville de Waddow nous l'intercepterons.<br />Il faudra être discret et rapide, personne ne doit nous voir.<br /><br />Il faut que tu saches que le QG est le grenier des habitants, la plante quant à elle, ça sera l'entrée.<br/><br/><b>Malutin</b></i>");
        }
    }
}