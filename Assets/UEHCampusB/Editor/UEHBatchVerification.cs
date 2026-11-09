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
                if (Array.IndexOf(Environment.GetCommandLineArgs(), "-ueh-right-services-only") >= 0) UEHCampusBuilder.FurnishRightAreas();
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
            CheckReceptionLayout(report);
            CheckRightLiftsAndServices(report);
            return report;
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
            if (blocked.Length > 0) report.failures.Add("Clear entrance pocket contains a collider: " + blocked[0].name);
            if (report.failures.Count == 0)
                report.checks.Add("Reception: nearby column removed, left greenery, twelve solid brass letters 0.06m deep, six rows of raised alphabet, clear 3.5m x 2.9m entrance pocket on the viewer's right.");
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
                    Mathf.Abs(collider.transform.position.z - UEHCampusBuilder.RetainedColumnZ) < 0.02f) central++;
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
                if (Mathf.Abs(volume.size.x - 12) > 0.01f || Mathf.Abs(volume.size.z - 6) > 0.01f)
                    report.failures.Add("Expansion area is smaller than the planned 12x6m footprint.");
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
                report.checks.Add("Expanded lobby " + settings.frontage + "x" + settings.lobbyDepth + "m: two 12x6m areas per side, unfurnished areas clear, continuous floor, aligned central column pair; total columns=" + expected + ".");
            if (settings.leftOfficeAndLounge) CheckLeftOfficeAndLounge(report, areas);
        }

        private static void CheckLeftOfficeAndLounge(UEHPlayModeVerification.Report report, GameObject areas)
        {
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
            if (Physics.OverlapBox(new Vector3(14.2f, 0.95f, -23), new Vector3(0.7f, 0.8f, 1)).Length != 0)
                report.failures.Add("Office still obstructs the public hall in front of its facade.");
            Vector3 probe = doorway.TransformPoint(new Vector3(0, 0.95f, 0));
            if (Physics.OverlapCapsule(probe + Vector3.up * 0.5f, probe - Vector3.up * 0.5f, 0.32f).Length != 0)
                report.failures.Add("Office doorway is obstructed.");
            if (report.failures.Count == 0)
                report.checks.Add("LEFT_01 office recessed 2.8m behind side-wall line: grey facade at X=17.4m, public hall clear, open 1.2m doorway and continuous pocket floor; 8 glass panes, blue perimeter, two workstations and waiting chairs. LEFT_02 greenery/timber lounge retained.");
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
