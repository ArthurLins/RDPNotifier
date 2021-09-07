using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDPNotifier.Entities
{
    public class HookUrl
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int Idle { get; set; }
    }
}
