using System;
using System.Data;

namespace Plus.HabboHotel.Users.Authenticator
{
    public static class HabboFactory
    {
        public static Habbo GenerateHabbo(DataRow Row, DataRow UserInfo)
        {
            return new Habbo(Convert.ToInt32(Row["id"]), Convert.ToString(Row["username"]), Convert.ToInt32(Row["rank"]), Convert.ToString(Row["motto"]), Convert.ToString(Row["look"]),
                Convert.ToString(Row["gender"]), Convert.ToInt32(Row["credits"]), Convert.ToInt32(Row["activity_points"]), 
                Convert.ToInt32(Row["home_room"]), PlusEnvironment.EnumToBool(Row["block_newfriends"].ToString()), Convert.ToInt32(Row["last_online"]), Convert.ToString(Row["ip_last"]),
                PlusEnvironment.EnumToBool(Row["hide_online"].ToString()), PlusEnvironment.EnumToBool(Row["hide_inroom"].ToString()),
                Convert.ToDouble(Row["account_created"]), Convert.ToInt32(Row["vip_points"]),Convert.ToString(Row["machine_id"]), Convert.ToString(Row["volume"]),
                PlusEnvironment.EnumToBool(Row["chat_preference"].ToString()), PlusEnvironment.EnumToBool(Row["focus_preference"].ToString()), PlusEnvironment.EnumToBool(Row["pets_muted"].ToString()), PlusEnvironment.EnumToBool(Row["bots_muted"].ToString()),
                PlusEnvironment.EnumToBool(Row["advertising_report_blocked"].ToString()), Convert.ToDouble(Row["last_change"].ToString()), Convert.ToInt32(Row["gotw_points"]),
                PlusEnvironment.EnumToBool(Convert.ToString(Row["ignore_invites"])), Convert.ToDouble(Row["time_muted"]), Convert.ToDouble(UserInfo["trading_locked"]),
                PlusEnvironment.EnumToBool(Row["allow_gifts"].ToString()), Convert.ToInt32(Row["friend_bar_state"]),  PlusEnvironment.EnumToBool(Row["disable_forced_effects"].ToString()),
                PlusEnvironment.EnumToBool(Row["allow_mimic"].ToString()), Convert.ToInt32(Row["rank_vip"]), Convert.ToInt32(Row["gps"]), Convert.ToInt32(Row["sante"]), Convert.ToInt32(Row["energie"]), Convert.ToInt32(Row["sommeil"]), Convert.ToInt32(Row["hygiene"]), Convert.ToInt32(Row["telephone"]), Convert.ToString(Row["telephone_name"]), Convert.ToInt32(Row["telephoneForfaitTel"]), Convert.ToInt32(Row["telForfaitSms"]), Convert.ToInt32(Row["telForfaitType"]), Convert.ToDateTime(Row["telForfaitReset"]), Convert.ToInt32(Row["banque"]), Convert.ToInt32(Row["savon"]), Convert.ToInt32(Row["timer"]), false, Convert.ToInt32(Row["prison"]), Convert.ToInt32(Row["hopital"]), Convert.ToInt32(Row["prison_count"]), Convert.ToInt32(Row["morts"]), Convert.ToInt32(Row["kills"]), Convert.ToInt32(Row["coca"]), Convert.ToInt32(Row["fanta"]), Convert.ToInt32(Row["blur"]), Convert.ToInt32(Row["audia8"]), Convert.ToInt32(Row["porsche911"]), Convert.ToInt32(Row["fiatpunto"]), Convert.ToInt32(Row["volkswagenjetta"]), Convert.ToInt32(Row["bmwi8"]), Convert.ToString(Row["cb"]), Convert.ToInt32(Row["sucette"]), Convert.ToInt32(Row["pain"]), Convert.ToInt32(Row["doliprane"]), null, Convert.ToInt32(Row["mutuelle"]), Convert.ToDateTime(Row["mutuelle_expiration"]), Convert.ToInt32(Row["gang"]), Convert.ToInt32(Row["gang_rank"]), null, Convert.ToInt32(Row["batte"]), Convert.ToInt32(Row["sabre"]), Convert.ToInt32(Row["ak47"]), Convert.ToInt32(Row["ak47_munitions"]), Convert.ToInt32(Row["uzi"]), Convert.ToInt32(Row["uzi_munitions"]), false, true, 0, false, null, Convert.ToDouble(Row["pierre"]), Convert.ToInt32(Row["confirmed"]), Convert.ToInt32(Row["coiffure"]), Convert.ToString(Row["policecasier"]), Convert.ToInt32(Row["carte"]), Convert.ToInt32(Row["facebook"]), Convert.ToInt32(Row["sac"]), Convert.ToInt32(Row["creations"]), Convert.ToInt32(Row["permis_arme"]), Convert.ToInt32(Row["clipper"]), Convert.ToInt32(Row["weed"]), Convert.ToInt32(Row["philipmo"]), Convert.ToInt32(Row["event_day"]), Convert.ToInt32(Row["event_points"]), Convert.ToInt32(Row["casino_jetons"]), Convert.ToInt32(Row["quizz_points"]), Convert.ToInt32(Row["cocktails"]), Convert.ToInt32(Row["white_hover"]), Convert.ToInt32(Row["audia3"]), Convert.ToDecimal(Row["eau"]), Convert.ToInt32(Row["casier_weed"]), Convert.ToInt32(Row["casier_cocktails"]));
        }
    }
}