using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Global;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Quests;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Users
{
    class UpdateFigureDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

           /* if (Session.GetHabbo().Rank != 8)
            {
                Session.SendWhisper("Utilisez la touche F1 de votre clavier pour vous changer.");
                return;
            }*/

            if (Session.GetHabbo().CurrentRoomId != 6)
            {
                Session.SendWhisper("Vous devez être à H&M pour pouvoir vous changer.");
                return;
            }

            if (Session.GetHabbo().Credits < 10)
            {
                Session.SendWhisper("Il vous faut 10 crédits pour pouvoir vous changer.");
                return;
            }

            string Gender = Packet.PopString().ToUpper();
            string Look = PlusEnvironment.GetGame().GetAntiMutant().RunLook(Packet.PopString());
            
            if (Session.GetHabbo().Gender == "m" && !Gender.Contains("M") && Session.GetHabbo().Rank == 1 || Session.GetHabbo().Gender == "f" && !Gender.Contains("F") && Session.GetHabbo().Rank == 1)
            {
                Session.SendWhisper("Impossible de changer de sexe.");
                return;
            }

            if (Look == Session.GetHabbo().Look)
                return;

            if ((DateTime.Now - Session.GetHabbo().LastClothingUpdateTime).TotalSeconds <= 2.0)
            {
                Session.GetHabbo().ClothingUpdateWarnings += 1;
                if (Session.GetHabbo().ClothingUpdateWarnings >= 25)
                    Session.GetHabbo().SessionClothingBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionClothingBlocked)
                return;

            Session.GetHabbo().LastClothingUpdateTime = DateTime.Now;

            string[] AllowedGendersM = { "M" };
            string[] AllowedGendersF = { "F" };
            string[] AllowedGenders = { "M", "F" };
            if (!AllowedGenders.Contains(Gender))
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Impossible de changer de sexe."));
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.PROFILE_CHANGE_LOOK);

            Session.GetHabbo().Credits -= 10;
            Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
            string NewLookWithoutCoiff;
            if (Session.GetHabbo().Rank != 8)
            {
                 NewLookWithoutCoiff = Session.GetHabbo().changeLookWithoutCoiff(Session.GetHabbo().Look, PlusEnvironment.FilterFigure(Look));
            }
            else
            {
                NewLookWithoutCoiff = PlusEnvironment.FilterFigure(Look);
            }
            Session.GetHabbo().Look = NewLookWithoutCoiff;
            Session.GetHabbo().Gender = Gender.ToLower();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE users SET look = @look, gender = @gender WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("look", Look);
                dbClient.AddParameter("gender", Gender);
                dbClient.RunQuery();
            }

            if (Session.GetHabbo().InRoom)
            {
                RoomUser RoomUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (RoomUser != null)
                {
                    Session.SendMessage(new UserChangeComposer(RoomUser, true));
                    Session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(RoomUser, false));
                    RoomUser.OnChat(RoomUser.LastBubble, "* Change son apparence [-10 CRÉDITS] *", true);
                    Group Gouvernement = null;
                    if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(18, out Gouvernement))
                    {
                        Gouvernement.ChiffreAffaire += 7;
                        Gouvernement.updateChiffre();
                    }
                }
            }
        }
    }
}
