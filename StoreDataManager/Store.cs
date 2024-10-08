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
            lock(_lock)
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

        public OperationStatus CreateTable()
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";
            
            // Solo crea la tabla sin datos predeterminados
            if (!File.Exists(tablePath)) // Verifica si la tabla ya existe
            {
                using (FileStream stream = File.Open(tablePath, FileMode.Create)) // Crea el archivo de la tabla
                {
                    // No se escribe ningún dato en el archivo.
                }
            }

            return OperationStatus.Success;
        }

        public OperationStatus Insert(int id, string nombre, string apellido, string apellido2)
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";

            // Verifica si la tabla existe
            if (!File.Exists(tablePath))
            {
                return OperationStatus.TableNotFound; // Cambia este valor si la tabla no existe
            }

            // Inserta datos en la tabla
            using (FileStream stream = File.Open(tablePath, FileMode.Append)) // Usa Append para agregar datos
            using (BinaryWriter writer = new(stream))
            {
                writer.Write(id);
                writer.Write(nombre.PadRight(30)); // Asegura el tamaño correcto
                writer.Write(apellido.PadRight(50));
                writer.Write(apellido2.PadRight(60));
            }

            return OperationStatus.Success; // Retorna el estado de éxito
        }


        public OperationStatus Select()
        {
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTE.Table";
            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryReader reader = new (stream))
            {
                
                while (stream.Position > stream.Length)
                {
                    Console.WriteLine(reader.ReadInt32());
                    Console.WriteLine(reader.ReadString());
                    Console.WriteLine(reader.ReadString());
                    Console.WriteLine(reader.ReadString());
                    
                }
                return OperationStatus.Success;
            }
        }
    }
}
