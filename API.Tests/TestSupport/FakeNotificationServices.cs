using LovEat.API.Services;

namespace LovEat.API.Tests.TestSupport
{
    public class FakeSmsService : ISmsService
    {
        public List<(string PhoneNumber, string Message)> SentMessages { get; } = new();
        public bool IsConfigured => true;

        public Task<(bool Success, string? ErrorMessage)> SendAsync(string phoneNumber, string message)
        {
            SentMessages.Add((phoneNumber, message));
            return Task.FromResult<(bool, string?)>((true, null));
        }
    }

    public class FakePushNotificationService : IPushNotificationService
    {
        public List<(string DeviceToken, string Title, string Body)> SentNotifications { get; } = new();
        public bool IsConfigured => true;

        public Task<(bool Success, string? ErrorMessage)> SendAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
        {
            SentNotifications.Add((deviceToken, title, body));
            return Task.FromResult<(bool, string?)>((true, null));
        }
    }
}
