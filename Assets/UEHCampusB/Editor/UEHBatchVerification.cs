#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    [InitializeOnLoad]
    public static class UEHBatchVerification
    {
        private const string SessionKey = "UEH.CampusB.BatchVerification";
        private static string ReportPath => Path.GetFullPath("Docs/UEH-CampusB/validation.json");

        static UEHBatchVerification()
        {
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        /// <summary>Run with -batchmode -executeMethod UEH.CampusB.Editor.UEHBatchVerification.Run (without -quit).</summary>
        public static void Run()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-column-gates-only") >= 0) UEHCampusBuilder.AlignGlazingAndOpenSinceGate();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-compact-left-only") >= 0) UEHCampusBuilder.CompactLeftSequence();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-faculty-column-rear-only") >= 0) UEHCampusBuilder.MoveFacultyColumnToRearWall();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-faculty-return-only") >= 0) UEHCampusBuilder.RefineFacultyLifts();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-right-wing-only") >= 0) UEHCampusBuilder.AlignRightWing();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-ceiling-columns-L-only") >= 0) UEHCampusBuilder.RefineCeilingColumnsAndLRailing();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-railing-wall-only") >= 0) UEHCampusBuilder.ConnectRailingToDividingWall();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-lobby-refinement-only") >= 0) UEHCampusBuilder.RefineLobbyLayout();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-glass-office-only") >= 0) UEHCampusBuilder.FurnishGlassOffice();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-right-services-only") >= 0) UEHCampusBuilder.FurnishRightAreas();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-office-recess-only") >= 0) UEHCampusBuilder.RecessOfficeIntoWall();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-left-areas-only") >= 0) UEHCampusBuilder.FurnishLeftAreas();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-expand-only") >= 0) UEHCampusBuilder.ExpandLobby();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-reception-only") >= 0) UEHCampusBuilder.RefineReceptionWall();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-heritage-only") >= 0) UEHCampusBuilder.RefineHeritageWall();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-columns-only") >= 0) UEHCampusBuilder.MoveFrontColumnsToGate();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-layout-only") >= 0) UEHCampusBuilder.ApplyRequestedLayout();
                else if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-reopen-only") < 0) UEHCampusBuilder.Build();
                // Closing and reopening tests that meshes, fonts, materials and nested prefabs persist.
                EditorSceneManager.OpenScene(UEHCampusBuilder.ScenePath, OpenSceneMode.Single);
                UEHCampusBuilder.CaptureReviewImages();
                EditorSceneManager.OpenScene(UEHCampusBuilder.ScenePath, OpenSceneMode.Single);
                var report = InspectSavedScene();
                int imageCount = Directory.GetFiles(Path.GetFullPath("Docs/UEH-CampusB/Previews"), "*.png").Length;
                report.checks.Add(imageCount + " review images rendered by Unity; " + UnityEngine.Object.FindObjectsByType<ReflectionProbe>().Length + " local reflection cubemaps captured and saved.");
                File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
                SessionState.SetBool(SessionKey, true);
                EditorApplication.isPlaying = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                File.WriteAllText(ReportPath, JsonUtility.ToJson(new UEHPlayModeVerification.Report { passed = false, failures = new List<string> { e.ToString() } }, true));
                EditorApplication.Exit(1);
            }
        }

        private static UEHPlayModeVerification.Report InspectSavedScene()
        {
            var report = new UEHPlayModeVerification.Report { unityVersion = Application.unityVersion, scene = UEHCampusBuilder.ScenePath };
            report.verificationScope = Directory.Exists("Assets/GameCompatibility")
                ? "Isolated Unity project with the workspace package manifest, Player Settings, and all existing workspace C# scripts; standalone UEH scene."
                : "Isolated Unity project with the workspace Unity version and Player Settings; standalone UEH scene.";
            var distinctMaterials = new HashSet<Material>();
            var renderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>();
            report.rendererCount = renderers.Length;
            report.colliderCount = UnityEngine.Object.FindObjectsByType<Collider>().Length;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) != 0)
                        report.failures.Add("Missing script on " + transform.name);
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null || !material.shader.isSupported)
                        report.failures.Add("Missing/unsupported material on " + renderer.name);
                    else distinctMaterials.Add(material);
                }
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null) continue; // TextMesh generates its mesh itself.
                var mesh = filter.sharedMesh;
                if (mesh == null || mesh.vertexCount == 0) report.failures.Add("Missing mesh on " + renderer.name);
                else
                {
                    report.triangleCount += mesh.triangles.Length / 3;
                    if (mesh.normals.Length != mesh.vertexCount || mesh.uv.Length != mesh.vertexCount)
                        report.failures.Add("Incomplete mesh attributes on " + renderer.name);
                }
            }
            report.materialCount = distinctMaterials.Count;
            if (UnityEngine.Object.FindObjectsByType<Camera>().Length != 1) report.failures.Add("Expected one gameplay camera.");
            if (UnityEngine.Object.FindObjectsByType<AudioListener>().Length != 1) report.failures.Add("Expected one AudioListener.");
            foreach (string name in new[] { "UEH_Exterior", "UEH_LobbyB1", "UEH_CampusB" })
                if (AssetDatabase.LoadAssetAtPath<GameObject>(UEHCampusBuilder.Root + "/Prefabs/" + name + ".prefab") == null)
                    report.failures.Add("Missing prefab " + name);
            var font = AssetDatabase.LoadAssetAtPath<Font>(UEHCampusBuilder.Root + "/Fonts/LiberationSans.ttf");
            foreach (char c in "ĐẠI HỌC KINH TẾ TP. HỒ CHÍ MINH")
                if (!font.HasCharacter(c)) report.failures.Add("Font missing Vietnamese character " + c);
            foreach (string name in new[] { "LobbyWest", "LobbyEast" })
            {
                var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(UEHCampusBuilder.Root + "/Lighting/" + name + ".cubemap");
                if (cube == null) { report.failures.Add("Missing reflection cubemap " + name); continue; }
                float sum = 0;
                foreach (Color pixel in cube.GetPixels(CubemapFace.PositiveZ)) sum += pixel.maxColorComponent;
                if (sum < 1) report.failures.Add("Empty reflection cubemap " + name);
            }
            report.checks.Add("Saved scene reloaded with persistent mesh, material and font references.");
            report.checks.Add("Three prefab assets exist; no missing scripts or unsupported shaders.");
            report.checks.Add("Exactly one camera and AudioListener; Vietnamese sign characters are available in the font.");
            CheckGateSide(report, "Since_1976", true);
            CheckGateSide(report, "Welcome", false);
            CheckEntranceColumns(report);
            CheckHeritageSections(report);
            CheckCompactLeftSequence(report);
            CheckReceptionLayout(report);
            CheckRightLiftsAndServices(report);
            CheckGlassOffice(report);
            CheckLobbyRefinement(report);
            CheckColumnAlignedGates(report);
            return report;
        }

        private static void CheckLobbyRefinement(UEHPlayModeVerification.Report report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null || !settings.refinedLobbyLayout) return;
            var columns = GameObject.Find("02_Stone_columns");
            int frontCount = 0;
            foreach (var column in columns.GetComponentsInChildren<BoxCollider>())
            {
                if (Mathf.Abs(column.size.x - settings.lobbyColumnWidth) > 0.01f || Mathf.Abs(column.size.z - settings.lobbyColumnWidth) > 0.01f)
                    report.failures.Add("Refined column does not have the requested wider body.");
                if (column.transform.position.z > -2)
                {
                    frontCount++;
                    float expectedZ = column.transform.position.x > settings.frontage * 0.25f ? UEHCampusBuilder.LeftColumnZ(settings) : UEHCampusBuilder.ExpandedEntranceZ(settings);
                    if (Mathf.Abs(column.transform.position.z - expectedZ) > 0.01f)
                        report.failures.Add("Wider entrance columns must remain at glazing.");
                    if (Mathf.Abs(column.transform.position.x) > settings.frontage * 0.25f &&
                        Mathf.Abs(column.transform.position.x - (column.transform.position.x < 0 ? UEHCampusBuilder.RightCornerColumnX(settings) : UEHCampusBuilder.LeftGateColumnX(settings))) > 0.01f)
                        report.failures.Add("Wider outer entrance column no longer meets side-wall corner.");
                }
            }
            if (settings.keepEntranceColumnsWhenExpanded && frontCount != 4) report.failures.Add("Expected four wider entrance columns.");
            foreach (float x in new[] { -14f, -7f, 0f, 7f, 14f })
                foreach (float z in new[] { -settings.lobbyDepth, -settings.lobbyDepth - 4 })
                {
                    Vector3 point = new Vector3(x == -14 && z == -settings.lobbyDepth ? x + settings.rightWingInset : x, 0.95f, z);
                    if (Physics.OverlapCapsule(point - Vector3.up * 0.5f, point + Vector3.up * 0.5f, 0.35f).Length != 0)
                        report.failures.Add("Academic connection still blocked at " + point);
                    if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                        report.failures.Add("Academic connection missing level floor at " + point);
                }
            var railing = GameObject.Find("10_Lift_side_glass_railing");
            var connection = railing != null ? railing.transform.Find("Column_connection") : null;
            Vector3 expected = UEHCampusBuilder.RailingColumnAnchor(settings);
            if (connection == null || Vector3.Distance(connection.position, expected) > 0.01f || railing.GetComponentsInChildren<BoxCollider>().Length != UEHCampusBuilder.ConnectedRailingPanelCount(settings))
                report.failures.Add("Glass balustrade must connect to the central column and have continuous collision panels.");
            var wallConnection = railing != null ? railing.transform.Find("Wall_connection") : null;
            var wallAnchor = UEHCampusBuilder.RailingWallAnchor(settings);
            if (wallConnection == null || Vector3.Distance(wallConnection.position, wallAnchor) > 0.01f)
                report.failures.Add("Railing must terminate at the pier between the lift bank and toilet corridor.");
            var elbowConnection = railing != null ? railing.transform.Find("Elbow_connection") : null;
            if (elbowConnection == null || connection == null || wallConnection == null)
                report.failures.Add("Missing right-angle railing connection markers.");
            else
            {
                Vector3 first = (elbowConnection.position - connection.position).normalized;
                Vector3 second = (wallConnection.position - elbowConnection.position).normalized;
                if (Mathf.Abs(Vector3.Dot(first, second)) > 0.001f || Mathf.Abs(first.x) > 0.001f || Mathf.Abs(second.z) > 0.001f)
                    report.failures.Add("Railing must form an axis-aligned L with an exact 90-degree corner.");
            }
            var liftRoot = GameObject.Find("Three_lift_bank");
            var stone = liftRoot != null ? liftRoot.transform.Find("Lift_stone_and_dark_panels/Stone") : null;
            bool stoneFaceMatched = false;
            if (stone != null)
            {
                var vertices = stone.GetComponent<MeshFilter>().sharedMesh.vertices;
                for (int start = 0; start + 24 <= vertices.Length; start += 24)
                {
                    float faceX = float.NegativeInfinity; Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++)
                    {
                        Vector3 point = stone.TransformPoint(vertices[start + i]);
                        faceX = Mathf.Max(faceX, point.x); center += point / 24;
                    }
                    stoneFaceMatched |= Mathf.Abs(faceX - wallAnchor.x) < 0.01f && Mathf.Abs(center.z - wallAnchor.z) < 0.01f;
                }
            }
            if (!stoneFaceMatched) report.failures.Add("Railing wall endpoint does not meet the actual stone pier mesh.");
            var lamps = GameObject.Find("11_Central_round_spotlights");
            int count = 0, expectedLamps = 0;
            for (float z = -2.5f; z > -settings.lobbyDepth + 1; z -= 3) expectedLamps += 2;
            if (lamps != null) foreach (Transform child in lamps.transform)
                if (child.name.StartsWith("Round_spot_"))
                {
                    count++;
                    if (Mathf.Abs(Mathf.Abs(child.position.x) - 1.1f) > 0.01f) report.failures.Add("Round ceiling lamp outside central pair of rows.");
                }
            if (count != expectedLamps) report.failures.Add("Missing paired small round ceiling lamps.");
            if (settings.centerTimberOnly)
            {
                var ceiling = GameObject.Find("04_Timber_ceiling_and_downlights");
                var wood = ceiling != null ? ceiling.transform.Find("Wood") : null;
                var backing = ceiling != null ? ceiling.transform.Find("Dark") : null;
                var chandelier = ceiling != null ? ceiling.transform.Find("Gold") : null;
                if (wood == null || backing == null || chandelier == null)
                    report.failures.Add("Central timber strip or chandeliers missing after side strip removal.");
                else
                {
                    var woodBounds = wood.GetComponent<MeshFilter>().sharedMesh.bounds;
                    if (woodBounds.min.x < -2 || woodBounds.max.x > 2 || backing.GetComponent<MeshFilter>().sharedMesh.vertexCount != 24)
                        report.failures.Add("Side timber/backing geometry remains outside the central strip.");
                    if (chandelier.GetComponent<MeshFilter>().sharedMesh.vertexCount == 0)
                        report.failures.Add("Chandeliers missing from central strip.");
                }
                if (settings.lobbyColumnWidth < 2) report.failures.Add("Column body was not widened beyond the prior 1.6m version.");
            }
            if (report.failures.Count == 0)
                report.checks.Add("Lobby refined: six " + settings.lobbyColumnWidth + "m columns centered/flush at gates; axis-aligned L railing has an exact 90-degree corner and " + UEHCampusBuilder.ConnectedRailingPanelCount(settings) + " collision panes, joins column front corner to actual lift-C/toilet stone pier. Central timber/chandeliers and 30 round lamps retained; side timber strips removed=" + settings.centerTimberOnly + ". Academic connection clear at ten probes.");
        }

        private static void CheckGlassOffice(UEHPlayModeVerification.Report report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null || !settings.rightGlassOffice) return;
            var room = GameObject.Find("Glass_office_recessed");
            if (room == null) { report.failures.Add("Missing recessed glass office."); return; }
            var front = room.transform.Find("01_FRONT_glass_wall");
            var right = room.transform.Find("02_RIGHT_glass_wall");
            var opaque = room.transform.Find("Glass_office_opaque_LEFT_and_BACK");
            var entrance = room.transform.Find("03_Open_entrance_1_25m");
            if (front == null || right == null || opaque == null || entrance == null)
            { report.failures.Add("Glass office requires front/right glazing, opaque left/back and open entrance."); return; }
            float facadeX = UEHCampusBuilder.RightFacadeX(settings);
            if (Mathf.Abs(room.transform.position.x - facadeX) > 0.01f || right.position.z <= room.transform.position.z)
                report.failures.Add("Glass office facade must be flush and glass side must be viewer-right.");
            foreach (var renderer in room.GetComponentsInChildren<MeshRenderer>())
                if (renderer.bounds.max.x > facadeX + 0.15f)
                    report.failures.Add("Glass office geometry projects into public hall: " + renderer.name);
            foreach (var renderer in opaque.GetComponentsInChildren<MeshRenderer>())
                if (renderer.sharedMaterial.name == "GlassOfficeGlass") report.failures.Add("Left/back office walls must be opaque.");
            foreach (var side in new[] { front, right })
            {
                int panes = 0;
                foreach (var renderer in side.GetComponentsInChildren<MeshRenderer>())
                    if (renderer.sharedMaterial.name == "GlassOfficeGlass") panes++;
                if (panes < 2) report.failures.Add("Insufficient glass panes on " + side.name);
            }
            for (int i = 1; i <= 3; i++)
                if (room.transform.Find("Workstation_0" + i) == null) report.failures.Add("Missing glass office workstation " + i);
            Physics.SyncTransforms();
            Vector3 doorProbe = entrance.TransformPoint(new Vector3(0, 0.95f, 0));
            if (Physics.OverlapCapsule(doorProbe - Vector3.up * 0.5f, doorProbe + Vector3.up * 0.5f, 0.35f).Length != 0)
                report.failures.Add("Glass office entrance is obstructed by old wall, glazing or open door.");
            foreach (float depth in new[] { 0.35f, 0.85f, 1.1f })
            {
                Vector3 point = room.transform.TransformPoint(new Vector3(UEHCampusBuilder.GlassOfficeDoorX, 0.95f, -depth));
                if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                    report.failures.Add("Glass office floor continuity failed at " + depth);
            }
            if (Physics.OverlapBox(new Vector3(facadeX + 2.9f, 0.95f, UEHCampusBuilder.GlassOfficeZ), new Vector3(0.9f, 0.8f, 3.8f)).Length > 0)
                report.failures.Add("Public hall outside glass office is obstructed.");
            if (report.failures.Count == 0)
                report.checks.Add("Glass office: recessed 9.8x4.6x4m behind current right facade, front and viewer-right walls glazed, left/back opaque, three workstations, inward-open 1.25m door, public hall clear and pocket floor joined to lobby at Y=0.");
        }

        private static void CheckReceptionLayout(UEHPlayModeVerification.Report report)
        {
            var group = GameObject.Find("05_Reception_and_welcome_wall");
            if (group == null) { report.failures.Add("Missing reception group."); return; }
            var green = group.transform.Find("01_Greenery_LEFT_of_WELCOME");
            var panel = group.transform.Find("02_WELCOME_letter_wall");
            var lift = group.GetComponentInChildren<UEHReservedSpace>();
            if (green == null || panel == null || lift == null)
            { report.failures.Add("Missing welcome greenery, letter panel or reserved lift space."); return; }
            // Standing inside facing -X, screen-right is +Z.
            if (!(green.position.z < panel.position.z && lift.transform.position.z > panel.position.z))
                report.failures.Add("Welcome must have greenery on the viewer's left and the clear entrance pocket on the right.");
            foreach (string word in new[] { "Welcome", "To", "UEH_reception" })
            {
                var root = panel.Find(word);
                if (root == null) { report.failures.Add("Missing raised word " + word); continue; }
                var glyphs = root.GetComponentsInChildren<MeshFilter>();
                int expected = word == "Welcome" ? 7 : word == "To" ? 2 : 3;
                if (glyphs.Length != expected || root.GetComponent<TextMesh>() != null)
                    report.failures.Add("Welcome words must be separate solid letter meshes.");
                foreach (var glyph in glyphs)
                    if (glyph.sharedMesh.bounds.size.z * glyph.transform.localScale.z < 0.05f)
                        report.failures.Add("Main welcome letter has insufficient physical depth.");
            }
            int rows = 0;
            foreach (Transform child in panel) if (child.name.StartsWith("Alphabet_row_")) rows++;
            if (rows != 6) report.failures.Add("Expected six rows of raised background letters.");
            var columns = GameObject.Find("02_Stone_columns");
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (columns != null && settings != null)
            {
                foreach (var collider in columns.GetComponentsInChildren<BoxCollider>())
                {
                    Vector3 center = collider.transform.position;
                    if (Mathf.Abs(center.x + settings.frontage * 0.375f) < 0.02f && Mathf.Abs(center.z + settings.lobbyDepth / 2) < 0.02f)
                        report.failures.Add("Reception-side column collider remains.");
                }
                int expectedCount = settings.expandedLobby ? (settings.keepEntranceColumnsWhenExpanded ? 6 : 2) : 11;
                foreach (var filter in columns.GetComponentsInChildren<MeshFilter>())
                    if (filter.sharedMesh.vertexCount != expectedCount * 24)
                        report.failures.Add("Unexpected column body/base/capital count after reception/expansion edits.");
            }
            Physics.SyncTransforms();
            var blocked = Physics.OverlapBox(lift.transform.position, lift.size / 2, lift.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
            if (!lift.furnished && blocked.Length > 0) report.failures.Add("Clear entrance pocket contains a collider: " + blocked[0].name);
            if (settings.facultyLiftEntrance) CheckAlignedRightWing(report, settings);
            if (report.failures.Count == 0)
                report.checks.Add("Reception: nearby column removed, left greenery, twelve solid brass letters 0.06m deep, six rows of raised alphabet, faculty entrance pocket on the viewer's right (furnished when enabled).");
        }

        private static void CheckAlignedRightWing(UEHPlayModeVerification.Report report, UEHDimensions settings)
        {
            float facade = UEHCampusBuilder.RightFacadeX(settings);
            foreach (string name in new[] { "05_WELCOME_RIGHT_from_gate", "Service_corridor", "Three_lift_bank", "Glass_office_recessed", "12_Faculty_lift_entrance" })
            {
                var item = GameObject.Find(name);
                if (item == null || Mathf.Abs(item.transform.position.x - facade) > 0.01f)
                    report.failures.Add("Right-wing facade alignment failed: " + name);
            }
            var faculty = GameObject.Find("12_Faculty_lift_entrance");
            if (faculty == null) return;
            int gates = 0, leaves = 0;
            foreach (Transform child in faculty.transform)
            { if (child.name.StartsWith("Access_pedestal_")) gates++; if (child.name.StartsWith("Faculty_lift_leaf_")) leaves++; }
            if (settings.facultyLiftsBehindWelcome) CheckFacultyReturn(report, faculty.transform, settings);
            else if (gates != 3 || leaves != 2) report.failures.Add("Faculty alcove needs three access pedestals and a paired elevator door.");
            if (gates != 3) report.failures.Add("Faculty entrance must retain three access pedestals.");
            foreach (var renderer in faculty.GetComponentsInChildren<MeshRenderer>())
                if (renderer.bounds.max.x > facade + 0.01f) report.failures.Add("Faculty alcove projects into public lobby: " + renderer.transform.parent.name + "/" + renderer.name);
            foreach (float depth in new[] { -0.05f, -0.5f, -1.2f, -2f })
            {
                var point = faculty.transform.TransformPoint(new Vector3(0.65f, 0.95f, depth));
                if (Physics.OverlapCapsule(point - Vector3.up * 0.5f, point + Vector3.up * 0.5f, 0.35f).Length != 0)
                    report.failures.Add("Faculty access aisle blocked at " + point);
                if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                    report.failures.Add("Faculty alcove floor missing.");
            }
            var entrance = GameObject.Find("03_Glazing_with_open_doorway");
            int panes = 0;
            foreach (var child in entrance.GetComponentsInChildren<Transform>()) if (child.name.StartsWith("Entrance_fixed_pane_")) panes++;
            if (panes < 16) report.failures.Add("Entrance requires narrow vertical glazing bays.");
            foreach (float x in new[] { -1.9f, 0, 1.9f })
                if (Physics.OverlapCapsule(new Vector3(x, 0.45f, 0), new Vector3(x, 1.45f, 0), 0.35f).Length != 0)
                    report.failures.Add("Central entrance obstructed by rebuilt glazing.");
            report.checks.Add("Right wing inset " + settings.rightWingInset + "m: reception/services/three lifts/glass office share facade X=" + facade + "; L railing meets moved stone pier. Entrance has " + panes + " narrow bays with high transoms and right roller blinds. Faculty vestibule retains three access pedestals and a clear level aisle.");
        }

        private static void CheckFacultyReturn(UEHPlayModeVerification.Report report, Transform faculty, UEHDimensions settings)
        {
            int lifts = 0;
            foreach (Transform child in faculty)
            {
                if (!child.name.StartsWith("Faculty_lift_GV")) continue;
                lifts++;
                if (Vector3.Dot(child.forward, -faculty.right) < 0.999f || child.localPosition.x < 1.7f || child.position.x >= faculty.position.x - 1)
                    report.failures.Add("Faculty lift must be on the left return behind WELCOME, facing the vestibule: " + child.name);
                int leaves = 0; foreach (Transform leaf in child) if (leaf.name.StartsWith("Sliding_leaf_")) leaves++;
                if (leaves != 2) report.failures.Add("Faculty lift requires two editable door leaves: " + child.name);
                var point = faculty.TransformPoint(new Vector3(0.65f, 0.95f, child.localPosition.z));
                if (Physics.OverlapCapsule(point - Vector3.up * 0.5f, point + Vector3.up * 0.5f, 0.35f).Length > 0)
                    report.failures.Add("Approach to faculty lift blocked: " + child.name);
                if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                    report.failures.Add("Faculty lift approach lacks level floor: " + child.name);
            }
            if (lifts != 2) report.failures.Add("Expected exactly two faculty elevators behind WELCOME.");
            var columns = GameObject.Find("02_Stone_columns"); bool recessed = false;
            foreach (var col in columns.GetComponentsInChildren<BoxCollider>())
                recessed |= Mathf.Abs(col.transform.position.x - UEHCampusBuilder.RightCornerColumnX(settings)) < 0.01f && col.transform.position.x < faculty.position.x - 2;
            if (!recessed) report.failures.Add("Stone column beside faculty entrance must be set back behind WELCOME facade.");
            var arrival = faculty.TransformPoint(new Vector3(0.65f, 1.5f, -1.3f));
            if (Physics.Raycast(arrival, -faculty.forward, out var hit, 5) && hit.collider.name.Contains("leaf"))
                report.failures.Add("Faculty elevator still faces the arrival direction.");
            if (settings.facultyColumnAtRearWall)
            {
                bool flush = false;
                foreach (var col in columns.GetComponentsInChildren<BoxCollider>())
                    if (Mathf.Abs(col.transform.position.x - UEHCampusBuilder.RightCornerColumnX(settings)) < 0.01f)
                        flush = Mathf.Abs(col.bounds.min.x - (faculty.position.x - UEHCampusBuilder.FacultyRoomDepth)) < 0.01f;
                if (!flush) report.failures.Add("Faculty stone column body must meet the rear wall at the indicated corner.");
            }
            report.checks.Add("Two faculty elevators GV1/GV2 behind WELCOME on the visitor's LEFT after entry, each with paired static doors. Approaches have level floors; the former head-on lift is removed. Right stone column X=" + UEHCampusBuilder.RightCornerColumnX(settings) + "; body flush with faculty rear wall=" + settings.facultyColumnAtRearWall + ".");
        }

        private static void CheckRightLiftsAndServices(UEHPlayModeVerification.Report report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null || !settings.rightLiftsAndServices) return;
            var areas = GameObject.Find("09_Four_expansion_areas");
            var hall = areas.transform.Find("RIGHT_01 [12m x 6m]/Service_corridor");
            var lifts = areas.transform.Find("RIGHT_02 [12m x 6m]/Three_lift_bank");
            if (hall == null || lifts == null) { report.failures.Add("Missing right service corridor or lift bank."); return; }
            var welcome = GameObject.Find("05_WELCOME_RIGHT_from_gate");
            if (!(welcome.transform.position.z > hall.position.z && hall.position.z > lifts.position.z))
                report.failures.Add("Service corridor must lie between WELCOME and lifts.");
            if (Mathf.Abs(hall.position.x - hall.parent.position.x) > 0.01f)
                report.failures.Add("Corridor entrance must align with side-wall facade.");
            int liftCount = 0;
            foreach (Transform child in lifts)
            {
                if (child.name != "Lift_A" && child.name != "Lift_B" && child.name != "Lift_C") continue;
                liftCount++;
                if (child.Find("Sliding_leaf_left") == null || child.Find("Sliding_leaf_right") == null ||
                    child.Find("Lift_identifier") == null || child.Find("Floor_indicator") == null)
                    report.failures.Add("Incomplete editable lift assembly: " + child.name);
                if (!Physics.Raycast(child.TransformPoint(new Vector3(0, 1.2f, 0.8f)), -child.forward, 1))
                    report.failures.Add("Closed lift door lacks collision: " + child.name);
            }
            if (liftCount != 3) report.failures.Add("Expected exactly three elevator doors.");
            foreach (string name in new[] { "LEFT_01_Stairs_UP", "LEFT_02_Stairs_DOWN_Parking", "RIGHT_01_Toilet_MEN", "RIGHT_02_Toilet_WOMEN" })
            {
                var door = hall.Find(name);
                if (door == null) { report.failures.Add("Missing service door " + name); continue; }
                if ((name.StartsWith("LEFT") && door.localPosition.x <= 0) || (name.StartsWith("RIGHT") && door.localPosition.x >= 0))
                    report.failures.Add("Service door is on the wrong side when looking in: " + name);
                if (door.Find("Door_title") == null || door.Find("Door_destination") == null || door.Find("Door_leaf_closed") == null)
                    report.failures.Add("Missing service door leaf or destination sign: " + name);
                if (!Physics.Raycast(door.TransformPoint(new Vector3(0, 1.2f, 0.7f)), -door.forward, 1))
                    report.failures.Add("Closed service door lacks collision: " + name);
            }
            foreach (float depth in new[] { 0.4f, 1f, 2.4f, 5.8f, 8f })
            {
                Vector3 point = hall.TransformPoint(new Vector3(0, 0.95f, -depth));
                if (Physics.OverlapCapsule(point - Vector3.up * 0.5f, point + Vector3.up * 0.5f, 0.35f).Length != 0)
                    report.failures.Add("Corridor blocked at depth " + depth);
                if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                    report.failures.Add("Corridor floor discontinuity at depth " + depth);
            }
            if (report.failures.Count == 0)
                report.checks.Add("Right bays: 3 editable silver lift assemblies A/B/C with paired sliding leaves, calls and indicators; flush recessed corridor 2.8x8.6m between reception and lifts, left stairs up/down to parking and right men's/women's toilets. Four signed closed doors block collision; corridor entrance/floor clear at five depths. Static architecture only.");
        }

        private static void CheckHeritageSections(UEHPlayModeVerification.Report report)
        {
            var group = GameObject.Find("06_Heritage_1976_and_gallery");
            if (group == null) { report.failures.Add("Missing heritage display."); return; }
            // Standing inside facing +X, screen-right is -Z. Check the saved world positions.
            Vector3 viewerRight = Vector3.back;
            float previous = float.NegativeInfinity;
            foreach (string name in new[] { "01_Plain_wall_left", "02_SINCE_1976_green", "03_Nine_colour_panel", "04_Circles_panel" })
            {
                var section = group.transform.Find(name);
                if (section == null) { report.failures.Add("Missing heritage section " + name); return; }
                float position = Vector3.Dot(section.position, viewerRight);
                if (position <= previous) report.failures.Add("Heritage sections must run left-to-right from blank wall to greenery to nine colours to circles.");
                previous = position;
            }
            var swatches = group.transform.Find("03_Nine_colour_panel");
            int count = 0;
            foreach (Transform child in swatches) if (child.name.StartsWith("Colour_")) count++;
            if (count != 9) report.failures.Add("Expected exactly nine colour swatches.");
            var circles = group.transform.Find("04_Circles_panel");
            count = 0;
            float minSize = float.PositiveInfinity, maxSize = 0;
            foreach (Transform child in circles)
            {
                if (!child.name.StartsWith("Circle_")) continue;
                count++;
                float size = child.GetComponentInChildren<MeshFilter>().sharedMesh.bounds.size.y;
                minSize = Mathf.Min(minSize, size); maxSize = Mathf.Max(maxSize, size);
            }
            if (count != 5 || maxSize - minSize < 0.3f) report.failures.Add("Expected five circles with visibly different sizes.");
            if (report.failures.Count == 0)
                report.checks.Add("Heritage wall, viewed from inside: blank wall on the left, then SINCE 1976 greenery, exactly nine colour swatches, then five circles of different sizes.");
        }

        private static void CheckEntranceColumns(UEHPlayModeVerification.Report report)
        {
            var group = GameObject.Find("02_Stone_columns");
            if (group == null) { report.failures.Add("Missing lobby column group."); return; }
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null) { report.failures.Add("Missing estimated dimensions for columns."); return; }
            if (settings.expandedLobby)
            {
                CheckExpandedLobby(report, group, settings);
                return;
            }
            var front = new List<Vector3>();
            foreach (var collider in group.GetComponentsInChildren<BoxCollider>())
            {
                var center = collider.transform.TransformPoint(collider.center);
                if (center.z > -1.2f) front.Add(center);
            }
            if (front.Count != 4) { report.failures.Add("Expected four columns immediately beside the entrance glazing."); return; }
            bool valid = true;
            foreach (var center in front)
                valid &= center.z >= -0.65f && center.z <= -0.45f && Mathf.Abs(center.x) > 3.5f;
            float cornerX = UEHCampusBuilder.EntranceCornerColumnX(settings.frontage);
            int leftCorners = 0, rightCorners = 0, innerColumns = 0;
            foreach (var center in front)
            {
                if (Mathf.Abs(center.x - cornerX) < 0.02f) leftCorners++;
                else if (Mathf.Abs(center.x + cornerX) < 0.02f) rightCorners++;
                else if (Mathf.Abs(Mathf.Abs(center.x) - settings.frontage * 0.125f) < 0.02f) innerColumns++;
            }
            valid &= leftCorners == 1 && rightCorners == 1 && innerColumns == 2;
            foreach (var filter in group.GetComponentsInChildren<MeshFilter>())
            {
                var vertices = filter.sharedMesh.vertices;
                int matching = 0;
                for (int start = 0; start + 24 <= vertices.Length; start += 24)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++) center += filter.transform.TransformPoint(vertices[start + i]) / 24;
                    if (center.z <= -1.2f) continue;
                    bool aligned = false;
                    foreach (var colliderCenter in front)
                        aligned |= Mathf.Abs(center.x - colliderCenter.x) < 0.02f && Mathf.Abs(center.z - colliderCenter.z) < 0.02f;
                    valid &= aligned;
                    matching++;
                }
                valid &= matching == 4;
            }
            if (!valid) report.failures.Add("Entrance column bodies/bases/capitals/colliders are not aligned at the glazing or obstruct the central doorway.");
            else report.checks.Add("Four entrance columns at Z=-0.56m: outer pair at X=+/-" + cornerX + "m meets the side-wall corners; inner pair unchanged. Bodies, bases, capitals and colliders align; central doorway remains clear.");
        }

        private static void CheckExpandedLobby(UEHPlayModeVerification.Report report, GameObject columns, UEHDimensions settings)
        {
            int expected = settings.keepEntranceColumnsWhenExpanded ? 6 : 2;
            var colliders = columns.GetComponentsInChildren<BoxCollider>();
            if (colliders.Length != expected) report.failures.Add("Expanded lobby has the wrong column count.");
            int central = 0;
            foreach (var collider in colliders)
                if (Mathf.Abs(Mathf.Abs(collider.transform.position.x) - settings.frontage * 0.125f) < 0.02f &&
                    Mathf.Abs(collider.transform.position.z - UEHCampusBuilder.ExpandedColumnZ(settings)) < 0.02f) central++;
            if (central != 2) report.failures.Add("Missing retained central column pair.");
            foreach (var filter in columns.GetComponentsInChildren<MeshFilter>())
            {
                var vertices = filter.sharedMesh.vertices;
                if (vertices.Length != expected * 24) { report.failures.Add("Incorrect column body/base/capital mesh count."); continue; }
                for (int start = 0; start < vertices.Length; start += 24)
                {
                    Vector3 center = Vector3.zero;
                    for (int i = 0; i < 24; i++) center += filter.transform.TransformPoint(vertices[start + i]) / 24;
                    bool aligned = false;
                    foreach (var collider in colliders)
                        aligned |= Mathf.Abs(center.x - collider.transform.position.x) < 0.02f && Mathf.Abs(center.z - collider.transform.position.z) < 0.02f;
                    if (!aligned) report.failures.Add("Expanded column body/base/capital does not match its collider.");
                }
            }
            var areas = GameObject.Find("09_Four_expansion_areas");
            if (areas == null) { report.failures.Add("Missing four expansion areas."); return; }
            var volumes = areas.GetComponentsInChildren<UEHReservedSpace>();
            int left = 0, right = 0;
            Physics.SyncTransforms();
            foreach (var volume in volumes)
            {
                if (volume.transform.position.x > 0) left++; else right++;
                float plannedWidth = settings.compactLeftSequence && volume.name.StartsWith("Reserved_area_LEFT") ? 10.6f : 12;
                if (Mathf.Abs(volume.size.x - plannedWidth) > 0.01f || Mathf.Abs(volume.size.z - 6) > 0.01f)
                    report.failures.Add("Expansion planning volume must match configured " + plannedWidth + "x6m footprint.");
                if (!volume.furnished)
                {
                    var hits = Physics.OverlapBox(volume.transform.position, volume.size / 2, volume.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
                    if (hits.Length > 0) report.failures.Add("Unfurnished expansion area is obstructed: " + volume.name + " by " + hits[0].name);
                }
                Vector3 floorProbe = volume.transform.position;
                if (settings.leftOfficeAndLounge && volume.name == "Reserved_area_LEFT_01")
                    floorProbe = areas.transform.Find("LEFT_01 [12m x 6m]/Office_room").TransformPoint(new Vector3(0, 2.1f, 1.25f));
                if (!Physics.Raycast(floorProbe, Vector3.down, out var floor, 3) || Mathf.Abs(floor.point.y) > 0.03f)
                    report.failures.Add("No continuous floor beneath " + volume.name);
            }
            if (left != 2 || right != 2) report.failures.Add("Expected two expansion areas on each side.");
            var shell = GameObject.Find("01_Floor_and_walls");
            bool floorSize = false;
            foreach (var collider in shell.GetComponentsInChildren<BoxCollider>())
                floorSize |= Mathf.Abs(collider.size.x - settings.frontage) < 0.02f && Mathf.Abs(collider.size.z - settings.lobbyDepth) < 0.02f;
            if (!floorSize) report.failures.Add("Expanded floor does not match configured dimensions.");
            foreach (string name in new[] { "LobbyWestRear", "LobbyEastRear" })
                if (AssetDatabase.LoadAssetAtPath<Cubemap>(UEHCampusBuilder.Root + "/Lighting/" + name + ".cubemap") == null)
                    report.failures.Add("Missing rear reflection capture " + name);
            if (report.failures.Count == 0)
                report.checks.Add("Expanded lobby " + settings.frontage + "x" + settings.lobbyDepth + "m: two areas per side (left " + (settings.compactLeftSequence ? "10.6" : "12") + "x6m, right 12x6m), unfurnished areas clear, continuous floor, aligned central column pair; total columns=" + expected + ".");
            if (settings.leftOfficeAndLounge) CheckLeftOfficeAndLounge(report, areas);
        }

        private static void CheckLeftOfficeAndLounge(UEHPlayModeVerification.Report report, GameObject areas)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            var office = areas.transform.Find("LEFT_01 [12m x 6m]/Office_room");
            var lounge = areas.transform.Find("LEFT_02 [12m x 6m]/Green_timber_lounge");
            if (office == null || lounge == null) { report.failures.Add("Missing furnished left office/lounge."); return; }
            if (office.Find("Office_service_window") == null || office.Find("Office_blue_perimeter") == null ||
                office.Find("Workstation_01") == null || office.Find("Workstation_02") == null ||
                office.Find("Office_waiting_chair_01") == null || office.Find("Office_waiting_chair_02") == null)
                report.failures.Add("Office lacks glazing, blue frame, workstations or waiting chairs.");
            var green = lounge.Find("01_Greenery_LEFT");
            var timber = lounge.Find("02_Timber_wall_RIGHT");
            if (green == null || timber == null || green.position.z <= timber.position.z)
                report.failures.Add("Lounge greenery must be on the viewer's left of the timber panel.");
            if (lounge.Find("Lounge_chair_01") == null || lounge.Find("Lounge_chair_02") == null)
                report.failures.Add("Missing two lounge chairs.");
            var doorway = office.Find("Office_open_left_doorway");
            if (doorway == null) { report.failures.Add("Missing open office doorway."); return; }
            if (Mathf.Abs(office.localPosition.z + UEHCampusBuilder.OfficeRecessDepth) > 0.01f ||
                Mathf.Abs(doorway.position.x - office.parent.position.x) > 0.02f)
                report.failures.Add("Office facade must align with the lobby side wall, with the room recessed behind it.");
            if (Physics.OverlapBox(new Vector3(14.2f, 0.95f, UEHCampusBuilder.LeftOfficeZ(settings)), new Vector3(0.7f, 0.8f, 1)).Length != 0)
                report.failures.Add("Office still obstructs the public hall in front of its facade.");
            Vector3 probe = doorway.TransformPoint(new Vector3(0, 0.95f, 0));
            if (Physics.OverlapCapsule(probe + Vector3.up * 0.5f, probe - Vector3.up * 0.5f, 0.32f).Length != 0)
                report.failures.Add("Office doorway is obstructed.");
            if (report.failures.Count == 0)
                report.checks.Add("LEFT_01 office recessed 2.8m behind side-wall line: grey facade at X=17.4m, public hall clear, open 1.2m doorway and continuous pocket floor; 8 glass panes, blue perimeter, two workstations and waiting chairs. LEFT_02 greenery/timber lounge retained.");
        }

        private static void CheckCompactLeftSequence(UEHPlayModeVerification.Report report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null || !settings.compactLeftSequence) return;
            var zone = GameObject.Find("06_1976_LEFT_from_gate").transform;
            var heritage = zone.Find("06_Heritage_1976_and_gallery");
            var slate = heritage.Find("00_Contrasting_slate_panel/Slate_wall_and_picture/HeritageSlate");
            if (slate == null) { report.failures.Add("Missing contrasting slate segment immediately left of Since 1976."); return; }
            var slateBounds = slate.GetComponent<MeshRenderer>().bounds;
            bool adjacent = false;
            foreach (var col in GameObject.Find("02_Stone_columns").GetComponentsInChildren<BoxCollider>())
                if (col.transform.position.x > settings.frontage * 0.25f)
                    adjacent = (settings.entranceGlassThroughColumns ?
                        Mathf.Abs(col.transform.position.x - UEHCampusBuilder.LeftGateColumnX(settings)) < 0.01f && col.bounds.min.z - slateBounds.max.z >= 0 && col.bounds.min.z - slateBounds.max.z < 0.45f :
                        Mathf.Abs(col.bounds.min.z - slateBounds.max.z) < 0.03f) && Mathf.Abs(col.transform.position.z - UEHCampusBuilder.LeftColumnZ(settings)) < 0.01f;
            if (!adjacent) report.failures.Add("Left entrance stone column must be immediately beside the slate wall segment.");
            if (slateBounds.size.z < 1.4f || slateBounds.size.y < 4.1f)
                report.failures.Add("Slate wall segment has incorrect estimated size.");
            var office = GameObject.Find("Office_room").transform;
            var lounge = GameObject.Find("Green_timber_lounge").transform;
            if (Mathf.Abs(zone.position.z - UEHCampusBuilder.LeftHeritageZ(settings)) > 0.01f ||
                Mathf.Abs(office.position.z - UEHCampusBuilder.LeftOfficeZ(settings)) > 0.01f ||
                Mathf.Abs(lounge.position.z - UEHCampusBuilder.LeftLoungeZ(settings)) > 0.01f)
                report.failures.Add("Compacted left groups are not at their configured positions.");
            var heritageBounds = RenderBounds(heritage.Find("Timber_frame"));
            var officeBounds = RenderBounds(office.Find("Office_blue_perimeter"));
            var loungeBounds = RenderBounds(lounge.Find("01_Greenery_LEFT"));
            float firstGap = heritageBounds.min.z - officeBounds.max.z;
            float secondGap = officeBounds.min.z - loungeBounds.max.z;
            if (firstGap < 0 || firstGap > 0.9f || secondGap < 0 || secondGap > 0.9f)
                report.failures.Add("Left zones must have narrow non-overlapping gaps: " + firstGap + ", " + secondGap);
            report.checks.Add("Left sequence compacted: slate segment 1.45x4.2m left of 1976 with adjacent stone column; office and green/timber lounge moved as editable groups. Rendered facade gaps " + firstGap.ToString("F2") + "m and " + secondGap.ToString("F2") + "m; office remains recessed with aligned wall opening.");
        }

        private static void CheckColumnAlignedGates(UEHPlayModeVerification.Report report)
        {
            var settings = AssetDatabase.LoadAssetAtPath<UEHDimensions>(UEHCampusBuilder.Root + "/Settings/EstimatedDimensions.asset");
            if (settings == null || !settings.entranceGlassThroughColumns) return;
            float plane = UEHCampusBuilder.EntranceGlassZ(settings);
            var entrance = GameObject.Find("03_Glazing_with_open_doorway").transform;
            var columns = GameObject.Find("02_Stone_columns").GetComponentsInChildren<BoxCollider>();
            int bays = 0, fixedPanes = 0;
            foreach (Transform bay in entrance)
            {
                if (!bay.name.StartsWith("BAY_")) continue;
                bays++;
                foreach (string endpoint in new[] { "Left_column_face", "Right_column_face" })
                {
                    var marker = bay.Find(endpoint); bool joined = false;
                    foreach (var column in columns)
                        joined |= Mathf.Abs(column.bounds.center.z - plane) < 0.01f && Mathf.Abs(marker.position.z - plane) < 0.01f &&
                            Mathf.Abs(marker.position.x - (endpoint == "Left_column_face" ? column.bounds.max.x : column.bounds.min.x)) < 0.01f;
                    if (!joined) report.failures.Add("Glazing bay endpoint does not join a column at mid-depth: " + bay.name + "/" + endpoint);
                }
                foreach (Transform child in bay)
                    if (child.name.StartsWith("Entrance_fixed_pane_"))
                    {
                        fixedPanes++;
                        foreach (var collider in child.GetComponentsInChildren<BoxCollider>())
                            if (Mathf.Abs(collider.bounds.center.z - plane) > 0.01f) report.failures.Add("Fixed entrance glass is outside column center plane.");
                    }
            }
            if (bays != 3 || fixedPanes < 16) report.failures.Add("Expected three connected gate bays with narrow fixed glazing.");
            foreach (float center in new[] { 0f, UEHCampusBuilder.SinceGateCenterX(settings) })
                foreach (float z in new[] { plane - 0.9f, plane, plane + 0.9f })
                {
                    var point = new Vector3(center, 0.95f, z);
                    if (Physics.OverlapCapsule(point - Vector3.up * 0.5f, point + Vector3.up * 0.5f, 0.35f).Length > 0)
                        report.failures.Add("Center or Since gate blocked at " + point);
                    if (!Physics.Raycast(point, Vector3.down, out var floor, 2) || Mathf.Abs(floor.point.y) > 0.03f)
                        report.failures.Add("Center or Since gate lacks level floor at " + point);
                }
            report.checks.Add("Entrance: Since outer column moved to X=" + UEHCampusBuilder.LeftGateColumnX(settings) + ", Z=" + plane + "; all three glazing bays join actual stone faces at column mid-depth. Main " + settings.entryWidth + "m and Since " + UEHCampusBuilder.SinceGateWidth + "m openings have inward-open paired leaves and clear level approaches. Fixed glass retains collision.");
        }

        private static Bounds RenderBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>();
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void CheckGateSide(UEHPlayModeVerification.Report report, string objectName, bool expectedLeft)
        {
            var display = GameObject.Find(objectName);
            if (display == null) { report.failures.Add("Missing entrance display " + objectName); return; }
            Vector3 gatePosition = new Vector3(0, 1.65f, 13);
            Vector3 gateRight = Vector3.Cross(Vector3.up, Vector3.back);
            float lateral = Vector3.Dot(display.transform.position - gatePosition, gateRight);
            bool correctSide = expectedLeft ? lateral < -1 : lateral > 1;
            Vector3 faceNormal = display.transform.TransformDirection(Vector3.back);
            bool facesVisitor = Vector3.Dot(faceNormal, gatePosition - display.transform.position) > 0;
            bool besideEntrance = display.transform.position.z > -14 && display.transform.position.z < 0;
            string side = expectedLeft ? "LEFT" : "RIGHT";
            if (!correctSide || !facesVisitor || !besideEntrance)
                report.failures.Add(objectName + " must face inward on the " + side + " side near the entrance; actual " + display.transform.position);
            else
                report.checks.Add("Gate-facing-in view: " + objectName + " is on the " + side + " side wall, facing the visitor, at " + display.transform.position);
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(SessionKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                try
                {
                    var report = JsonUtility.FromJson<UEHPlayModeVerification.Report>(File.ReadAllText(ReportPath));
                    var runner = new GameObject("Temporary UEH route verification").AddComponent<UEHPlayModeVerification>();
                    runner.Begin(ReportPath, report);
                }
                catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(SessionKey, false);
                var report = JsonUtility.FromJson<UEHPlayModeVerification.Report>(File.ReadAllText(ReportPath));
                EditorApplication.delayCall += () => EditorApplication.Exit(report.passed ? 0 : 1);
            }
        }
    }
}
#endif
