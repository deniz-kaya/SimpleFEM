using System.Runtime.Serialization;
using System.Text;
using SimpleFEM.Interfaces;
using Microsoft.Data.Sqlite;
using SimpleFEM.Derived;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;

public class DatabaseStructure : IStructure
{
    private string StructureName;
    private StructureSettings settings;
    private string connectionString;
    
    public DatabaseStructure(string folderPath, string fileName, StructureSettings settings)
    {
        string filepath = Path.Combine(folderPath, fileName + ".structure");
        this.settings = settings;
        StructureName = fileName;
        InitialiseDatabase(filepath);
    }
    
    private void InitialiseDatabase(string filepath)
    {
        bool newDB = false;
        if (!(Path.Exists(filepath) && FileIsSqliteDatabase(filepath)))
        {
            newDB = true;
            File.Create(filepath);
        }
        connectionString = $"Data Source={filepath}";

        using (SqliteConnection conn = new SqliteConnection(connectionString))
        {
            conn.Open();
            SqliteCommand command = conn.CreateCommand();
            command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Nodes() (
                        ID INTEGER PRIMARY KEY,
                        X REAL NOT NULL,
                        Y REAL NOT NULL,
                    );
                    CREATE TABLE IF NOT EXISTS Elements() (
                        ID INTEGER PRIMARY KEY, 
                        MaterialID INTEGER NOT NULL,
                        SectionID INTEGER NOT NULL,
                        Node1ID INTEGER NOT NULL,
                        Node2ID INTEGER NOT NULL,
                        FOREIGN KEY MaterialID REFERENCES Materials(ID)
                        FOREIGN KEY SectionID REFERENCES Sections(ID)
                        FOREIGN KEY Node1ID REFERENCES Nodes(ID)
                        FOREIGN KEY Node2ID REFERENCES Nodes(ID)
                    );
                    CREATE TABLE IF NOT EXISTS Loads() (
                        NodeID INTEGER PRIMARY KEY,
                        ForceX REAL NOT NULL,
                        ForceY REAL NOT NULL,
                        Moment REAL NOT NULL, 
                        FOREIGN KEY Node1ID REFERENCES Nodes(ID)
                    );
                    CREATE TABLE IF NOT EXISTS BoundaryConditions() (
                        NodeID INTEGER PRIMARY KEY,
                        FixedX BOOLEAN NOT NULL,
                        FixedY BOOLEAN NOT NULL,
                        FixedRotation BOOLEAN NOT NULL,
                    );
                    CREATE TABLE IF NOT EXISTS Sections() (
                        ID INTEGER PRIMARY KEY,
                        tag STRING,
                        A REAL NOT NULL,
                        I REAL NOT NULL,
                    );
                    CREATE TABLE IF NOT EXISTS Materials() (
                        ID INTEGER PRIMARY KEY,
                        tag STRING,
                        E REAL NOT NULL,
                    );
            ";
            if (newDB)
            {
                command.CommandText += @"
                    CREATE TABLE IF NOT EXISTS Settings() (
                        ID INTEGER PRIMARY KEY CHECK (ID = 1),
                        GridSpacing REAL NOT NULL
                    );
                    INSERT INTO Settings (ID, GridSpacing) VALUES (1, @value)
                ";
                command.Parameters.AddWithValue("@value", settings.gridSpacing);
                command.ExecuteNonQuery();
            }
            else
            {
                command.ExecuteNonQuery();
                command.CommandText = @"
                    SELECT GridSpacing FROM Settings WHERE ID = 1; 
                ";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    //could fail
                    settings = new StructureSettings(reader.GetFloat(0));
                }
            }
            conn.Close();
        }
    }
    private bool FileIsSqliteDatabase(string filepath)
    {
        const int sqliteHeaderSize = 16;
        byte[] bytes = new byte[sqliteHeaderSize];
        using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            stream.Read(bytes, 0, sqliteHeaderSize);
        }
        string header = Encoding.ASCII.GetString(bytes);
        return header.Contains("SQLite format");
    }

    private int GetSectionID(Section section, SqliteConnection conn)
    {
        SqliteCommand command = conn.CreateCommand();

        int id = -1;
        using (SqliteTransaction transaction = conn.BeginTransaction())
        {
            command.CommandText = @"
            SELECT ID FROM Sections WHERE A = @a, I = @i;
        ";
            command.Parameters.AddWithValue("@a", section.A);
            command.Parameters.AddWithValue("@i", section.I);
            object result = command.ExecuteScalar();

            if (result != null)
            {
                id = Convert.ToInt32(result);
            }
            else
            {
                command.CommandText = @"
                    INSERT INTO Sections (A, I) VALUES (@a, @i); SELECT last_inserted_rowid();
                ";
                command.Parameters.AddWithValue("@a", section.A);
                command.Parameters.AddWithValue("@i", section.I);
                result = command.ExecuteScalar();
                id = Convert.ToInt32(result);
            }
            transaction.Commit();
        }
        
        return id;
    }

    private int GetMaterialID(Material material, SqliteConnection conn)
    {
        SqliteCommand command = conn.CreateCommand();

        int id = -1;
        using (SqliteTransaction transaction = conn.BeginTransaction())
        {
            command.CommandText = @"
            SELECT ID FROM Materials WHERE E = @e;
        ";
            command.Parameters.AddWithValue("@e", material.E);
            object result = command.ExecuteScalar();

            if (result != null)
            {
                id = Convert.ToInt32(result);
            }
            else
            {
                command.CommandText = @"
                    INSERT INTO Materials (E) VALUES (@e); SELECT last_inserted_rowid();
                ";
                command.Parameters.AddWithValue("@e", material.E);
                result = command.ExecuteScalar();
                id = Convert.ToInt32(result);
            }
            transaction.Commit();
        }
        
        return id; 
    }
    public bool AddElement(Element e)
    {
        using (SqliteConnection conn = new SqliteConnection(connectionString))
        {
            conn.Open();
            SqliteCommand command = conn.CreateCommand();
            command.CommandText = @"
                
            ";
            conn.Close();
        }
    }
    
}