#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        public static float GlassOfficeZ => RightOfficeZ(dimensions);
        public static float GlassOfficeWidth => dimensions != null && dimensions.extendedGlassOfficeAndClosedRear ? 16 : 9.8f;
        public const float GlassOfficeDepth = 4.6f;
        public const float GlassOfficeHeight = 4;
        public static float GlassOfficeDoorX => -GlassOfficeWidth / 2 + 1.55f;

        [MenuItem("UEH/Campus B/12. Build recessed glass office near lifts")]
        public static void FurnishGlassOffice()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before building the glass office.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.rightLiftsAndServices)
                throw new InvalidOperationException("Build the right lifts and services before this office.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            dimensions.rightGlassOffice = true;
            EditorUtility.SetDirty(dimensions);
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                PopulateRightGlassOffice(contents.transform.Find("09_Four_expansion_areas"));
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupGlassOfficeViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_GLASS_OFFICE_UPDATED Recessed " + GlassOfficeWidth + "x4.6x4m office at Z=" + GlassOfficeZ + "; front and viewer-right walls glass, left/back opaque; inward-open 1.25m entrance.");
        }

        private static void BuildRightWallWithGlassOffice(Part shell, float w, float d, float h)
        {
            // Depth intervals in metres from the entrance; the second opening is beyond
            // the 24m exterior podium cladding, so no exterior cladding intersects this office.
            var holes = new List<Vector3>();
            if (dimensions.facultyLiftEntrance) holes.Add(dimensions.facultyLiftsBehindWelcome ? new Vector3(0, 5.9f, 4.3f) : new Vector3(2.1f, 5.9f, 3.6f));
            if (dimensions.rightLiftsAndServices)
                holes.Add(new Vector3(-ServiceCorridorZ - ServiceCorridorWidth / 2, -ServiceCorridorZ + ServiceCorridorWidth / 2, ServicePortalHeight));
            holes.Add(new Vector3(-GlassOfficeZ - GlassOfficeWidth / 2 - 0.15f, -GlassOfficeZ + GlassOfficeWidth / 2 + 0.15f, GlassOfficeHeight));
            holes.Sort((a, b) => a.x.CompareTo(b.x));
            float previous = 0;
            foreach (var hole in holes)
            {
                if (hole.x < previous || hole.y >= d) throw new InvalidOperationException("Right wall openings overlap or exceed the lobby.");
                if (hole.x > previous) shell.Box("Ivory", new Vector3(RightFacadeX(dimensions) - 0.45f, h / 2, -(previous + hole.x) / 2), new Vector3(0.3f, h, hole.x - previous), true);
                shell.Box("Ivory", new Vector3(RightFacadeX(dimensions) - 0.45f, (hole.z + h) / 2, -(hole.x + hole.y) / 2), new Vector3(0.3f, h - hole.z, hole.y - hole.x), true);
                previous = hole.y;
            }
            shell.Box("Ivory", new Vector3(RightFacadeX(dimensions) - 0.45f, h / 2, -(previous + d) / 2), new Vector3(0.3f, h, d - previous), true);
        }

        private static void PopulateRightGlassOffice(Transform areas)
        {
            var bay = areas.Find("RIGHT_02 [12m x 6m]");
            if (bay == null) throw new InvalidOperationException("Missing remaining right bay.");
            var old = bay.Find("Glass_office_recessed");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            bay.GetComponentInChildren<UEHReservedSpace>().purpose = "Cụm thang máy phía trước và office kính lùi sau vách ở phần trống phía trong. Kích thước ước lượng.";
            string path = Root + "/Materials/GlassOfficeGlass.mat";
            var glass = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (glass == null)
            {
                glass = new Material(materials["ClearGlass"]) { name = "GlassOfficeGlass" };
                AssetDatabase.CreateAsset(glass, path);
            }
            glass.color = new Color(0.66f, 0.8f, 0.82f, 0.15f);
            glass.SetFloat("_Glossiness", 0.82f); glass.SetFloat("_Metallic", 0.05f);
            materials["GlassOfficeGlass"] = glass; EditorUtility.SetDirty(glass);
            var room = RightGroup(bay, "Glass_office_recessed", new Vector3(bay.localPosition.z - GlassOfficeZ, 0, 0));
            float halfWidth = GlassOfficeWidth / 2;
            var solid = new Part("Glass_office_opaque_LEFT_and_BACK", room, "GlassOffice");
            solid.Box("OfficeInterior", new Vector3(GlassOfficeWidth / 2 + 0.05f, 2, -GlassOfficeDepth / 2), new Vector3(0.1f, 4, GlassOfficeDepth), true);
            solid.Box("OfficeInterior", new Vector3(0, 2, -GlassOfficeDepth - 0.05f), new Vector3(GlassOfficeWidth + 0.2f, 4, 0.1f), true);
            solid.Box("White", new Vector3(0, 3.95f, -GlassOfficeDepth / 2), new Vector3(GlassOfficeWidth + 0.2f, 0.1f, GlassOfficeDepth + 0.2f), true);
            RightRoomFloor(solid, room, GlassOfficeWidth + 0.2f, GlassOfficeDepth + 0.2f);
            solid.Finish();

            // Continue the building behind the neighbouring lift facade. This enclosed buffer
            // is outside the office boundary, so the right wall remains glass without a sky void.
            var context = new Part("Building_context_beyond_right_glass", room, "GlassOffice");
            context.Box("OfficeInterior", new Vector3(-halfWidth - 1.45f, 2, -2.3f), new Vector3(0.1f, 4, 4.8f), true);
            context.Box("OfficeInterior", new Vector3(-halfWidth - 0.75f, 2, -4.65f), new Vector3(1.5f, 4, 0.1f), true);
            RightRoomFloor(context, room, 1.5f, 4.8f, -halfWidth - 0.75f);
            context.Box("White", new Vector3(-halfWidth - 0.75f, 3.95f, -2.3f), new Vector3(1.5f, 0.1f, 4.8f), true);
            context.Finish();

            // Local +X is viewer-left, -X is viewer-right when looking at the office from the lobby.
            var front = RightGroup(room, "01_FRONT_glass_wall", Vector3.zero);
            float doorStart = GlassOfficeDoorX - 0.625f, doorEnd = GlassOfficeDoorX + 0.625f;
            GlassOfficePanel(front, "Front_small_right_pane", -halfWidth, doorStart);
            int paneCount = Mathf.CeilToInt((halfWidth - doorEnd) / 2.6f);
            float paneWidth = (halfWidth - doorEnd) / paneCount;
            for (int i = 0; i < paneCount; i++)
                GlassOfficePanel(front, "Front_main_pane_" + (i + 1), doorEnd + i * paneWidth, doorEnd + (i + 1) * paneWidth);
            var frontFrame = new Part("Glass_office_front_frame", front, "GlassOffice");
            var posts = new List<float> { -halfWidth, doorStart };
            for (int i = 0; i <= paneCount; i++) posts.Add(doorEnd + i * paneWidth);
            foreach (float x in posts)
                frontFrame.Box("Metal", new Vector3(x, 1.95f, 0), new Vector3(0.045f, 3.9f, 0.09f));
            foreach (float y in new[] { 0.06f, 3.87f })
                frontFrame.Box("Metal", new Vector3(0, y, 0), new Vector3(GlassOfficeWidth + 0.05f, 0.07f, 0.09f));
            frontFrame.Box("Metal", new Vector3(GlassOfficeDoorX, 2.68f, 0), new Vector3(1.25f, 0.055f, 0.09f));
            frontFrame.Box("GlassOfficeGlass", new Vector3(GlassOfficeDoorX, 3.275f, 0), new Vector3(1.2f, 1.12f, 0.025f), true);
            frontFrame.Box("Teal", new Vector3(0.8f, 3.43f, 0.05f), new Vector3(1.9f, 0.36f, 0.04f));
            frontFrame.Finish();
            Text(front, "Glass_office_sign", "OFFICE", new Vector3(0.8f, 3.43f, 0.08f), 1.45f, 0.24f, "White");

            var right = RightGroup(room, "02_RIGHT_glass_wall", new Vector3(-halfWidth, 0, -GlassOfficeDepth / 2));
            right.localRotation = Quaternion.Euler(0, 90, 0);
            for (int i = 0; i < 2; i++) GlassOfficePanel(right, "Right_side_pane_" + (i + 1), -2.3f + i * 2.3f, -2.3f + (i + 1) * 2.3f);
            var sideFrame = new Part("Glass_office_right_frame", right, "GlassOffice");
            foreach (float x in new[] { -2.3f, 0, 2.3f })
                sideFrame.Box("Metal", new Vector3(x, 1.95f, 0), new Vector3(0.045f, 3.9f, 0.09f));
            foreach (float y in new[] { 0.06f, 3.87f })
                sideFrame.Box("Metal", new Vector3(0, y, 0), new Vector3(4.65f, 0.07f, 0.09f));
            sideFrame.Finish();

            var entrance = RightGroup(room, "03_Open_entrance_1_25m", new Vector3(GlassOfficeDoorX, 0, 0));
            var door = RightGroup(entrance, "Glass_door_open_INWARD", new Vector3(-0.625f, 0, -0.07f));
            door.localRotation = Quaternion.Euler(0, 90, 0);
            var leaf = new Part("Glass_office_inward_open_door", door, "GlassOffice");
            leaf.Box("GlassOfficeGlass", new Vector3(0.625f, 1.32f, 0), new Vector3(1.19f, 2.57f, 0.025f), true);
            foreach (float x in new[] { 0.025f, 1.225f })
                leaf.Box("Metal", new Vector3(x, 1.32f, 0), new Vector3(0.04f, 2.64f, 0.065f));
            foreach (float y in new[] { 0.03f, 2.63f })
                leaf.Box("Metal", new Vector3(0.625f, y, 0), new Vector3(1.24f, 0.04f, 0.065f));
            leaf.Box("Silver", new Vector3(1.08f, 1.25f, 0.06f), new Vector3(0.03f, 0.4f, 0.035f));
            leaf.Finish();
            Text(entrance, "Entrance_label", "LỐI VÀO", new Vector3(0, 2.95f, 0.04f), 0.75f, 0.15f, "Teal");

            int workstationCount = dimensions.extendedGlassOfficeAndClosedRear ? 6 : 3;
            float furnitureSpan = GlassOfficeWidth - 4.2f;
            for (int i = 0; i < workstationCount; i++)
                BuildGlassWorkstation(room, i, -furnitureSpan / 2 + i * furnitureSpan / (workstationCount - 1));
            var storage = new Part("Glass_office_storage", room, "GlassOffice");
            storage.Box("Wood", new Vector3(0, 0.5f, -4.32f), new Vector3(GlassOfficeWidth - 1.5f, 1, 0.5f), true);
            storage.Box("White", new Vector3(0, 1.02f, -4.32f), new Vector3(GlassOfficeWidth - 1.45f, 0.04f, 0.52f));
            for (int shelf = 0; shelf < workstationCount; shelf++)
            {
                float x = -furnitureSpan / 2 + shelf * furnitureSpan / (workstationCount - 1);
                for (int i = 0; i < 4; i++)
                    storage.Box(i % 2 == 0 ? "Teal" : "Ivory", new Vector3(x + i * 0.1f, 1.23f, -4.32f), new Vector3(0.07f, 0.4f, 0.28f));
            }
            storage.Finish();
            var planter = RightGroup(room, "Indoor_planter", new Vector3(halfWidth - 0.8f, 0, -0.85f));
            planter.localScale = Vector3.one * 0.65f;
            Plant(planter, "Glass_office_plant", Vector3.zero, "GlassOffice");
            var ceiling = new Part("Glass_office_ceiling_lights", room, "GlassOffice");
            for (int i = 0; i < workstationCount; i++)
            {
                float x = -furnitureSpan / 2 + i * furnitureSpan / (workstationCount - 1);
                ceiling.Box("Metal", new Vector3(x, 3.88f, -2.3f), new Vector3(1.3f, 0.04f, 0.38f));
                ceiling.Box("WarmLight", new Vector3(x, 3.85f, -2.3f), new Vector3(1.22f, 0.02f, 0.3f));
                ServiceLight(room, "Office ceiling wash " + x, new Vector3(x, 3.45f, -2.3f), 5.2f, 1.15f);
            }
            ceiling.Finish();
        }

        private static void GlassOfficePanel(Transform parent, string name, float start, float end)
        {
            var part = new Part(name, parent, "GlassOffice");
            float width = end - start, center = (start + end) / 2;
            part.Box("GlassOfficeGlass", new Vector3(center, 1.95f, 0), new Vector3(width - 0.045f, 3.78f, 0.025f), true);
            // Small frosted safety band; upper and lower glass stay transparent.
            part.Box("White", new Vector3(center, 1.12f, 0.016f), new Vector3(width - 0.1f, 0.045f, 0.003f));
            part.Finish();
        }

        private static void BuildGlassWorkstation(Transform room, int index, float xPosition)
        {
            var station = RightGroup(room, "Workstation_0" + (index + 1), new Vector3(xPosition, 0, -3.15f));
            var desk = new Part("Glass_office_workstation_0" + (index + 1), station, "GlassOffice");
            desk.Box("Wood", new Vector3(0, 0.76f, 0), new Vector3(1.8f, 0.08f, 0.8f), true);
            foreach (float x in new[] { -0.78f, 0.78f })
                foreach (float z in new[] { -0.3f, 0.3f })
                    desk.Box("Metal", new Vector3(x, 0.36f, z), new Vector3(0.045f, 0.72f, 0.045f));
            desk.Box("OfficeCharcoal", new Vector3(0, 1.1f, -0.16f), new Vector3(0.65f, 0.4f, 0.05f));
            desk.Box("BlueGlass", new Vector3(0, 1.1f, -0.126f), new Vector3(0.57f, 0.32f, 0.012f));
            desk.Box("Metal", new Vector3(0, 0.88f, -0.16f), new Vector3(0.055f, 0.16f, 0.06f));
            desk.Box("OfficeCharcoal", new Vector3(0, 0.815f, 0.17f), new Vector3(0.48f, 0.025f, 0.17f));
            desk.Box("Ivory", new Vector3(-0.62f, 0.82f, 0.09f), new Vector3(0.28f, 0.045f, 0.35f));
            desk.Box("OfficeCharcoal", new Vector3(0, 0.48f, 0.95f), new Vector3(0.58f, 0.14f, 0.58f), true);
            desk.Box("OfficeCharcoal", new Vector3(0, 0.85f, 1.2f), new Vector3(0.58f, 0.63f, 0.1f));
            desk.Cylinder("Silver", new Vector3(0, 0.22f, 0.95f), new Vector3(0.07f, 0.22f, 0.07f));
            desk.Box("Metal", new Vector3(0, 0.07f, 0.95f), new Vector3(0.55f, 0.035f, 0.06f));
            desk.Box("Metal", new Vector3(0, 0.07f, 0.95f), new Vector3(0.06f, 0.035f, 0.55f));
            desk.Finish();
        }

        private static void SetupGlassOfficeViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "26_Glass_office_front", "27_Glass_office_right_corner", "28_Glass_office_inside", "29_Lifts_and_glass_office" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "26_Glass_office_front", new Vector3(-7.2f, 1.8f, GlassOfficeZ), new Vector3(-19.5f, 1.9f, GlassOfficeZ));
            Viewpoint(root.transform, "27_Glass_office_right_corner", new Vector3(RightFacadeX(dimensions) + 2, 1.8f, GlassOfficeZ + GlassOfficeWidth / 2 + 1), new Vector3(RightFacadeX(dimensions) - 2, 1.8f, GlassOfficeZ));
            Viewpoint(root.transform, "28_Glass_office_inside", new Vector3(RightFacadeX(dimensions) - 3.5f, 1.7f, GlassOfficeZ - GlassOfficeWidth / 2 + 1.5f), new Vector3(RightFacadeX(dimensions) - 1.7f, 1.8f, GlassOfficeZ + GlassOfficeWidth / 2 - 2));
            Viewpoint(root.transform, "29_Lifts_and_glass_office", new Vector3(-3, 2.1f, -33), new Vector3(-18.5f, 2, -33));
        }
    }
}
#endif
