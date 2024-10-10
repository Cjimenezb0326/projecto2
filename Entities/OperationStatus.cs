namespace Entities
{
    public enum OperationStatus
    {
        Success,
        Failure,
        TableNotFound,    // Maneja el caso en que no se encuentra la tabla
        TableNotEmpty,    // Maneja el caso en que la tabla no está vacía
        RowNotFound,      // Agregado para manejar el caso en que no se encuentran filas para eliminar
        DeleteFailed,      // Agregado para manejar cualquier fallo durante la operación de eliminación
        NoRowsDeleted
    }
}
