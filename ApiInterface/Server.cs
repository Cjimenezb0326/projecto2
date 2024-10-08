using System.Net.Sockets;
using System.Net;
using System.Text;
using ApiInterface.InternalModels;
using System.Text.Json;
using ApiInterface.Exceptions;
using ApiInterface.Processors;
using ApiInterface.Models;
using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;

namespace ApiInterface
{
    public class Server
    {
        private static IPEndPoint serverEndPoint = new(IPAddress.Loopback, 11000);
        private static int supportedParallelConnections = 1;

        public static async Task Start()
        {
            using Socket listener = new(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(serverEndPoint);
            listener.Listen(supportedParallelConnections);
            Console.WriteLine($"Server ready at {serverEndPoint}");

            while (true)
            {
                var handler = await listener.AcceptAsync();
                try
                {
                    var rawMessage = GetMessage(handler);
                    var requestObject = ConvertToRequestObject(rawMessage);
                    var response = ProcessRequest(requestObject);
                    SendResponse(response, handler);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error procesando la solicitud: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    await SendErrorResponse(null, "Unknown exception", handler);
                }
                finally
                {
                    handler.Close();
                }
            }
        }

        private static string GetMessage(Socket handler)
        {
            using (NetworkStream stream = new NetworkStream(handler))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadLine() ?? String.Empty;
            }
        }

        private static Request ConvertToRequestObject(string rawMessage)
        {
            Console.WriteLine($"Mensaje recibido: {rawMessage}");
            return JsonSerializer.Deserialize<Request>(rawMessage) ?? throw new InvalidRequestException();
        }

        private static Response ProcessRequest(Request requestObject)
        {
            Console.WriteLine($"Procesando solicitud: {JsonSerializer.Serialize(requestObject)}");
            var processor = ProcessorFactory.Create(requestObject);
            var response = processor.Process();
            Console.WriteLine($"Enviando respuesta: {JsonSerializer.Serialize(response)}");
            return response;
        }

        private static void SendResponse(Response response, Socket handler)
        {
            using (NetworkStream stream = new NetworkStream(handler))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(JsonSerializer.Serialize(response));
            }
        }

        private static async Task SendErrorResponse(Request request, string reason, Socket handler)
        {
            var response = new Response
            {
                Request = request, // Asegúrate de pasar el objeto Request correctamente con RequestType inicializado
                Status = OperationStatus.Failure, // Establece el estado de la operación como fallido
                ResponseBody = reason // Establece el cuerpo de la respuesta con el mensaje de error
            };

            using (NetworkStream stream = new NetworkStream(handler))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(JsonSerializer.Serialize(response));
            }
        }

    }
}
