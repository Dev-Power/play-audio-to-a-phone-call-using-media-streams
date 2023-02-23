using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1;
using Microsoft.AspNetCore.Mvc;
using PlayAudioUsingMediaStreams.WebApi.Services;

namespace PlayAudioUsingMediaStreams.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalSoundboardController : Controller
{
    private readonly SoundService _soundService;
    private readonly SpeechRecognitionService _speechRecognitionService;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public AnimalSoundboardController(
        SoundService soundService,
        SpeechRecognitionService speechRecognitionService,
        IHostApplicationLifetime applicationLifetime
    )
    {
        _soundService = soundService;
        _speechRecognitionService = speechRecognitionService;
        _applicationLifetime = applicationLifetime;
    }

    [HttpGet]
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
        string streamSid = null;
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        await using var speechRecognitionStream = await _speechRecognitionService.InitStream();
        while (!receiveResult.CloseStatus.HasValue &&
               !_applicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            using var jsonDocument = JsonDocument.Parse(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
            var eventMessage = jsonDocument.RootElement.GetProperty("event").GetString();

            switch (eventMessage)
            {
                case "connected":
                    Console.WriteLine("Event: connected");
                    break;
                case "start":
                    Console.WriteLine("Event: start");
                    streamSid = jsonDocument.RootElement.GetProperty("streamSid").GetString();
                    Console.WriteLine($"StreamId: {streamSid}");

                    // Do not await task, leave this task running in the background for the duration of the websocket connection
                    var _ = ListenForSpeechRecognition(webSocket, streamSid, speechRecognitionStream)
                        .ConfigureAwait(false);
                    break;
                case "media":
                    var payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString();
                    await _speechRecognitionService.SendAudio(payload);
                    break;
                case "stop":
                    Console.WriteLine("Event: stop");
                    break;
            }

            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        if (receiveResult.CloseStatus.HasValue)
        {
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        else if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.EndpointUnavailable,
                "Server shutting down",
                CancellationToken.None);
        }
    }

    private async Task ListenForSpeechRecognition(
        WebSocket webSocket,
        string streamSid,
        AsyncResponseStream<StreamingRecognizeResponse> speechRecognitionStream
    )
    {
        while (await speechRecognitionStream.MoveNextAsync())
        {
            var word = speechRecognitionStream.Current?.Results.FirstOrDefault()
                ?.Alternatives.FirstOrDefault()
                ?.Words.FirstOrDefault();
            if (word == null) continue;

            Console.WriteLine($"Word: [{word.Word}]. Confidence: {word.Confidence:N2}");
            if (word.Confidence < 0.5)
            {
                Console.WriteLine($"Low confidence. Skipping the word [{word.Word}]");
                continue;
            }

            var utterance = word.Word.Trim().ToLower();
            if (!_soundService.TryFindSoundByKeyword(utterance, out var soundToPlay))
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
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }
}