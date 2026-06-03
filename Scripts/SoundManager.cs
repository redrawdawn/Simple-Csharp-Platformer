using Godot;
using System;
using System.IO;

// A tiny helper for playing short sound effects.
// Any script can call SoundManager.Play(this, "res://Audio/sound.wav").
public static class SoundManager
{
    public static void Play(Node source, string soundPath, float volumeDb = -6.0f)
    {
        AudioStream stream = LoadStream(soundPath);

        // If the file path is wrong, do nothing instead of crashing the game.
        if (stream == null || source.GetTree()?.CurrentScene == null)
        {
            return;
        }

        AudioStreamPlayer player = new AudioStreamPlayer();
        player.Stream = stream;
        player.VolumeDb = volumeDb;

        // Put the temporary sound player in the current level.
        source.GetTree().CurrentScene.AddChild(player);

        // Remove the sound player after the sound finishes.
        player.Finished += player.QueueFree;
        player.Play();
    }

    public static AudioStream LoadStream(string soundPath)
    {
        if (soundPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            AudioStreamWav plainWav = LoadPlainWav(soundPath);
            if (plainWav != null)
            {
                return plainWav;
            }
        }

        AudioStream stream = GD.Load<AudioStream>(soundPath);
        if (stream != null)
        {
            return stream;
        }

        return LoadPlainWav(soundPath);
    }

    private static AudioStreamWav LoadPlainWav(string soundPath)
    {
        string filePath = ProjectSettings.GlobalizePath(soundPath);
        if (!File.Exists(filePath))
        {
            return null;
        }

        byte[] fileBytes = File.ReadAllBytes(filePath);
        int fmtOffset = FindChunk(fileBytes, "fmt ");
        int dataOffset = FindChunk(fileBytes, "data");

        if (fmtOffset < 0 || dataOffset < 0)
        {
            return null;
        }

        ushort audioFormat = BitConverter.ToUInt16(fileBytes, fmtOffset + 8);
        ushort channels = BitConverter.ToUInt16(fileBytes, fmtOffset + 10);
        int sampleRate = BitConverter.ToInt32(fileBytes, fmtOffset + 12);
        ushort bitsPerSample = BitConverter.ToUInt16(fileBytes, fmtOffset + 22);
        int dataSize = BitConverter.ToInt32(fileBytes, dataOffset + 4);

        if (audioFormat != 1 || bitsPerSample != 16)
        {
            return null;
        }

        byte[] audioData = new byte[dataSize];
        Array.Copy(fileBytes, dataOffset + 8, audioData, 0, dataSize);

        AudioStreamWav wav = new AudioStreamWav();
        wav.Format = AudioStreamWav.FormatEnum.Format16Bits;
        wav.MixRate = sampleRate;
        wav.Stereo = channels == 2;
        wav.Data = audioData;
        return wav;
    }

    private static int FindChunk(byte[] bytes, string chunkName)
    {
        byte[] chunkBytes = System.Text.Encoding.ASCII.GetBytes(chunkName);

        for (int i = 12; i < bytes.Length - 8; i++)
        {
            if (bytes[i] == chunkBytes[0] &&
                bytes[i + 1] == chunkBytes[1] &&
                bytes[i + 2] == chunkBytes[2] &&
                bytes[i + 3] == chunkBytes[3])
            {
                return i;
            }
        }

        return -1;
    }
}
