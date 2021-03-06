using Cassia;
using LiteDB;
using Microsoft.Win32;
using RDPNotifier.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDPNotifier
{
    public partial class DiscordHookForm : Form
    {
        public static string HookUrl { get; set; } = null;
        public static int IdleTimeout { get; set; } = 0;

        private ILiteCollection<HookUrl> Hooks;

        public DiscordHookForm(ILiteCollection<HookUrl> hooks)
        {
            InitializeComponent();
            Hooks = hooks;
            var hook = Hooks.FindById(1);
            if (hook != null)
            {
                HookUrl = hook.Url;
                IdleTimeout = hook.Idle;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HookUrl = discordLink.Text;
            IdleTimeout = decimal.ToInt32(idleMinutes.Value);
            Hooks.Upsert(1, new HookUrl { Url = HookUrl, Idle = IdleTimeout });
            HideForm();
        }

        private void HideForm()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = Icon;
            Hide();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            Show();
        }

        private void DiscordHookForm_Shown(object sender, EventArgs e)
        {
            if (HookUrl != null)
            {
                discordLink.Text = HookUrl;
                idleMinutes.Value = IdleTimeout;
                HideForm();
                Hide();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Program.ListenSessionTick();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/ArthurLins/RDPNotifier");
        }
    }
}
