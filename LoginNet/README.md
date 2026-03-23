# LoginNet - Sistema de Autenticación CEPLAN

Sistema de login desarrollado en ASP.NET Core con validación de credenciales, control de intentos fallidos y gestión de sesiones.

## Requisitos

- .NET 10.0 SDK
- SQL Server (local o Express)
- Windows

## Configuración

### 1. Base de Datos

Ejecutar el script SQL en `Scripts/CreateDatabase.sql` para crear la base de datos y los usuarios de prueba.

```sql
-- En SQL Server Management Studio:
-- Abrir Scripts/CreateDatabase.sql y ejecutar
```

### 2. Configuración

Editar `appsettings.json` con tu configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=LoginNetDB;..."
  },
  "EmailSettings": {
    "SmtpHost": "smtp.tucorreo.com",
    "SmtpPort": "587",
    "Usuario": "tu-correo@gmail.com",
    "Clave": "password-de-aplicacion"
  }
}
```

### 3. Ejecutar

```bash
cd LoginNet
dotnet run
```

Abrir http://localhost:5000 o http://localhost:5001

## Usuarios de Prueba

| Documento | Tipo | Contraseña |
|----------|------|------------|
| 46844596 | DNI | 123456 |
| 45678912 | DNI | 123456 |
| CE001234 | CE | 123456 |

## Funcionalidades

- Login con selector DNI/CE
- Validación de credenciales
- Contador de Intentos Fallidos (CVF)
- Bloqueo temporal tras 5 intentos fallidos
- Notificación por email al bloquear
- Extensión de sesión antes de expirar (49 segundos)
- Diseño responsive con Bootstrap 5.3

## Estructura del Proyecto

```
LoginNet/
├── Controllers/
│   └── AccountController.cs    # Controlador de autenticación
├── Models/
│   ├── Usuario.cs               # Modelo de usuario
│   └── LoginViewModel.cs        # ViewModel para login
├── Views/Account/
│   ├── Login.cshtml             # Pantalla de login
│   ├── Perfil.cshtml            # Perfil de usuario
│   └── Bloqueado.cshtml         # Cuenta bloqueada
├── Services/
│   └── EmailService.cs          # Servicio de email
├── Data/
│   └── AppDbContext.cs          # Contexto de EF Core
├── Scripts/
│   └── CreateDatabase.sql       # Script de base de datos
└── appsettings.json            # Configuración
```

## Notas

- La contraseña se almacena como hash SHA256
- El timezone configurado es Lima, Perú (SA Pacific Standard Time)
- La sesión expira después de 30 minutos de inactividad
- El modal de advertencia aparece 60 segundos antes de expirar
