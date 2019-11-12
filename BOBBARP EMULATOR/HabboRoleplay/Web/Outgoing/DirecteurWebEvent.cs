using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus;
using Fleck;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GroupsRank;

namespace Bobba.HabboRoleplay.Web.Outgoing
{
    class DirecteurWebEvent : IWebEvent
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
                #region promouvoir
                case "promouvoir":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if (Client.GetHabbo().Travaille == false)
                        {
                            Client.SendWhisper("Veuillez travailler pour modifier le rang d'un employé.");
                            return;
                        }

                        if (Client.GetHabbo().getCooldown("directeur") == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().RankInfo.Rank != 2)
                            return;

                        Habbo Habbo = PlusEnvironment.GetHabboById(User.userChangeRank);
                        if (Habbo == null)
                        {
                            Client.SendWhisper("Vous ne pouvez pas modifier le rang de cet employé pour le moment.");
                            return;
                        }

                        Group Group = null;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Client.GetHabbo().TravailId, out Group))
                            return;

                        if (!Group.IsMember(Habbo.Id) || !Group.IsAdmin(Client.GetHabbo().Id) || Group.IsAdmin(Habbo.Id))
                            return;

                        GroupRank NewRank = null;
                        PlusEnvironment.GetGame().getGroupRankManager().TryGetRank(Habbo.TravailId, Habbo.RankId + 1, out NewRank);
                        if (NewRank == null)
                            return;

                        Client.GetHabbo().addCooldown("directeur", 2000);
                        Group.updateRank(Habbo.Id);
                        User.OnChat(User.LastBubble, "* Promouvoit " + Habbo.Username + " en tant que " + NewRank.Name + " *", true);
                        if(Habbo.InRoom)
                        {
                            Habbo.GetClient().SendWhisper(Client.GetHabbo().Username + " a promu votre rang de travail en tant que " + NewRank.Name + ".");
                        }
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "directeur;show;" + Habbo.Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + Habbo.Look + "&head_direction=2&gesture=sml&size=l;" + Habbo.RankInfo.Name);

                        if (NewRank.Rank == 2)
                        {
                            Group.MakeAdmin(Habbo.Id);
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "directeur;hide");
                        }
                        break;
                    }
                #endregion
                #region retrograder
                case "retrograder":
                    {
                        Room Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null)
                            return;

                        if(Client.GetHabbo().Travaille == false)
                        {
                            Client.SendWhisper("Veuillez travailler pour modifier le rang d'un employé.");
                            return;
                        }

                        if (Client.GetHabbo().getCooldown("directeur") == true)
                        {
                            Client.SendWhisper("Veuillez patienter.");
                            return;
                        }

                        if (Client.GetHabbo().RankInfo.Rank != 2)
                            return;

                        Habbo Habbo = PlusEnvironment.GetHabboById(User.userChangeRank);
                        if (Habbo == null)
                        {
                            Client.SendWhisper("Vous ne pouvez pas modifier le rang de cet employé pour le moment.");
                            return;
                        }

                        Group Group = null;
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Client.GetHabbo().TravailId, out Group))
                            return;

                        if (!Group.IsMember(Habbo.Id) || !Group.IsAdmin(Client.GetHabbo().Id) || Group.IsAdmin(Habbo.Id))
                            return;

                        GroupRank NewRank = null;
                        PlusEnvironment.GetGame().getGroupRankManager().TryGetRank(Habbo.TravailId, Habbo.RankId - 1, out NewRank);
                        if (NewRank == null)
                        {
                            Client.SendWhisper("Il n'existe pas de rang plus bas que celui que possède " + Habbo.Username + ".");
                            return;
                        }

                        Client.GetHabbo().addCooldown("directeur", 2000);
                        Group.retrograderRank(Habbo.Id);
                        User.OnChat(User.LastBubble, "* Rétrograde " + Habbo.Username + " en tant que " + NewRank.Name + " *", true);

                        if (Habbo.InRoom)
                        {
                            Habbo.GetClient().SendWhisper(Client.GetHabbo().Username + " a rétrogradé votre rang de travail en tant que " + NewRank.Name + ".");
                        }
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "directeur;show;" + Habbo.Username + ";//habbo.fr/habbo-imaging/avatarimage?figure=" + Habbo.Look + "&head_direction=2&gesture=sml&size=l;" + Habbo.RankInfo.Name);
                        break;
                    }
                    #endregion
            }
        }
    }
}
