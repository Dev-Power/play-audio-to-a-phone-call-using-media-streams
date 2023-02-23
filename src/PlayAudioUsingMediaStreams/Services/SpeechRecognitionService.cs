using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1;
using Google.Protobuf;

namespace PlayAudioUsingMediaStreams.WebApi.Services;

public class SpeechRecognitionService
{
    private StreamingRecognitionConfig _streamingConfig = new()
    {
        Config = new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Mulaw,
            SampleRateHertz = 8000,
            LanguageCode = "en-US",
            EnableWordConfidence = true,
            UseEnhanced = true
        },
        InterimResults = true
    };

    private SpeechClient _speechClient;

    private SpeechClient.StreamingRecognizeStream _streamingRecognizeStream;

    public SpeechRecognitionService(SpeechClient speechClient)
    {
        _speechClient = speechClient;
    }
    
    public async Task<AsyncResponseStream<StreamingRecognizeResponse>> InitStream()
    {
        _streamingRecognizeStream = _speechClient.StreamingRecognize();
        await _streamingRecognizeStream.WriteAsync(new StreamingRecognizeRequest
        {
            StreamingConfig = _streamingConfig,
        });

        return _streamingRecognizeStream.GetResponseStream();
    }

    public async Task SendAudio(string payload)
    {
        await _streamingRecognizeStream.WriteAsync(new StreamingRecognizeRequest
        {
            AudioContent = ByteString.FromBase64(payload)
        });
    }
}