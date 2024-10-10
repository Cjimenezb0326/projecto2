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

        public OperationStatus CreateTable()
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";
            
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
            
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound;
            }

            List<Tuple<int, string, string, string>> rows = new List<Tuple<int, string, string, string>>();
            
            // Lee las filas existentes en la tabla
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < stream.Length)
                {
                    int id = reader.ReadInt32();
                    string nombre = reader.ReadString().Trim();
                    string apellido = reader.ReadString().Trim();
                    string apellido2 = reader.ReadString().Trim();

                    rows.Add(new Tuple<int, string, string, string>(id, nombre, apellido, apellido2));
                }
            }

            // Si no hay cláusula WHERE, elimina todas las filas
            if (string.IsNullOrEmpty(whereClause))
            {
                File.Delete(tablePath); // Borra el archivo y luego recrea la tabla vacía
                using (FileStream stream = File.Open(tablePath, FileMode.Create)) { }
                return OperationStatus.Success;
            }

            // Filtra las filas que NO coinciden con la cláusula WHERE (las que quedarán en la tabla)
            var filteredRows = rows.Where(row =>
            {
                // Verifica las condiciones WHERE, aquí un ejemplo simple solo con 'id' como filtro
                if (whereClause.Contains($"id = '{row.Item1}'"))
                {
                    return false; // Esta fila será eliminada
                }
                return true; // Esta fila se mantiene
            }).ToList();

            // Reescribe la tabla solo con las filas que no fueron eliminadas
            using (FileStream stream = File.Open(tablePath, FileMode.Create))
            using (BinaryWriter writer = new(stream))
            {
                foreach (var row in filteredRows)
                {
                    writer.Write(row.Item1);
                    writer.Write(row.Item2.PadRight(30));
                    writer.Write(row.Item3.PadRight(50));
                    writer.Write(row.Item4.PadRight(60));
                }
            }

            // Aquí se puede implementar la lógica para actualizar los índices asociados si existen
            // ActualizarÍndices(tableName, filteredRows);

            return OperationStatus.Success;
        }

    }   
}
