using Twilio.TwiML.Voice;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

[ApiController]
[Route("[controller]")]
public class IncomingCallController : TwilioController
{
    [HttpPost]
    public TwiMLResult Index()
    {
        var response = new VoiceResponse();
        response.Say("Say animal names to hear their sounds.");
        
        var ngrokUrl = Environment.GetEnvironmentVariable("NGROK_URL_WITHOUT_PROTOCOL");
        var connect = new Connect();
        connect.Stream(name: "Animal Soundboard", url: $"wss://{ngrokUrl}/animalsoundboard");
        response.Append(connect);
        
        Console.WriteLine(response.ToString());
        return TwiML(response);
    }
}