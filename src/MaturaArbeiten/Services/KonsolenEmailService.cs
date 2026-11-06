using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Services;

public sealed class KonsolenEmailService(
    ILogger<KonsolenEmailService> logger,
    IHostEnvironment umgebung) : IEmailService
{
    public bool Senden(Mail mail)
    {
        if (!umgebung.IsDevelopment())
        {
            logger.LogWarning("Es ist noch kein E-Mail-Dienst eingerichtet. Eine E-Mail wurde nicht verschickt.");
            return false;
        }

        logger.LogInformation("E-Mail an {An}: {Betreff}\n{Text}", mail.An, mail.Betreff, mail.Text);
        return true;
    }
}