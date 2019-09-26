using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Discord.WebSocket;

namespace BoveeBot
{
    class DataStorage
    {
        private static SortedSet<string> _swears = new SortedSet<string>();
        private static string swearfile = "Data/_swearstore.json";

        static DataStorage()
        {
            if (!File.Exists(swearfile))
            {
                SaveToJson(_swears, swearfile);
                return;
            }
            _swears = ReadFromJson<SortedSet<string>>(swearfile);
        }

        public static void SaveToJson(Object content, string filepath)
        {
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                string json = JsonConvert.SerializeObject(content, Formatting.Indented);
                sw.Write(json);
            }
        }

        public static T ReadFromJson<T>(string filepath)
        {
            using (StreamReader sr = new StreamReader(filepath))
            {
                var json = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
                return json;
            }
        }

        public static bool AddSwear(string swear)
        {
            if (_swears.Contains(swear)) return false;
            else {
                _swears.Add(swear);
                SaveToJson(_swears, swearfile);
                return true;
            }
        }

        public static bool DelSwear(string swear)
        {
            if (_swears.Contains(swear))
            {
                _swears.Remove(swear);
                SaveToJson(_swears, swearfile);
                return true;
            } else return false;
        }

        public static List<string> GetAllSwears()
        {
            return _swears.ToList();
        }
        
        public static bool SaveExists(string filepath)
        {
            return File.Exists(filepath);
        }
    }
}