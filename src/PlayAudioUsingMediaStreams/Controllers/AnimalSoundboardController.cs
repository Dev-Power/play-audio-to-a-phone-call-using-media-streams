using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalSoundboardController : Controller
{
    [HttpPost]
    public TwiMLResult Index()
    {
        throw new NotImplementedException();
    }
}