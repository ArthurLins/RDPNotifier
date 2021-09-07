using LiteDB;
using RDPNotifier.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDPNotifier.Forms
{
    public partial class RegisterDialog : Form
    {
        private string ClientId = null;
        private ILiteCollection<User> Users;
        public RegisterDialog(ILiteCollection<User> users)
        {
            InitializeComponent();
            Users = users;
        }

        public void ShowForm(string clientId)
        {
            if (clientId == "" || clientId == null)
            {
                return;
            }
            Task.Factory.StartNew(() =>
            {
                ClientId = clientId;
                clientIdTextbox.Text = clientId;
                nameTextbox.Text = "";
                discordIdTextbox.Text = "";
                ShowDialog();
            });
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Users.Upsert(ClientId, new User { Name = nameTextbox.Text, DiscordId = discordIdTextbox.Text,  WinId = ClientId });
            MessageBox.Show("Informações salvas com sucesso.");
            Hide();
        }

    }
}
