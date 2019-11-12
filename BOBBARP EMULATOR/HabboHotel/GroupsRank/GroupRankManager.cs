using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Incoming;
using System.Collections.Concurrent;

using Plus.Database.Interfaces;
using log4net;

namespace Plus.HabboHotel.GroupsRank
{
    public class GroupRankManager
    {
        private ConcurrentDictionary<string, GroupRank> _groupsRank;

        public GroupRankManager()
        {
            this.Init();
        }

        public bool TryGetRank(int TravailID, int RankID, out GroupRank GroupRank)
        {
            GroupRank = null;
            string Name = Convert.ToString(TravailID) + "/" + Convert.ToString(RankID);

            if (this._groupsRank.ContainsKey(Name))
                return this._groupsRank.TryGetValue(Name, out GroupRank);

            DataRow Row = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `groups_rank` WHERE `rank_id` = @id AND `job_id` = @job LIMIT 1");
                dbClient.AddParameter("id", RankID);
                dbClient.AddParameter("job", TravailID);
                Row = dbClient.getRow();

                if (Row != null)
                {
                    GroupRank = new GroupRank(Convert.ToInt32(Row["rank_id"]), Convert.ToInt32(Row["job_id"]), Convert.ToString(Row["name"]), Convert.ToString(Row["look_h"]), Convert.ToString(Row["look_f"]), Convert.ToInt32(Row["salaire"]), Convert.ToInt32(Row["work_everywhere"]), Convert.ToInt32(Row["rank"]));
                    this._groupsRank.TryAdd(Name, GroupRank);
                    return true;
                }
            }
            return false;
        }

        public void Init()
        {
            _groupsRank = new ConcurrentDictionary<string, GroupRank>();
        }
    }
}