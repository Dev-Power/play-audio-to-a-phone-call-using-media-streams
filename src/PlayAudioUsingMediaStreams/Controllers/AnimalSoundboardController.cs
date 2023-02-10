namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

[ApiController]
[Route("[controller]")]
public class AnimalSoundboardController : TwilioController
{
    [HttpPost]
    public TwiMLResult Index()
    {
        throw new NotImplementedException();
    }
}