using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDPNotifier.Entities
{
    public class User
    {
        [BsonId]
        public string WinId { get; set; }
        public string Name { get; set; }
        public string DiscordId { get; set; }
    }
}
