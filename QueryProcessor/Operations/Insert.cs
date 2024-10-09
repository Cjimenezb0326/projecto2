using StoreDataManager;
using Entities;

namespace QueryProcessor.Operations
{
    internal class Insert
    {
        internal OperationStatus Execute(string tableName, int id, string nombre, string apellido, string apellido2)
        {
            if (tableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)) // Comparar con el nombre de la tabla
            {
                return Store.GetInstance().Insert(id, nombre, apellido, apellido2); // Llama a Insert en Store
            }

            return OperationStatus.TableNotFound; // Retorna este estado si la tabla no es encontrada
        }
    }

}
