#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        [MenuItem("UEH/Campus B/9. Furnish left office and green timber lounge")]
        public static void FurnishLeftAreas()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before furnishing the left areas.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            dimensions = AssetDatabase.LoadAssetAtPath<UEHDimensions>(Root + "/Settings/EstimatedDimensions.asset");
            if (dimensions == null || !dimensions.expandedLobby)
                throw new InvalidOperationException("Expand the lobby before furnishing these bays.");
            materials.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { Root + "/Materials" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                materials[material.name] = material;
            }
            signFont = AssetDatabase.LoadAssetAtPath<Font>(Root + "/Fonts/LiberationSans.ttf");
            if (signFont == null) throw new FileNotFoundException("Missing UEH sign font.");
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var areas = contents.transform.Find("09_Four_expansion_areas");
                if (areas == null) throw new InvalidOperationException("Missing expanded side bays.");
                PopulateLeftOfficeAndLounge(areas);
                dimensions.leftOfficeAndLounge = true;
                RebuildOfficeWallOpening(contents.transform);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                EditorUtility.SetDirty(dimensions);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
            TrimOfficeExteriorGroundWall();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            SetupLeftAreaViewpoints();
            Physics.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("UEH_LEFT_AREAS_UPDATED LEFT_01: grey office, glazed service window, blue perimeter, open left doorway, workstations and black waiting chairs. LEFT_02: greenery on viewer-left, timber wall and two chairs.");
        }

        private static void PopulateLeftOfficeAndLounge(Transform areas)
        {
            var officeBay = areas.Find("LEFT_01 [12m x 6m]");
            var loungeBay = areas.Find("LEFT_02 [12m x 6m]");
            if (officeBay == null || loungeBay == null) throw new InvalidOperationException("Missing LEFT_01/02 bays.");
            Mat("OfficeGrey", "858B8D", 0.22f);
            Mat("OfficeInterior", "D8DEDC", 0.22f);
            Mat("OfficeCharcoal", "222E35", 0.25f);
            Mat("OfficeLeafDark", "234E2D", 0.1f);
            var glassPath = Root + "/Materials/OfficeGlass.mat";
            var glass = AssetDatabase.LoadAssetAtPath<Material>(glassPath);
            if (glass == null)
            {
                glass = new Material(materials["ClearGlass"]) { name = "OfficeGlass" };
                AssetDatabase.CreateAsset(glass, glassPath);
            }
            glass.color = new Color(0.57f, 0.68f, 0.69f, 0.24f);
            glass.SetFloat("_Glossiness", 0.75f);
            materials["OfficeGlass"] = glass;
            EditorUtility.SetDirty(glass);

            foreach (var bay in new[] { officeBay, loungeBay })
            {
                foreach (string name in new[] { "Bay_frame_LEFT_01", "Bay_frame_LEFT_02", "Office_room", "Green_timber_lounge" })
                { var old = bay.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
                bay.GetComponentInChildren<UEHReservedSpace>().furnished = true;
            }
            officeBay.GetComponentInChildren<UEHReservedSpace>().purpose = "Office và ghế chờ; khoang 12 x 6 m đã bố trí. Cửa bên trái mở để đi vào.";
            loungeBay.GetComponentInChildren<UEHReservedSpace>().purpose = "Mảng cây xanh bên trái, vách gỗ và hai ghế; khoang 12 x 6 m đã bố trí.";
            BuildLeftOffice(officeBay);
            BuildGreenTimberLounge(loungeBay);
            var proxy = areas.parent.Find("06_1976_LEFT_from_gate/06_Heritage_1976_and_gallery/05_Inner_service_blue_outline");
            if (proxy != null) UnityEngine.Object.DestroyImmediate(proxy.gameObject);
        }

        private static void BuildLeftOffice(Transform bay)
        {
            var room = new GameObject("Office_room").transform;
            room.SetParent(bay, false);
            const float front = 2.8f;
            var shell = new Part("Office_shell", room, "Office");
            shell.Box("OfficeInterior", new Vector3(0, 2, -0.05f), new Vector3(10.2f, 4, 0.16f), true);
            foreach (int side in new[] { -1, 1 })
                shell.Box("OfficeInterior", new Vector3(side * 5.1f, 2, 1.4f), new Vector3(0.16f, 4, 2.8f), true);
            shell.Box("White", new Vector3(0, 4.05f, 1.4f), new Vector3(10.35f, 0.14f, 3.0f), true);
            shell.Finish();

            var facade = new Part("Office_grey_facade", room, "Office");
            // Window and door openings are built as actual gaps in the wall.
            facade.Box("OfficeGrey", new Vector3(0, 0.625f, front), new Vector3(6.8f, 1.25f, 0.2f), true);
            facade.Box("OfficeGrey", new Vector3(0, 3.5f, front), new Vector3(6.8f, 1.0f, 0.2f), true);
            foreach (int side in new[] { -1, 1 })
            {
                facade.Box("OfficeGrey", new Vector3(side * 3.0f, 2.125f, front), new Vector3(0.8f, 1.75f, 0.2f), true);
                facade.Box("OfficeGrey", new Vector3(side * 3.5f, 2, front), new Vector3(0.2f, 4, 0.2f), true);
                facade.Box("OfficeGrey", new Vector3(side * 4.95f, 2, front), new Vector3(0.3f, 4, 0.2f), true);
                facade.Box("OfficeGrey", new Vector3(side * 4.2f, 3.5f, front), new Vector3(1.2f, 1, 0.2f), true);
            }
            facade.Finish();

            var window = new Part("Office_service_window", room, "Office");
            for (int pane = 0; pane < 4; pane++)
            {
                float x = -1.95f + pane * 1.3f;
                window.Box("OfficeGlass", new Vector3(x, 1.88f, front + 0.015f), new Vector3(1.24f, 1.18f, 0.025f), true);
                window.Box("OfficeGlass", new Vector3(x, 2.75f, front + 0.015f), new Vector3(1.24f, 0.44f, 0.025f), true);
            }
            foreach (float x in new[] { -2.6f, -1.3f, 0, 1.3f, 2.6f })
                window.Box("OfficeCharcoal", new Vector3(x, 2.125f, front + 0.05f), new Vector3(0.065f, 1.82f, 0.09f));
            foreach (float y in new[] { 1.25f, 2.5f, 3.0f })
                window.Box("OfficeCharcoal", new Vector3(0, y, front + 0.05f), new Vector3(5.28f, 0.065f, 0.09f));
            window.Box("Silver", new Vector3(0, 1.24f, front + 0.11f), new Vector3(5.35f, 0.06f, 0.36f));
            window.Finish();

            // Looking at this wall, screen-left is local +X. Its 1.2m doorway is open.
            var openDoor = new GameObject("Office_open_left_doorway").transform;
            openDoor.SetParent(room, false);
            openDoor.localPosition = new Vector3(4.2f, 0, front);
            var jamb = new Part("Office_open_door_frame", openDoor, "Office");
            foreach (int side in new[] { -1, 1 })
                jamb.Box("OfficeCharcoal", new Vector3(side * 0.61f, 1.5f, 0), new Vector3(0.06f, 3, 0.15f));
            jamb.Box("OfficeCharcoal", new Vector3(0, 3, 0), new Vector3(1.28f, 0.07f, 0.15f));
            // Door leaf parked along the inner side wall, outside the walking opening.
            jamb.Box("OfficeCharcoal", new Vector3(0.76f, 1.4f, -0.63f), new Vector3(0.06f, 2.8f, 1.15f), true);
            jamb.Finish();
            var door = new Part("Office_closed_right_door", room, "Office");
            door.Box("OfficeCharcoal", new Vector3(-4.2f, 1.5f, front), new Vector3(1.2f, 3, 0.09f), true);
            door.Box("BlueGlass", new Vector3(-4.2f, 1.95f, front + 0.06f), new Vector3(0.77f, 1.7f, 0.025f));
            door.Box("Silver", new Vector3(-3.77f, 1.35f, front + 0.12f), new Vector3(0.035f, 0.5f, 0.04f));
            door.Finish();

            var led = new Part("Office_blue_perimeter", room, "Office");
            led.Box("OfficeCharcoal", new Vector3(0, 4.08f, front + 0.05f), new Vector3(10.55f, 0.16f, 0.18f));
            led.Box("CoolLight", new Vector3(0, 4.08f, front + 0.16f), new Vector3(10.5f, 0.07f, 0.06f));
            foreach (int side in new[] { -1, 1 })
            {
                led.Box("OfficeCharcoal", new Vector3(side * 5.2f, 2.05f, front + 0.05f), new Vector3(0.16f, 4.1f, 0.18f));
                led.Box("CoolLight", new Vector3(side * 5.2f, 2.05f, front + 0.16f), new Vector3(0.07f, 4.1f, 0.06f));
            }
            led.Box("OfficeCharcoal", new Vector3(0, 4.09f, front + 0.21f), new Vector3(1.7f, 0.22f, 0.035f));
            led.Finish();
            Text(room, "Office_sign", "OFFICE", new Vector3(0, 4.08f, front + 0.25f), 1.25f, 0.15f, "White");
            Text(room, "Office_exit_sign", "EXIT", new Vector3(4.2f, 3.25f, front + 0.13f), 0.55f, 0.14f, "LeafLight");

            foreach (float x in new[] { -1.55f, 1.55f }) BuildOfficeWorkstation(room, x);
            var storage = new Part("Office_back_storage", room, "Office");
            storage.Box("OfficeInterior", new Vector3(0, 0.55f, 0.25f), new Vector3(6.1f, 1.1f, 0.5f), true);
            storage.Box("Wood", new Vector3(0, 1.12f, 0.25f), new Vector3(6.16f, 0.05f, 0.55f));
            for (int i = 0; i < 12; i++)
                storage.Box(i % 3 == 0 ? "Teal" : "OfficeCharcoal", new Vector3(-2.7f + i * 0.15f, 1.4f, 0.2f), new Vector3(0.09f, 0.5f, 0.28f));
            storage.Finish();
            BuildWaitingChair(room, "Office_waiting_chair_01", -2.7f, 4.15f, true);
            BuildWaitingChair(room, "Office_waiting_chair_02", -4.05f, 4.15f, true);
            var bin = new Part("Office_small_bin", room, "Office");
            bin.Cylinder("OfficeCharcoal", new Vector3(-1.35f, 0.25f, 3.7f), new Vector3(0.38f, 0.25f, 0.38f), true);
            bin.Finish();
            AddOfficeLight(room, "Office interior light", new Vector3(0, 3.65f, 1.4f), new Color(1, 0.95f, 0.86f), 1.0f, 6);
            foreach (int side in new[] { -1, 1 })
                AddOfficeLight(room, "Office blue wash " + side, new Vector3(side * 4.9f, 1.8f, 3.1f), new Color(0.18f, 0.57f, 1), 0.45f, 3);
            AlignOfficeBehindWall(room);
        }

        private static void BuildOfficeWorkstation(Transform room, float x)
        {
            var workstation = new GameObject("Workstation_" + (x < 0 ? "01" : "02")).transform;
            workstation.SetParent(room, false);
            workstation.localPosition = new Vector3(x, 0, 1.35f);
            var desk = new Part("Office_desk_" + (x < 0 ? "01" : "02"), workstation, "Office");
            desk.Box("Wood", new Vector3(0, 0.76f, 0), new Vector3(1.7f, 0.08f, 0.75f), true);
            foreach (float legX in new[] { -0.72f, 0.72f })
                foreach (float legZ in new[] { -0.27f, 0.27f })
                    desk.Box("Silver", new Vector3(legX, 0.36f, legZ), new Vector3(0.05f, 0.72f, 0.05f));
            desk.Box("OfficeCharcoal", new Vector3(0, 1.09f, 0.1f), new Vector3(0.62f, 0.39f, 0.045f));
            desk.Box("BlueGlass", new Vector3(0, 1.09f, 0.125f), new Vector3(0.55f, 0.32f, 0.01f));
            desk.Box("OfficeCharcoal", new Vector3(0, 0.87f, 0.1f), new Vector3(0.05f, 0.15f, 0.06f));
            desk.Box("OfficeCharcoal", new Vector3(0, 0.815f, -0.13f), new Vector3(0.5f, 0.025f, 0.18f));
            desk.Box("OfficeCharcoal", new Vector3(0, 0.47f, -0.65f), new Vector3(0.58f, 0.13f, 0.55f), true);
            desk.Box("OfficeCharcoal", new Vector3(0, 0.85f, -0.89f), new Vector3(0.58f, 0.64f, 0.1f));
            desk.Cylinder("Silver", new Vector3(0, 0.22f, -0.65f), new Vector3(0.065f, 0.22f, 0.065f));
            desk.Finish();
        }

        private static void BuildWaitingChair(Transform parent, string name, float x, float z, bool upholstered)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            group.localPosition = new Vector3(x, 0, z);
            string seat = upholstered ? "OfficeCharcoal" : "Wood";
            var chair = new Part(name, group, "Office");
            float width = upholstered ? 1.05f : 0.65f;
            chair.Box(seat, new Vector3(0, 0.48f, 0), new Vector3(width, upholstered ? 0.24f : 0.1f, 0.75f), true);
            chair.Box(seat, new Vector3(0, 0.83f, -0.33f), new Vector3(width, 0.65f, 0.14f));
            foreach (int side in new[] { -1, 1 })
            {
                if (upholstered) chair.Box(seat, new Vector3(side * width / 2, 0.68f, 0), new Vector3(0.13f, 0.33f, 0.8f));
                foreach (float legZ in new[] { -0.29f, 0.29f })
                    chair.Box("OfficeCharcoal", new Vector3(side * width * 0.4f, 0.22f, legZ), new Vector3(0.045f, 0.44f, 0.045f));
            }
            chair.Finish();
        }

        private static void BuildGreenTimberLounge(Transform bay)
        {
            var lounge = new GameObject("Green_timber_lounge").transform;
            lounge.SetParent(bay, false);
            var green = HeritageSection(lounge, "01_Greenery_LEFT", -4.2f);
            var leaves = new Part("Lounge_greenery", green, "Office");
            leaves.Box("OfficeLeafDark", new Vector3(0, 1.95f, 0), new Vector3(1.8f, 3.6f, 0.12f));
            for (int row = 0; row < 15; row++)
                for (int col = 0; col < 7; col++)
                {
                    int i = row * 7 + col;
                    leaves.Leaf(i % 4 == 0 ? "LeafLight" : i % 3 == 0 ? "OfficeLeafDark" : "Leaf",
                        new Vector3(-0.69f + col * 0.23f, 0.29f + row * 0.235f, 0.085f + i % 3 * 0.01f), new Vector2(0.34f, 0.4f), i * 137.5f);
                }
            leaves.Box("OfficeCharcoal", new Vector3(0, 0.18f, 0.2f), new Vector3(1.85f, 0.36f, 0.5f), true);
            leaves.Finish();
            var timber = HeritageSection(lounge, "02_Timber_wall_RIGHT", 0.95f);
            var panel = new Part("Lounge_timber_panel", timber, "Office");
            panel.Box("Wood", new Vector3(0, 2.1f, -0.04f), new Vector3(8.3f, 3.3f, 0.16f));
            foreach (float x in new[] { -4.1f, 4.1f })
                panel.Box("Border", new Vector3(x, 2.1f, 0.065f), new Vector3(0.06f, 3.25f, 0.045f));
            foreach (float y in new[] { 0.48f, 3.72f })
                panel.Box("Border", new Vector3(0, y, 0.065f), new Vector3(8.3f, 0.06f, 0.045f));
            panel.Finish();
            BuildWaitingChair(lounge, "Lounge_chair_01", -2.6f, 2.2f, false);
            BuildWaitingChair(lounge, "Lounge_chair_02", -4.0f, 2.2f, false);
        }

        private static void AddOfficeLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var light = new GameObject(name).AddComponent<Light>();
            light.transform.SetParent(parent, false); light.transform.localPosition = position;
            light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range;
            light.shadows = LightShadows.None;
        }

        private static void SetupLeftAreaViewpoints()
        {
            var root = GameObject.Find("Review viewpoints [select and align view]");
            if (root == null) return;
            foreach (string name in new[] { "15_Office_front", "16_Office_window_detail", "17_Green_timber_lounge", "18_Left_side_sequence", "19_Office_flush_wall", "20_Office_inside" })
            { var old = root.transform.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject); }
            Viewpoint(root.transform, "15_Office_front", new Vector3(6.8f, 1.7f, -23), new Vector3(15, 2, -23));
            Viewpoint(root.transform, "16_Office_window_detail", new Vector3(11.4f, 2, -21), new Vector3(15.5f, 2, -23));
            Viewpoint(root.transform, "17_Green_timber_lounge", new Vector3(7, 1.7f, -37), new Vector3(17, 2, -37));
            Viewpoint(root.transform, "18_Left_side_sequence", new Vector3(2, 2.1f, -22), new Vector3(17.4f, 2, -25));
            Viewpoint(root.transform, "19_Office_flush_wall", new Vector3(9, 1.75f, -14.5f), new Vector3(17.4f, 2.1f, -24));
            Viewpoint(root.transform, "20_Office_inside", new Vector3(19.4f, 1.65f, -18.8f), new Vector3(18.8f, 1.7f, -23.5f));
        }
    }
}
#endif
