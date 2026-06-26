// =============================================================================
// MOCK REFACTORING EXAM #07 — C# .NET
// Time limit: 60 minutes
// Task: Identify all code smells, then refactor the code below.
// Rules: preserve all existing behaviour, use .NET 8 idioms, no new NuGet packages.
// =============================================================================
//
// SMELLS TO FIND (fill in before you start coding):
//   1. _______________________________________________
//   2. _______________________________________________
//   3. _______________________________________________
//   4. _______________________________________________
//   5. _______________________________________________
//   6. _______________________________________________
//
// YOUR REFACTORED CODE GOES BELOW THE DASHED LINE AT THE BOTTOM.
// =============================================================================

namespace MessyNotify
{
    public class SmtpSender
    {
        private readonly string _host;
        private readonly int _port;

        public SmtpSender(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Send(string to, string msg)
        {
            Console.WriteLine($"[SMTP:{_host}:{_port}] Sending email to {to}: {msg}");
        }
    }

    public class SmsGateway
    {
        private readonly string _apiKey;

        public SmsGateway(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void Send(string to, string msg)
        {
            Console.WriteLine($"[SMS apiKey={_apiKey}] Sending SMS to {to}: {msg}");
        }
    }

    public class PushSender
    {
        private readonly string _endpoint;

        public PushSender(string endpoint)
        {
            _endpoint = endpoint;
        }

        public void Send(string to, string msg)
        {
            Console.WriteLine($"[Push endpoint={_endpoint}] Sending push to {to}: {msg}");
        }
    }

    public class NotificationDispatcher
    {
        public void Dispatch(string channel, string recipient, string message)
        {
            if (channel == "email")
            {
                Console.WriteLine($"[Notify] Sending {channel} to {recipient}");
                var sender = new SmtpSender("smtp.internal", 587);
                sender.Send(recipient, message);
            }
            else if (channel == "sms")
            {
                Console.WriteLine($"[Notify] Sending {channel} to {recipient}");
                var gateway = new SmsGateway("sms-api-key-abc");
                gateway.Send(recipient, message);
            }
            else if (channel == "push")
            {
                Console.WriteLine($"[Notify] Sending {channel} to {recipient}");
                var push = new PushSender("https://push.internal/send");
                push.Send(recipient, message);
            }
            else
            {
                throw new ArgumentException($"Unknown channel: {channel}");
            }
        }
    }
}

// =============================================================================
// YOUR REFACTORED CODE BELOW THIS LINE
// -----------------------------------------------------------------------------
