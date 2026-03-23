using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginNet.Data;
using LoginNet.Models;
using LoginNet.Services;
using System.Security.Cryptography;
using System.Text;

namespace LoginNet.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public AccountController(AppDbContext context, EmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    private const int MaxIntentosFallidos = 5;
    private const int MinutosBloqueo = 2;

    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetString("UsuarioId") != null)
        {
            return RedirectToAction("Perfil");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NumeroDocumento == model.Usuario && u.TipoDocumento == model.TipoDocumento);

        if (usuario == null)
        {
            ModelState.AddModelError(string.Empty, "El usuario no existe.");
            return View(model);
        }

        if (usuario.EstaBloqueado)
        {
            var tiempoRestante = (usuario.FechaBloqueo!.Value - DateTime.Now).Minutes;
            ModelState.AddModelError(string.Empty, $"Su cuenta está bloqueada. Intente nuevamente en {tiempoRestante} minutos.");
            return View(model);
        }

        if (!usuario.Activo)
        {
            ModelState.AddModelError(string.Empty, "La cuenta está desactivada. Contacte al área de soporte.");
            return View(model);
        }

        var claveCorrecta = VerificarClave(model.Contraseña, usuario.ClaveHash);
        
        if (!claveCorrecta)
        {
            usuario.CVF++;

            if (usuario.CVF >= MaxIntentosFallidos)
            {
                usuario.FechaBloqueo = DateTime.Now.AddMinutes(MinutosBloqueo);
                usuario.CVF = 0;
                await _emailService.EnviarNotificacionBloqueoAsync(usuario.Correo, usuario.Nombre, MinutosBloqueo);
                await _context.SaveChangesAsync();

                TempData["MensajeBloqueo"] = $"Su cuenta ha sido bloqueada temporalmente por {MinutosBloqueo} minutos debido a múltiples intentos fallidos.";
                return RedirectToAction("Bloqueado");
            }

            await _context.SaveChangesAsync();

            var intentosRestantes = MaxIntentosFallidos - usuario.CVF;
            ModelState.AddModelError(string.Empty, $"Credenciales inválidas. Le quedan {intentosRestantes} intento(s).");
            return View(model);
        }

        usuario.CVF = 0;
        usuario.FechaBloqueo = null;
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
        HttpContext.Session.SetString("UsuarioNombre", $"{usuario.Nombre} {usuario.Apellido}");
        HttpContext.Session.SetString("UsuarioDocumento", usuario.NumeroDocumento);
        HttpContext.Session.SetString("UsuarioCorreo", usuario.Correo);
        HttpContext.Session.SetString("UsuarioTipo", usuario.TipoDocumento);
        HttpContext.Session.SetString("SessionStart", DateTime.Now.ToString("o"));

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Perfil");
    }

    public IActionResult Bloqueado()
    {
        ViewBag.Mensaje = TempData["MensajeBloqueo"] ?? "Su cuenta ha sido bloqueada temporalmente.";
        return View();
    }

    public IActionResult Perfil()
    {
        var usuarioId = HttpContext.Session.GetString("UsuarioId");
        if (usuarioId == null)
        {
            return RedirectToAction("Login");
        }

        var usuario = new Usuario
        {
            Id = int.Parse(usuarioId),
            Nombre = HttpContext.Session.GetString("UsuarioNombre") ?? "",
            NumeroDocumento = HttpContext.Session.GetString("UsuarioDocumento") ?? "",
            Correo = HttpContext.Session.GetString("UsuarioCorreo") ?? "",
            TipoDocumento = HttpContext.Session.GetString("UsuarioTipo") ?? "DNI"
        };

        return View(usuario);
    }

    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        TempData["MensajeSesionExpirada"] = "Su sesión ha expirado debido a inactividad.";
        return RedirectToAction("Login");
    }

    public IActionResult VerificarSesion()
    {
        var sessionStart = HttpContext.Session.GetString("SessionStart");
        if (string.IsNullOrEmpty(sessionStart))
        {
            return Json(new { expirada = true });
        }

        var startTime = DateTime.Parse(sessionStart);
        var tiempoExpiracion = TimeSpan.FromMinutes(2);
        
        if (DateTime.Now - startTime > tiempoExpiracion)
        {
            HttpContext.Session.Clear();
            return Json(new { expirada = true });
        }

        var segundosRestantes = (int)(tiempoExpiracion - (DateTime.Now - startTime)).TotalSeconds;
        
        return Json(new { 
            expirada = false, 
            segundosRestantes = segundosRestantes,
            mostrarWarning = segundosRestantes <= 60
        });
    }

    [HttpPost]
    public IActionResult ExtenderSesion()
    {
        HttpContext.Session.SetString("SessionStart", DateTime.Now.ToString("o"));
        return Json(new { success = true });
    }

    public IActionResult Index()
    {
        return RedirectToAction("Login");
    }

    [HttpPost]
    [Route("api/registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Datos inválidos", errors = ModelState });
        }

        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u => u.NumeroDocumento == model.NumeroDocumento);

        if (usuarioExiste)
        {
            return BadRequest(new { success = false, message = "El usuario ya existe" });
        }

        var usuario = new Usuario
        {
            NumeroDocumento = model.NumeroDocumento,
            TipoDocumento = model.TipoDocumento,
            Nombre = model.Nombre,
            Apellido = model.Apellido,
            Correo = model.Correo,
            ClaveHash = HashClave(model.Contraseña),
            CVF = 0,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            message = "Usuario registrado correctamente",
            usuario = new {
                usuario.Id,
                usuario.NumeroDocumento,
                usuario.TipoDocumento,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Correo
            }
        });
    }

    public static string HashClave(string contraseña)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(contraseña));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerificarClave(string contraseña, string hash)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
        var hashBase64 = Convert.ToBase64String(hashedBytes);
        return hashBase64 == hash;
    }
}
