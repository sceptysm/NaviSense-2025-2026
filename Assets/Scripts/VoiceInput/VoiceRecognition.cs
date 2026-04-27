using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Collections.Generic;
using Immersal.Samples.Navigation;

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
#if UNITY_EDITOR
        // Only allow voice when navigation target menu is open
        if (IsTargetSelectionActive() && Input.GetKeyDown(KeyCode.V) && !isRecording)
        {
            StartRecording();
        }
#endif
    }

    // Call this from a UI Button (recommended for mobile)
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

        return NavigationManager.Instance.IsTargetsListOpen();
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

    // ---------------- RESPONSE MODELS ----------------

    [Serializable]
    public class GoogleResponseWrapper
    {
        public Result[] results;
    }

    [Serializable]
    public class Result
    {
        public Alternative[] alternatives;
    }

    [Serializable]
    public class Alternative
    {
        public string transcript;
    }

    // ---------------- RESPONSE HANDLING ----------------

    void HandleResponse(string json)
    {
        try
        {
            GoogleResponseWrapper response =
                JsonUtility.FromJson<GoogleResponseWrapper>(json);

            if (response == null ||
                response.results == null ||
                response.results.Length == 0)
            {
                Debug.Log("No speech recognized");
                return;
            }

            string transcript =
                response.results[0].alternatives[0].transcript;

            if (string.IsNullOrEmpty(transcript))
            {
                Debug.Log("Empty transcript");
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

    // ---------------- COMMAND HANDLING ----------------

    void HandleCommand(string command)
    {
        Debug.Log("Processing command: " + command);

        foreach (var category in NavigationTargets.NavigationTargetsDict)
        {
            foreach (GameObject target in category.Value)
            {
                IsNavigationTarget navTarget = target.GetComponent<IsNavigationTarget>();

                if (navTarget == null)
                    continue;

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