using System;
using System.Linq;
using System.Text;
using System.Data;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ChiffreCommand : IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            if (Session.GetHabbo().Rank == 8 || Session.GetHabbo().TravailId == 18 && Session.GetHabbo().RankId == 3 && Session.GetHabbo().Travaille == true)
                return true;

            return false;
        }

        public string TypeCommand
        {
            get { return "staff"; }
        }

        public string Parameters
        {
            get { return "<montant>"; }
        }

        public string Description
        {
            get { return "Ajouter un montant à un chiffre d'affaires d'une entreprise"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 2)
            {
                Session.SendWhisper("Syntaxe invalide, tapez :chiffre <montant>");
                return;
            }

            if (Session.GetHabbo().CurrentRoom.Group == null)
                return;

            int num;
            if (!Int32.TryParse(Params[1], out num) || Params[1].StartsWith("0") || 0 > Convert.ToInt32(Params[1]))
            {
                Session.SendWhisper("Le montant n'est pas valide.");
                return;
            }

            if(Session.GetHabbo().CurrentRoom.Group.Id == 1 || Session.GetHabbo().CurrentRoom.Group.Usine == 1 || Session.GetHabbo().CurrentRoom.Group.PayedByEtat == 1 || Session.GetHabbo().CurrentRoom.Group.Id == 18)
            {
                Session.SendWhisper("Cette entreprise n'a pas besoin d'être augmenté ou ne peut pas être augmenté.");
                return;
            }

            Session.GetHabbo().CurrentRoom.Group.ChiffreAffaire += Convert.ToInt32(Params[1]);
            Session.GetHabbo().CurrentRoom.Group.updateChiffre();

            if (Session.GetHabbo().Username != "ADMIN-UBrain" || Session.GetHabbo().Travaille == false)
            {
                Group Gouvernement = null;
                if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                {
                    Gouvernement.ChiffreAffaire -= Convert.ToInt32(Params[1]);
                    Gouvernement.updateChiffre();
                }
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            User.OnChat(User.LastBubble, "* Relance l'économie de cette entreprise de " + Params[1] + " crédits * ", true);
        }
    }
}