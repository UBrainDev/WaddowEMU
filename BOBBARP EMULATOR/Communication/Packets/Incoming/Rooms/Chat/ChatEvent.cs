using System;

using Plus.Core;
using Plus.Communication.Packets.Incoming;
using Plus.Utilities;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class ChatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            string Message = StringCharFilter.Escape(Packet.PopString());
            if (Message.Length > 100)
                Message = Message.Substring(0, 100);

            int Colour = Packet.PopInt();

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
                Colour = 0;

            User.UnIdle();

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Vous êtes muté.");
                return;
            }

            if (Session.GetHabbo().Hopital == 1)
            {
                Session.SendWhisper("Vous ne pouvez pas parler lorsque vous êtes hospitalisé.");
                return;
            }

            if (Message.StartsWith(":", StringComparison.CurrentCulture) && PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
                return;

            if (Session.GetHabbo().Prison == 2)
            {
                Session.SendWhisper("Vous ne pouvez pas parler car vous êtes placé en cellule d'isolement.");
                return;
            }

            if (Session.GetHabbo().CurrentRoomId == 55 && User.HaveTicket == false && Session.GetHabbo().TravailId != 18 && Session.GetHabbo().TravailId != 4)
            {
                Session.SendWhisper("Vous ne pouvez pas parler tant que vous n'avez pas acheter de ticket d'entrée.");
                return;
            }

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (Session.GetHabbo().ArmeEquiped == null || Session.GetHabbo().ArmeEquiped != null &&  Message != "x")
            {
                if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                {
                    int MuteTime;
                    if (User.IncrementAndCheckFlood(out MuteTime))
                    {
                        Session.SendMessage(new FloodControlComposer(MuteTime));
                        return;
                    }
                }
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new ChatlogEntry(Session.GetHabbo().Id, Room.Id, Message, UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message))
            {
                Session.GetHabbo().BannedPhraseCount++;
                if (Session.GetHabbo().BannedPhraseCount >= PlusStaticGameSettings.BannedPhrasesAmount)
                {
                    PlusEnvironment.GetGame().GetModerationManager().BanUser("System", HabboHotel.Moderation.ModerationBanType.USERNAME, Session.GetHabbo().Username, "Spamming banned phrases (" + Message + ")", (PlusEnvironment.GetUnixTimestamp() + 78892200));
                    Session.Disconnect();
                    return;
                }

                Session.SendMessage(new ChatComposer(User.VirtualId, Message, 0, Colour));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);

            if(Session.GetHabbo().CurrentRoomId == PlusEnvironment.Quizz && Message.ToLower() == PlusEnvironment.QuizzReponse)
            {
                string GoodReponse = Message.ToLower();
                PlusEnvironment.QuizzReponse = null;

                Session.GetHabbo().Quizz_Points += 1;
                Session.GetHabbo().updateQuizzPoint();

                if(Session.GetHabbo().Quizz_Points == 10)
                {
                    PlusEnvironment.Quizz = 0;

                    // LOT
                    User.OnChat(User.LastBubble, "* Gagne le quizz [+150 CRÉDITS] *", true);
                    Session.GetHabbo().Credits += 150;
                    Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "my_stats;" + Session.GetHabbo().Credits + ";" + Session.GetHabbo().Duckets + ";" + Session.GetHabbo().EventPoints);

                    // RESET QUIZZ
                    PlusEnvironment.GetGame().GetClientManager().resetQuizz();
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE users SET quizz_points = 0");
                    }

                    // ALERTE + RESET WEBEVENT
                    foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                    {
                        if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "quizz;hideall");
                        UserInRoom.GetClient().SendMessage(new WhisperComposer(UserInRoom.VirtualId, Session.GetHabbo().Username + " a gagné le quizz !", 0, 34));
                    }
                    return;
                }
                else
                {
                    foreach (RoomUser UserInRoom in Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetUserList().ToList())
                    {
                        if (UserInRoom == null || UserInRoom.IsBot || UserInRoom.GetClient() == null || UserInRoom.GetClient().GetHabbo() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "quizz;hideQuestion");
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(UserInRoom.GetClient(), "quizz;refreshClassement;" + UserInRoom.GetClient().GetHabbo().Quizz_Points);
                        UserInRoom.GetClient().SendMessage(new WhisperComposer(UserInRoom.VirtualId, Session.GetHabbo().Username + " a obtenu la bonne réponse qui était : \"" + GoodReponse + "\".", 0, 34));
                    }

                    return;
                }
            }

            if (Session.GetHabbo().AntiAFK != 0)
            {
                if (Message == Convert.ToString(Session.GetHabbo().AntiAFK))
                {
                    Session.GetHabbo().AntiAFK = 0;
                    Session.SendWhisper("[ANTI AFK] Vous avez bien répondu.");
                    return;
                }

                Session.GetHabbo().AntiAFKAttempt += 1;
                if (Session.GetHabbo().AntiAFKAttempt > 2)
                {
                    Session.GetHabbo().AntiAFK = 0;
                    Session.GetHabbo().Timer = 7;
                    Session.GetHabbo().updateTimer();
                    Session.SendWhisper("[ANTI AFK] Vous n'avez pas bien répondu.");
                    Session.GetHabbo().stopWork();
                    return;
                }

                Session.SendWhisper("[ANTI AFK] Mauvaise réponse, il vous reste " + Convert.ToString(3 - Session.GetHabbo().AntiAFKAttempt) + " tentative(s).");
                return;
            }

            if (User.isMakingCard == true)
            {
                if (User.isMakingCardCode != null && User.isMakingCardCode == Message)
                {
                    User.isMakingCard = false;
                    Session.GetHabbo().Cb = Message;
                    Session.GetHabbo().updateCb();
                    Session.SendWhisper("[CARTE BANCAIRE] Votre code de carte bancaire est le : " + Message + "");
                    User.OnChat(User.LastBubble, "* Récupère sa carte bancaire *", true);
                    User.Frozen = false;
                    return;
                }

                if (User.isMakingCardCode != null && User.isMakingCardCode != Message)
                {
                    User.isMakingCardCode = null;
                    Session.SendWhisper("[CARTE BANCAIRE] Vous n'avez pas rentrer le même code que le précédent, veuillez rentrer un nouveau code.");
                    return;
                }

                int num;
                if (!Int32.TryParse(Message, out num))
                {
                    Session.SendWhisper("[CARTE BANCAIRE] Le code indiqué est invalide.");
                    return;
                }

                if (Message.Length != 4)
                {
                    Session.SendWhisper("[CARTE BANCAIRE] Le code doit être composé de 4 chiffres.");
                    return;
                }

                User.isMakingCardCode = Message;
                Session.SendWhisper("[CARTE BANCAIRE] Veuillez confirmer votre nouveau votre code.");
                return;
            }

            if(Message == "x" && Session.GetHabbo().ArmeEquiped != null)
            {
                if(Session.GetHabbo().ArmeEquiped == "uzi" || Session.GetHabbo().ArmeEquiped == "ak47")
                {
                    PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, ":tirer " + User.userFocused);
                    return;
                }
                else if (Session.GetHabbo().ArmeEquiped == "sabre")
                {
                    PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, ":planter " + User.userFocused);
                    return;
                }
                else if (Session.GetHabbo().ArmeEquiped == "batte")
                {
                    PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, ":frapper " + User.userFocused);
                    return;
                }
                else if (Session.GetHabbo().ArmeEquiped == "taser")
                {
                    PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, ":taser " + User.userFocused);
                    return;
                }
            }

            if (Session.GetHabbo().inCallWithUsername != null)
            {
                GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetHabbo().inCallWithUsername);
                Session.SendWhisper("[À " + TargetClient.GetHabbo().Username + "] " + Message);
                TargetClient.SendWhisper("[DE " + Session.GetHabbo().Username + "] " + Message);
            }
            else
            {
                User.OnChat(User.LastBubble, Message, false);
            }
        }
    }
}