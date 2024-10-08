using ApiInterface.InternalModels;
using Entities;

namespace ApiInterface.Models
{
    internal class Response
    {
        public required Request Request { get; set; }  // Campo requerido, debe inicializarse
        public required OperationStatus Status { get; set; }  // Campo requerido, debe inicializarse
        public required string ResponseBody { get; set; }  // Campo requerido, debe inicializarse
    }
}
