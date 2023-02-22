using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PlayAudioUsingMediaStreams.WebApi.Services;
using Twilio.AspNet.Core;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;
 
[ApiController]
[Route("[controller]")]
public class AnimalSoundboardController : TwilioController
{
    private readonly SoundService _soundService;
    private readonly SpeechRecognitionService _speechRecognitionService;

    public AnimalSoundboardController(SoundService soundService, SpeechRecognitionService speechRecognitionService)
    {
        _soundService = soundService;
        _speechRecognitionService = speechRecognitionService;
    }
    
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Soundboard(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    
    private async Task Soundboard(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        var streamSid = string.Empty;

        while (!receiveResult.CloseStatus.HasValue)
        {
            using (var jsonDocument = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count)))
            {
                string eventMessage = jsonDocument.RootElement.GetProperty("event").GetString();

                switch (eventMessage)
                {
                    case "connected":
                        var responseStream = await _speechRecognitionService.InitStream();
                        
                        Task.Run(async () =>
                        {
                            while (await responseStream.MoveNextAsync())
                            {
                                var currentResult = responseStream.Current.Results[0];
                                foreach (var word in currentResult?.Alternatives[0].Words)
                                {
                                    Console.WriteLine($"Word: [{word.Word}]. Confidence: {word.Confidence:N2}");
                                    if (word.Confidence < 0.5)
                                    {
                                        Console.WriteLine($"Low confidence. Skipping the word [{word.Word}]");
                                        continue;
                                    }
                                    
                                    var utterance = word.Word.Trim().ToLower();
                                    var soundToPlay = _soundService.FindSoundByKeyword(utterance);
                                    if (soundToPlay == null)
                                    {
                                        continue;
                                    }
                                    Console.WriteLine($"Animal detected: {soundToPlay.Name}");
                                    
                                    var mediaMessage = new
                                    {
                                        streamSid, 
                                        @event = "media", 
                                        media = new
                                        {
                                            payload = soundToPlay.AudioDataAsBase64
                                        }
                                    };
                                    
                                    var rawJson = JsonSerializer.Serialize(mediaMessage);
                                    var responseBuffer = Encoding.UTF8.GetBytes(rawJson);
                                    
                                    await webSocket.SendAsync(
                                        new ArraySegment<byte>(responseBuffer, 0, responseBuffer.Length),
                                        receiveResult.MessageType,
                                        receiveResult.EndOfMessage,
                                        CancellationToken.None);
                                }
                            }
                        });
                        break;
                    
                    case "start":
                        streamSid = jsonDocument.RootElement.GetProperty("streamSid").GetString();
                        Console.WriteLine($"StreamId: {streamSid}");
                        break;
                    
                    case "media":
                        string payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString();
                        await _speechRecognitionService.SendAudio(payload);
                        break;
                    
                    case "stop":
                        break;
                }
            }
            
            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}