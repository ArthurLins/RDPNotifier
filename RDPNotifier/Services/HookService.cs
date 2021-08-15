using LiteDB;
using RDPNotifier.Entities;
using RDPNotifier.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RDPNotifier.Services
{
    public class HookService
    {
        private ILiteCollection<User> Users;
        private RegisterDialog Dialog;
        private HttpClient Client = new HttpClient();


        public HookService(ILiteCollection<User> users, RegisterDialog dialog)
        {
            Users = users;
            Dialog = dialog;
        }

        public void OnConnect(string clientId, string currentUser)
        {
            var user = Users.FindById(clientId);
            if (user is null)
            {
                Dialog.ShowForm(clientId);
                SendJoinWebhook(clientId, currentUser);
                return;
            }
            SendJoinWebhook(clientId, currentUser, user.Name, user.DiscordId);
        }

        public void OnDisconnect(string clientId, string currentUser)
        {
            var user = Users.FindById(clientId);
            if (user is null)
            {
                Dialog.Hide();
                SendLeaveWebhook(clientId, currentUser);
                return;
            }
            SendLeaveWebhook(clientId, currentUser, user.Name, user.DiscordId);
        }

        public void SendJoinWebhook(string clientId, string currentUser, string name =null, string discordId = "")
        {
            if (DiscordHookForm.HookUrl is null)
            {
                return;
            }
            var msg = Message("Entrou no Servidor", $"Id: {clientId} \\n Usuário do servidor: {currentUser} \\n Nome: {name ?? "Não informado"} \\n Discord: <@{discordId ?? ""}>", 7732992);
            Debug.WriteLine(msg);
            var content = new StringContent(msg, Encoding.UTF8, "application/json");
            Client.PostAsync(DiscordHookForm.HookUrl, content);
        }
        public void SendLeaveWebhook(string clientId, string currentUser, string name = "", string discordId = "")
        {
            if (DiscordHookForm.HookUrl is null)
            {
                return;
            }
            var msg = Message("Saiu no Servidor", $"Id: {clientId} \\n Usuário do servidor: {currentUser} \\n Nome: {name ?? "Não informado"} \\n Discord: <@{discordId ?? ""}>", 16711680);
            var content = new StringContent(msg, Encoding.UTF8, "application/json");
            Client.PostAsync(DiscordHookForm.HookUrl, content);
        }

        private string Message(string title, string description, int color)
        {
            return "{\"username\":\"{{username}}\",\"avatar_url\":\"{{avatar_url}}\",\"embeds\":[{\"title\":\"{{title}}\",\"description\":\"{{description}}\",\"color\":{{color}}}]}"
                .Replace("{{title}}", title)
                .Replace("{{username}}", "Server")
                .Replace("{{avatar_url}}", "https://i.imgur.com/LFcuWvE.png")
                .Replace("{{description}}", description)
                .Replace("{{color}}", color.ToString());
        }
    }
}
