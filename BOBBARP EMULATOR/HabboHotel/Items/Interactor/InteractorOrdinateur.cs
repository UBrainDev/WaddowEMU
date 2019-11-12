using System;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorOrdinateur : IFurniInteractor
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
                return;

            if (User.makeAction == true)
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            if ((Session.GetHabbo().TravailId == 3 || Session.GetHabbo().TravailId == 6 || Session.GetHabbo().TravailId == 15 || Session.GetHabbo().TravailId == 12 || (Session.GetHabbo().TravailId == 4 && Session.GetHabbo().CurrentRoomId == 20)) && Session.GetHabbo().Travaille == true || Session.GetHabbo().TravailId == 21 && Session.GetHabbo().Travaille == true)
            {
                User.makeAction = true;
                if (User.ConnectedMetier == false)
                {
                    User.Frozen = true;
                    User.OnChat(User.LastBubble, "* Allume l'ordinateur *", true);

                    System.Timers.Timer timer1 = new System.Timers.Timer(2000);
                    timer1.Interval = 2000;
                    timer1.Elapsed += delegate
                    {
                        if (Session.GetHabbo().TravailId == 3)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de la Banque Populaire *", true);
                        }
                        else if (Session.GetHabbo().TravailId == 4)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de la Police Nationale *", true);
                        }
                        else if (Session.GetHabbo().TravailId == 6)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de Harmonie Mutuelle *", true);
                        }
                        else if (Session.GetHabbo().TravailId == 12)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de Bouygues Telecom *", true);
                        }
                        else if (Session.GetHabbo().TravailId == 15)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de Hair Salon *", true);
                        }
                        else if (Session.GetHabbo().TravailId == 21)
                        {
                            User.OnChat(User.LastBubble, "* Rentre ses identifiants et se connecte au réseau de Orpi Immobilier *", true);
                        }
                        timer1.Stop();
                    };
                    timer1.Start();

                    System.Timers.Timer timer2 = new System.Timers.Timer(3500);
                    timer2.Interval = 3500;
                    timer2.Elapsed += delegate
                    {
                        Item.ExtraData = "";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(2, true);
                        User.ConnectedMetier = true;
                        if (Session.GetHabbo().TravailId == 3)
                        {
                            Session.SendWhisper("Vous êtes connecté au réseau de la Banque Populaire.");
                        }
                        else if (Session.GetHabbo().TravailId == 4)
                        {
                            Session.SendWhisper("Vous êtes connecté au réseau de la Police Nationale.");
                        }
                        else if (Session.GetHabbo().TravailId == 6)
                        {
                            Session.SendWhisper("Vous êtes connecté au réseau de Harmonie Mutuelle.");
                        }
                        else if (Session.GetHabbo().TravailId == 12)
                        {
                            Session.SendWhisper("Vous êtes connecté au réseau de Bouygues Telecom.");
                        }
                        else if (Session.GetHabbo().TravailId == 15)
                        {
                            Session.SendWhisper("Vous êtes connecté au réseau de Hair Salon.");
                        }
                        else if (Session.GetHabbo().TravailId == 21)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "appart", "orpiLogin");
                            Session.SendWhisper("Vous êtes connecté au réseau de Orpi Immobilier.");
                        }
                        User.makeAction = false;
                        timer2.Stop();
                    };
                    timer2.Start();
                }
                else
                {
                    if (Session.GetHabbo().TravailId == 3)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de la Banque Populaire *", true);
                    }
                    else if (Session.GetHabbo().TravailId == 4)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de la Police Nationale *", true);
                    }
                    else if (Session.GetHabbo().TravailId == 6)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de Harmonie Mutuelle *", true);
                    }
                    else if (Session.GetHabbo().TravailId == 12)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de Bouygues Telecom *", true);
                    }
                    else if (Session.GetHabbo().TravailId == 15)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de Hair Salon *", true);
                    }
                    else if (Session.GetHabbo().TravailId == 21)
                    {
                        User.OnChat(User.LastBubble, "* Se déconnecte du réseau de Orpi Immobilier *", true);
                    }

                    System.Timers.Timer timer1 = new System.Timers.Timer(3000);
                    timer1.Interval = 3000;
                    timer1.Elapsed += delegate
                    {
                        User.OnChat(User.LastBubble, "* Éteint l'ordinateur *", true);
                        timer1.Stop();
                    };
                    timer1.Start();

                    System.Timers.Timer timer2 = new System.Timers.Timer(4000);
                    timer2.Interval = 4000;
                    timer2.Elapsed += delegate
                    {
                        Item.ExtraData = "1";
                        Item.UpdateState(false, true);
                        Item.RequestUpdate(2, true);
                        if (Session.GetHabbo().TravailId == 3)
                        {
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de la Banque Populaire.");
                        }
                        else if (Session.GetHabbo().TravailId == 4)
                        {
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de la Police Nationale.");
                        }
                        else if (Session.GetHabbo().TravailId == 6)
                        {
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de Harmonie Mutuelle.");
                        }
                        else if (Session.GetHabbo().TravailId == 12)
                        {
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de Bouygues Telecom.");
                        }
                        else if (Session.GetHabbo().TravailId == 15)
                        {
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de Hair Salon.");
                        }
                        else if (Session.GetHabbo().TravailId == 21)
                        {
                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "ordinateur;hide");
                            Session.SendWhisper("Vous vous êtes déconnecté du réseau de Orpi Immobilier.");
                        }
                        User.ConnectedMetier = false;
                        User.makeAction = false;
                        User.Frozen = false;
                        timer2.Stop();
                    };
                    timer2.Start();
                }
            }
            else if (Session.GetHabbo().TravailId == 10 && Session.GetHabbo().Travaille == true)
            {
                if (Session.GetHabbo().Commande == null)
                {
                    Session.SendWhisper("Vous n'avez aucune commande en cours.");
                    return;
                }

                if (User.Purchase == "")
                {
                    Session.SendWhisper("La commande de " + Session.GetHabbo().Commande + " est vide.");
                    return;
                }

                User.CarryItem(0);

                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().Commande);
                if (TargetClient == null || TargetClient.GetHabbo().CurrentRoom != Session.GetHabbo().CurrentRoom)
                {
                    Session.GetHabbo().Commande = null;
                    User.Purchase = "";
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                    return;
                }

                RoomUser TargetUser = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                if (TargetUser.isTradingItems)
                {
                    Session.SendWhisper("Veuillez patienter, " + TargetClient.GetHabbo().Username + " fait un échange.");
                    return;
                }

                User.makeAction = true;
                User.Frozen = true;
                User.CarryItem(0);
                User.OnChat(User.LastBubble, "* Scanne les produits de " + TargetClient.GetHabbo().Username + " *", true);

                System.Timers.Timer timer2 = new System.Timers.Timer(3500);
                timer2.Interval = 3500;
                timer2.Elapsed += delegate
                {
                    User.OnChat(User.LastBubble, "* Valide la commande de " + TargetClient.GetHabbo().Username + " et lui donne le montant *", true);
                    if (Session.GetHabbo().Id != TargetClient.GetHabbo().Id)
                    {
                        Session.GetHabbo().Commande = null;
                        User.Purchase = "";
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
                    }

                    TargetUser.Transaction = "panier";
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "transaction;<b>" + Session.GetHabbo().Username + "</b> souhaite vous <b>valider votre commande</b> pour <b>" + TargetClient.GetHabbo().getPriceOfPanier() + " crédits </b>.;" + TargetClient.GetHabbo().getPriceOfPanier());
                    User.makeAction = false;
                    User.Frozen = false;
                    timer2.Stop();
                };
                timer2.Start();
            }
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}