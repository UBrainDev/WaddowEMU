using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

using Plus.HabboHotel.Rooms.Chat.Commands.User;
using Plus.HabboHotel.Rooms.Chat.Commands.Moderator;

using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Items.Wired;


namespace Plus.HabboHotel.Rooms.Chat.Commands
{
    public class CommandManager
    {
        /// <summary>
        /// Command Prefix only applies to custom commands.
        /// </summary>
        private string _prefix = ":";

        /// <summary>
        /// Commands registered for use.
        /// </summary>
        private readonly Dictionary<string, IChatCommand> _commands;

        /// <summary>
        /// The default initializer for the CommandManager
        /// </summary>
        public CommandManager(string Prefix)
        {
            this._prefix = Prefix;
            this._commands = new Dictionary<string, IChatCommand>();
            
            this.RegisterStaff();
            this.RegisterArme();
            this.RegisterDirecteurs();
            this.RegisterDivers();
            this.RegisterApparts();
            this.RegisterGang();
            this.RegisterItems();
            this.RegisterTravaux();
        }

        /// <summary>
        /// Request the text to parse and check for commands that need to be executed.
        /// </summary>
        /// <param name="Session">Session calling this method.</param>
        /// <param name="Message">The message to parse.</param>
        /// <returns>True if parsed or false if not.</returns>
        public bool Parse(GameClient Session, string Message)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                return false;

            if (!Message.StartsWith(_prefix))
                return false;

