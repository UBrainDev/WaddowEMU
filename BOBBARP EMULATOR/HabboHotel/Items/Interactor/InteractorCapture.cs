using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorCapture : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            if (!Session.GetHabbo().CurrentRoom.Description.Contains("GHETTO"))
                return;

            if (Session.GetHabbo().CurrentRoom.Capture == Session.GetHabbo().Gang)
            {
                Session.SendWhisper("Votre gang contrôle déjà cette zone.");
                return;
            }

            if (Session.GetHabbo().Gang == 0)
            {
                Session.SendWhisper("Vous ne pouvez pas capturer cette zone car vous n'avez pas de gang.");
                return;
            }

            if (User.Capture != 0)
            {
                Session.SendWhisper("Vous êtes déjà en train de capturer cette zone.");
                return;
            }

            if (PlusEnvironment.GetGame().GetClientManager().checkCapture(Session.GetHabbo().CurrentRoomId) == true)
            {
                Session.SendWhisper("Un civil est déjà en train de capturer cette zone.");
                return;
            }
            
            User.CaptureProgress = 0;
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "capture;update;" + User.CaptureProgress);
            User.Capture = Session.GetHabbo().CurrentRoomId;
            User.OnChat(User.LastBubble, "* Commence à capturer cette zone *", true);
            if (Session.GetHabbo().CurrentRoom.Capture != 0)
            {
                PlusEnvironment.GetGame().GetClientManager().sendGangMsg(Session.GetHabbo().CurrentRoom.Capture, Session.GetHabbo().Username + " est en train de capturer votre zone : [" + Session.GetHabbo().CurrentRoom.Id + "] " + Session.GetHabbo().CurrentRoom.Name + " .");
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}