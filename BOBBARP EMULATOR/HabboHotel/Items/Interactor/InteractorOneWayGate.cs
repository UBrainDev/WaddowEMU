using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorOneWayGate : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser != 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.ClearMovement(true);
                    User.UnlockWalking();
                }

                Item.InteractingUser = 0;
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (Item.InteractingUser2 != User.UserId)
                Item.InteractingUser2 = User.UserId;

            if (User == null)
            {
                return;
            }
            if (Item.GetBaseItem().InteractionType == InteractionType.ONE_WAY_GATE)
            {
                if (User.Coordinate != Item.SquareInFront && User.CanWalk)
                {
                    User.MoveTo(Item.SquareInFront);
                    return;
                }
                if (!Item.GetRoom().GetGameMap().ValidTile(Item.SquareBehind.X, Item.SquareBehind.Y) ||
                    !Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, false)
                    || !Item.GetRoom().GetGameMap().SquareIsOpen(Item.SquareBehind.X, Item.SquareBehind.Y, false))
                {
                    return;
                }

                if ((User.LastInteraction - PlusEnvironment.GetUnixTimestamp() < 0) && User.InteractingGate &&
                    User.GateId == Item.Id)
                {
                    User.InteractingGate = false;
                    User.GateId = 0;
                }


                if (!Item.GetRoom().GetGameMap().CanWalk(Item.SquareBehind.X, Item.SquareBehind.Y, User.AllowOverride))
                {
                    return;
                }

                if(Item.Id == 32078 && Session.GetHabbo().EventCount != 6)
                {
                    Session.SendWhisper("Vous devez montrer votre passeport à la sécurité avant de pouvoir monter à bord.");
                    return;
                }

                if (Item.GetBaseItem().SpriteId == 2599)
                {
                    if (Session.GetHabbo().Travaille == false || Session.GetHabbo().TravailInfo.RoomId != Session.GetHabbo().CurrentRoomId && Session.GetHabbo().RankInfo.WorkEverywhere == 0)
                    {
                        Session.SendWhisper("Vous ne pouvez pas rentrer car vous ne travaillez pas ou vous ne travaillez pas ici.");
                        return;
                    }
                    else
                    {
                        if (Item.InteractingUser == 0)
                        {
                            User.InteractingGate = true;
                            User.GateId = Item.Id;
                            Item.InteractingUser = User.HabboId;

                            User.CanWalk = false;

                            if (User.IsWalking && (User.GoalX != Item.SquareInFront.X || User.GoalY != Item.SquareInFront.Y))
                            {
                                User.ClearMovement(true);
                            }

                            User.AllowOverride = true;
                            User.MoveTo(Item.Coordinate);

                            Item.RequestUpdate(4, true);
                        }
                    }
                }
                else if (Item.GetBaseItem().SpriteId == 2598 && Session.GetHabbo().CurrentRoomId == 55)
                {
                    if (User.HaveTicket == false && Session.GetHabbo().TravailId != 18 && Session.GetHabbo().TravailId != 4)
                    {
                        Session.SendWhisper("Vous devez payer votre ticket pour pourvoir rentrer.");
                        return;
                    }
                    else
                    {
                        if (Item.InteractingUser == 0)
                        {
                            User.InteractingGate = true;
                            User.GateId = Item.Id;
                            Item.InteractingUser = User.HabboId;

                            User.CanWalk = false;

                            if (User.IsWalking && (User.GoalX != Item.SquareInFront.X || User.GoalY != Item.SquareInFront.Y))
                            {
                                User.ClearMovement(true);
                            }

                            User.AllowOverride = true;
                            User.MoveTo(Item.Coordinate);

                            Item.RequestUpdate(4, true);
                        }
                    }
                }
                else if(Item.GetBaseItem().SpriteId == 2603 && Session.GetHabbo().CurrentRoomId == 56)
                {
                    if (PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "green") >= 4)
                    {
                        Session.SendWhisper("Il y a déjà 4 joueurs vert qui jouent actuellement.");
                        return;
                    }
                    else if (Session.GetHabbo().Travaille == true)
                    {
                        Session.SendWhisper("Vous ne pouvez pas jouer pendant que vous travaillez.");
                        return;
                    }
                    else
                    {
                        if (PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "blue") + PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "red") == 0)
                        {
                            PlusEnvironment.footballResetScore();
                        }

                        if (Session.GetHabbo().Conduit != null)
                        {
                            Session.GetHabbo().stopConduire();
                        }
                        Session.GetHabbo().updateAvatarEvent("hr-515-33.ch-3112-85-1408.hd-600-1.sh-725-85.lg-3116-85-1408", "hr-100-0.ch-245-85.hd-180-4.lg-3116-85-1408.sh-290-85", "[JOUE AU FOOT] Équipe verte");
                        User.OnChat(User.LastBubble, "* Rejoint l'équipe verte *", true);
                        Session.GetHabbo().footballTeam = "green";
                        Session.GetHabbo().footballSpawnInItem("green");
                        return;
                    }
                }
                else if (Item.GetBaseItem().SpriteId == 2597 && Session.GetHabbo().CurrentRoomId == 56)
                {
                    if (PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "blue") >= 4)
                    {
                        Session.SendWhisper("Il y a déjà 4 joueurs bleu qui jouent actuellement.");
                        return;
                    }
                    else if (Session.GetHabbo().Travaille == true)
                    {
                        Session.SendWhisper("Vous ne pouvez pas jouer pendant que vous travaillez.");
                        return;
                    }
                    else
                    {
                        if(PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "blue") + PlusEnvironment.GetGame().GetClientManager().footballCountUserPlay(Session.GetHabbo().CurrentRoom, "red") == 0)
                        {
                            PlusEnvironment.footballResetScore();
                        }

                        if (Session.GetHabbo().Conduit != null)
                        {
                            Session.GetHabbo().stopConduire();
                        }
                        Session.GetHabbo().updateAvatarEvent("hr-515-33.ch-3112-82-1408.hd-600-1.sh-725-82.lg-3116-1341-1408", "hr-100-0.ch-245-82.hd-180-4.sh-290-82.lg-3116-82-1408", "[JOUE AU FOOT] Équipe bleu");
                        User.OnChat(User.LastBubble, "* Rejoint l'équipe bleu *", true);
                        Session.GetHabbo().footballTeam = "blue";
                        Session.GetHabbo().footballSpawnInItem("blue");
                        return;
                    }
                }
                else
                {
                    if (Item.InteractingUser == 0)
                    {
                        User.InteractingGate = true;
                        User.GateId = Item.Id;
                        Item.InteractingUser = User.HabboId;

                        User.CanWalk = false;

                        if (User.IsWalking && (User.GoalX != Item.SquareInFront.X || User.GoalY != Item.SquareInFront.Y))
                        {
                            User.ClearMovement(true);
                        }

                        User.AllowOverride = true;
                        User.MoveTo(Item.Coordinate);

                        Item.RequestUpdate(4, true);
                    }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}