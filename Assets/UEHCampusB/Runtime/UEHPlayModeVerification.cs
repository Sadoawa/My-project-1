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
            var route = new List<Vector3>(points);
            if (settings != null && settings.rightGlassOffice)
            {
                int officeIndex = route.FindIndex(point => point == new Vector3(-14.25f, 0, -37));
                route.InsertRange(officeIndex + 1, new[]
                {
                    new Vector3(-14.25f, 0, -35.15f), new Vector3(-18.5f, 0, -35.15f),
                    new Vector3(-18.5f, 0, -38.5f), new Vector3(-18.5f, 0, -35.15f),
                    new Vector3(-14.25f, 0, -35.15f), new Vector3(-14.25f, 0, -37)
                });
            }
            if (settings != null && settings.refinedLobbyLayout)
            {
                int liftIndex = route.FindIndex(point => point == new Vector3(-14.25f, 0, -23.4f));
                if (liftIndex >= 0)
                    route.InsertRange(liftIndex, new[]
                    {
                        new Vector3(-2.6f, 0, -19.5f), new Vector3(-2.6f, 0, -26), new Vector3(-14.25f, 0, -26)
                    });
                int academicIndex = route.FindIndex(point => point == new Vector3(0, 0, -44));
                route.InsertRange(academicIndex + 1, new[]
                {
                    new Vector3(-12, 0, -46), new Vector3(-12, 0, -52),
                    new Vector3(12, 0, -52), new Vector3(12, 0, -46), new Vector3(0, 0, -46)
                });
            }
            if (settings != null && settings.rightWingInset > 0)
                for (int i = 0; i < route.Count; i++)
                    if (route[i].x <= -14) route[i] += Vector3.right * settings.rightWingInset;
            if (settings != null && settings.facultyLiftEntrance)
            {
                int index = route.FindIndex(point => point == new Vector3(0, 0, -10));
                float facade = -settings.frontage / 2 + 0.6f + settings.rightWingInset;
                route.InsertRange(index + 1, settings.facultyLiftsBehindWelcome ? new[]
                {
                    new Vector3(-9.8f, 0, -5.8f), new Vector3(facade + 1.1f, 0, -4.65f),
                    new Vector3(facade - 2.05f, 0, -4.65f), new Vector3(facade - 2.05f, 0, -5.05f),
                    new Vector3(facade - 2.05f, 0, -4.65f), new Vector3(facade - 5.35f, 0, -4.65f),
                    new Vector3(facade - 5.35f, 0, -5.05f), new Vector3(facade - 5.35f, 0, -4.65f),
                    new Vector3(facade + 1.1f, 0, -4.65f), new Vector3(-9.8f, 0, -5.8f), new Vector3(0, 0, -10)
                } : new[]
                {
                    new Vector3(-9.8f, 0, -5.8f), new Vector3(facade + 1.1f, 0, -4.65f),
                    new Vector3(facade - 2, 0, -4.65f), new Vector3(facade + 1.1f, 0, -4.65f),
                    new Vector3(-9.8f, 0, -5.8f), new Vector3(0, 0, -10)
                });
            }
            if (settings != null && settings.compactLeftSequence)
                for (int i = 0; i < route.Count; i++)
                {
                    var point = route[i];
                    if (point.x < 0) continue;
                    if ((Mathf.Abs(point.x - 11.9f) < 0.01f || Mathf.Abs(point.x - 18.45f) < 0.01f) && Mathf.Abs(point.z + 18.8f) < 0.01f)
                        point.z += 2.9f;
                    else if (Mathf.Abs(point.x - 11.9f) < 0.01f && Mathf.Abs(point.z + 23) < 0.01f) point.z += 2.9f;
                    else if (Mathf.Abs(point.x - 11.9f) < 0.01f && Mathf.Abs(point.z + 37) < 0.01f) point.z += 6.05f;
                    route[i] = point;
                }
            if (settings != null && settings.entranceGlassThroughColumns)
            {
                int entryIndex = route.FindIndex(point => point == new Vector3(0, 0, 1));
                float sideCenter = (settings.frontage * 0.125f + settings.frontage / 2 - 0.3f + settings.lobbyColumnWidth / 2) / 2;
                route.InsertRange(entryIndex + 1, new[]
                {
                    new Vector3(sideCenter, 0, 1), new Vector3(sideCenter, 0, -5),
                    new Vector3(sideCenter, 0, 1), new Vector3(0, 0, 1)
                });
            }
            foreach (var target in route)
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
            float entryGlassZ = settings != null && settings.entranceGlassThroughColumns ? -(settings.lobbyColumnWidth + 0.24f) / 2 : 0;
            if (player.transform.position.z > entryGlassZ + 0.1f) report.failures.Add("Fixed glass panel did not stop CharacterController.");
            else report.checks.Add("Fixed glass stops the player while the central entrance remains walkable.");
            if (settings != null && settings.facultyLiftEntrance)
            {
                float facade = -settings.frontage / 2 + 0.6f + settings.rightWingInset;
                if (settings.facultyLiftsBehindWelcome)
                {
                    foreach (float depth in new[] { 2.05f, 5.35f })
                    {
                        player.Teleport(new Vector3(facade - depth, 0.08f, -4.6f), 180);
                        for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(0, -0.035f, -0.08f)); yield return null; }
                        if (player.transform.position.z < -5.6f) report.failures.Add("Left-return faculty elevator doors did not stop player at depth " + depth);
                        else report.checks.Add("Faculty lift at depth " + depth + "m faces left after entry and its closed doors block the player.");
                    }
                }
                else
                {
                    player.Teleport(new Vector3(facade - 1.8f, 0.08f, -4), 270);
                    for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(-0.08f, -0.035f, 0)); yield return null; }
                    if (player.transform.position.x < facade - 2.85f) report.failures.Add("Faculty elevator closed doors did not stop player.");
                    else report.checks.Add("Faculty elevator paired static doors stop the player after the clear pedestal aisle.");
                }
                player.Teleport(new Vector3(-8, 0.08f, -2), 0);
                for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(0, -0.035f, 0.08f)); yield return null; }
                if (player.transform.position.z > entryGlassZ + 0.1f) report.failures.Add("Rebuilt right entrance glass did not stop player.");
                else report.checks.Add("Rebuilt right fixed entrance glazing blocks the player.");
            }
            if (settings != null && settings.rightGlassOffice)
            {
                player.Teleport(new Vector3(-16.2f + settings.rightWingInset, 0.08f, -38.5f), 270);
                for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(-0.08f, -0.035f, 0)); yield return null; }
                if (player.transform.position.x < -17.2f + settings.rightWingInset) report.failures.Add("Glass office front pane did not stop player.");
                else report.checks.Add("Glass office front glazing blocks the player; open doorway remains walkable.");
                player.Teleport(new Vector3(-19.6f + settings.rightWingInset, 0.08f, -34.6f), 0);
                for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(0, -0.035f, 0.08f)); yield return null; }
                if (player.transform.position.z > -33.8f) report.failures.Add("Glass office right pane did not stop player.");
                else report.checks.Add("Glass office viewer-right glazing also blocks the player.");
            }
            if (settings != null && settings.refinedLobbyLayout)
            {
                float columnSideX = -settings.frontage * 0.125f - settings.lobbyColumnWidth / 2;
                float railX = (columnSideX - settings.frontage / 2 + 0.68f + settings.rightWingInset) / 2;
                float railZ = -21.25f;
                player.Teleport(new Vector3(railX, 0.08f, railZ + 1.3f), 180);
                for (int i = 0; i < 50; i++) { player.Body.Move(new Vector3(0, -0.035f, -0.08f)); yield return null; }
                if (player.transform.position.z < railZ - 0.15f)
                    report.failures.Add("Glass balustrade did not stop player.");
                else report.checks.Add("Long leg of L-shaped glass railing stops the CharacterController; the route around the wider column to lifts remains walkable.");
            }
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
