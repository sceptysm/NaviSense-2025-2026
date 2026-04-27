using System;
using System.IO;
using UnityEngine;

public static class WavUtility
{
    const int HEADER_SIZE = 44;

    public static byte[] FromAudioClip(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            int samples = clip.samples * clip.channels;
            float[] data = new float[samples];
            clip.GetData(data, 0);

            byte[] bytesData = ConvertToPCM16(data);

            WriteHeader(stream, clip, bytesData.Length);

            stream.Write(bytesData, 0, bytesData.Length);

            return stream.ToArray();
        }
    }

    private static byte[] ConvertToPCM16(float[] samples)
    {
        byte[] pcm = new byte[samples.Length * 2];

        int index = 0;
        foreach (float sample in samples)
        {
            short value = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
            byte[] bytes = BitConverter.GetBytes(value);

            pcm[index++] = bytes[0];
            pcm[index++] = bytes[1];
        }

        return pcm;
    }

    private static void WriteHeader(Stream stream, AudioClip clip, int dataLength)
    {
        int hz = clip.frequency;
        int channels = clip.channels;

        stream.Seek(0, SeekOrigin.Begin);

        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        stream.Write(riff, 0, 4);

        byte[] chunkSize = BitConverter.GetBytes(dataLength + 36);
        stream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        stream.Write(wave, 0, 4);

        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        stream.Write(fmt, 0, 4);

        byte[] subChunk1 = BitConverter.GetBytes(16);
        stream.Write(subChunk1, 0, 4);

        ushort audioFormat = 1;
        stream.Write(BitConverter.GetBytes(audioFormat), 0, 2);

        ushort numChannels = (ushort)channels;
        stream.Write(BitConverter.GetBytes(numChannels), 0, 2);

        stream.Write(BitConverter.GetBytes(hz), 0, 4);

        int byteRate = hz * channels * 2;
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);

        ushort blockAlign = (ushort)(channels * 2);
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        ushort bitsPerSample = 16;
        stream.Write(BitConverter.GetBytes(bitsPerSample), 0, 2);

        byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        stream.Write(dataString, 0, 4);

        stream.Write(BitConverter.GetBytes(dataLength), 0, 4);
    }
}