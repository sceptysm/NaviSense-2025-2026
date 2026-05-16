using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

/// <summary>
/// Sends WAV audio to Google Cloud Speech-to-Text and returns the transcript.
/// </summary>
public static class GoogleSpeechAPI
{
    [Serializable] private class SpeechResponse { public SpeechResult[] results; }
    [Serializable] private class SpeechResult { public SpeechAlternative[] alternatives; }
    [Serializable] private class SpeechAlternative { public string transcript; }

    /// <summary>
    /// Sends audio to Google Speech-to-Text.
    /// Call via StartCoroutine. The transcript is returned through the callback;
    /// empty string is returned on failure.
    /// </summary>
    public static IEnumerator Recognize(byte[] wav, int sampleRate, Action<string> onResult)
    {
        string body = $@"{{
            ""config"": {{
                ""encoding"":        ""LINEAR16"",
                ""sampleRateHertz"": {sampleRate},
                ""languageCode"":    ""en-US""
            }},
            ""audio"": {{
                ""content"": ""{Convert.ToBase64String(wav)}""
            }}
        }}";

        using var request = new UnityWebRequest(
            "https://speech.googleapis.com/v1/speech:recognize?key=" + ApiKeyConfig.GOOGLE_API_KEY,
            "POST"
        );
        request.uploadHandler   = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[GoogleSpeechAPI] " + request.error);
            onResult(string.Empty);
            yield break;
        }

        onResult(ParseTranscript(request.downloadHandler.text));
    }


    private static string ParseTranscript(string json)
    {
        try
        {
            var response = JsonUtility.FromJson<SpeechResponse>(json);
            return response?.results?[0]?.alternatives?[0]?.transcript ?? string.Empty;
        }
        catch (Exception e)
        {
            Debug.LogError("[GoogleSpeechAPI] JSON parse error: " + e.Message);
            return string.Empty;
        }
    }
}