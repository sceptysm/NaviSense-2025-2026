using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Immersal.Samples.Navigation
{
    public class HapticNavigationFeedback : MonoBehaviour
    {
        [Header("Navigation")]
        public Transform arCamera;
        private List<Vector3> currentPath = new List<Vector3>();
        private bool hasPath = false;

        [Header("Haptic Settings")]
        
        [Tooltip("Angle within which no vibration occurs (dead zone)")]
        public float alignmentDeadZone = 10f;

        [Tooltip("Maximum angle for vibration calculation (180 = opposite direction)")]
        public float maxAngleForVibration = 180f;

        [Tooltip("Fastest vibration interval when facing completely wrong way (seconds)")]
        public float minVibrationInterval = 0.05f;

        [Tooltip("Slowest vibration interval when only slightly off alignment (seconds)")]
        public float maxVibrationInterval = 4.0f;

        [Tooltip("Duration of each vibration pulse (seconds)")]
        public float vibrationDuration = 0.05f;

        [Tooltip("Exponent for the interpolation curve. < 1 means frequency ramps up sharply " +
                 "even at small angles, making misalignment immediately noticeable.")]
        public float vibrationCurveExponent = 0.35f;

        [Header("Waypoint Settings")]
        [Tooltip("How close to current waypoint before switching to next (meters)")]
        public float waypointReachedDistance = 2.0f;

        private Coroutine vibrationCoroutine;
        private bool isNavigating = false;

        void Start()
        {
            if (arCamera == null)
                arCamera = Camera.main.transform;
        }

        public void StartNavigation()
        {
            // Idempotent — don't restart the coroutine if already running
            if (isNavigating) return;

            isNavigating = true;

            if (vibrationCoroutine != null)
                StopCoroutine(vibrationCoroutine);

            vibrationCoroutine = StartCoroutine(HapticFeedbackLoop());
            Debug.Log("[Haptic] Started path-following feedback");
        }

        public void StopNavigation()
        {
            isNavigating = false;
            hasPath = false;

            if (vibrationCoroutine != null)
            {
                StopCoroutine(vibrationCoroutine);
                vibrationCoroutine = null;
            }

            Debug.Log("[Haptic] Stopped path-following feedback");
        }

        /// <summary>
        /// Called by NavigationManager each time the path is recalculated,
        /// passing the full ordered list of NavMesh/graph corners.
        /// </summary>
        public void UpdatePath(List<Vector3> pathCorners)
        {
            currentPath = new List<Vector3>(pathCorners);
            hasPath = currentPath.Count >= 2;
            Debug.Log($"[Haptic] Path updated with {currentPath.Count} corners");
        }

        IEnumerator HapticFeedbackLoop()
        {
            while (isNavigating)
            {
                if (!hasPath || arCamera == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                float angle = CalculateAngleToNextPathWaypoint();

                // No vibration when pointing close enough to the correct direction
                if (angle <= alignmentDeadZone)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                // Map angle → [0, 1] then apply exponential curve
                // exponent < 1  →  curve bends upward, so even small deviations
                // produce a strong jump in frequency (very noticeable)
                float normalizedAngle = Mathf.Clamp01(
                    (angle - alignmentDeadZone) / (maxAngleForVibration - alignmentDeadZone));
                float curvedAngle = Mathf.Pow(normalizedAngle, vibrationCurveExponent);

                // Lerp interval: at curvedAngle=0 → maxInterval (slow), at 1 → minInterval (fast)
                float vibrationInterval = Mathf.Lerp(maxVibrationInterval, minVibrationInterval, curvedAngle);

#if !UNITY_STANDALONE
                Handheld.Vibrate();
#endif
                Debug.Log($"[Haptic] Angle: {angle:F1}° | Curved: {curvedAngle:F2} | Interval: {vibrationInterval:F2}s");

                yield return new WaitForSeconds(vibrationDuration);
                yield return new WaitForSeconds(Mathf.Max(0f, vibrationInterval - vibrationDuration));
            }
        }

        // ─── Path-following helpers ──────────────────────────────────────────────

        float CalculateAngleToNextPathWaypoint()
        {
            if (!hasPath || arCamera == null)
                return 0f;

            Vector3 playerPos = arCamera.position;
            int nextIndex = FindNextWaypointIndex(playerPos);
            Vector3 targetWaypoint = currentPath[nextIndex];

            // Flatten to horizontal plane so vertical tilt is ignored
            Vector3 toWaypoint = targetWaypoint - playerPos;
            toWaypoint.y = 0f;
            toWaypoint.Normalize();

            Vector3 cameraForward = arCamera.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            return Vector3.Angle(cameraForward, toWaypoint);
        }

        /// <summary>
        /// Finds the index of the next corner the player should walk toward.
        /// Projects the player onto each path segment and returns the end-vertex
        /// of the closest segment — i.e. the immediate next turn on the path.
        /// </summary>
        int FindNextWaypointIndex(Vector3 playerPos)
        {
            if (currentPath.Count == 1)
                return 0;

            int closestSegmentIndex = 0;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                float d = DistanceToLineSegment(playerPos, currentPath[i], currentPath[i + 1]);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closestSegmentIndex = i;
                }
            }

            // Target is the far end of the closest segment
            return Mathf.Min(closestSegmentIndex + 1, currentPath.Count - 1);
        }

        /// <summary>Returns the perpendicular distance from <paramref name="point"/>
        /// to the finite segment [lineStart, lineEnd].</summary>
        float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float lineLengthSq = line.sqrMagnitude;

            if (lineLengthSq < 0.001f)
                return Vector3.Distance(point, lineStart);

            float t = Mathf.Clamp01(Vector3.Dot(point - lineStart, line) / lineLengthSq);
            Vector3 closestPoint = lineStart + t * line;
            return Vector3.Distance(point, closestPoint);
        }

        // ─── Editor gizmos ───────────────────────────────────────────────────────

        void OnDrawGizmos()
        {
            if (!isNavigating || !hasPath || arCamera == null)
                return;

            // Full path in cyan
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);

            Vector3 playerPos = arCamera.position;
            int nextIndex = FindNextWaypointIndex(playerPos);
            Vector3 targetWaypoint = currentPath[nextIndex];

            Vector3 toWaypoint = targetWaypoint - playerPos;
            toWaypoint.y = 0f;
            toWaypoint.Normalize();

            Vector3 cameraForward = arCamera.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // Green = where the camera is pointing
            Gizmos.color = Color.green;
            Gizmos.DrawRay(arCamera.position, cameraForward * 2f);

            // Yellow sphere = next waypoint target
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetWaypoint, 0.3f);

            // Red = direction the user should be facing
            Gizmos.color = Color.red;
            Gizmos.DrawRay(arCamera.position, toWaypoint * 2f);
        }
    }
}
