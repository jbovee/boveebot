using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace BoveeBot
{
    class DataStorage
    {
        public class SwearStorage
        {
            public SortedSet<string> swears = new SortedSet<string>();
        }
        public class Storage
        {
            public SwearStorage swearstore = new SwearStorage();
        }
        private static Storage _store = new Storage();
        /*
        {
        "swearjar": {
            "swears": [],
            "users": {
                "username/user object": {
                    "owed": int,
                    "used": Dictionary<string, int>
                }
            }
            "other module": {}
        }
        */
        static DataStorage()
        {
            // Load data
            if (!ValidateStorageFile("_datastore.json")) return;
            using (StreamReader sr = new StreamReader("_datastore.json"))
            {
                var json = JsonConvert.DeserializeObject<Storage>(sr.ReadToEnd());
                _store = json;
            }
        }

        public static void SaveData()
        {
            // Save data
            using (StreamWriter sw = new StreamWriter("_datastore.json"))
            {
                string json = JsonConvert.SerializeObject(_store, Formatting.Indented);
                sw.Write(json);
            }
        }

        private static bool ValidateStorageFile(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "");
                SaveData();
                return false;
            }
            return true;
        }

        public static bool AddSwear(string swear)
        {
            if (_store.swearstore.swears.Contains(swear))
            {
                return false;
            } else {
                _store.swearstore.swears.Add(swear);
                SaveData();
                return true;
            }
        }

        public static bool DelSwear(string swear)
        {
            if (_store.swearstore.swears.Contains(swear))
            {
                _store.swearstore.swears.Remove(swear);
                SaveData();
                return true;
            } else {
                return false;
            }
        }

        public static List<string> GetAllSwears()
        {
            return _store.swearstore.swears.ToList();
        }
    }
}