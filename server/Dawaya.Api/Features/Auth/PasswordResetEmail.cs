using Dawaya.Api.Infrastructure.Email;

namespace Dawaya.Api.Features.Auth;

// Email text in the user's language. Arabic is Egyptian and plain (BR-17).
public static class PasswordResetEmail
{
    public static EmailMessage Create(string to, string language, string link) => language == "en"
        ? new EmailMessage(to, "Reset your Dawaya password",
            $"Someone asked to reset the password for your Dawaya account.\n\nTo choose a new password, open this link within 1 hour:\n{link}\n\nIf this wasn't you, you can ignore this email. Your password won't change.")
        : new EmailMessage(to, "تغيير كلمة السر في دوايا",
            $"حد طلب تغيير كلمة السر لحسابك في دوايا.\n\nعشان تختار كلمة سر جديدة، افتح اللينك ده خلال ساعة:\n{link}\n\nلو مش إنت اللي طلبت، تجاهل الرسالة دي، وكلمة السر مش هتتغير.");
}
