using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using SimpleFEM.Interfaces;
using Microsoft.Data.Sqlite;
using SimpleFEM.Types.Settings;
using SimpleFEM.Types.StructureTypes;

namespace SimpleFEM.Base;


public class DatabaseStructure : IStructure
{
    /*
        COMMENTS FOR COMMON PATTERNS
        Comments for common patterns can be found here, I will not copy and paste the same comments for each time these patterns appear.
                     
        using (SqliteConnection conn = new SqliteConnection(_connectionString)) 
        {
            conn.Open();
            SqliteCommand command = conn.CreateCommand();
            ...other things...
            conn.Close();
        }
        This pattern creates a new connection to the database specified by the connection string,
        and then creates a command associated with this connection
        After having done all necessary operations, the connection is closed by conn.Close()
        and the conn object is properly disposed of by C#s garbage collection
        
        command.Parameters.AddWithValue("@paramIdentifier", paramValue);
        This pattern replaces the @paramIdentifier in the CommandText of command with the value of paramValue.

        public bool SomeFunction(var V, SqliteConnection conn) {}
        The key thing to notice here is that we pass the already present connection to the function here
        This pattern is used when validating things while a connection is open, 
        e.g. if a candidate element is valid.
             
        using (SqliteTransaction transaction = conn.BeginTransaction()) {
             ...
             transaction.Commit();
        }
        Using transactions is a safer way to modify the elements of the structure, as if at any point there is an error, the changes to the database are not committed.
        this reduces the risk of corruption of the database
        
        using (SqliteDataReader reader = retrieveCommand.ExecuteReader()) {
            ...
        }
        This is similar to how we create a transaction and a connection. By creating the reader like this, 
        we ensure that garbage is collected after we are done with the reader
    */
    private readonly string _structureName;
    private readonly string _connectionString;
    public DatabaseStructure(string filepath, StructureSettings settings)
    {
        _structureName = Path.GetFileNameWithoutExtension(filepath);
        _connectionString = $"Data Source={filepath}";
        InitialiseDatabase();
        //settings are given therefore we must initialise them
        InitialiseStructureSettings(settings);
        InitialiseMaterialsAndSections();
    }

    public DatabaseStructure(string fullFilepath)
    {
        _structureName = Path.GetFileNameWithoutExtension(fullFilepath);
        _connectionString = $"Data Source={fullFilepath}";

        InitialiseDatabase();
        InitialiseMaterialsAndSections();
    }
    private void InitialiseMaterialsAndSections()
    {
        AddMaterial(Material.Steel);
        AddMaterial(Material.Aluminium);
        AddMaterial(Material.Concrete);
        AddSection(Section.UB533x312x273);
        AddSection(Section.UC254x254x132);
        AddSection(Section.SHS100x100x5);
    }
    
