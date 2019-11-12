using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorPanier : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";
            Item.UpdateNeeded = true;

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnRemove(GameClient Session, Item Item)
        {
            Item.ExtraData = "0";

            if (Item.InteractingUser > 0)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Item.InteractingUser);

                if (User != null)
                {
                    User.CanWalk = true;
                }
            }
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (!Gamemap.TilesTouching(User.X, User.Y, Item.GetX, Item.GetY))
            {
                User.MoveToIfCanWalk(Item.SquareInFront);
                return;
            }

            if (Session.GetHabbo().getNumberItemPanier() > 99)
            {
                Session.SendWhisper("Vous avez déjà 100 articles dans votre panier.");
                return;
            }

            if (User.Transaction == "panier")
                return;

            if (Item.GetBaseItem().SpriteId == 3554)
            {
                if (Session.GetHabbo().CurrentRoomId != 4)
                    return;

                if (Session.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper("Vous devez posséder un sac pour pouvoir stocker des utilitaires.");
                    return;
                }

                if (Session.GetHabbo().getMaxItem("coca") < (Session.GetHabbo().Coca + Session.GetHabbo().getNumberOfItemPanier("coca") + 1))
                {
                    Session.SendWhisper("Avec votre sac à dos vous pouvez stocker " + Session.GetHabbo().getMaxItem("coca") + " cocas maximum.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                User.CarryItem(1015);
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && Session.GetHabbo().getMaxItem("coca") > (Session.GetHabbo().Coca + Session.GetHabbo().getNumberOfItemPanier("coca") + 5))
                {
                    Session.GetHabbo().addPanier("coca-coca-coca-coca-coca");
                    User.OnChat(User.LastBubble, "* Ajoute cinq cocas à son panier *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("coca");
                    User.OnChat(User.LastBubble, "* Ajoute un coca à son panier *", true);
                }
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 3231)
            {
                if (Session.GetHabbo().CurrentRoomId != 4)
                    return;

                if (Session.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper("Vous devez posséder un sac pour pouvoir stocker des utilitaires.");
                    return;
                }

                if (Session.GetHabbo().getMaxItem("fanta") < (Session.GetHabbo().Fanta + Session.GetHabbo().getNumberOfItemPanier("fanta") + 1))
                {
                    Session.SendWhisper("Avec votre sac à dos vous pouvez stocker " + Session.GetHabbo().getMaxItem("fanta") + " fantas maximum.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                User.CarryItem(1015);
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && Session.GetHabbo().getMaxItem("fanta") > (Session.GetHabbo().Fanta + Session.GetHabbo().getNumberOfItemPanier("fanta") + 5))
                {
                    Session.GetHabbo().addPanier("fanta-fanta-fanta-fanta-fanta");
                    User.OnChat(User.LastBubble, "* Ajoute cinq fantas à son panier *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("fanta");
                    User.OnChat(User.LastBubble, "* Ajoute un fanta à son panier *", true);
                }
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 3367)
            {
                if (Session.GetHabbo().CurrentRoomId != 4)
                    return;

                if (Session.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper("Vous devez posséder un sac pour pouvoir stocker des utilitaires.");
                    return;
                }

                if (Session.GetHabbo().getMaxItem("sucette") < (Session.GetHabbo().Sucette + Session.GetHabbo().getNumberOfItemPanier("sucette") + 1))
                {
                    Session.SendWhisper("Avec votre sac à dos vous pouvez stocker " + Session.GetHabbo().getMaxItem("sucette") + " sucettes maximum.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                User.CarryItem(1015);
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && Session.GetHabbo().getMaxItem("sucette") > (Session.GetHabbo().Sucette + Session.GetHabbo().getNumberOfItemPanier("sucette") + 5))
                {
                    Session.GetHabbo().addPanier("sucette-sucette-sucette-sucette-sucette");
                    User.OnChat(User.LastBubble, "* Ajoute cinq Chupa Chups à son panier *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("sucette");
                    User.OnChat(User.LastBubble, "* Ajoute une Chupa Chups à son panier *", true);
                }
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 8188)
            {
                if (Session.GetHabbo().CurrentRoomId != 4)
                    return;

                if (Session.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper("Vous devez posséder un sac pour pouvoir stocker des utilitaires.");
                    return;
                }

                if (Session.GetHabbo().getMaxItem("pain") < (Session.GetHabbo().Pain + Session.GetHabbo().getNumberOfItemPanier("pain") + 1))
                {
                    Session.SendWhisper("Avec votre sac à dos vous pouvez stocker " + Session.GetHabbo().getMaxItem("pain") + " baguettes maximum.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                User.CarryItem(1015);
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && Session.GetHabbo().getMaxItem("pain") > (Session.GetHabbo().Pain + Session.GetHabbo().getNumberOfItemPanier("pain") + 5))
                {
                    Session.GetHabbo().addPanier("pain-pain-pain-pain-pain");
                    User.OnChat(User.LastBubble, "* Ajoute cinq baguettes à son panier *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("pain");
                    User.OnChat(User.LastBubble, "* Ajoute une baguette à son panier *", true);
                }
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 3273)
            {
                if (Session.GetHabbo().CurrentRoomId != 4)
                    return;

                if (Session.GetHabbo().Sac == 0)
                {
                    Session.SendWhisper("Vous devez posséder un sac pour pouvoir stocker des utilitaires.");
                    return;
                }

                if (Session.GetHabbo().getMaxItem("eau") < (Session.GetHabbo().Eau + Session.GetHabbo().getNumberOfItemPanier("eau") + 1))
                {
                    Session.SendWhisper("Avec votre sac à dos vous pouvez stocker " + Session.GetHabbo().getMaxItem("eau") + "L d'eau maximum.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);

                User.CarryItem(1015);
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && Session.GetHabbo().getMaxItem("eau") > (Session.GetHabbo().Eau + Session.GetHabbo().getNumberOfItemPanier("eau") + 5))
                {
                    Session.GetHabbo().addPanier("eau-eau-eau-eau-eau");
                    User.OnChat(User.LastBubble, "* Ajoute cinq bouteilles d'eau à son panier *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("eau");
                    User.OnChat(User.LastBubble, "* Ajoute une bouteille d'eau à son panier *", true);
                }
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 3586)
            {
                if (Session.GetHabbo().TravailId != 10)
                    return;

                if (Session.GetHabbo().Travaille != true)
                    return;

                if (Session.GetHabbo().Commande == null)
                {
                    Session.SendWhisper("Vous n'avez aucune commande en cours.");
                    return;
                }

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().Commande);
                if(TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                {
                    Session.GetHabbo().Commande = null;
                    User.Purchase = "";
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                    return;
                }

                RoomUser TargetUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                if (TargetUser.Transaction != null || TargetUser.isTradingItems)
                    return;

                if (TargetClient.GetHabbo().getMaxItem("doliprane") < (TargetClient.GetHabbo().Doliprane + TargetClient.GetHabbo().getNumberOfItemPanier("doliprane") + 1))
                {
                    Session.SendWhisper("Le sac à dos de "+TargetClient.GetHabbo().Username + " peut stocker maximum " + TargetClient.GetHabbo().getMaxItem("doliprane") + " dolipranes.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && TargetClient.GetHabbo().getMaxItem("doliprane") > (Session.GetHabbo().Doliprane + Session.GetHabbo().getNumberOfItemPanier("doliprane") + 5))
                {
                    Session.GetHabbo().addPanier("doliprane-doliprane-doliprane-doliprane-doliprane");
                    User.OnChat(User.LastBubble, "* Ajoute cinq dolipranes à la commande de " + TargetClient.GetHabbo().Username + " *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("doliprane");
                    User.OnChat(User.LastBubble, "* Ajoute un doliprane à la commande de " + TargetClient.GetHabbo().Username + " *", true);
                }
                
                TargetUser.Purchase = User.Purchase;
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }
            else if (Item.GetBaseItem().SpriteId == 3608)
            {
                if (Session.GetHabbo().TravailId != 10)
                    return;

                if (Session.GetHabbo().Travaille != true)
                    return;

                if (Session.GetHabbo().Commande == null)
                {
                    Session.SendWhisper("Vous n'avez aucune commande en cours.");
                    return;
                }

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().Commande);
                if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                {
                    Session.GetHabbo().Commande = null;
                    User.Purchase = "";
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                    return;
                }

                RoomUser TargetUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                if (TargetUser.Transaction != null || TargetUser.isTradingItems)
                    return;

                if (TargetClient.GetHabbo().getMaxItem("savon") < (TargetClient.GetHabbo().Savon + TargetClient.GetHabbo().getNumberOfItemPanier("savon") + 1))
                {
                    Session.SendWhisper("Le sac à dos de " + TargetClient.GetHabbo().Username + " peut stocker maximum " + TargetClient.GetHabbo().getMaxItem("savon") + " savons.");
                    return;
                }

                Item.RequestUpdate(2, true);
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                
                if (User.lastAction.AddSeconds(1) > DateTime.Now && Session.GetHabbo().getNumberItemPanier() < 96 && TargetClient.GetHabbo().getMaxItem("savon") > (Session.GetHabbo().Savon + Session.GetHabbo().getNumberOfItemPanier("savon") + 5))
                {
                    Session.GetHabbo().addPanier("savon-savon-savon-savon-savon");
                    User.OnChat(User.LastBubble, "* Ajoute cinq savons à la commande de " + TargetClient.GetHabbo().Username + " *", true);
                }
                else
                {
                    Session.GetHabbo().addPanier("savon");
                    User.OnChat(User.LastBubble, "* Ajoute un savon à la commande de " + TargetClient.GetHabbo().Username + " *", true);
                }
                
                TargetUser.Purchase = User.Purchase;
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            }

            User.lastAction = DateTime.Now;
            System.Timers.Timer timer2 = new System.Timers.Timer(500);
            timer2.Interval = 500;
            timer2.Elapsed += delegate
            {
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                timer2.Stop();
            };
            timer2.Start();
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}