# Auth2 - WPF Microsoft OAuth2, Outlook y OneDrive

Esta aplicación de escritorio WPF en C# permite:
- Autenticación OAuth2 con cuentas Microsoft (Hotmail/Outlook)
- Envío de correos electrónicos
- Cambio entre distintas cuentas Microsoft
- Subida de archivos a OneDrive
- Almacenamiento local seguro del token de acceso

## Dependencias principales
- Microsoft.Identity.Client
- Microsoft.Graph
- Microsoft.Graph.Auth (preview)

## Primeros pasos
1. Registra tu aplicación en Azure Portal para obtener el ClientId y configurar los permisos de Microsoft Graph (Mail.Send, Files.ReadWrite, offline_access, User.Read).
2. Agrega el ClientId y configuración en el código fuente.
3. Compila y ejecuta el proyecto:

```pwsh
dotnet build
dotnet run
```

## Notas
- El token de acceso se almacena localmente de forma segura.
- Puedes cambiar entre distintas cuentas Microsoft desde la interfaz.
- Se utiliza la autenticación y APIs oficiales de Microsoft.
