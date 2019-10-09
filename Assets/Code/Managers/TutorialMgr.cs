using ProjectQuorum.Tools;
using UnityEngine;

namespace ProjectQuorum.Managers
{

    public class TutorialMgr : MonoBehaviour
    {
        static public TutorialMgr Instance;

        public Material material;

        [Space]
        [Header("Paint Tool")]
        public Color PaintHighlightColor;

        public Color PaintCenterColor;

        public int PaintRadius = 2;

        [Space]
        [Header("Neuron Tool")]
        public Color NeuronHighlightColor;
        public Color NeuronSecondaryColor;

        public int MinNeigbors = 3;

        public int NeuronRadius = 2;

        public int NeuronSecondaryRadius = 2;

        [Space]
        public float maxScale = 1f;

        [HideInInspector]
        public bool dirty;

        private bool _assigned;

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            if (dirty) { AssignImage(); }
        }

        public void LateUpdate()
        {
            dirty = !_assigned;
        }

        public void Hide()
        {
            Debug.Log("Hiding tutorial overlay!");

            dirty = false;
            _assigned = true;
            gameObject.SetActive(false);
        }

        public void FlagAsDirty()
        {
            dirty = true;
            _assigned = false;
        }

        public Vector3 GetOneScale()
        {
            // TODO: Clean up this function to work more nicely with other zoom functions
            float sizeRatio = GameMgr.CurrentSizeRatio();
            float scaleX = maxScale;
            float scaleY = maxScale;

            if (sizeRatio >= 1f)
                scaleY /= sizeRatio;
            else
                scaleX *= sizeRatio;

            return new Vector3(scaleX, scaleY, 1f); // ok
        }

        public void AssignImage()
        {
            if (GameMgr.Instance.CurrentLevel.Tool == ToolTypes.Segment)
            {
                Hide();
                return;
            }

            transform.parent.localScale = GetOneScale(); // Scale parent instead to autoscale everything else

            Texture2D basePhoto = GameMgr.CurrentTex2D();

            int cp = basePhoto.width * basePhoto.height;
            Color32[] cc = new Color32[cp];
            for (int i = 0; i < cp; i++)
            {
                cc[i] = UIMgr.Instance.canvasClearColor;
            }

            if (GameMgr.Instance.CurrentLevel.Tool == ToolTypes.Paint)
            {
                Texture2D groundTruthPhoto = GameMgr.CurrentGroundTruthTex2D();

                if (groundTruthPhoto == null)
                {
                    Hide();
                    return;
                }

                if (GameMgr.Instance.CurrentLevel.TutorialMode == TutorialMode.Full)
                {
                    Color[] pixels = groundTruthPhoto.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i] != Color.black)
                        {
                            cc[i] = PaintHighlightColor;
                        }
                    }
                }
                else if (GameMgr.Instance.CurrentLevel.TutorialMode == TutorialMode.Simple)
                {
                    PaintTool paintTool = ToolMgr.Instance.activeTool as PaintTool;

                    // Check to make sure the ground truths have been assigned yet.
                    if (paintTool == null || paintTool.groundTruthBlobs == null)
                    {
                        return;
                    }

                    foreach (var blob in paintTool.groundTruthBlobs)
                    {
                        int cx = (int)blob.Value.center.x;
                        int cy = (int)blob.Value.center.y;

                        //Debug.LogFormat("Checking blob at ({0}, {1}) with center at {2}", blob.Value.center.x, blob.Value.center.y, (cx + groundTruthPhoto.width * cy));

                        DrawCenter(cc, cx, cy, basePhoto.width, basePhoto.height, PaintRadius, PaintCenterColor);
                    }
                }
                else
                {
                    Hide();
                    return;
                }
            }
            else if (GameMgr.Instance.CurrentLevel.Tool == ToolTypes.Neuron)
            {
                NeuronTool neuronTool = ToolMgr.Instance.activeTool as NeuronTool;

                // Check to make sure the ground truths have been assigned yet.
                if (neuronTool == null || neuronTool.groundTruthGraph == null)
                {
                    return;
                }

                if (GameMgr.Instance.CurrentLevel.TutorialMode == TutorialMode.Full)
                {
                    foreach (Node n in neuronTool.groundTruthGraph.nodes)
                    {
                        //Debug.LogFormat("Checking node at ({0}, {1})", n.center.x, n.center.y);

                        int nx = Mathf.RoundToInt(basePhoto.width * n.center.x);
                        int ny = Mathf.RoundToInt(basePhoto.height * n.center.y);

                        if (n.neighbors.Count >= MinNeigbors)
                        {
                            DrawCenter(cc, nx, ny, basePhoto.width, basePhoto.height, NeuronRadius, NeuronHighlightColor);
                        }
                        else
                        {
                            DrawCenter(cc, nx, ny, basePhoto.width, basePhoto.height, NeuronSecondaryRadius, NeuronSecondaryColor);
                        }
                    }
                }
                else if (GameMgr.Instance.CurrentLevel.TutorialMode == TutorialMode.Simple)
                {
                    foreach (Node n in neuronTool.groundTruthGraph.nodes)
                    {
                        if (n.neighbors.Count < MinNeigbors) continue;

                        //Debug.LogFormat("Checking node at ({0}, {1})", n.center.x, n.center.y);

                        int nx = Mathf.RoundToInt(basePhoto.width * n.center.x);
                        int ny = Mathf.RoundToInt(basePhoto.height * n.center.y);

                        DrawCenter(cc, nx, ny, basePhoto.width, basePhoto.height, NeuronRadius, NeuronHighlightColor);
                    }
                }
                else
                {
                    Hide();
                    return;
                }
            }
            else
            {
                Hide();
                return;
            }

            Texture2D tex = new Texture2D(basePhoto.width, basePhoto.height);
            tex.SetPixels32(0, 0, tex.width, tex.height, cc);
            tex.Apply();

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = material;
            renderer.material.SetTexture("_MainTex", tex);

            _assigned = true;
        }

        private void DrawCenter(Color32[] pixels, int cx, int cy, int photoWidth, int photoHeight, int radius, Color drawColor)
        {
            for (int i = cx - radius; i < cx + radius; i++)
            {
                if (i < 0 || i >= photoWidth) continue;

                for (int j = cy - radius; j < cy + radius; j++)
                {
                    if (j < 0 || j >= photoHeight) continue;

                    // TODO: Ugly radius check for circle, should clean up for speedier results.
                    if (Mathf.Sqrt((i - cx) * (i - cx) + (j - cy) * (j - cy)) >= radius) continue;

                    int p = i + photoWidth * j;
                    pixels[p] = drawColor;
                }
            }
        }
    }

}