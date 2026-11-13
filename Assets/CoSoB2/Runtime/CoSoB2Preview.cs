using UnityEngine;

namespace UEH.B2
{
    public class CoSoB2Preview : MonoBehaviour
    {
        public void View(int index)
        {
            Vector3[] positions = { new Vector3(35, 21, -36), new Vector3(22, 3, -12), new Vector3(-20, 16, 30), new Vector3(0, 2.2f, 13.5f), new Vector3(0, 1.7f, -5) };
            Vector3[] targets = { new Vector3(0, 5, 4), new Vector3(-2, 5, 0), new Vector3(0, 4, 9), new Vector3(4.2f, 2, 9), new Vector3(0, 2, 8) };
            transform.position = positions[index];
            transform.LookAt(targets[index]);
        }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 310, 235), GUI.skin.box);
            GUILayout.Label("CO SO B2 | VIDEO STUDY");
            GUILayout.Label("Estimated dimensions / hidden sides simplified");
            string[] views = { "1. Overview", "2. Front arcade", "3. Courtyard overview", "4. Courtyard windows", "5. Entrance passage" };
            for (int i = 0; i < views.Length; i++) if (GUILayout.Button(views[i], GUILayout.Height(28))) View(i);
            GUILayout.EndArea();
        }
    }
}