    private void InitialiseDatabase()
    {
        //create database tables if they do not exist
         using (SqliteConnection conn = new SqliteConnection(_connectionString))
         {
             conn.Open();
             SqliteCommand command = conn.CreateCommand();
             command.CommandText = @"
                     PRAGMA foreign_keys = ON;
                     CREATE TABLE IF NOT EXISTS Nodes (
                         ID INTEGER PRIMARY KEY,
                         X REAL NOT NULL,
                         Y REAL NOT NULL
                     );
                     CREATE TABLE IF NOT EXISTS Elements (
                         ID INTEGER PRIMARY KEY, 
                         MaterialID INTEGER NOT NULL,
                         SectionID INTEGER NOT NULL,
                         Node1ID INTEGER NOT NULL,
                         Node2ID INTEGER NOT NULL,
                         FOREIGN KEY (MaterialID) REFERENCES Materials(ID) ON DELETE CASCADE,
                         FOREIGN KEY (SectionID) REFERENCES Sections(ID) ON DELETE CASCADE,
                         FOREIGN KEY (Node1ID) REFERENCES Nodes(ID) ON DELETE CASCADE,
                         FOREIGN KEY (Node2ID) REFERENCES Nodes(ID) ON DELETE CASCADE
                     );
                     CREATE TABLE IF NOT EXISTS Loads (
                         NodeID INTEGER PRIMARY KEY,
                         ForceX REAL NOT NULL,
                         ForceY REAL NOT NULL,
                         Moment REAL NOT NULL, 
                         FOREIGN KEY (NodeID) REFERENCES Nodes(ID) ON DELETE CASCADE
                     );
                     CREATE TABLE IF NOT EXISTS BoundaryConditions (
                         NodeID INTEGER PRIMARY KEY,
                         FixedX BOOLEAN NOT NULL,
                         FixedY BOOLEAN NOT NULL,
                         FixedRotation BOOLEAN NOT NULL,
                         FOREIGN KEY (NodeID) REFERENCES Nodes(ID) ON DELETE CASCADE
                     );
                     CREATE TABLE IF NOT EXISTS Sections (
                         ID INTEGER PRIMARY KEY,
                         Description STRING NOT NULL,
                         A REAL NOT NULL,
                         I REAL NOT NULL
                     );
                     CREATE TABLE IF NOT EXISTS Materials (
                         ID INTEGER PRIMARY KEY,
                         Description STRING NOT NULL,
                         E REAL NOT NULL
                     );
                    CREATE TABLE IF NOT EXISTS Settings (
                        ID INTEGER PRIMARY KEY CHECK (ID = 1),
                        GridSpacing REAL NOT NULL
                    );
             ";
             command.ExecuteNonQuery();
             conn.Close();
         }
    }

    private void InitialiseStructureSettings(StructureSettings settings)
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand setSettingsCommand = conn.CreateCommand();
                setSettingsCommand.CommandText = @"
                    INSERT INTO Settings (ID, GridSpacing) VALUES (1, @gridSpacing);
                ";
                setSettingsCommand.Parameters.AddWithValue("@gridSpacing", settings.GridSpacing);
                
