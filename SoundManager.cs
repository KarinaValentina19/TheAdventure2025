using NAudio.Wave;

namespace TheAdventure;

public static class SoundManager
{
    public static void PlaySound(string filePath)
    {
        using var audioFile = new AudioFileReader(filePath);
        using var outputDevice = new WaveOutEvent();
        outputDevice.Init(audioFile);
        outputDevice.Play();

        while (outputDevice.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(100);
        }
    }
}
