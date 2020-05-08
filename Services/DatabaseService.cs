using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BoveeBot.Core;

namespace BoveeBot
{
    public class DatabaseService
    {
        private readonly IConfigurationRoot _config;
        private readonly SQLiteConnection _database;
        private readonly string connstring = "Data Source=Data/botstorage.db;New=False;Version=3;Compress=True";

        public DatabaseService(IConfigurationRoot config)
        {
            _config = config;

            if (!File.Exists("Data/botstorage.db"))
            {
                SQLiteConnection.CreateFile("Data/botstorage.db");
            }
            _database = new SQLiteConnection(connstring);
            InitDB();
        }

        // Local variable in classes that need it (swearjar, groups, users?)
        // Update that variable with most recent changes
        // Keep copy variable in database class
        // Use database function to copy value whenever local variable is changed
        // Write copied variables in database class to database every minute

        public void InitDB()
        {
            SQLiteConnection db;
            try
            {
                db = _database;
            } catch
            {
                db = new SQLiteConnection(connstring);
            }
            db.Open();
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Swear'", db))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                    if (rdr.Read()) {
                        db.Close();
                        return;
                    }
            }

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE Swear (value TEXT) UNIQUE (value)", db))
                cmd.ExecuteNonQuery();

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE User (snowflake INT, owed INT) UNIQUE (snowflake)", db))
                cmd.ExecuteNonQuery();

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE 'Group' (name TEXT)", db))
                cmd.ExecuteNonQuery();

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE UserSwear (userID INT, swearID INT, count INT)", db))
                cmd.ExecuteNonQuery();

            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE GroupUser (groupID INT, userID INT)", db))
                cmd.ExecuteNonQuery();
            
            using (SQLiteCommand cmd = new SQLiteCommand("CREATE TABLE SaltyAvgs (groupID INT, userID INT)", db))
                cmd.ExecuteNonQuery();
            
            db.Close();
        }

        public bool AddSwear(string swear)
        {
            SQLiteConnection db;
            try
            {
                db = _database;
            } catch
            {
                db = new SQLiteConnection(connstring);
            }
            db.Open();
            using (SQLiteCommand cmd = new SQLiteCommand($"INSERT INTO Swear (value) VALUES ('{swear}')", db))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                } catch
                {
                    db.Close();
                    return false;
                }
            }
            db.Close();
            return true;
        }

        public bool DelSwear(string swear)
        {
            SQLiteConnection db;
            try
            {
                db = _database;
            } catch
            {
                db = new SQLiteConnection(connstring);
            }
            db.Open();
            using (SQLiteCommand cmd = new SQLiteCommand($"DELETE FROM Swear WHERE value='{swear}'", db))
                try {
                    cmd.ExecuteNonQuery();
                } catch {
                    db.Close();
                    return false;
                }
            
            db.Close();
            return true;
        }

        public List<string> GetAllSwears()
        {
            SQLiteConnection db;
            try
            {
                db = _database;
            } catch
            {
                db = new SQLiteConnection(connstring);
            }
            db.Open();
            SortedSet<string> allswears = new SortedSet<string>();
            using (SQLiteCommand cmd = new SQLiteCommand($"SELECT value FROM swear ORDER BY value DESC", db))
            {
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        allswears.Add(rdr.GetString(0));
                    }
                }
            }
            db.Close();
            return allswears.ToList();
        }

        /*
        public bool AddUser(User usr)
        {
            // Command to add to User table
            // Iterate through Used dictionary and add to UserSwears table
        }

        public bool DelUser(User usr)
        {
            // Command to del from User table
            // Command to del from UserSwears table
        }

        public void GetOrAddUser(User usr)
        {
        }
        */
    }
}