using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BoveeBot.Core
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public uint Owed { get; set; }
        public Dictionary<string, uint> Used { get; set; }
        public User (ulong id, string uname = null, uint owed = 0, Dictionary<string, uint> used = null)
        {
            Id = id;
            Username = uname;
            Owed = owed;
            if (used == null) used = new Dictionary<string, uint>();
        }
    }
}