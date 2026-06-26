// =============================================================================
// MOCK EXAM #07 — ANSWER KEY
// Smells fixed:
//   1. Replace Conditional    → INotificationChannel interface; EmailChannel / SmsChannel / PushChannel
//   2. Tight coupling         → channels registered in Dictionary, injected into dispatcher
//   3. Magic strings          → NotificationChannel enum (Email, Sms, Push)
//   4. DRY violation          → single log line in Dispatch() before delegating
//   5. Hardcoded config       → EmailSettings / SmsSettings / PushSettings records injected
//   6. SRP violation          → each channel owns its own construction and send logic
// =============================================================================

namespace CleanNotify
{
    // -------------------------------------------------------------------------
    // Smell 3 fix: enum replaces "email" / "sms" / "push" magic strings
    // -------------------------------------------------------------------------
    public enum NotificationChannel
    {
        Email,
        Sms,
        Push
    }

    // -------------------------------------------------------------------------
    // Smell 1 fix: polymorphism replaces if/else chain
    // -------------------------------------------------------------------------
    public interface INotificationChannel
    {
        void Send(string recipient, string message);
    }

    // -------------------------------------------------------------------------
    // Smell 5 fix: config extracted into records, injected from outside
    // -------------------------------------------------------------------------
    public record EmailSettings(string Host, int Port);
    public record SmsSettings(string ApiKey);
    public record PushSettings(string Endpoint);

    // -------------------------------------------------------------------------
    // Smell 2 & 6 fix: each channel owns its own wiring and send logic;
    //                   no construction inside the dispatcher
    // -------------------------------------------------------------------------
    public class EmailChannel : INotificationChannel
    {
        private readonly EmailSettings _settings;

        public EmailChannel(EmailSettings settings)
        {
            _settings = settings;
        }

        public void Send(string recipient, string message)
        {
            // In production this would use SmtpClient or MailKit.
            Console.WriteLine($"[SMTP:{_settings.Host}:{_settings.Port}] Sending email to {recipient}: {message}");
        }
    }

    public class SmsChannel : INotificationChannel
    {
        private readonly SmsSettings _settings;

        public SmsChannel(SmsSettings settings)
        {
            _settings = settings;
        }

        public void Send(string recipient, string message)
        {
            Console.WriteLine($"[SMS apiKey={_settings.ApiKey}] Sending SMS to {recipient}: {message}");
        }
    }

    public class PushChannel : INotificationChannel
    {
        private readonly PushSettings _settings;

        public PushChannel(PushSettings settings)
        {
            _settings = settings;
        }

        public void Send(string recipient, string message)
        {
            Console.WriteLine($"[Push endpoint={_settings.Endpoint}] Sending push to {recipient}: {message}");
        }
    }

    // -------------------------------------------------------------------------
    // Smell 2 & 6 fix: dispatcher has no knowledge of concrete senders;
    // Smell 4 fix:      single log statement before delegating
    // -------------------------------------------------------------------------
    public class NotificationDispatcher
    {
        private readonly IReadOnlyDictionary<NotificationChannel, INotificationChannel> _channels;

        public NotificationDispatcher(
            IReadOnlyDictionary<NotificationChannel, INotificationChannel> channels)
        {
            _channels = channels;
        }

        public void Dispatch(NotificationChannel channel, string recipient, string message)
        {
            // Smell 4 fix: one log line, not three copies
            Console.WriteLine($"[Notify] Sending {channel} to {recipient}");

            if (!_channels.TryGetValue(channel, out var sender))
                throw new ArgumentException($"No channel registered for {channel}");

            sender.Send(recipient, message);
        }
    }

    // -------------------------------------------------------------------------
    // Composition root / usage example (not part of production code)
    // -------------------------------------------------------------------------
    internal static class Program
    {
        private static void Main()
        {
            var channels = new Dictionary<NotificationChannel, INotificationChannel>
            {
                [NotificationChannel.Email] = new EmailChannel(new EmailSettings("smtp.internal", 587)),
                [NotificationChannel.Sms]   = new SmsChannel(new SmsSettings("sms-api-key-abc")),
                [NotificationChannel.Push]  = new PushChannel(new PushSettings("https://push.internal/send"))
            };

            var dispatcher = new NotificationDispatcher(channels);
            dispatcher.Dispatch(NotificationChannel.Email, "user@example.com", "Hello via email");
            dispatcher.Dispatch(NotificationChannel.Sms,   "+1234567890",       "Hello via SMS");
            dispatcher.Dispatch(NotificationChannel.Push,  "device-token-xyz",  "Hello via push");
        }
    }
}
