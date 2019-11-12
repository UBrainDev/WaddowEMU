using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorTrash : IFurniInteractor
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

            if(User.isTradingItems)
            {
                Session.SendWhisper("Vous ne pouvez pas fouiller de poubelle pendant que vous faites un échange.");
                return;
            }

            User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

            if (PlusEnvironment.Trash.ContainsKey(Item.Id))
            {
                Session.SendWhisper("Cette poubelle a déjà été fouillée récemment, revenez plus tard.");
                return;
            }

            PlusEnvironment.Trash.Add(Item.Id, DateTime.Now);
            System.Timers.Timer timer1 = new System.Timers.Timer(300000);
            timer1.Interval = 300000;
            timer1.Elapsed += delegate
            {
                PlusEnvironment.Trash.Remove(Item.Id);
                Item.ExtraData = "0";
                Item.UpdateState(false, true);
                timer1.Stop();
            };
            timer1.Start();

            if (Session.GetHabbo().Hygiene > 20)
            {
                Session.GetHabbo().Hygiene = Session.GetHabbo().Hygiene - 20;
            }
            else
            {
                Session.GetHabbo().Hygiene = 0;
            }
            Session.GetHabbo().resetEffectEvent();

            Item.ExtraData = "1";
            Item.UpdateState(false, true);
            Session.GetHabbo().updateHygiene();
            User.OnChat(User.LastBubble, "* Fouille la poubelle [-20% HYGIENE] *", true);
            Session.SendMessage(new WhisperComposer(User.VirtualId, "HYGIÈNE : " + Session.GetHabbo().Hygiene + "/100", 0, 34));

            Random trashRecompense = new Random();
            int trash = trashRecompense.Next(1, 20);

            #region Récompense
            if(trash == 1)
            {
                Session.SendWhisper("Vous avez trouvé un coca et deux dolipranes dans la poubelle.");
                Session.GetHabbo().Coca += 1;
                Session.GetHabbo().updateCoca();
                Session.GetHabbo().Doliprane += 2;
                Session.GetHabbo().updateDoliprane();
            }
            else if(trash == 2)
            {
                Session.SendWhisper("Vous avez trouvé trois sucettes et un cocktail molotov dans la poubelle.");
                Session.GetHabbo().Sucette += 3;
                Session.GetHabbo().updateSucette();
                Session.GetHabbo().Cocktails += 1;
                Session.GetHabbo().updateCocktails();
            }
            else if (trash == 5)
            {
                Session.SendWhisper("Vous avez trouvé 10 crédits dans la poubelle.");
                Session.GetHabbo().Credits += 10;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
            }
            else if (trash == 6)
            {
                Session.SendWhisper("Vous avez trouvé trois savon et deux grames de weed dans la poubelle.");
                Session.GetHabbo().Savon += 3;
                Session.GetHabbo().updateSavon();
                Session.GetHabbo().Weed += 2;
                Session.GetHabbo().updateWeed();
            }
            else if (trash == 7)
            {
                Session.SendWhisper("Vous avez trouvé deux fanta et un savon dans la poubelle.");
                Session.GetHabbo().Fanta += 2;
                Session.GetHabbo().updateFanta();
                Session.GetHabbo().Savon += 1;
                Session.GetHabbo().updateSavon();
            }
            else if (trash == 10)
            {
                Session.SendWhisper("Vous avez trouvé un dolipranes et une baguette dans la poubelle.");
                Session.GetHabbo().Doliprane += 1;
                Session.GetHabbo().updateDoliprane();
                Session.GetHabbo().Pain += 1;
                Session.GetHabbo().updatePain();
            }
            else if (trash == 14)
            {
                Session.SendWhisper("Vous avez trouvé trois grammes de weed dans la poubelle.");
                Session.GetHabbo().Weed += 3;
                Session.GetHabbo().updateWeed();
            }
            else if (trash == 17)
            {
                Session.SendWhisper("Vous avez trois cocktails molotov dans la poubelle.");
                Session.GetHabbo().Cocktails += 3;
                Session.GetHabbo().updateCocktails();
            }
            else if (trash == 18)
            {
                Session.SendWhisper("Vous avez deux cocktails molotov dans la poubelle.");
                Session.GetHabbo().Cocktails += 2;
                Session.GetHabbo().updateCocktails();
            }
            else
            {
                Session.SendWhisper("Vous n'avez rien trouvé dans la poubelle.");
            }
            #endregion
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}