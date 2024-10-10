param ( 
    [Parameter(Mandatory = $true)]
    [string]$IP,
    [Parameter(Mandatory = $true)]
    [int]$Port
)

$ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port)

function Send-Message {
    param (
        [Parameter(Mandatory=$true)]
        [pscustomobject]$message,
        [Parameter(Mandatory=$true)]
        [System.Net.Sockets.Socket]$client
    )

    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $writer = New-Object System.IO.StreamWriter($stream)
    try {
        $writer.WriteLine($message)
        $writer.Flush() # Asegura que se envía el mensaje
    }
    finally {
        $writer.Close()
        $stream.Close()
    }
}

function Receive-Message {
    param (
        [System.Net.Sockets.Socket]$client
    )
    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $reader = New-Object System.IO.StreamReader($stream)
    try {
        # Leer la línea y verificar si es nula
        $line = $reader.ReadLine()
        if ($null -ne $line) { 
            return $line 
        } else { 
            return "" 
        }
    }
    finally {
        $reader.Close()
        $stream.Close()
    }
}

function Send-SQLCommand {
    param (
        [string]$command
    )
    # Crear el socket para la conexión
    $client = New-Object System.Net.Sockets.Socket($ipEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
    $client.Connect($ipEndPoint) # Conectar al servidor

    # Crear el objeto de solicitud para el comando SQL
    $requestObject = [PSCustomObject]@{
        RequestType = 0;
        RequestBody = $command
    }

    Write-Host -ForegroundColor Green "Sending command: $command"

    # Convertir el objeto de solicitud a JSON
    $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress
    Send-Message -client $client -message $jsonMessage

    # Recibir y procesar la respuesta
    $response = Receive-Message -client $client
    Write-Host -ForegroundColor Green "Response received: $response"

    # Validar la respuesta y convertirla a un objeto de PowerShell si es válida
    if ($null -ne $response -and $response -ne "") {
        $responseObject = ConvertFrom-Json -InputObject $response
        Write-Output $responseObject
    } else {
        Write-Host -ForegroundColor Red "Error: No se recibió respuesta válida del servidor."
    }

    # Cerrar la conexión
    $client.Shutdown([System.Net.Sockets.SocketShutdown]::Both)
    $client.Close()
}

# Ejemplos de uso de la función
#Send-SQLCommand -command "CREATE DATABASE TestDB"
#Send-SQLCommand -command "SET DATABASE TestDB"

#Send-SQLCommand -command "CREATE TABLE ESTUDIANTE"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (1, 'Carlos', 'Jimenez', 'Brenes')"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (2, 'Leandro', 'Ruiz', 'Acuna')"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (3, 'Leonardo', 'Araya', 'Martinez')"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (4, 'Carlos', 'Arrieta', 'Acuna')"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (5, 'David', 'Paniagua', 'Ramirez')"
#Send-SQLCommand -command "INSERT INTO ESTUDIANTES VALUES (6, 'Bryan', 'Ruiz', 'Gonzales')"
#Send-SQLCommand -command "DELETE FROM ESTUDIANTE WHERE id = 3 ;"
Send-SQLCommand -command "DELETE FROM ESTUDIANTE ;"
Send-SQLCommand -command "SELECT * FROM ESTUDIANTES ;"
#Send-SQLCommand -command "SELECT * FROM ESTUDIANTES WHERE apellido = 'Ruiz'; "
#Send-SQLCommand -command "DROP TABLE ESTUDIANTE"