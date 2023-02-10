namespace PlayAudioUsingMediaStreams.WebApi;

public class Sound
{
    public string Name { get; set; }
    public List<string> Keywords { get; set; }
    public byte[] AudioData { get; set; }
}