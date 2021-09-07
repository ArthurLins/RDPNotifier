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
        private static int idleDiff = 0;
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
                Application.Run(DiscordHookForm);
            }
        }
        public static void ListenSessionTick()
        {
            try
            {
                //Access Detection
                var inUse = SystemService.CurrentUserIsInTS(out var session);
                if (CurrentUser == null && inUse)
                {
                    Connect(session.ClientName);
                }
                else if (CurrentUser != null && !inUse)
                {
                    Disconnect(CurrentUser);
                }
                else if (CurrentUser != null && inUse)
                {
                    if (!session.ClientName.Equals(CurrentUser, StringComparison.OrdinalIgnoreCase))
                    {

                        Disconnect(CurrentUser);
                    }
                }

                //Idle detection        
                if (inUse && DiscordHookForm.IdleTimeout > 0 && CurrentUser != null)
                {
                    SystemService.CursorTick();
                    var time = SystemService.GetCursorIdleTime();
                    var diff = CalculateIdleDiff(time);
                    if (diff != idleDiff)
                    {
                        if (diff == 0 && idleDiff > 0)
                        {
                            //Resume
                            idleDiff = 0;
                            return;
                        }
                        idleDiff = diff;
                        Idle(CurrentUser, time);
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        private static int CalculateIdleDiff(TimeSpan time)
        {
            return (int)Math.Floor(time.TotalMilliseconds / (DiscordHookForm.IdleTimeout  * 60 * 1000f));
        }

        private static void Connect(string id)
        {
            SystemService.ResetTimer();
            CurrentUser = id;
            HookService.OnConnect(id, Environment.UserName);
        }

        private static void Disconnect(string id)
        {
            SystemService.ResetTimer();
            HookService.OnDisconnect(id, Environment.UserName);
            CurrentUser = null;
        }

        private static void Idle(string id, TimeSpan timeSpan)
        {
            HookService.OnUserIdle(id, Environment.UserName, timeSpan);
        }
    }
}
