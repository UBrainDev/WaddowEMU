using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class MoveAvatarEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null || !User.CanWalk)
                return;

            int MoveX = Packet.PopInt();
            int MoveY = Packet.PopInt();

            if (MoveX == User.X && MoveY == User.Y)
                return;

            if (User.RidingHorse)
            {
                RoomUser Horse = Room.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                if (Horse != null)
                    Horse.MoveTo(MoveX, MoveY);
            }

            if (Session.GetHabbo().Menotted)
                return;

            if (Session.GetHabbo().CurrentRoomId == 56)
            {
                if (Session.GetHabbo().footballTeam == "green" && PlusEnvironment.footballEngagot == "blue" || Session.GetHabbo().footballTeam == "blue" && PlusEnvironment.footballEngagot == "green" || Session.GetHabbo().footballTeam == "blue" && PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "green") == 0 || Session.GetHabbo().footballTeam == "green" && PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "blue") == 0)
                    return;
            }

            // RESET CONDUITE
            if (Session.GetHabbo().Conduit != null && User.Item_On != 8387 && Session.GetHabbo().Conduit != "pinkHoverboard" && Session.GetHabbo().Conduit != "blackHoverboard" && Session.GetHabbo().Conduit != "whiteHoverboard")
            {
                if (User.Item_On != 1000000109)
                {
                    Session.GetHabbo().stopConduire();
                }
            }

            if (Session.GetHabbo().ArmeEquiped == "cocktail" && (Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO") || PlusEnvironment.Purge == true) && Session.GetHabbo().CurrentRoomId != 18 && Session.GetHabbo().CurrentRoomId != 20 && Session.GetHabbo().CurrentRoomId != 3 && Session.GetHabbo().CurrentRoomId != PlusEnvironment.Salade && User.Immunised == false)
            {
                Room CurrentRoom = Session.GetHabbo().CurrentRoom;
                Random TokenRand = new Random();
                int randomId = TokenRand.Next(590000, 590090095);

                Item CocktailItem = new Item(randomId, CurrentRoom.Id, 3000501, "", MoveX, MoveY, 0, 0, Session.GetHabbo().Id, 0, 0, 0, string.Empty, Room);
                if (CurrentRoom.GetRoomItemHandler().SetFloorItemByForce(null, CocktailItem, MoveX, MoveY, 0, true, false, true))
                {
                    CurrentRoom.SendMessage(new ObjectUpdateComposer(CocktailItem, CurrentRoom.OwnerId));

                    Session.GetHabbo().Cocktails -= 1;
                    Session.GetHabbo().updateCocktails();
                    Session.GetHabbo().ArmeEquiped = null;
                    Session.GetHabbo().resetEffectEvent();

                    User.OnChat(User.LastBubble, "* Jette un cocktail molotov *", true);
                
                    CocktailItem.ExtraData = "1";
                    CocktailItem.UpdateState(false, true);

                    foreach (RoomUser UserInRoom in CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                    {
                        if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                            continue;

                        CurrentRoom.GetRoomUserManager().UpdateUserStatus(UserInRoom, true);
                    }

                    System.Timers.Timer timer1 = new System.Timers.Timer(10000);
                    timer1.Interval = 10000;
                    timer1.Elapsed += delegate
                    {
                        foreach (RoomUser UserInRoom in CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                        {
                            if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null || User.isBruling == false || User.isBrulingItem != CocktailItem.Id)
                                continue;

                            User.isBruling = false;
                        }

                        CurrentRoom.GetRoomItemHandler().RemoveFurniture(null, CocktailItem.Id);
                        timer1.Stop();
                    };
                    timer1.Start();
                }
                return;
            }

            User.MoveTo(MoveX, MoveY);
        }
    }
}