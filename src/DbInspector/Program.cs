using System;
using Microsoft.Data.Sqlite;

class Program
{
    static void Main()
    {
        InspectDb(@"e:\Git\Quasar\src\BEAM.App\beam_fixed.db");
    }

    static void InspectDb(string dbPath)
    {
        Console.WriteLine($"\nInspecting database: {dbPath}");

        if (!System.IO.File.Exists(dbPath))
        {
            Console.WriteLine("ERROR: Database file not found!");
            return;
        }

        using var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();

        Console.WriteLine("\n--- TABLES ---");
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Table: {reader.GetString(0)}");
            }
        }

        Console.WriteLine("\n--- DEVICES TABLE ---");
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Devices";
            using var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var name = reader["DeviceName"];
                    var id = reader["Id"];
                    var active = reader["IsActive"];
                    Console.WriteLine($"Device: {name} ({id}) - Active: {active}");
                }
            }
            else
            {
                Console.WriteLine("No devices found in table.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading Devices: {ex.Message}");
        }

        Console.WriteLine("\n--- EVENT STORE ---");
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT StreamId, EventType, Timestamp FROM EventStore ORDER BY Timestamp DESC LIMIT 10";
            using var reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var type = reader.GetString(1);
                    var streamId = reader.GetGuid(0);
                    var time = reader.GetDateTime(2);
                    Console.WriteLine($"Event: {type} | Stream: {streamId} | Time: {time}");
                }
            }
            else
            {
                Console.WriteLine("No events found in EventStore.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading EventStore: {ex.Message}");
        }
    }
}
