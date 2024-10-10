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
                return new CreateTable(tableName).Execute();
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

                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (whereClause.Contains("id"))
                    {
                        bool indexAvailable = CheckIndexAvailable(tableName, "id");

                        if (indexAvailable)
                        {
                            return DeleteUsingIndex(tableName, whereClause);
                        }
                    }
                }

                return Store.GetInstance().Delete(tableName, whereClause);
            }

            if (sentence.StartsWith("UPDATE"))
            {
                // Extraer el nombre de la tabla y la cláusula SET y WHERE (si existe)
                var parts = sentence.Split(new[] { ' ', '=', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5 || !sentence.Contains("SET"))
                {
                    throw new InvalidOperationException("Invalid UPDATE command");
                }

                string tableName = parts[1];
                string setClause = sentence.Substring(sentence.IndexOf("SET") + 4);
                string whereClause = null;

                if (sentence.Contains("WHERE"))
                {
                    whereClause = sentence.Substring(sentence.IndexOf("WHERE") + 6).Trim();
                }

                // Procesar la cláusula SET
                var setColumns = new List<string>();
                var setValues = new List<string>();
                foreach (var setPart in setClause.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var setPair = setPart.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (setPair.Length == 2)
                    {
                        setColumns.Add(setPair[0].Trim());
                        setValues.Add(setPair[1].Trim().Trim('\''));
                    }
                }

                // Verificar índices en las columnas de la cláusula WHERE
                if (!string.IsNullOrEmpty(whereClause))
                {
                    if (whereClause.Contains("id"))
                    {
                        bool indexAvailable = CheckIndexAvailable(tableName, "id");

                        if (indexAvailable)
                        {
                            var rowsToUpdate = GetRowsUsingIndex(tableName, whereClause);
                            return UpdateRows(rowsToUpdate, setColumns, setValues);
                        }
                    }
                }

                // Si no hay índice o no hay WHERE, proceder con búsqueda secuencial
                var allRows = Store.GetInstance().GetAllRows(tableName);
                return UpdateRows(allRows, setColumns, setValues);
            }

            throw new UnknownSQLSentenceException();
        }

        // Función auxiliar para verificar si existe un índice sobre una columna
        private static bool CheckIndexAvailable(string tableName, string columnName)
        {
            return tableName == "ESTUDIANTE" && columnName == "id"; // Solo un ejemplo
        }

        // Función auxiliar para eliminar usando un índice
        private static OperationStatus DeleteUsingIndex(string tableName, string whereClause)
        {
            Console.WriteLine($"Usando índice para eliminar de {tableName} con condición {whereClause}");
            return Store.GetInstance().Delete(tableName, whereClause);
        }

        // Método para obtener las filas usando un índice
        private static IEnumerable<Row> GetRowsUsingIndex(string tableName, string whereClause)
        {
            Console.WriteLine($"Usando índice para obtener filas de {tableName} con condición {whereClause}");
            return Store.GetInstance().GetRowsUsingIndex(tableName, whereClause);
        }

        // Método para actualizar las filas
        private static OperationStatus UpdateRows(IEnumerable<Row> rows, List<string> setColumns, List<string> setValues)
        {
            foreach (var row in rows)
            {
                for (int i = 0; i < setColumns.Count; i++)
                {
                    row.UpdateColumn(setColumns[i], setValues[i]);

                    // Si la columna actualizada está indexada, actualizar el índice
                    if (CheckIndexAvailable(row.TableName, setColumns[i]))
                    {
                        UpdateIndex(row, setColumns[i]);
                    }
                }
            }
            return OperationStatus.Success; // O el estado que corresponda
        }

        // Método ficticio para actualizar el índice
        private static void UpdateIndex(Row row, string columnName)
        {
            Console.WriteLine($"Actualizando índice para {columnName} en {row.TableName}");
            // Lógica para actualizar el índice correspondiente
        }
    }
}
