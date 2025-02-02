using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    public SmsService(string accountSid, string authToken, string twilioPhoneNumber)
    {
        _accountSid = accountSid;
        _authToken = authToken;
        _twilioPhoneNumber = twilioPhoneNumber;
    }

    public async Task SendSmsAsync(string toPhoneNumber, string message)
    {
        TwilioClient.Init(_accountSid, _authToken);

        var messageSent = await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(_twilioPhoneNumber),
            to: new PhoneNumber(toPhoneNumber)  
        );

        Console.WriteLine($"SMS Gönderildi: {messageSent.Sid}");
    }
}
