using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;
using Twilio.TwiML.Voice;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class IncomingCallController : TwilioController
{
    [HttpPost]
    public TwiMLResult Index()
    {
        var response = new VoiceResponse();
        response.Say("Say animal names to hear their sounds.");

        var connect = new Connect();
        connect.Stream(
            name: "Animal Soundboard", 
            url: Url.Action(
                action: "Get", 
                controller: "AnimalSoundboard",
                values: null,
                protocol: "wss"
            )
        );
        response.Append(connect);

        Console.WriteLine(response.ToString());
        return TwiML(response);
    }
}