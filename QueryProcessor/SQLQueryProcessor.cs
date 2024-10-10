using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;
using System.Collections.Generic;

namespace QueryProcessor
{
    public class SQLQueryProcessor
    {
        public static OperationStatus Execute(string sentence)
        {
            if (sentence.StartsWith("CREATE DATABASE"))
            {
                var parts = sentence.Split(new[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid CREATE DATABASE command");
                }
                string dbName = parts[2];
                return new CreateDatabase().Execute(dbName);
            }

            if (sentence.StartsWith("SET DATABASE"))
            {
                var parts = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid SET DATABASE command");
                }
                string dbName = parts[2];
                return new SetDatabase().Execute(dbName);
            }

            if (sentence.StartsWith("CREATE TABLE"))
            {
                return new CreateTable().Execute();
            }

            if (sentence.StartsWith("INSERT INTO"))
            {
                var parts = sentence.Split(new[] { ' ', '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6)
                {
                    throw new InvalidOperationException("Invalid INSERT command");
                }

                string tableName = parts[2];
                int id = int.Parse(parts[4]);
                string nombre = parts[5].Trim('\'');
                string apellido = parts[6].Trim('\'');
                string apellido2 = parts[7].Trim('\'');

                return new Insert().Execute(tableName, id, nombre, apellido, apellido2);
            }

            if (sentence.StartsWith("UPDATE"))
                    {
                        return ExecuteUpdate(sentence);
                    }

                    throw new UnknownSQLSentenceException();
                }

            private static OperationStatus ExecuteUpdate(string sentence)
            {
                var parts = sentence.Split(new[] { ' ', '=', ',', 'WHERE' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid UPDATE command");
                }

                string tableName = parts[1]; // Nombre de la tabla
                Dictionary<string, string> updates = new(); // Diccionario para los campos a actualizar

                // Recolectar las actualizaciones en el formato SET
                for (int i = 2; i < parts.Length - 1; i += 2)
                {
                    string columnName = parts[i];
                    string newValue = parts[i + 1].Trim('\''); // Eliminar comillas
                    updates[columnName] = newValue;
                }

                string? whereClause = null;
                if (sentence.Contains("WHERE"))
                {
                    whereClause = sentence.Substring(sentence.IndexOf("WHERE") + 6).Trim(); // Extraer cláusula WHERE
                }

                // Verificar si hay índices disponibles para las columnas en el WHERE
                if (!string.IsNullOrEmpty(whereClause))
                {
                    // Supongamos que estamos buscando índices sobre las columnas utilizadas
                    foreach (var column in updates.Keys)
                    {
                        if (CheckIndexAvailable(tableName, column))
                        {
                            // Aquí puedes implementar lógica para usar índices si es necesario
                            Console.WriteLine($"Usando índice para actualizar en {tableName} con condición {whereClause}");
                        }
                    }
                }

                // Llamar al método Update en Store para cada columna que debe ser actualizada
                OperationStatus status = OperationStatus.Success;
                foreach (var update in updates)
                {
                    var result = Store.GetInstance().Update(tableName, update.Key, update.Value, whereClause);
                    if (result != OperationStatus.Success)
                    {
                        status = result; // Guarda el estado en caso de que no sea exitoso
                    }
                }

                return status;
            }
            }

            if (sentence.StartsWith("SELECT"))
            {
                var filter = sentence.Contains("WHERE") ? sentence.Substring(sentence.IndexOf("WHERE") + 6).Trim() : null;
                return new Select().Execute(filter);
            }

            if (sentence.StartsWith("DROP TABLE"))
            {
                var parts = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid DROP TABLE command");
                }

                string tableName = parts[2];
                return Store.GetInstance().DropTable(tableName);
            }

            if (sentence.StartsWith("DELETE"))
            {
                // Extraer el nombre de la tabla y la cláusula WHERE (si existe)
                var parts = sentence.Split(new[] { ' ', '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid DELETE command");
                }

                string tableName = parts[2];
                string whereClause = null;

                if (sentence.Contains("WHERE"))
                {
                    whereClause = sentence.Substring(sentence.IndexOf("WHERE") + 5).Trim();
                }

                // Si WHERE está presente, intentamos buscar un índice para la columna de la condición
                if (!string.IsNullOrEmpty(whereClause))
                {
                    // Supongamos que estamos buscando índices sobre la columna 'id'
                    if (whereClause.Contains("id"))
                    {
                        // Implementación simplificada para verificar si hay un índice disponible
                        bool indexAvailable = CheckIndexAvailable(tableName, "id");

                        if (indexAvailable)
                        {
                            // Usar el índice para eliminar las filas correspondientes
                            return DeleteUsingIndex(tableName, whereClause);
                        }
                    }
                }

                // Si no hay índice o no hay WHERE, se procede con búsqueda secuencial o eliminación total
                return Store.GetInstance().Delete(tableName, whereClause);
            }

            throw new UnknownSQLSentenceException();
        }

        // Función auxiliar para verificar si existe un índice sobre una columna
        private static bool CheckIndexAvailable(string tableName, string columnName)
        {
            // Implementación ficticia que revisa si hay un índice
            // En un sistema real, esto podría verificar un catálogo de índices o estructuras de datos
            return tableName == "ESTUDIANTE" && columnName == "id"; // Solo un ejemplo
        }

        // Función auxiliar para eliminar usando un índice
        private static OperationStatus DeleteUsingIndex(string tableName, string whereClause)
        {
            // Aquí estaría la lógica de eliminación basada en el índice
            // En un sistema real, buscaría las filas correspondientes mediante el índice y las eliminaría
            Console.WriteLine($"Usando índice para eliminar de {tableName} con condición {whereClause}");
            
            // Llamada a Store para eliminar las filas usando el índice
            return Store.GetInstance().Delete(tableName, whereClause);
        }
    }
}
