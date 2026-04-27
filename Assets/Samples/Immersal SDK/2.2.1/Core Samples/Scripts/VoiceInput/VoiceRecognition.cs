using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;
using Immersal.Samples.Navigation;
using Newtonsoft.Json.Linq;

public class VoiceRecognition : MonoBehaviour
{
    [Header("Google API Key")]
    public string apiKey = ApiKeyConfig.GOOGLE_API_KEY;

    [Header("Recording Settings")]
    public int recordingLength = 5;
    public int frequency = 16000;

    private AudioClip clip;
    private bool isRecording = false;
    private string micDevice;

    void Start()
    {
        // Select microphone safely
        if (Microphone.devices.Length > 0)
        {
            micDevice = Microphone.devices[0];
            Debug.Log("Mic found: " + micDevice);
        }
        else
        {
            Debug.LogError("No microphone detected!");
        }
    }

    void Update()
    {
        // Only allow voice when target selection UI is open
        if (!IsTargetSelectionActive())
            return;

#if UNITY_EDITOR
        // Keyboard only for testing in editor
        if (Input.GetKeyDown(KeyCode.V) && !isRecording)
        {
            StartRecording();
        }
#endif
    }

    // Call this from a UI button
    public void StartVoiceInput()
    {
        if (!IsTargetSelectionActive())
        {
            Debug.Log("Voice disabled: Target menu not open");
            return;
        }

        if (!isRecording)
        {
            StartRecording();
        }
    }

    bool IsTargetSelectionActive()
    {
        if (NavigationManager.Instance == null)
            return false;

        return NavigationManager.Instance.TargetsListIsOpen();
    }

    void StartRecording()
    {
        if (micDevice == null)
        {
            Debug.LogError("No microphone available");
            return;
        }

        Debug.Log("Recording started.");
        clip = Microphone.Start(micDevice, false, recordingLength, frequency);
        isRecording = true;

        Invoke(nameof(StopRecording), recordingLength);
    }

    void StopRecording()
    {
        Microphone.End(micDevice);
        isRecording = false;

        Debug.Log("Processing voice.");

        byte[] wavData = WavUtility.FromAudioClip(clip);
        StartCoroutine(SendToGoogle(wavData));
    }

    IEnumerator SendToGoogle(byte[] audioData)
    {
        string base64Audio = Convert.ToBase64String(audioData);

        string json = @"{
            ""config"": {
                ""encoding"": ""LINEAR16"",
                ""sampleRateHertz"": 16000,
                ""languageCode"": ""en-US""
            },
            ""audio"": {
                ""content"": """ + base64Audio + @"""
            }
        }";

        UnityWebRequest request = new UnityWebRequest(
            "https://speech.googleapis.com/v1/speech:recognize?key=" + apiKey,
            "POST"
        );

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Google API Error: " + request.error);
        }
        else
        {
            HandleResponse(request.downloadHandler.text);
        }
    }

    void HandleResponse(string json)
    {
        try
        {
            var parsed = JObject.Parse(json);

            string transcript =
                parsed["results"]?[0]?["alternatives"]?[0]?["transcript"]?.ToString();

            if (string.IsNullOrEmpty(transcript))
            {
                Debug.Log("No speech recognized");
                return;
            }

            Debug.Log("You said: " + transcript);

            HandleCommand(transcript.ToLower());
        }
        catch (Exception e)
        {
            Debug.LogError("JSON parse error: " + e.Message);
        }
    }

    void HandleCommand(string command)
    {
        Debug.Log("Processing command: " + command);

        foreach (var category in NavigationTargets.NavigationTargetsDict)
        {
            foreach (GameObject target in category.Value)
            {
                IsNavigationTarget navTarget = target.GetComponent<IsNavigationTarget>();

                if (navTarget == null) continue;

                string targetName = navTarget.targetName.ToLower();

                if (command.Contains(targetName))
                {
                    Debug.Log("Matched target: " + navTarget.targetName);
                    StartNavigation(navTarget);
                    return;
                }
            }
        }

        Debug.LogWarning("No matching destination found");
    }

    void StartNavigation(IsNavigationTarget navTarget)
    {
        Debug.Log("Navigating to: " + navTarget.targetName);

        NavigationManager.Instance.InitializeNavigationDirect(navTarget);
    }
}