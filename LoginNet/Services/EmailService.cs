using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LoginNet.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarNotificacionBloqueoAsync(string correo, string nombre, int minutosBloqueo)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["RemitenteNombre"] ?? "Sistema Login",
            emailSettings["Remitente"] ?? "noreply@ejemplo.com"
        ));
        message.To.Add(new MailboxAddress(nombre, correo));
        message.Subject = "Cuenta bloqueada temporalmente";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #8B1D1D; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>CEPLAN</h1>
                    </div>
                    <div style='padding: 30px; background-color: #f9f9f9;'>
                        <h2 style='color: #333;'>Estimado/a {nombre},</h2>
                        <p style='color: #555; font-size: 16px;'>
                            Su cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos de inicio de sesión.
                        </p>
                        <div style='background-color: #fff; border-left: 4px solid #D97706; padding: 15px; margin: 20px 0;'>
                            <p style='margin: 0; color: #333;'>
                                <strong>Motivo:</strong> Se excedió el número máximo de intentos de validación (5 intentos).
                            </p>
                            <p style='margin: 10px 0 0 0; color: #333;'>
                                <strong>Duración del bloqueo:</strong> {minutosBloqueo} minutos.
                            </p>
                        </div>
                        <p style='color: #555; font-size: 14px;'>
                            Si usted no realizó estos intentos, por favor comuníquese con el área de soporte inmediatamente.
                        </p>
                        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                        <p style='color: #888; font-size: 12px; text-align: center;'>
                            Este es un correo automático. No responda a este mensaje.
                        </p>
                    </div>
                </div>
            "
        };

        try
        {
            using var client = new SmtpClient();
            
            var host = emailSettings["SmtpHost"] ?? "smtp.ejemplo.com";
            var port = int.Parse(emailSettings["SmtpPort"] ?? "587");
            var username = emailSettings["Usuario"] ?? "";
            var password = emailSettings["Clave"] ?? "";

            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar email: {ex.Message}");
        }
    }
}
