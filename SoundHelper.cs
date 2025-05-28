using System;
using System.Runtime.InteropServices;

namespace TheAdventure;

public static class SoundHelper
{
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

    private const uint SND_FILENAME = 0x00020000;
    private const uint SND_ASYNC = 0x0001;

    public static void PlayWav(string filePath)
    {
        PlaySound(filePath, IntPtr.Zero, SND_FILENAME | SND_ASYNC);
    }
}
