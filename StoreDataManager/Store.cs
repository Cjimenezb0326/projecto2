﻿using Entities;
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

    }   
}
