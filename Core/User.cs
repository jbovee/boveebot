using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BoveeBot.Core
{
    public class User
    {
        public ulong Id;
        public string Username;
        public uint Owed;
        public Dictionary<string, uint> Used;
    }
}