            if (Message == _prefix + "commandes")
            {
                StringBuilder List = new StringBuilder();
                List.Append("Commandes d'items:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "items")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes diverses:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "divers")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes de travaux:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "travail")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes d'appartements:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "appart")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes de directeurs:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "directeur")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes de gang:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "gang")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                List.Append("\nCommandes criminelles:\n_____________________________________________________\n");
                foreach (var cmdList in _commands.ToList())
                {
                    if (cmdList.Value.TypeCommand != "arme")
                        continue;

                    if (!cmdList.Value.getPermission(Session))
                        continue;

                    List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                }
                if (Session.GetHabbo().Rank == 8)
                {
                    List.Append("\nCommandes staff:\n_____________________________________________________\n");
                    foreach (var cmdList in _commands.ToList())
                    {
                        if (cmdList.Value.TypeCommand != "staff")
                            continue;

                        if (!cmdList.Value.getPermission(Session))
                            continue;

                        List.Append(":" + cmdList.Key + " " + cmdList.Value.Parameters + " - " + cmdList.Value.Description + "\n");
                    }
                }
                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return true;
            }

            Message = Message.Substring(1);
            string[] Split = Message.Split(' ');

            if (Split.Length == 0)
                return false;

            IChatCommand Cmd = null;
            if (_commands.TryGetValue(Split[0].ToLower(), out Cmd))
            {
                if (Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    this.LogCommand(Session.GetHabbo().Id, Message, Session.GetHabbo().MachineId);

                if (!Cmd.getPermission(Session))
                    return false;


                Session.GetHabbo().IChatCommand = Cmd;
                Session.GetHabbo().CurrentRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, Session.GetHabbo(), this);

                Cmd.Execute(Session, Session.GetHabbo().CurrentRoom, Split);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers the default set of commands.
        /// </summary>
        private void RegisterDivers()
        {
            this.Register("miser", new MiserCommand());
            this.Register("exit", new ExitCommand());
            this.Register("donner", new DonnerCommand());
            this.Register("look", new LookCommand());
            this.Register("info", new InfoCommand());
            this.Register("facebook", new FacebookCommand());
            this.Register("sit", new SitCommand());
            this.Register("emptyitems", new EmptyItemsCommand());
            this.Register("taxi", new TaxiCommand());
        }

        private void RegisterApparts()
        {
            this.Register("pickall", new PickallCommand());
            this.Register("appartid", new AppartidCommand());
            this.Register("renouveler", new RenouvelerCommand());
        }

        private void RegisterTravaux()
        {
            this.Register("travailler", new TravaillerCommand());
            this.Register("arreter", new ArreterCommand());

            // Casino
            this.Register("jetons", new JetonsCommand());
            this.Register("convertir", new ConvertirCommand());
            this.Register("roulette", new RouletteCommand());

            // Salon de coiffure
            this.Register("voiture", new VoitureCommand());

            // Salon de coiffure
            this.Register("vendre", new VendreCommand());
            this.Register("laver", new LaverCommand());
            this.Register("coiffure", new CoiffureCommand());
            this.Register("bons", new BonsCommand());

            // Armurerie
            this.Register("munitions", new MunitionsCommand());
            this.Register("arme", new ArmeCommand());
            this.Register("portarme", new PortArmeCommand());

            // Quincaillerie
            this.Register("sac", new SacCommand());
            this.Register("gps", new GpsCommand());

            // Usine
            this.Register("creations", new CreationsCommand());

            // Gouvernement
            this.Register("papiers", new PapierCommand());
            this.Register("alerte", new AlerteCommand());
            this.Register("ticket", new TicketCommand());

            // Bouygues
            this.Register("telephone", new TelephoneCommand());
            this.Register("forfait", new ForfaitCommand());

            // Police
            this.Register("liberer", new LibererCommand());
            this.Register("menotter", new MenotterCommand());
            this.Register("lacrymo", new LacrymoCommand());
            this.Register("emp", new EmpCommand());
            this.Register("taser", new TaserCommand());
            this.Register("affaires", new AffairesCommand());
            this.Register("rechercher", new RechercherCommand());
            this.Register("radio", new RadioCommand());
            this.Register("localiser", new LocaliserCommand());
            this.Register("pierre", new PierreCommand());
            this.Register("cleanwanted", new CleanWantedCommand());
            this.Register("fouiller", new FouillerCommand());
            this.Register("confisquer", new ConfisquerCommand());

            // Pharmacie
            this.Register("commande", new CommandeCommand());

            // Mutuelle Harmonie
            this.Register("souscrire", new SouscrireCommand());
            this.Register("mutuelle", new MutuelleCommand());
            this.Register("resilier", new ResilierCommand());

            // Banque
            this.Register("solde", new SoldeCommand());
            this.Register("deposer", new DeposerCommand());
            this.Register("retirer", new RetirerCommand());
            this.Register("cb", new CBCommand());

            // Café
            this.Register("bingo", new BingoCommand());
            this.Register("boisson", new BoissonCommand());
            this.Register("gagnant", new GagnantCommand());
            this.Register("eurorp", new EuroRPCommand());
            this.Register("tabac", new TabacCommand());
            this.Register("clipper", new ClipperCommand());

            // Proxy
            this.Register("panier", new PanierCommand());

            // HOPITAL
            this.Register("analyser", new AnalyserCommand());
            this.Register("soigner", new SoignerCommand());
            this.Register("lever", new LeverCommand());
        }

        private void RegisterItems()
        {
            this.Register("echanger", new EchangerCommand());
            this.Register("boire", new BoireCommand());
            this.Register("fumer", new FumerCommand());
            this.Register("manger", new MangerCommand());
            this.Register("medicament", new MedicamentCommand());
        }

        private void RegisterDirecteurs()
        {
            this.Register("suspendre", new SuspendreCommand());
            this.Register("recruter", new RecruterCommand());
            this.Register("promouvoir", new PromouvoirCommand());
            this.Register("retrograder", new RetrograderCommand());
            this.Register("virer", new VirerCommand());
        }

        private void RegisterGang()
        {
            this.Register("gang", new GangCommand());
        }

        private void RegisterArme()
        {
            this.Register("eq", new EqCommand());
            this.Register("tirer", new TirerCommand());
            this.Register("frapper", new TaperCommand());
            this.Register("taper", new TaperCommand());
            this.Register("planter", new PlanterCommand());
            this.Register("duel", new DuelCommand());
        }

        /// <summary>
        /// Registers the moderator set of commands.
        /// </summary>
        private void RegisterStaff()
        {
            this.Register("setz", new SetzCommand());
            this.Register("quizz", new QuizzCommand());
            this.Register("question", new QuestionCommand());
            this.Register("reponse", new ReponseCommand());
            this.Register("loyer", new LoyerCommand());
            this.Register("coords", new CoordsCommand());
            this.Register("musique", new MusiqueCommand());
            this.Register("restart", new RestartCommand());
            this.Register("cellule", new CelluleCommand());
            this.Register("purge", new PurgeCommand());
            this.Register("salade", new SaladeCommand());
            this.Register("zombie", new ZombieCommand());
            this.Register("reseau", new ReseauCommand());
            this.Register("chiffre", new ChiffreCommand());
            this.Register("roommute", new RoommuteCommand());
            this.Register("enable", new EnableCommand());
            this.Register("summon", new SummonCommand());
            this.Register("ban", new BanCommand());
            this.Register("ipban", new IPBanCommand());
            this.Register("ui", new UserInfoCommand());
            this.Register("userinfo", new UserInfoCommand());
            this.Register("ha", new HotelAlertCommand());
            this.Register("hotelalert", new HotelAlertCommand());
            this.Register("dc", new DisconnectCommand());
            this.Register("disconnect", new DisconnectCommand());
            this.Register("update_bans", new UpdateBansCommand());
        }

        /// <summary>
        /// Registers a Chat Command.
        /// </summary>
        /// <param name="CommandText">Text to type for this command.</param>
        /// <param name="Command">The command to execute.</param>
        public void Register(string CommandText, IChatCommand Command)
        {
            this._commands.Add(CommandText, Command);
        }

        public static string MergeParams(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(" ");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public static string MergeParamsByVirgule(string[] Params, int Start)
        {
            var Merged = new StringBuilder();
            for (int i = Start; i < Params.Length; i++)
            {
                if (i > Start)
                    Merged.Append(",");
                Merged.Append(Params[i]);
            }

            return Merged.ToString();
        }

        public void LogCommand(int UserId, string Data, string MachineId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `logs_client_staff` (`user_id`,`data_string`,`machine_id`, `timestamp`) VALUES (@UserId,@Data,@MachineId,@Timestamp)");
                dbClient.AddParameter("UserId", UserId);
                dbClient.AddParameter("Data", Data);
                dbClient.AddParameter("MachineId", MachineId);
                dbClient.AddParameter("Timestamp", PlusEnvironment.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }

        public bool TryGetCommand(string Command, out IChatCommand IChatCommand)
        {
            return this._commands.TryGetValue(Command, out IChatCommand);
        }
    }
}
