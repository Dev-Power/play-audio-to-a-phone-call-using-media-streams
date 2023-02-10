using System.Reflection;

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
        response.Say("If you can hear this, your setup works!");
        return TwiML(response);
    }
}