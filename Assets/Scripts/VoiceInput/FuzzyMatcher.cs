using UnityEngine;

/// <summary>
/// Matching utilities for voice command recognition.
/// </summary>
public static class FuzzyMatcher
{
    /// <summary>
    /// Returns a 0–1 confidence score between a transcript and a candidate string.
    /// </summary>
    public static float MatchScore(string transcript, string candidate)
    {
        if (transcript == candidate) return 1.0f;
        if (transcript.Contains(candidate) ||
            candidate.Contains(transcript)) 
            return 0.9f;

        int distance = LevenshteinDistance(transcript, candidate);
        int maxLength = Mathf.Max(transcript.Length, candidate.Length);

        float score = (maxLength == 0) ? 1f : 1f - (distance / (float)maxLength);
        
        return score;
    }

    /// <summary>
    /// Computes the Levenshtein edit distance between two strings.
    /// Lower = more similar.
    /// </summary>
    private static int LevenshteinDistance(string a, string b)
    {
        int[,] d = new int[a.Length + 1, b.Length + 1];

        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;

        for (int i = 1; i <= a.Length; i++)
        for (int j = 1; j <= b.Length; j++)
        {
            int cost = a[i - 1] == b[j - 1] ? 0 : 1;
            d[i, j]  = Mathf.Min(
                d[i - 1, j] + 1,
                d[i, j - 1] + 1,
                d[i - 1, j - 1] + cost
            );
        }

        return d[a.Length, b.Length];
    }
}