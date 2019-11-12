using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items.Wired;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;

                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                    return;

                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
            }

            if (!Room.GetRoomUserManager().AddAvatarToRoom(Session))
            {
                Room.GetRoomUserManager().RemoveUserFromRoom(Session, false, false);
                return;//TODO: Remove?
            }

            Room.SendObjects(Session);

            //Status updating for messenger, do later as buggy.

            try
            {
                if (Session.GetHabbo().GetMessenger() != null)
                    Session.GetHabbo().GetMessenger().OnStatusChanged(true);
            }
            catch { }

            if (Session.GetHabbo().GetStats().QuestID > 0)
                PlusEnvironment.GetGame().GetQuestManager().QuestReminder(Session, Session.GetHabbo().GetStats().QuestID);

            Session.SendMessage(new RoomEntryInfoComposer(Room.RoomId, Room.CheckRights(Session, true)));
            Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness, PlusEnvironment.EnumToBool(Room.Hidewall.ToString())));

            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);

            if (ThisUser != null && Session.GetHabbo().PetId == 0)
                Room.SendMessage(new UserChangeComposer(ThisUser, false));

            Session.SendMessage(new RoomEventComposer(Room.RoomData, Room.RoomData.Promotion));

            if (Room.GetWired() != null)
                Room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, Session.GetHabbo());

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                Session.SendMessage(new FloodControlComposer((int)Session.GetHabbo().FloodTime - (int)PlusEnvironment.GetUnixTimestamp()));

            #region resetCommande
            if(Session.GetHabbo().Commande != null)
            {
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().Commande);
                if (TargetClient.GetHabbo() != null)
                {
                    if (TargetClient.GetHabbo().TravailId == 10 && TargetClient.GetHabbo().Travaille == true)
                    {
                        Room TargetRoom = TargetClient.GetHabbo().CurrentRoom;
                        RoomUser TargetUser = TargetRoom.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
                        TargetUser.CarryItem(0);
                        TargetUser.OnChat(TargetUser.LastBubble, "* Arrête de prendre la commande de " + Session.GetHabbo().Username + " *", true);
                        TargetUser.Purchase = "";
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "panier", "send");
                    }

                    TargetClient.GetHabbo().Commande = null;
                }

                Session.GetHabbo().Commande = null;
            }
            #endregion
            #region resetPanier
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "panier", "send");
            #endregion

            Session.GetHabbo().resetEffectEvent();
            #region ghetto, purge
            if (Room.Description.Contains("GHETTO") || PlusEnvironment.Purge == true)
            {
                ThisUser.Immunised = true;
                Session.GetHabbo().Effects().ApplyEffect(96);
                Random TokenRand = new Random();
                int tokenNumber = TokenRand.Next(1600, 2894354);
                string Token = "IMMUNISED-" + tokenNumber;
                ThisUser.ImmunisedToken = Token;
                Session.SendWhisper("Vous êtes immunisé pendant 7 secondes.");
                System.Timers.Timer timer1 = new System.Timers.Timer(7000);
                timer1.Interval = 7000;
                timer1.Elapsed += delegate
                {
                    if (ThisUser.Immunised == true && ThisUser.ImmunisedToken == Token)
                    {
                        ThisUser.Immunised = false;
                        Session.GetHabbo().resetEffectEvent();
                        Session.SendWhisper("Vous n'êtes plus immunisé.");
                    }
                    timer1.Stop();
                };
                timer1.Start();

                if (Session.GetHabbo().Conduit != null && Session.GetHabbo().TravailId != 4 || Session.GetHabbo().TravailId == 4 && Session.GetHabbo().Travaille == false)
                {
                    Session.SendWhisper("Vous ne pouvez pas conduire de véhicule ici.");
                    Session.GetHabbo().stopConduire();
                }

                if (Room.Description.Contains("GHETTO"))
                {
                    if (Room.Capture == 0)
                    {
                        Session.SendWhisper("Cette zone est controlé par aucun gang.");
                    }
                    else
                    {
                        Session.SendWhisper("Cette zone est controlé par le gang " + Session.GetHabbo().getNameOfThisGang(Room.Capture) + ".");
                    }
                }
            }
            #endregion
            #region carte d'identité
            if(Room.Id == 48 && Session.GetHabbo().Carte == 0)
            {
                Session.SendWhisper("N'oubliez pas de réaliser votre carte d'identité pour pouvoir postuler dans des entreprises.");
                 Session.SendWhisper("En cadeau de bienvenue , le gouvernement vous donne un sac Eastpack et un téléphone rechargeable !");
            }
            #endregion
            #region ResetTelephone
            if (Session.GetHabbo().Telephone == 1)
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "telephone;setReseau;" + Session.GetHabbo().CurrentRoom.Reseau);

                if (Session.GetHabbo().CurrentRoom.Reseau <= 1 && Session.GetHabbo().inCallWithUsername != null)
                {
                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().inCallWithUsername);
                    if (TargetClient != null)
                    {
                        TargetClient.GetHabbo().isCalling = false;
                        TargetClient.GetHabbo().inCallWithUsername = null;
                        TargetClient.SendWhisper(Session.GetHabbo().Username + " n'a plus de réseau.");
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(TargetClient, "telephone;closeAppel");
                    }

                    Session.GetHabbo().isCalling = false;
                    Session.GetHabbo().inCallWithUsername = null;
                    Session.SendWhisper("Vous n'avez pas de réseau dans cette zone.");
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "telephone;closeAppel");
                }
            }
            #endregion
            #region Football
            Session.GetHabbo().footballTeam = null;
            if(Room.Id == 56)
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "footballScore;show;" + PlusEnvironment.footballGoalGreen + ";" + PlusEnvironment.footballGoalBlue);
            }
            else
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "footballScore;hide");
            }
            #endregion
            #region WebEvent
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "foutain;hide");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "slot_machine;hide");
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "musique;stop");
            #endregion
            #region Loyer
            if (Room.Loyer == 1 && Room.OwnerId != 3)
            {
                if (DateTime.Now > Room.Loyer_Date)
                {
                    Room.endlocationRoom();
                }

                if (Room.OwnerId == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("Vous devrez renouveler votre loyer avant le " + Room.Loyer_Date.Day + "/" + Room.Loyer_Date.Month + " à " + Room.Loyer_Date.Hour + ":" + Room.Loyer_Date.Minute + " en tapant :renouveler.");
                }
            }
            else if (Room.Loyer == 1 && Room.OwnerId == 3)
            {
                Session.SendWhisper("Cet appartement est en location pour " + Session.GetHabbo().CurrentRoom.Prix_Vente + " crédits par semaine.");
            }
            #endregion
            #region Casino
            if(Room.Id == 46)
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "roulette_casino;show");
            }
            else
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "roulette_casino;hide");
            }
            #endregion
            #region Quizz
            if (Room.Id == PlusEnvironment.Quizz)
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "quizz;refreshClassement;" + Session.GetHabbo().Quizz_Points);
            }
            else
            {
                PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "quizz;hideall");
            }
            #endregion

            Session.GetHabbo().updateHomeRoom(Session.GetHabbo().CurrentRoomId);
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "login", "connected");
        }
    }
}