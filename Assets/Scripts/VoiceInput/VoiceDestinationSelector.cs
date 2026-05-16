using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using Immersal.Samples.Navigation;

/// <summary>
/// Plays an audio prompt, records the user's spoken destination,
/// and initiates navigation to the matched target.
///
/// Attach to any persistent GameObject in the navigation scene.
/// Wire the public methods to a UI button.
/// </summary>
public class VoiceDestinationSelector : MonoBehaviour
{
    // Inspector ===============

    [Header("Prompt")]
    [Tooltip("Clip played before recording starts (e.g. 'What is your destination?')")]
    [SerializeField] private AudioClip promptClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Recording")]
    [SerializeField] private int recordingSeconds = 5;
    [SerializeField] private int sampleRate= 16000;

    [Header("Matching")]
    [Tooltip("0 = accept any partial match, 1 = exact only. 0.6 is a sensible default.")]
    [SerializeField, Range(0f, 1f)] private float confidenceThreshold = 0.6f;

    // States ===============

    private bool isRecording;
    private string micDevice;

    // Events ===============

    
    public event Action<string> OnDestinationSelected;
    public event Action OnNoMatchFound;


    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.LogError("[VoiceDestination] Microphone permission denied.");
            yield break;
        }

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[VoiceDestination] No microphone found.");
            yield break;
        }

        micDevice = Microphone.devices[0];
    }


    /// <summary>
    /// Call this from your voice button's OnClick event.
    /// Plays the prompt then starts recording.
    /// </summary>
    public void BeginVoiceSelection()
    {
        if (isRecording) return;
        StartCoroutine(PromptThenRecord());
    }


    private IEnumerator PromptThenRecord()
    {
        // Play the prompt clip and wait for it to finish
        if (promptClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(promptClip);
            yield return new WaitForSeconds(promptClip.length);
        }

        // Short pause so the prompt echo doesn't bleed into the recording
        yield return new WaitForSeconds(0.25f);

        yield return Record();
    }

    private IEnumerator Record()
    {
        if (string.IsNullOrEmpty(micDevice))
        {
            Debug.LogError("[VoiceDestination] No microphone available.");
            yield break;
        }

        isRecording = true;
        AudioClip clip = Microphone.Start(micDevice, false, recordingSeconds, sampleRate);

        Debug.Log("[VoiceDestination] Recording...");
        yield return new WaitForSeconds(recordingSeconds);

        Microphone.End(micDevice);
        isRecording = false;

        string transcript = string.Empty;
        yield return GoogleSpeechAPI.Recognize(
            WavUtility.FromAudioClip(clip),
            sampleRate,
            result => transcript = result
        );
 
        if (string.IsNullOrEmpty(transcript))
        {
            Debug.LogWarning("[VoiceDestination] Nothing recognised.");
            OnNoMatchFound?.Invoke();
            yield break;
        }
 
        Debug.Log($"[VoiceDestination] Heard: \"{transcript}\"");
        TryNavigateTo(transcript.ToLower());
    }

    // Destination Matching ===============

        private void TryNavigateTo(string transcript)
    {
        IsNavigationTarget bestMatch = null;
        float bestScore = 0f;
 
        foreach (var target in FindObjectsOfType<IsNavigationTarget>())
        {
            if (string.IsNullOrEmpty(target.targetName)) continue;
 
            float score = FuzzyMatcher.MatchScore(transcript, target.targetName.ToLower());
 
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = target;
            }
        }
 
        if (bestMatch != null && bestScore >= confidenceThreshold)
        {
            Debug.Log($"[VoiceDestination] Matched '{bestMatch.targetName}' (score {bestScore:P})");
            NavigationManager.Instance.InitializeNavigationDirect(bestMatch);
            OnDestinationSelected?.Invoke(bestMatch.targetName);
        }
        else
        {
            Debug.LogWarning($"[VoiceDestination] No match above threshold for \"{transcript}\"");
            OnNoMatchFound?.Invoke();
        }
    }
}