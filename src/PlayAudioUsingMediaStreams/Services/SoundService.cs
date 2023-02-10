namespace PlayAudioUsingMediaStreams.WebApi.Services;

public class SoundService
{
    private const string AudioRoot = "../../audio";
    private const int WavHeaderBytesToSkip = 58;
    
    private List<Sound> _sounds = new()
    {
        new() { Name = "dog", Keywords = new List<string> { "dog", "canine", "pooch", "hound" } },
        new() { Name = "cat", Keywords = new List<string> { "cat", "kitty", "kitten" } },
        new() { Name = "bird", Keywords = new List<string> { "bird" } },
        new() { Name = "elephant", Keywords = new List<string> { "elephant" } },
    };

    public SoundService()
    {
        // Load all files into memory once to avoid constant disk access
        foreach (var sound in _sounds)
        {
            var audioFilePath = $"{AudioRoot}/{sound.Name}.wav";
            var rawAudioData = File.ReadAllBytes(audioFilePath);
            
            // Skip the header bytes while copying
            sound.AudioData = new byte[rawAudioData.Length - WavHeaderBytesToSkip];
            Array.Copy(rawAudioData, WavHeaderBytesToSkip, sound.AudioData, 0, sound.AudioData.Length);
        }
    }

    public Sound FindSoundByKeyword(string keyword)
    {
        return _sounds.FirstOrDefault(s => s.Keywords.Contains(keyword));
    }
}