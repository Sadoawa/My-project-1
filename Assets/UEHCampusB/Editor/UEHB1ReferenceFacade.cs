#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UEH.CampusB.Editor
{
    public static partial class UEHCampusBuilder
    {
        [MenuItem("UEH/Campus B/29. Refine B1 porch and stepped upper facade")]
        public static void RefineB1ReferenceFacadeAndCapture() => RefineB1FrameAndCapture();

        private static IEnumerable<Transform> GetDirectChildren(Transform root)
        { foreach (Transform child in root) yield return child; }

        private static void RemoveFloatingB1EntranceLabel()
        {
            string path = Root + "/Prefabs/UEH_LobbyB1.prefab";
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var label = contents.transform.Find("Entrance_label");
                if (label != null) UnityEngine.Object.DestroyImmediate(label.gameObject);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        private static void B1Cornice(Part p, float right, float left, float front, float rear, float y)
        {
            float width = left - right, depth = front - rear;
            p.Box("Ivory", new Vector3((left + right) / 2, y + 0.18f, (front + rear) / 2), new Vector3(width + 1.3f, 0.36f, depth + 1.3f));
            foreach (float z in new[] { front + 0.58f, rear - 0.58f })
            {
                p.Box("White", new Vector3((left + right) / 2, y + 0.4f, z), new Vector3(width + 1.5f, 0.10f, 0.32f));
                p.Box("B1Sandstone", new Vector3((left + right) / 2, y + 0.05f, z - 0.03f), new Vector3(width + 1.35f, 0.10f, 0.23f));
            }
            foreach (float x in new[] { right - 0.58f, left + 0.58f })
                p.Box("White", new Vector3(x, y + 0.4f, (front + rear) / 2), new Vector3(0.32f, 0.1f, depth + 1.5f));
        }

        private static void B1Rail(Part p, float a, float b, float y, float z)
        {
            p.Box("Metal", new Vector3((a + b) / 2, y + 1.34f, z), new Vector3(b - a, 0.065f, 0.07f));
            p.Box("White", new Vector3((a + b) / 2, y + 0.53f, z), new Vector3(b - a, 0.16f, 0.2f));
            for (float x = a; x <= b; x += 0.19f)
                p.Box("Silver", new Vector3(x, y + 0.91f, z), new Vector3(0.028f, 0.8f, 0.035f));
        }

        private static void B1Flowers(Part p, float x, float y, float z, bool trailing)
        {
            p.Box("Stone", new Vector3(x, y + 0.61f, z - 0.25f), new Vector3(1.8f, 0.4f, 0.6f));
            for (int i = 0; i < 11; i++)
            {
                var center = new Vector3(x - 0.75f + i * 0.15f, y + 1.15f + Mathf.Sin(i * 1.8f) * 0.23f, z - 0.2f);
                p.Sphere("Leaf", center, new Vector3(0.55f, 0.7f, 0.6f));
                for (int flower = 0; flower < 7; flower++)
                {
                    float angle = flower * 2.4f + i;
                    p.Sphere("Flower", center + new Vector3(Mathf.Cos(angle) * 0.23f, 0.16f + Mathf.Sin(angle) * 0.18f, 0.21f + flower % 2 * 0.11f), new Vector3(0.15f, 0.13f, 0.14f));
                }
                if (trailing && i % 2 == 0)
                    for (int drop = 0; drop < 4; drop++)
                    {
                        Vector3 stem = center + new Vector3(Mathf.Sin(i + drop) * 0.09f, -drop * 0.16f, 0.39f);
                        p.Sphere("Leaf", stem, new Vector3(0.22f, 0.24f, 0.21f));
                        p.Sphere("Flower", stem + new Vector3(0.07f, 0, 0.1f), new Vector3(0.13f, 0.12f, 0.14f));
                    }
            }
        }

        private static void B1WindowGrid(Part p, float a, float b, float bottom, float top, float z, float module, string frameMaterial = "Silver", float outward = 1f)
        {
            p.Box("BlueGlass", new Vector3((a + b) / 2, (top + bottom) / 2, z - 0.12f * outward), new Vector3(b - a, top - bottom, 0.09f));
            int count = Mathf.Max(1, Mathf.RoundToInt((b - a) / module));
            for (int i = 0; i <= count; i++)
                p.Box(frameMaterial, new Vector3(Mathf.Lerp(a, b, (float)i / count), (top + bottom) / 2, z), new Vector3(0.07f, top - bottom, 0.12f));
            for (int row = 0; row <= 2; row++)
                p.Box(frameMaterial, new Vector3((a + b) / 2, Mathf.Lerp(bottom, top, row / 2f), z), new Vector3(b - a, 0.065f, 0.12f));
        }

        private static void BuildB1ReferenceEnvelope(Transform parent, float h, float right, float left, float rear)
        {
            float front = B1FrameFrontZ, depth = front - rear, span = left - right;
            float centerX = (left + right) / 2, centerZ = (front + rear) / 2;
            var cladding = new Part("02_Podium_structure", parent, "B1Frame");
            foreach (var f in ServiceExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, h)) cladding.Box("Ivory", f.Key, f.Value);
            foreach (var f in OfficeExteriorWallFragments(dimensions.frontage, dimensions.exteriorDepth, h)) cladding.Box("Ivory", f.Key, f.Value);
            cladding.Finish();
            var root = RightGroup(parent, "18_B1_structural_frame", Vector3.zero);
            var members = new Part("01_Frame_columns_and_ring_beams", root, "B1Frame");
            foreach (float x in B1FrontPierAxes) AddB1FramePier(members, x, front, true, 0);
            var depths = B1SidePierDepths;
            for (int i = 1; i < depths.Length; i++)
            {
                AddB1FramePier(members, right, depths[i], false, -1);
                AddB1FramePier(members, left, depths[i], false, 1);
            }
            for (int i = 1; i < B1FrontPierAxes.Length - 1; i++) AddB1FramePier(members, B1FrontPierAxes[i], rear, false, 0);
            foreach (float y in new[] { h, B1FrameTop })
            {
                foreach (float z in new[] { front, rear })
                    members.Box("White", new Vector3(centerX, y + 0.05f, z), new Vector3(span + 0.6f, 0.5f, 0.8f));
                foreach (float x in new[] { right, left })
                    members.Box("White", new Vector3(x, y + 0.05f, centerZ), new Vector3(0.8f, 0.5f, depth));
                for (int i = 1; i < depths.Length - 1; i++)
                    members.Box("Ivory", new Vector3(centerX, y + 0.1f, depths[i]), new Vector3(span, 0.3f, 0.4f));
                foreach (float x in new[] { -4.5f, 4.5f, LeftGateColumnX(dimensions) })
                    members.Box("Ivory", new Vector3(x, y + 0.1f, centerZ), new Vector3(0.38f, 0.4f, depth));
            }
            members.Finish();
            BuildB1PodiumDecks(root, h, right, left, rear);
            BuildB1LowerFacade(root, h, right, left, rear);
            BuildB1SteppedTerraces(root, right, left, rear);
            var paving = new Part("04_Side_and_rear_paving", root, "B1Frame");
            float half = dimensions.frontage / 2;
            foreach (int side in new[] { -1, 1 })
            {
                float edge = side < 0 ? right - 0.9f : left + 0.9f;
                float inner = side * half;
                paving.Box("Pavement", new Vector3((edge + inner) / 2, -0.15f, centerZ), new Vector3(Mathf.Abs(edge - inner), 0.3f, depth + 1.8f), true);
            }
            float extra = -rear - dimensions.lobbyDepth + 0.9f;
            paving.Box("Pavement", new Vector3(centerX, -0.15f, -dimensions.lobbyDepth - extra / 2), new Vector3(span + 1.8f, 0.3f, extra), true);
            // Finish the floor under the deeper porch between the building glass and the new front axis.
            paving.Box("Pavement", new Vector3(centerX, -0.15f, (front + EntranceGlassZ(dimensions)) / 2), new Vector3(span + 1.8f, 0.3f, front - EntranceGlassZ(dimensions)), true);
            paving.Finish();
            BuildB1CanopyName(parent, right, left);
            BuildB1FloorClosures(parent);

        }

        private static void BuildB1LowerFacade(Transform root, float h, float right, float left, float rear)
        {
            float front = B1FrameFrontZ;
            var depths = B1SidePierDepths;
            var facade = new Part("03_Balconies_louvres_and_windows", root, "B1Frame");
            var axes = B1FrontPierAxes;
            float bayTop = B1FrameTop - 0.35f;
            // Continuous rear infill seals the narrow joint behind the middle structural pier.
            facade.Box("Ivory", new Vector3((axes[1] + axes[3]) / 2, (h + bayTop) / 2, front - 4.32f), new Vector3(axes[3] - axes[1] - 0.6f, bayTop - h, 0.2f));
            // A single tall screen above OCB, rather than a stack of repeated glazed storeys.
            float a = axes[3] + 0.52f, b = axes[4] - 0.52f, screenZ = front - 0.25f;
            facade.Box("Dark", new Vector3((a + b) / 2, (h + bayTop) / 2, screenZ - 0.25f), new Vector3(b - a, bayTop - h, 0.1f));
            for (float y = h + 0.5f; y < bayTop - 0.12f; y += 0.16f)
                facade.Box("Silver", new Vector3((a + b) / 2, y, screenZ), new Vector3(b - a, 0.065f, 0.2f));
            facade.Box("Orange", new Vector3((a + b) / 2, bayTop - 0.05f, screenZ + 0.02f), new Vector3(b - a, 0.14f, 0.23f));
            foreach (float x in new[] { a, (a + b) / 2, b })
                facade.Box("Metal", new Vector3(x, (h + bayTop) / 2, screenZ + 0.03f), new Vector3(0.065f, bayTop - h, 0.16f));
            // Deep open central balcony, backed by recessed doors and a warm timber ceiling.
            for (int bay = 1; bay < 3; bay++)
            {
                a = axes[bay] + 0.55f; b = axes[bay + 1] - 0.55f;
                B1Rail(facade, a, b, h, front - 0.1f);
                for (float x = a + 1.3f; x < b - 0.5f; x += 2.7f) B1Flowers(facade, x, h, front - 0.16f, true);
            }
            BuildB1BalconyClosuresAndGlazing(root, h, front, axes);
            BuildB1FourRightWindowStacks(root, h, front, axes[0], axes[1]);
            float midFloor = h + (B1FrameTop - h) / 2;
            // Glazed returns follow the complete 52m envelope; the ground-level rear stays open.
            for (int i = 0; i < depths.Length - 1; i++)
            {
                float za = depths[i] - 0.6f, zb = depths[i + 1] + 0.6f;
                foreach (int side in new[] { -1, 1 })
                {
                    float x = side < 0 ? right + 0.12f : left - 0.12f;
                    facade.Box("BlueGlass", new Vector3(x, (h + bayTop) / 2, (za + zb) / 2), new Vector3(0.08f, bayTop - h - 0.4f, za - zb));
                    for (float z = zb; z < za; z += 1.8f)
                        facade.Box("White", new Vector3(x + side * 0.08f, (h + bayTop) / 2, z), new Vector3(0.16f, bayTop - h, 0.12f));
                    facade.Box("White", new Vector3(x + side * 0.08f, midFloor, (za + zb) / 2), new Vector3(0.16f, 0.25f, za - zb));
                }
            }
            facade.Finish();
        }

        private static void BuildB1SteppedTerraces(Transform parent, float right, float left, float rear)
        {
            // A shallow setback on both open floors keeps a broad podium below the narrower tower.
            right += B1UpperTerraceInset;
            left -= B1UpperTerraceInset;
            rear += B1UpperTerraceInset;
            var terrace = new Part("06_Stepped_open_terraces", parent, "B1Reference");
            for (int level = 0; level < 2; level++)
            {
                float y = B1FrameTop + level * 3.6f;
                float front = B1FrameFrontZ - B1UpperTerraceInset;
                float r = right, l = left;
                if (level > 0) B1Cornice(terrace, r, l, front, rear, y);
                float back = front - 3.65f;
                terrace.Box("Ivory", new Vector3((r + l) / 2, y + 1.9f, back - 0.35f), new Vector3(l - r, 3.25f, 0.2f));
                B1WindowGrid(terrace, -4.0f, l - 0.7f, y + 0.5f, y + 3.15f, back + 0.02f, 1.5f);
                B1Rail(terrace, -4.0f, l - 0.6f, y, front - 0.10f);
                foreach (float x in new[] { -4.5f, 4.5f, 14.7f, l })
                    terrace.Box("Ivory", new Vector3(x, y + 1.95f, front - 0.10f), new Vector3(0.65f, 3.3f, 0.65f));
                for (float x = -1.5f; x < l - 1.5f; x += 4.5f) B1Flowers(terrace, x, y, front - 0.25f, false);
                B1WindowGrid(terrace, r + 0.5f, -5.0f, y + 0.5f, y + 3.15f, front - 0.45f, 1.4f);
                foreach (float x in new[] { r, (r - 4.5f) / 2, -4.5f })
                    terrace.Box("White", new Vector3(x, y + 1.9f, front - 0.15f), new Vector3(0.5f, 3.4f, 0.5f));
                // Both open floors share a mildly recessed outline; their columns and rails align vertically.
                foreach (float x in new[] { r, l })
                {
                    for (float z = -8; z > rear + 1; z -= 8)
                        terrace.Box("Ivory", new Vector3(x, y + 1.95f, z), new Vector3(0.65f, 3.3f, 0.65f));
                    float railX = x == r ? r - 0.12f : l + 0.12f;
                    terrace.Box("Metal", new Vector3(railX, y + 1.34f, (front + rear) / 2), new Vector3(0.07f, 0.065f, front - rear));
                    terrace.Box("White", new Vector3(railX, y + 0.53f, (front + rear) / 2), new Vector3(0.2f, 0.16f, front - rear));
                    for (float z = rear + 0.25f; z < front; z += 0.24f)
                        terrace.Box("Silver", new Vector3(railX, y + 0.91f, z), new Vector3(0.035f, 0.8f, 0.028f));
                }
                B1Rail(terrace, r, l, y, rear);
            }
            B1Cornice(terrace, right, left, B1FrameFrontZ - B1UpperTerraceInset, rear, B1FrameTop + 7.2f);
            terrace.Finish();
        }

        private static void BuildB1ReferenceTower(Transform parent)
        {
            float bottom = B1FrameTop + 7.65f, top = dimensions.towerHeight;
            float width = dimensions.frontage - 3.5f, half = width / 2;
            float front = B1TowerFrontZ, depth = B1TowerDepth;
            int floors = dimensions.upperFloors;
            float step = (top - bottom) / floors;
            var shell = new Part("06_Tower_shell_reference", parent, "B1Reference");
            // Both facade zones share the same rear edge; only the balcony glazing is recessed.
            const float split = -1.8f;
            float rear = front - depth;
            shell.Box("BlueGlass", new Vector3((-half + split) / 2, (bottom + top) / 2, (front - 0.10f + rear) / 2), new Vector3(split + half, top - bottom, depth - 0.10f));
            shell.Box("BlueGlass", new Vector3((split + half) / 2, (bottom + top) / 2, (front - 1.5f + rear) / 2), new Vector3(half - split, top - bottom, depth - 1.5f));
            shell.Box("Ivory", new Vector3(0, top + 0.25f, front - depth / 2), new Vector3(width + 0.8f, 0.5f, depth + 1.4f));
            shell.Finish();
            var facade = new Part("07_Tower_horizontal_bands_and_vertical_window_bays", parent, "B1Reference");
            // Viewer-left: deep horizontal balconies. Viewer-right: full-height vertical window stacks.
            for (int floor = 0; floor < floors; floor++)
            {
                float y = bottom + floor * step;
                bool crown = floor >= floors - 2;
                if (!crown)
                {
                    float terraceFront = front + 1.2f;
                    facade.Box("Ivory", new Vector3((split + half) / 2, y + 0.2f, front - 0.6f), new Vector3(half - split, 0.4f, 3.5f));
                    facade.Box("Ivory", new Vector3((split + half) / 2, y + 0.8f, terraceFront), new Vector3(half - split, 0.85f, 0.22f));
                    facade.Box("White", new Vector3((split + half) / 2, y + 1.27f, terraceFront + 0.09f), new Vector3(half - split + 0.4f, 0.12f, 0.45f));
                    B1WindowGrid(facade, split + 0.3f, half - 0.3f, y + 0.5f, y + step - 0.25f, front - 1.2f, 1.3f);
                    for (float x = split + 0.5f; x < half; x += 4.3f)
                        facade.Box("Ivory", new Vector3(x, y + step / 2, front - 0.65f), new Vector3(0.3f, step, 0.3f));
                }
                float end = crown ? half : split;
                B1WindowGrid(facade, -half + 0.6f, end - 0.4f, y + 0.45f, y + step - 0.3f, front + 0.02f, 1.0f);
                facade.Box("Ivory", new Vector3((-half + end) / 2, y + 0.15f, front + 0.06f), new Vector3(end + half, 0.3f, 0.4f));
                // Side elevations keep slim glazing mullions and continuous floor edges.
                foreach (float x in new[] { -half, half })
                {
                    facade.Box("Ivory", new Vector3(x, y + 0.15f, front - depth / 2), new Vector3(0.25f, 0.3f, depth));
                    for (float z = front - depth; z < front; z += 3.2f)
                        facade.Box("White", new Vector3(x, y + step / 2, z), new Vector3(0.18f, step, 0.22f));
                }
            }
            // Complete the lengthened rear elevation at the same floor levels.
            for (int floor = 0; floor < floors; floor++)
            {
                float y = bottom + floor * step;
                B1WindowGrid(facade, -half + 0.45f, half - 0.45f, y + 0.4f, y + step - 0.2f, rear - 0.05f, 2.4f, "White", -1f);
                facade.Box("Ivory", new Vector3(0, y + 0.15f, rear - 0.08f), new Vector3(width + 0.25f, 0.3f, 0.35f));
            }
            foreach (float x in new[] { -half, -half + (split + half) / 3, -half + (split + half) * 2 / 3, split, half })
                facade.Box("Ivory", new Vector3(x, (bottom + top) / 2, front + 0.10f), new Vector3(0.95f, top - bottom, 0.5f));
            float crownBottom = bottom + (floors - 2) * step;
            foreach (float x in new[] { 3.0f, 8.8f })
                facade.Box("Ivory", new Vector3(x, (crownBottom + top) / 2, front + 0.10f), new Vector3(0.95f, top - crownBottom, 0.5f));
            facade.Finish();
        }

        private static void SetupB1FacadeViewpoints(Transform views)
        {
            SetupB1FloorClosureViewpoints(views);
            SetupB1TowerProportionViewpoints(views);
            SetupB1RaisedSignViewpoints(views);
            SetupB1BalconyDetailViewpoints(views);
            SetupB1FourthFloorViewpoints(views);
            SetupB1CanopyViewpoints(views);
            foreach (Transform child in new List<Transform>(GetDirectChildren(views)))
                if (child.name.StartsWith("88_") || child.name.StartsWith("89_") || child.name.StartsWith("90_") || child.name.StartsWith("91_") || child.name.StartsWith("92_") || child.name.StartsWith("93_")) UnityEngine.Object.DestroyImmediate(child.gameObject);
            Viewpoint(views, "88_B1_refined_front", new Vector3(2, 24, 61), new Vector3(1, 25, -8));
            Viewpoint(views, "89_B1_reference_upper_storeys", new Vector3(28, 11, 28), new Vector3(0, 20, -1));
            Viewpoint(views, "90_B1_deeper_porch_columns", new Vector3(20, 1.7f, 0.6f), new Vector3(-8, 2.2f, 3.0f));
            Viewpoint(views, "91_B1_entrance_without_floating_labels", new Vector3(0, 1.7f, 6.0f), new Vector3(0, 2.7f, -12));
            Viewpoint(views, "92_B1_stepped_terraces_and_screen", new Vector3(2, 10, 34), new Vector3(1, 12, -1));
            Viewpoint(views, "93_B1_full_depth_refined_frame", new Vector3(60, 21, -24), new Vector3(12, 7, -24));
        }

        private static void CaptureB1FacadePreviews()
        {
            string output = Path.GetFullPath("Docs/UEH-CampusB/Previews");
            foreach (Transform point in GameObject.Find("Review viewpoints [select and align view]").transform)
            {
                int number;
                if (point.name.Length < 3 || !int.TryParse(point.name.Substring(0, 2), out number) || number < 88 || number > 93) continue;
                var go = new GameObject("Temporary B1 facade camera");
                var camera = go.AddComponent<Camera>(); camera.enabled = false;
                camera.transform.SetPositionAndRotation(point.position, point.rotation);
                camera.fieldOfView = number == 88 ? 64 : number == 89 ? 68 : number == 90 ? 76 : number == 91 ? 65 : number == 92 ? 62 : 68;
                camera.nearClipPlane = 0.08f; camera.farClipPlane = 250;
                var target = new RenderTexture(1600, 1200, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                var prior = RenderTexture.active; camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, point.name + ".png"), texture.EncodeToPNG());
                camera.targetTexture = null; RenderTexture.active = prior; target.Release();
                UnityEngine.Object.DestroyImmediate(texture); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
            }
        }
    }
}
#endif