                setSettingsCommand.ExecuteNonQuery();
                transaction.Commit();
            }
            conn.Close();
        }
    }
    public static bool FileIsSqliteDatabase(string filepath)
    {
        //the header size of the sqlite file
        const int sqliteHeaderSize = 16;
        byte[] bytes = new byte[sqliteHeaderSize];
        //use a filestream to read the first 16 bytes of the file, i.e. the header
        using (FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            stream.Read(bytes, 0, sqliteHeaderSize);
        }
        //convert read bytes to ASCII encoded string
        string header = Encoding.ASCII.GetString(bytes);
        //if the header contains this, then it must be an Sqlite database therefore we return the result 
        return header.Contains("SQLite format");
    }
    private bool ValidElement(Element e, SqliteConnection conn)
    {
        if (e.Node1ID == e.Node2ID || !MaterialExists(e.MaterialID, conn) || !SectionExists(e.SectionID, conn))
        {
            return false;
        }
        
        SqliteCommand checkNodeIds = conn.CreateCommand();
        checkNodeIds.CommandText = @"
            SELECT ID FROM Elements WHERE
            (Node1ID = @n1 AND Node2ID = @n2) OR (Node1ID = @n2 AND Node2ID = @n1);
        ";
            
        checkNodeIds.Parameters.AddWithValue("@n1", e.Node1ID);
        checkNodeIds.Parameters.AddWithValue("@n2", e.Node2ID);
        
        //executecalar can return null if there are no matches
        object? checkNodeIdResult = checkNodeIds.ExecuteScalar();
        if (checkNodeIdResult != null)
        {
            return false;
        }
        //return true if there were no matches therefore it is a valid element
        return true;
        
    }

    public bool ValidNodeID(int nodeID)
    {
        bool valid;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            valid = NodeExists(nodeID, conn);
            conn.Close();
        }

        return valid;
    }

    public bool ValidElementID(int elementID)
    {
        bool valid;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            valid = ElementExists(elementID, conn);
            conn.Close();
        }

        return valid;
    } 
    private bool ValidNode(Vector2 v, SqliteConnection conn, out int index)
    {
        //initialise index as an invalid index
        index = -1;
        SqliteCommand checkNodeValidity = conn.CreateCommand();
        checkNodeValidity.CommandText = @"
            SELECT ID FROM Nodes WHERE
            (X = @x AND Y = @y);
        ";
            
        checkNodeValidity.Parameters.AddWithValue("@x", v.X);
        checkNodeValidity.Parameters.AddWithValue("@y", v.Y);
            
        //execute scalar can return null if there were no matches
        object? checkNodeResult = checkNodeValidity.ExecuteScalar();
        
        if (checkNodeResult != null)
        {
            //if a node was matchewd, set index variable to the ID of the node
            index = Convert.ToInt32(checkNodeResult);
            return false;
        }
        // return true if executescalar value was null, meaning no ID's for said property were matched
        return true;
    }

    private bool ElementExists(int elementID, SqliteConnection conn)
    {
        SqliteCommand elementIDCheck = conn.CreateCommand();

        elementIDCheck.CommandText = @"
            SELECT ID FROM Elements WHERE ID = @id;
        ";

        elementIDCheck.Parameters.AddWithValue("@id", elementID);
        
        //null if no ID was matched
        object? result = elementIDCheck.ExecuteScalar();
        if (result == null)
        {
            return false;
        }
        //return true if a matching element was found
        return true;
    }

    private bool NodeExists(int nodeID, SqliteConnection conn) 
    {
        SqliteCommand nodeIDCheck = conn.CreateCommand();

        nodeIDCheck.CommandText = @"
            SELECT X FROM Nodes WHERE ID = @id;
        ";

        nodeIDCheck.Parameters.AddWithValue("@id", nodeID);
        
        //executescalar returns null if no X was matched
        object? result = nodeIDCheck.ExecuteScalar();
        if (result == null)
        {
            return false;
        }
        //return true if a matching node was found
        return true;
    }
    public bool AddNode(Vector2 v, out int index)
    {
        //set index of added node from the start to an invalid value
        index = -1;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!ValidNode(v, conn, out int idx))
            {
                //if not a valid new node, set index to index of the existing node and return 
                conn.Close();
                index = idx;
                return false;
            }
            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand insertCommand = conn.CreateCommand();
                insertCommand.CommandText = @"
                INSERT INTO Nodes (X, Y) VALUES (@x, @y);
                SELECT last_insert_rowid();
                ";
                
                insertCommand.Parameters.AddWithValue("@x", v.X);
                insertCommand.Parameters.AddWithValue("@y", v.Y);

                index = Convert.ToInt32(insertCommand.ExecuteScalar());
                
                transaction.Commit();
            }

            conn.Close();
        }

        return true;
    }

    public void RemoveElement(int elementID)
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!ElementExists(elementID, conn))
            {
                //if element doesnt exist, fail silently
                conn.Close();
                return;
            }

            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand deleteCommand = conn.CreateCommand();
                deleteCommand.CommandText = @"
                    DELETE FROM Elements WHERE ID = @id;
                ";
                
                deleteCommand.Parameters.AddWithValue("@id", elementID);
                deleteCommand.ExecuteNonQuery();
                
                transaction.Commit();
            }
            conn.Close();
        }
    }

    private void RemoveNodeConnectedFeatures(int nodeID, SqliteConnection conn)
    {
        using (SqliteTransaction transaction = conn.BeginTransaction())
        {
            SqliteCommand deleteCommand = conn.CreateCommand();
            deleteCommand.CommandText = @"
                DELETE FROM Elements WHERE Node1ID = @id OR Node2ID = @id;
                DELETE FROM Loads WHERE NodeID = @id;
                DELETE FROM BoundaryConditions WHERE NodeID = @id;
            ";
            deleteCommand.Parameters.AddWithValue("@id", nodeID);
            deleteCommand.ExecuteNonQuery();
            transaction.Commit();
        }
    }
    public void RemoveNode(int nodeID)
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!NodeExists(nodeID, conn))
            {
                //if node doesnt exist, fail silently
                conn.Close();
                return;
            }
            
            //first remove the connected features of the node from other tables
            RemoveNodeConnectedFeatures(nodeID, conn);
            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand deleteCommand = conn.CreateCommand();
                deleteCommand.CommandText = @"
                    DELETE FROM Nodes WHERE ID = @id;
                ";
 
                deleteCommand.Parameters.AddWithValue("@id", nodeID);
                deleteCommand.ExecuteNonQuery();
                
                transaction.Commit();
            }

            conn.Close();
        }
    }

    public bool AddNode(Vector2 v)
    {
        return AddNode(v, out _);
    }

    public string GetName()
    {
        return _structureName;
    }

    public bool AddElement(Element e)
    {
        return AddElement(e, out _);
    }

    public List<int> GetElementIndexesSorted()
    {
        //initialise indexes as a new list
        List<int> indexes = new List<int>();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                PRAGMA foreign_keys = ON;
                SELECT ID FROM Elements ORDER BY ID ASC;
            ";
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                //until there are no other columns that have been matched, keep adding the indexes to the list
                while (reader.Read())
                {
                    indexes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
        }

        return indexes;
    }

    public StructureSettings GetStructureSettings()
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand getCommand = conn.CreateCommand();
            getCommand.CommandText = @"
                SELECT GridSpacing FROM Settings WHERE ID = 1;
            ";
            object? result = getCommand.ExecuteScalar();
            if (result == null)
            {
                //this shouldnt happen, and has never happened within testing
                //it exists as more of a debug failsafe, so that I know where the problem is if there is one during development
                throw new Exception("Something went seriously wrong, please make sure that this doesnt happen again.");
            }
            conn.Close();
            return new StructureSettings(Convert.ToSingle(result));
        } 
    }
    public bool AddElement(Element e, out int index)
    {
        //initialise invalid index from the start
        index = -1;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!ValidElement(e, conn))
            {
                conn.Close();
                return false;
            }

            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand insertCommand = conn.CreateCommand();
                insertCommand.CommandText = @"
                INSERT INTO Elements (MaterialID, SectionID, Node1ID, Node2ID) VALUES (@mid, @sid, @n1, @n2);
                SELECT last_insert_rowid();
                ";
                insertCommand.Parameters.AddWithValue("@mid", e.MaterialID);
                insertCommand.Parameters.AddWithValue("@sid", e.SectionID);

                insertCommand.Parameters.AddWithValue("@n1", e.Node1ID);
                insertCommand.Parameters.AddWithValue("@n2", e.Node2ID);

                index = Convert.ToInt32(insertCommand.ExecuteScalar());
                
                transaction.Commit();
            }

            conn.Close();
        }

        return true;
    }

    private bool MaterialExists(int materialID, SqliteConnection conn)
    {
        SqliteCommand materialIDCheck = conn.CreateCommand();

        materialIDCheck.CommandText = @"
            SELECT ID FROM Materials WHERE ID = @id;
        ";

        materialIDCheck.Parameters.AddWithValue("@id", materialID);
        
        //executescalar returns null if nothing was matched
        object? result = materialIDCheck.ExecuteScalar();
        if (result == null)
        {
            return false;
        }
        //if a material with the given id exists, return true
        return true;
    }

    private bool SectionExists(int sectionID, SqliteConnection conn)
    {
        SqliteCommand sectionDCheck = conn.CreateCommand();

        sectionDCheck.CommandText = @"
            SELECT ID FROM Sections WHERE ID = @id;
        ";

        sectionDCheck.Parameters.AddWithValue("@id", sectionID);
        
        //executescalar returns null if nothing was matched
        object? result = sectionDCheck.ExecuteScalar();
        if (result == null)
        {
            return false;
        }
        //if a section with the given id exists, return true
        return true;
    }
    public Load GetLoad(int nodeIndex)
    {
        Load load;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT ForceX, ForceY, Moment FROM Loads WHERE NodeID = @nodeID;
            ";
            retrieveCommand.Parameters.AddWithValue("@nodeID", nodeIndex);

            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read()) //this means that there is a row of values that was matched
                {
                    load = new Load(reader.GetFloat(0), reader.GetFloat(1), reader.GetFloat(2));
                }
                else
                {
                    //set to the default, empty load, as no row was matched
                    load = Load.Default;
                }
            
            }
            conn.Close();
        }

        return load;
    }
    public List<int> GetNodeIndexesSorted()
    {
        //initialise indexes list 
        List<int> indexes = new List<int>();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT ID FROM Nodes ORDER BY ID ASC;
            ";
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                //until there are no more rows left, keep adding the next matched index to the list
                while (reader.Read())
                {
                    indexes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
        }

        return indexes;
    }

    public void SetLoad(int nodeID, Load load)
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!NodeExists(nodeID, conn))
            {
                //fail silently if a node doesnt exist
                conn.Close();
                return;
            }

            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand updateCommand = conn.CreateCommand();
                updateCommand.CommandText = @"
                    INSERT INTO Loads (NodeID, ForceX, ForceY, Moment)
                    VALUES (@id, @forceX, @forceY, @moment)
                    ON CONFLICT(NodeID) DO UPDATE
                    SET ForceX = @forceX, ForceY = @forceY, Moment = @moment;
                ";
                updateCommand.Parameters.AddWithValue("@id", nodeID);
                updateCommand.Parameters.AddWithValue("@forceX", load.ForceX);
                updateCommand.Parameters.AddWithValue("@forceY", load.ForceY);
                updateCommand.Parameters.AddWithValue("@moment", load.Moment);
                updateCommand.ExecuteNonQuery();
                transaction.Commit();
            }

            conn.Close();
        }
    }

    public BoundaryCondition GetBoundaryCondition(int nodeIndex)
    {
        BoundaryCondition bc;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!NodeExists(nodeIndex, conn))
            {
                conn.Close();
                //the program is robust enough where this should not happen, however it is included for modularity
                throw new IndexOutOfRangeException("Invalid nodeID");
            }
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT NodeID, FixedX, FixedY, FixedRotation FROM BoundaryConditions WHERE NodeID = @nodeID;
            ";
            retrieveCommand.Parameters.AddWithValue("@nodeID", nodeIndex);

            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    bc = new BoundaryCondition(reader.GetBoolean(1), reader.GetBoolean(2), reader.GetBoolean(3));
                }
                else
                {
                    //return default empty boundarycondition value
                    bc = BoundaryCondition.Default;
                }
            
            }
            conn.Close();
        }

        return bc;
    }

    public Element GetElement(int elementID)
    {
        Element e = new Element();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!ElementExists(elementID, conn))
            {
                conn.Close();
                //the program as it is, is robust enough where this should not happen, however it is included for modularity
                throw new IndexOutOfRangeException("Invalid element ID.");
            }

            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT Node1ID, Node2ID, MaterialID, SectionID
                FROM Elements 
                WHERE ID = @id;
            ";
            retrieveCommand.Parameters.AddWithValue("@id", elementID);
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    e.Node1ID = reader.GetInt32(0);
                    e.Node2ID = reader.GetInt32(1);
                    e.MaterialID = reader.GetInt32(2);
                    e.SectionID = reader.GetInt32(3);
                }
            }

            conn.Close();
        }

        return e;
    }

    public Node GetNode(int nodeID)
    {
        Node n = new();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!NodeExists(nodeID, conn))
            {
                conn.Close();
                //the program is robust enough where this should not happen, however it is included for modularity
                throw new IndexOutOfRangeException("Invalid node ID.");
            }

            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT X,Y from Nodes WHERE ID = @id
            ";
            retrieveCommand.Parameters.AddWithValue("@id", nodeID);
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    n.Pos.X = reader.GetFloat(0);
                    n.Pos.Y = reader.GetFloat(1);
                }
            }

            conn.Close();
        }

        return n;
    }

    public void SetBoundaryCondition(int nodeID, BoundaryCondition boundaryCondition)
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!NodeExists(nodeID, conn))
            {
                //fail silently if the nodeId is invalid
                conn.Close();
                return;
            }

            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand updateCommand = conn.CreateCommand();
                updateCommand.CommandText = @"
                    INSERT INTO BoundaryConditions (NodeID, FixedX, FixedY, FixedRotation)
                    VALUES (@id, @fixedX, @fixedY, @fixedRotation)
                    ON CONFLICT(NodeID) DO UPDATE
                    SET FixedX = @fixedX, FixedY = @fixedY, FixedRotation = @fixedRotation
                ";
                
                updateCommand.Parameters.AddWithValue("@id", nodeID);
                updateCommand.Parameters.AddWithValue("@fixedX", boundaryCondition.FixedX);
                updateCommand.Parameters.AddWithValue("@fixedY", boundaryCondition.FixedY);
                updateCommand.Parameters.AddWithValue("@fixedRotation", boundaryCondition.FixedRotation);

                updateCommand.ExecuteNonQuery();
                transaction.Commit();
            }

            conn.Close();
        }
    }

    public int GetElementCount()
    {
        int count;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            
            SqliteCommand countCommand = conn.CreateCommand();
            countCommand.CommandText = @"
                SELECT Count(*) FROM Elements;
            ";
            count = Convert.ToInt32(countCommand.ExecuteScalar());
            conn.Close();
        }

        return count;
    }
    public int GetNodeCount()
    {
        int count;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            
            SqliteCommand countCommand = conn.CreateCommand();
            countCommand.CommandText = @"
                SELECT Count(*) FROM Nodes;
            ";
            count = Convert.ToInt32(countCommand.ExecuteScalar());
            conn.Close();
        }

        return count;
    }
    public int GetBoundaryConditionCount()
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand countCommand = conn.CreateCommand();
            countCommand.CommandText = @"
                SELECT SUM(
                    CASE WHEN FixedX <> FALSE THEN 1 ELSE 0 END +
                    CASE WHEN FixedY <> FALSE THEN 1 ELSE 0 END +
                    CASE WHEN FixedRotation <> FALSE THEN 1 ELSE 0 END                       
                )
                FROM BoundaryConditions;
            ";
            object? result = countCommand.ExecuteScalar();
            conn.Close();
            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            //return default value 0 mull value returned by executescalar (shouldnt happen, but for robustness sake)
            return 0;
        }
    }

    public int GetLoadCount()
    {
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand countCommand = conn.CreateCommand();
            countCommand.CommandText = @"
                SELECT SUM(
                    CASE WHEN ForceX <> 0 THEN 1 ELSE 0 END +
                    CASE WHEN ForceY <> 0 THEN 1 ELSE 0 END +
                    CASE WHEN Moment <> 0 THEN 1 ELSE 0 END                 
                )
                FROM Loads;
            ";
            object? result = countCommand.ExecuteScalar();
            conn.Close();
            if (result != null)
            {
                return Convert.ToInt32(result);
            }
            //return default value 0 mull value returned by executescalar (shouldnt happen, but for robustness sake)
            return 0;
        }
    }

    public List<int> GetSectionIndexesSorted()
    {
        List<int> indexes = new List<int>();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT ID From Sections;
            ";
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                //until there are no more rows left, keep adding the next matched index to the list
                while (reader.Read())
                {
                    indexes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
        }

        return indexes;
        
    }

    public Section GetSection(int sectionID)
    {
        Section sect = Section.Dummy;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!SectionExists(sectionID, conn))
            {
                conn.Close();
                //the program is robust enough where this should not happen, however it is included for modularity
                throw new IndexOutOfRangeException("Invalid section ID.");
            }       
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT Description, I, A FROM Sections WHERE ID = @id;
            ";
            retrieveCommand.Parameters.AddWithValue("@id", sectionID);
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    sect = new Section(reader.GetString(0), reader.GetFloat(1), reader.GetFloat(2));
                }
            }   
            conn.Close();
        }
        return sect;
    } 
    public List<int> GetMaterialIndexesSorted() 
    {
        List<int> indexes = new List<int>();
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT ID From Materials;
            ";
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                //until there are no more rows left, keep adding the next matched index to the list
                while (reader.Read())
                {
                    indexes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
        }

        return indexes;
    }

    public Material GetMaterial(int materialID)
    {
        Material mat = Material.Dummy;
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (!MaterialExists(materialID, conn))
            {
                conn.Close();
                //the program is robust enough where this should not happen, however it is included for modularity
                throw new IndexOutOfRangeException("Invalid material ID.");
            }
            SqliteCommand retrieveCommand = conn.CreateCommand();
            retrieveCommand.CommandText = @"
                SELECT Description, E FROM Materials WHERE ID = @id;
            ";
            retrieveCommand.Parameters.AddWithValue("@id", materialID);
            using (SqliteDataReader reader = retrieveCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    mat = new Material(reader.GetString(0), reader.GetFloat(1));
                }
            }   
            conn.Close();
        }
        return mat;
    }

    private bool SectionExists(Section sect, SqliteConnection conn)
    {
        SqliteCommand checkCommand = conn.CreateCommand();
        
        checkCommand.CommandText = @"                                                 
        SELECT ID FROM Sections WHERE A = @a AND I = @i LIMIT 1;                 
    ";
        checkCommand.Parameters.AddWithValue("@a", sect.A);
        checkCommand.Parameters.AddWithValue("@i", sect.I);
        
        //if an ID was matched, result will not be null so return will not be null
        object? result = checkCommand.ExecuteScalar();    
        return result != null;             
    }

    private bool MaterialExists(Material mat, SqliteConnection conn)
    {
        SqliteCommand checkCommand = conn.CreateCommand();

        checkCommand.CommandText = @"
            SELECT ID FROM Materials WHERE E = @e LIMIT 1;
        ";
        checkCommand.Parameters.AddWithValue("@e", mat.E);
        
        //if an ID was matched, result will not be null so return will not be null
        object? result = checkCommand.ExecuteScalar();
        return result != null;
    } 
    public void AddMaterial(Material mat)
    {
        if (mat.E <= 0)
        {
            //do not add silently if material E is invalid
            return;
        }
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (MaterialExists(mat, conn))
            {
                //if material already exists, do not add silently
                conn.Close();
                return;
            }
            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand addCommand = conn.CreateCommand();
                addCommand.CommandText = @"
                    INSERT INTO Materials (Description, E) VALUES (@desc, @e);
                ";
                addCommand.Parameters.AddWithValue("@desc", mat.Description);
                addCommand.Parameters.AddWithValue("@e", mat.E);
                
                addCommand.ExecuteNonQuery();
                transaction.Commit();
            }

            conn.Close();
        }
    }

    public void AddSection(Section sect)
    {
        if (sect.I <= 0 || sect.A <= 0)
        {
            //do not add silently if section properties are invalid
            return;
        }
        using (SqliteConnection conn = new SqliteConnection(_connectionString))
        {
            conn.Open();
            if (SectionExists(sect, conn))
            {
                //do not add silently if a duplicate section already exists
                conn.Close();
                return;
            }
            using (SqliteTransaction transaction = conn.BeginTransaction())
            {
                SqliteCommand addCommand = conn.CreateCommand();
                addCommand.CommandText = @"
                    INSERT INTO Sections (Description, A, I) VALUES (@desc, @a, @i);
                ";
                addCommand.Parameters.AddWithValue("@desc", sect.Description);
                addCommand.Parameters.AddWithValue("@a", sect.A);
                addCommand.Parameters.AddWithValue("@i", sect.I);
                addCommand.ExecuteNonQuery();
                transaction.Commit();
            }

            conn.Close();
        }
    }
}