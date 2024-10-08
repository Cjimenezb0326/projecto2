using Entities;
using StoreDataManager;

namespace QueryProcessor.Operations
{
    internal class Select
    {
        public OperationStatus Execute(string? filter = null)
        {
            // Implementación del filtro, si se proporciona
            if (!string.IsNullOrEmpty(filter))
            {
                // Aquí puedes implementar la lógica de selección basada en el filtro
                return Store.GetInstance().SelectWithFilter(filter); // Este sería el método en Store para filtrar
            }

            // Si no hay filtro, selecciona todo
            return Store.GetInstance().Select();
        }
    }
}
