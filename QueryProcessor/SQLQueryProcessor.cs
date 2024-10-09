using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;

namespace QueryProcessor
{
    public class SQLQueryProcessor
    {
        public static OperationStatus Execute(string sentence)
        {
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
                // Pasar un filtro si está presente en la consulta
                var filter = sentence.Contains("WHERE") ? sentence.Substring(sentence.IndexOf("WHERE") + 6) : null;
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
        }
    }
}
