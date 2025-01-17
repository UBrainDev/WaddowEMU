﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class SitCommand :IChatCommand
    {
        public bool getPermission(GameClient Session)
        {
            return true;
        }

        public string TypeCommand
        {
            get { return "divers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "S'asseoir"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.Statusses.ContainsKey("lie") || User.isLying || User.RidingHorse || User.IsWalking || Session.GetHabbo().Prison == 2)
                return;

            if (Session.GetHabbo().getCooldown("command_sit"))
            {
                Session.SendWhisper("Veuillez patienter.");
                return;
            }

            Session.GetHabbo().addCooldown("command_sit", 3000);
            User.OnChat(User.LastBubble, "* S'asseoit *", true);
            if (!User.Statusses.ContainsKey("sit"))
            {
                if ((User.RotBody % 2) == 0)
                {
                    if (User == null)
                        return;

                    try
                    {
                        User.Statusses.Add("sit", "1.0");
                        User.Z -= 0.35;
                        User.isSitting = true;
                        User.UpdateNeeded = true;
                    }
                    catch { }
                }
                else
                {
                    User.RotBody--;
                    User.Statusses.Add("sit", "1.0");
                    User.Z -= 0.35;
                    User.isSitting = true;
                    User.UpdateNeeded = true;
                }
            }
            else if (User.isSitting == true)
            {
                User.Z += 0.35;
                User.Statusses.Remove("sit");
                User.Statusses.Remove("1.0");
                User.isSitting = false;
                User.UpdateNeeded = true;
            }
        }
    }
}
