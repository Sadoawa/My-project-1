#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UEH.CampusB
{
    /// <summary>Created only by the batch verifier; never saved into the delivered scene.</summary>
    public sealed class UEHPlayModeVerification : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public string unityVersion;
            public string scene;
            public string verificationScope;
            public bool passed;
            public int rendererCount, materialCount, colliderCount, triangleCount;
            public float simulatedRouteMetres;
            public List<string> checks = new List<string>();
            public List<string> failures = new List<string>();
        }

        public void Begin(string reportPath, Report report)
        {
            StartCoroutine(Verify(reportPath, report));
        }

        private IEnumerator Verify(string reportPath, Report report)
        {
            var player = FindFirstObjectByType<UEHWalkthroughController>();
            player.externalControl = true;
            player.Teleport(new Vector3(0, 0.08f, 13), 180);
            yield return null;
            var settings = UnityEditor.AssetDatabase.LoadAssetAtPath<UEHDimensions>("Assets/UEHCampusB/Settings/EstimatedDimensions.asset");
            bool expanded = settings != null && settings.expandedLobby;
            bool leftFurnished = settings != null && settings.leftOfficeAndLounge;
            bool rightFurnished = settings != null && settings.rightLiftsAndServices;
            var points = expanded && leftFurnished && rightFurnished ? new[]
            {
                new Vector3(0, 0, 1), new Vector3(0, 0, -10),
                new Vector3(11.9f, 0, -18.8f), new Vector3(18.45f, 0, -18.8f),
                new Vector3(11.9f, 0, -18.8f), new Vector3(11.9f, 0, -23),
                new Vector3(-14.25f, 0, -19.5f), new Vector3(-19.8f, 0, -19.5f),
                new Vector3(-19.8f, 0, -20.3f), new Vector3(-19.8f, 0, -18.7f),
                new Vector3(-19.8f, 0, -19.5f), new Vector3(-23.2f, 0, -19.5f),
                new Vector3(-23.2f, 0, -20.3f), new Vector3(-23.2f, 0, -18.7f),
                new Vector3(-23.2f, 0, -19.5f), new Vector3(-25, 0, -19.5f),
                new Vector3(-14.25f, 0, -19.5f), new Vector3(-14.25f, 0, -23.4f),
                new Vector3(-14.25f, 0, -27), new Vector3(-14.25f, 0, -30.6f),
                new Vector3(-14.25f, 0, -37), new Vector3(11.9f, 0, -37),
                new Vector3(0, 0, -44), new Vector3(0, 0, -settings.lobbyDepth - 4), new Vector3(0, 0, 13)
            } : expanded && leftFurnished ? new[]
            {
                new Vector3(0, 0, 1), new Vector3(0, 0, -10),
                new Vector3(11.9f, 0, -18.8f), new Vector3(18.45f, 0, -18.8f),
                new Vector3(11.9f, 0, -18.8f), new Vector3(11.9f, 0, -23),
                new Vector3(-14.25f, 0, -23), new Vector3(-14.25f, 0, -37), new Vector3(11.9f, 0, -37),
                new Vector3(0, 0, -44), new Vector3(0, 0, -settings.lobbyDepth - 4), new Vector3(0, 0, 13)
            } : expanded ? new[]
            {
                new Vector3(0, 0, 1), new Vector3(0, 0, -10),
                new Vector3(14.25f, 0, -23), new Vector3(-14.25f, 0, -23),
                new Vector3(-14.25f, 0, -37), new Vector3(14.25f, 0, -37),
                new Vector3(0, 0, -44), new Vector3(0, 0, -settings.lobbyDepth - 4), new Vector3(0, 0, 13)
            } : new[]
            {
                new Vector3(0, 0, 1), new Vector3(0, 0, -10),
                new Vector3(-10, 0, -17), new Vector3(10, 0, -17),
                new Vector3(0, 0, -17), new Vector3(0, 0, -28), new Vector3(0, 0, 13)
            };
            foreach (var target in points)
            {
                int step = 0;
                while (HorizontalDistance(player.transform.position, target) > 0.10f && step++ < 1500)
                {
                    Vector3 delta = target - player.transform.position;
                    delta.y = 0;
                    Vector3 movement = Vector3.ClampMagnitude(delta, player.walkSpeed / 60);
                    float before = HorizontalDistance(player.transform.position, target);
                    player.Body.Move(movement + Vector3.down * 0.035f);
                    report.simulatedRouteMetres += Mathf.Max(0, before - HorizontalDistance(player.transform.position, target));
                    if (player.transform.position.y < -0.3f || player.transform.position.y > 0.4f)
                    { report.failures.Add("Ground continuity failed at " + player.transform.position); break; }
                    yield return null;
                }
                if (HorizontalDistance(player.transform.position, target) > 0.15f)
                    report.failures.Add("Walk could not reach " + target + "; stopped at " + player.transform.position);
                else report.checks.Add("Walk reached " + target + " at 4.8m/s simulated in 1/60s steps.");
            }
            player.Teleport(new Vector3(16.5f, 0.08f, -8), 90);
            for (int i = 0; i < 80; i++) { player.Body.Move(new Vector3(0.08f, -0.035f, 0)); yield return null; }
            if (player.transform.position.x > 17.6f) report.failures.Add("Side wall did not stop CharacterController.");
            else report.checks.Add("Solid side wall stops the 0.35m radius CharacterController.");
            player.Teleport(new Vector3(8, 0.08f, -2), 0);
            for (int i = 0; i < 80; i++) { player.Body.Move(new Vector3(0, -0.035f, 0.08f)); yield return null; }
            if (player.transform.position.z > 0.1f) report.failures.Add("Fixed glass panel did not stop CharacterController.");
            else report.checks.Add("Fixed glass stops the player while the central entrance remains walkable.");
            player.Teleport(player.entranceSpawn, 180);
            report.passed = report.failures.Count == 0;
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
            Debug.Log("UEH_PLAYMODE_VERIFIED passed=" + report.passed + " route=" + report.simulatedRouteMetres);
            UnityEditor.EditorApplication.isPlaying = false;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0;
            return Vector3.Distance(a, b);
        }
    }
}
#endif
