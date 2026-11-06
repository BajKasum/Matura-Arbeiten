using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Services;

public interface IEmailService
{
    bool Senden(Mail mail);
}