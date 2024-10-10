using Entities;
using System.IO;

namespace StoreDataManager
{
    public sealed class Store
    {
        private static Store? instance = null;
        private static readonly object _lock = new object();

        public static Store GetInstance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new Store();
                }
                return instance;
            }
        }

        private const string DatabaseBasePath = @"C:\TinySql\";
        private const string DataPath = $@"{DatabaseBasePath}\Data";
        private const string SystemCatalogPath = $@"{DataPath}\SystemCatalog";
        private const string SystemDatabasesFile = $@"{SystemCatalogPath}\SystemDatabases.table";
        private const string SystemTablesFile = $@"{SystemCatalogPath}\SystemTables.table";

        public Store()
        {
            this.InitializeSystemCatalog();
        }

        private void InitializeSystemCatalog()
        {
            Directory.CreateDirectory(SystemCatalogPath);
        }
        public OperationStatus CreateDatabase(string dbName)
        {
            var dbPath = $@"{DatabaseBasePath}{dbName}";
            if (!Directory.Exists(dbPath)) 
            {
                Directory.CreateDirectory(dbPath);
            }

            return OperationStatus.Success;
        }

        public OperationStatus SetDatabase(string dbName)
        {
            var dbPath = $@"{DatabaseBasePath}{dbName}";

            if (!Directory.Exists(dbPath))
            {
                return OperationStatus.TableNotFound; 
            }



            return OperationStatus.Success;
        }

        public OperationStatus CreateTable(string tableName)
        {
            var tablePath = $@"{DataPath}\TESTDB\{tableName}.Table";
            
            // Solo crea la tabla si no existe
            if (!File.Exists(tablePath)) 
            {
                using (FileStream stream = File.Open(tablePath, FileMode.Create)) { }
            }

            return OperationStatus.Success;
        }

        public OperationStatus Insert(int id, string nombre, string apellido, string apellido2)
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";

            // Verifica si la tabla existe
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound; 
            }

            // Inserta datos en la tabla
            using (FileStream stream = File.Open(tablePath, FileMode.Append))
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(id);
                writer.Write(nombre.PadRight(30)); // Ajusta el tamaño del string
                writer.Write(apellido.PadRight(50));
                writer.Write(apellido2.PadRight(60));
            }

            return OperationStatus.Success; 
        }

        public OperationStatus Select()
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";
            
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Lee los datos de la tabla
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString().Trim();
                    string apellido = reader.ReadString().Trim();
                    string apellido2 = reader.ReadString().Trim();

                    Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}, Apellido2: {apellido2}");
                }
            }

            return OperationStatus.Success;
        }

        public OperationStatus SelectWithFilter(string filter)
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";
            
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Lee los datos de la tabla
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString().Trim();
                    string apellido = reader.ReadString().Trim();
                    string apellido2 = reader.ReadString().Trim();

                    
                    if (filter.Contains($"id = '{id}'") )//|| filter.Contains($"nombre = '{nombre}'")  )
                    {
                        Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}, Apellido2: {apellido2}");
                    }
                    if (filter.Contains($"nombre = '{nombre}'"))
                    {
                        Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}, Apellido2: {apellido2}");
                    }
                    if (filter.Contains($"apellido = '{apellido}'"))
                    {
                        Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}, Apellido2: {apellido2}");
                    }  
                    if (filter.Contains($"apellido2 = '{apellido2}'"))
                    {
                        Console.WriteLine($"ID: {id}, Nombre: {nombre}, Apellido: {apellido}, Apellido2: {apellido2}");
                    }                                          
                }
            }

            return OperationStatus.Success;
        }

        public OperationStatus DropTable(string tableName)
        {
            var tablePath = $@"{DataPath}\TESTDB\{tableName}.Table";

            
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            
            FileInfo fileInfo = new FileInfo(tablePath);
            if (fileInfo.Length > 0) 
            {
                return OperationStatus.TableNotEmpty;
            }

            
            File.Delete(tablePath);

            return OperationStatus.Success;
        }

        public OperationStatus Delete(string tableName, string? whereClause = null)
        {
            var tablePath = $@"{DataPath}\TESTDB\{tableName}.Table";

            // Verifica si la tabla existe
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            List<(int, string, string, string)> rowsToKeep = new();
            bool isRowDeleted = false;

            // Lee el archivo y guarda las filas que no se eliminarán
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Lee los datos de la tabla
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString().Trim();
                    string apellido = reader.ReadString().Trim();
                    string apellido2 = reader.ReadString().Trim();

                    bool shouldDelete = false;

                    // Verifica si hay una cláusula WHERE y si la fila cumple con la condición
                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        if (whereClause.Contains($"id = {id}")) shouldDelete = true;
                        if (whereClause.Contains($"nombre = '{nombre}'")) shouldDelete = true;
                        if (whereClause.Contains($"apellido = '{apellido}'")) shouldDelete = true;
                        if (whereClause.Contains($"apellido2 = '{apellido2}'")) shouldDelete = true;
                    }

                    // Si no hay cláusula WHERE, elimina todas las filas
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        shouldDelete = true;
                    }

                    if (!shouldDelete)
                    {
                        // Guarda la fila si no se debe eliminar
                        rowsToKeep.Add((id, nombre, apellido, apellido2));
                    }
                    else
                    {
                        isRowDeleted = true;
                    }
                }
            }

            if (!isRowDeleted && !string.IsNullOrEmpty(whereClause))
            {
                // Si no se eliminó ninguna fila con la condición dada
                return OperationStatus.NoRowsDeleted;
            }

            // Sobrescribe el archivo con las filas que se mantienen
            using (FileStream stream = File.Open(tablePath, FileMode.Create))
            using (BinaryWriter writer = new(stream))
            {
                foreach (var row in rowsToKeep)
                {
                    writer.Write(row.Item1);
                    writer.Write(row.Item2.PadRight(30));
                    writer.Write(row.Item3.PadRight(50));
                    writer.Write(row.Item4.PadRight(60));
                }
            }

            return OperationStatus.Success;
        }
        public OperationStatus Update(string tableName, string columnName, string newValue, string? whereClause = null)
        {
            var tablePath = $@"{DataPath}\TESTDB\{tableName}.Table";

            // Verifica si la tabla existe
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            List<(int, string, string, string)> rowsToUpdate = new();
            bool isRowUpdated = false;

            // Lee el archivo y guarda las filas que se van a actualizar
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    // Lee los datos de la tabla
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString().Trim();
                    string apellido = reader.ReadString().Trim();
                    string apellido2 = reader.ReadString().Trim();

                    bool shouldUpdate = false;

                    // Verifica si hay una cláusula WHERE y si la fila cumple con la condición
                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        if (whereClause.Contains($"id = {id}")) shouldUpdate = true;
                        if (whereClause.Contains($"nombre = '{nombre}'")) shouldUpdate = true;
                        if (whereClause.Contains($"apellido = '{apellido}'")) shouldUpdate = true;
                        if (whereClause.Contains($"apellido2 = '{apellido2}'")) shouldUpdate = true;
                    }

                    // Si no hay cláusula WHERE, actualiza todas las filas
                    if (string.IsNullOrEmpty(whereClause))
                    {
                        shouldUpdate = true;
                    }

                    if (shouldUpdate)
                    {
                        isRowUpdated = true;
                        // Actualiza el valor de la columna especificada
                        switch (columnName.ToLower())
                        {
                            case "nombre":
                                nombre = newValue.PadRight(30);
                                break;
                            case "apellido":
                                apellido = newValue.PadRight(50);
                                break;
                            case "apellido2":
                                apellido2 = newValue.PadRight(60);
                                break;
                            default:
                                throw new Exception($"La columna '{columnName}' no existe.");
                        }
                    }

                    // Guarda la fila (actualizada o no)
                    rowsToUpdate.Add((id, nombre, apellido, apellido2));
                }
            }

            if (!isRowUpdated && !string.IsNullOrEmpty(whereClause))
            {
                // Si no se actualizó ninguna fila con la condición dada
                return OperationStatus.NoRowsUpdated;
            }

            // Sobrescribe el archivo con las filas actualizadas
            using (FileStream stream = File.Open(tablePath, FileMode.Create))
            using (BinaryWriter writer = new(stream))
            {
                foreach (var row in rowsToUpdate)
                {
                    writer.Write(row.Item1);
                    writer.Write(row.Item2); // La cadena ya está ajustada
                    writer.Write(row.Item3);
                    writer.Write(row.Item4);
                }
            }

            return OperationStatus.Success;
        }
    }   
}
