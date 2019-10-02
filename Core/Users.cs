using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;

namespace BoveeBot.Core
{
    public class Users
    {
        private static List<User> users;
        private static string usersfile = "Data/_userstore.json";

        static Users()
        {
            if (DataStorage.SaveExists(usersfile))
            {
                users = DataStorage.ReadFromJson<List<User>>(usersfile);
            } else {
                users = new List<User>();
                DataStorage.SaveToJson(users, usersfile);
            }
        }

        public static User GetOrCreateUser(SocketUser user)
        {
            var result = from u in users
                where u.Id == user.Id
                select u;
            
            var pickuser = result.FirstOrDefault();
            if (pickuser == null) pickuser = CreateUser(user.Id);
            return pickuser;
        }

        public static List<User> GetAllUsers() => users;

        private static User CreateUser(ulong id)
        {
            var newUser = new User()
            {
                Id = id,
                Username = "",
                Owed = 0,
                Used = new Dictionary<string, uint>()
            };

            users.Add(newUser);
            DataStorage.SaveToJson(users, usersfile);
            return newUser;
        }

        public static void IncrementOwed(User user)
        {
            user.Owed++;
            DataStorage.SaveToJson(users, usersfile);
        }

        public static void AddOrIncrementUsed(User user, string used) 
        {
            if (user.Used.ContainsKey(used)) user.Used[used]++;
            else user.Used.Add(used, 1);
            DataStorage.SaveToJson(users, usersfile);
        }
    }
}