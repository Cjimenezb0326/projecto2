﻿using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;

namespace QueryProcessor
{
    public class SQLQueryProcessor
    {
        public static OperationStatus Execute(string sentence)
        {
            if (sentence.StartsWith("CREATE DATABASE"))
            {
                // Extraer el nombre de la base de datos
                var parts = sentence.Split(new[] { ' ', '(' ,')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) // Espera al menos 3 partes: CREATE, DATABASE, <nombre_base_datos>
                {
                    throw new InvalidOperationException("Invalid CREATE DATABASE command");
                }

                string dbName = parts[2];
                return new CreateDatabase().Execute(dbName);
            }

            if (sentence.StartsWith("SET DATABASE"))
            {
                var parts = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) // Espera al menos 3 partes: SET, DATABASE, <nombre_base_datos>
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
                // Extraer los valores del comando INSERT
                var parts = sentence.Split(new[] { ' ', '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 6) // Espera al menos 6 partes: INSERT, INTO, TableName, VALUES, (valores)
                {
                    throw new InvalidOperationException("Invalid INSERT command");
                }

                // Obtener el nombre de la tabla
                string tableName = parts[2];

                // Obtener los valores a insertar
                int id = int.Parse(parts[4]); // ID es el primer valor en el formato
                string nombre = parts[5].Trim('\''); // Remover comillas
                string apellido = parts[6].Trim('\'');
                string apellido2 = parts[7].Trim('\'');

                return new Insert().Execute(tableName, id, nombre, apellido, apellido2);
            }

            if (sentence.StartsWith("SELECT"))
            {
                // Extrae el filtro de la consulta SQL
                var filter = sentence.Contains("WHERE") ? sentence.Substring(sentence.IndexOf("WHERE") + 6).Trim() : null;
                return new Select().Execute(filter);
            }


            if (sentence.StartsWith("DROP TABLE"))
            {
                // Extraer el nombre de la tabla
                var parts = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("Invalid DROP TABLE command");
                }

                string tableName = parts[2];

                // Llama al método DropTable en la clase Store
                return Store.GetInstance().DropTable(tableName);
            }
            
            else
            {
                throw new UnknownSQLSentenceException();

            }
            if (sentence.StartsWith("DELETE FROM"))
            {
                // Dividir la sentencia por espacios en blanco
                var parts = sentence.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length < 3) // Debe tener al menos 3 partes: DELETE, FROM, <nombre_tabla>
                {
                    throw new InvalidOperationException("Invalid DELETE command");
                }

                // Obtener el nombre de la tabla
                string tableName = parts[2];

                // Verificar si hay una cláusula WHERE
                string? whereClause = null;
                int whereIndex = Array.IndexOf(parts, "WHERE");
                if (whereIndex != -1 && whereIndex + 1 < parts.Length)
                {
                    // Unir los elementos después de WHERE para formar la cláusula
                    whereClause = string.Join(" ", parts[(whereIndex + 1)..]);
                }

                // Llamar al método Delete de la clase Store
                return Store.GetInstance().Delete(tableName, whereClause);
            }
        }
    }
}
