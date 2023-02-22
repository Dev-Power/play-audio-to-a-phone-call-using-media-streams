using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

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