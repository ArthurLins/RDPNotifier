using Cassia;
using LiteDB;
using RDPNotifier.Entities;
using RDPNotifier.Forms;
using RDPNotifier.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDPNotifier
{
    public class Program
    {

        private static RegisterDialog RegisterDialog;
        private static DiscordHookForm DiscordHookForm;
        private static HookService HookService;
        public static string CurrentUser = null;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var db = new LiteDatabase(@".\data.db"))
            {
                var hooks = db.GetCollection<HookUrl>("hooks");
                DiscordHookForm = new DiscordHookForm(hooks);
                var users = db.GetCollection<User>("users");
                RegisterDialog = new RegisterDialog(users);
                HookService = new HookService(users, RegisterDialog);
                ListenSession();
                Application.Run(DiscordHookForm);
            }
        }
        public static void ListenSession()
        {
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var inUse = CurrentUserIsInTS(out var session);
                    if (CurrentUser == null && inUse)
                    {
                        CurrentUser = session.ClientName;
                        HookService.OnConnect(CurrentUser, Environment.UserName);
                    }
                    else if (CurrentUser != null && !inUse)
                    {                    
                        HookService.OnDisconnect(CurrentUser, Environment.UserName);
                        CurrentUser = null;
                    }
                    await Task.Delay(1000);
                }
            });
        }
        public static bool CurrentUserIsInTS(out ITerminalServicesSession currentSession)
        {
            var tsMgr = new TerminalServicesManager();
            var localSvr = tsMgr.GetLocalServer();
            var sessions = localSvr.GetSessions();
            foreach (var session in sessions)
            {
                if (!Environment.UserName.Equals(session.UserName, StringComparison.OrdinalIgnoreCase)) continue;

                if (session.ConnectionState == ConnectionState.Active ||
                   session.ConnectionState == ConnectionState.Connected)
                {
                    currentSession = session;
                    return true;
                }
            }
            currentSession = null;
            return false;
        }
    }
}
