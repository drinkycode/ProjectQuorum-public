using ProjectQuorum.Data.Variables;
using ProjectQuorum.Managers;
using ProjectQuorum.UI;

using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace ProjectQuorum
{

    public class MarkupBase
    {
        public Vector2 center;
        public float percent;
        public int index;
    }

    public class ToolBase : MonoBehaviour
    {
        public ToolTypes tool;

        [Space]
        [Header("Base Tool Variables")]
        public Vector2 imageOffset = new Vector2();

        public FloatVariable Zoom;

        public float RecordedAccuracy = 0f;

        public FloatVariable LevelScore;

        [Space]
        [Header("Touch/Input Variables")]
        public TouchableListener TouchableArea;

        public bool ForceLabelUpdate = true;

        [HideInInspector]
        public bool mystery;

        protected Texture2D _canvas;

        protected byte[] _savedBytes;

        protected Texture2D _tutorialCanvas;

        public bool SavingCanvasFinished = false;

        protected int[] _indices;

        protected Dictionary<MarkupBase, MarkupBase> _userMarkupToGTMarkupMap;

        protected RectTransform _progressContainer;

        private bool _initialized;

        //====================  BASIC INTERFACE  ====================//

        virtual protected void InitTool()
        {
            // Set up initial tool offsets.
            imageOffset.x = ImageMgr.Instance.transform.localPosition.x - 0.5f;
            imageOffset.y = ImageMgr.Instance.transform.localPosition.y - 0.5f;

            _progressContainer = ProgressMgr.Instance.transform.GetComponent<RectTransform>();
        }

        virtual protected void UnloadTool()
        {
            // To be overridden within specific tool class.
            _savedBytes = null;
        }

        virtual protected void UpdateTool()
        {
            Vector2 texCoord;
            if (BasicInputCheck(out texCoord))
            {
                // Handle tool stuff
            }
            else
            {
                // Cancel tool stuff
            }
        }

        virtual protected void UpdateToolLate()
        {
            // If anything needs to be updated after the tool frame has ticked.
        }

        virtual protected List<MarkupBase> GetAllUserMarkups()
        {
            List<MarkupBase> allUserMarkups = new List<MarkupBase>();
            //grab markups from native data structures
            return allUserMarkups;
        }

        virtual protected bool IsMarkupSelected(MarkupBase userMarkup)
        {
            return false;
        }

        virtual protected MarkupBase ComputeClosestGTMarkup(MarkupBase userMarkup)
        {
            return null;
        }

        virtual protected float ComputeScoreBetweenMarkup(MarkupBase userMarkup, MarkupBase gtMarkup)
        {
            return 1f;
        }

        virtual protected Vector2 CalculateLabelPositionForMarkup(MarkupBase userMarkup)
        {
            return ConvertPixelToLabel(ConvertUVToPixel(userMarkup.center));
        }

        virtual protected IEnumerator ParseGroundTruths()
        {
            yield break;
        }

        virtual protected int GetGroundTruthCount()
        {
            return 0;
        }

        //====================  HELPER FUNCTIONS  ====================//

        protected bool BasicInputCheck(out Vector2 uv)
        {
            uv = new Vector2(-50000, -50000);

            if (TouchableArea.IsOver)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, UIMgr.Instance.imageLayer))
                {
                    uv = hit.textureCoord;
                }
            }

            return uv.x != -50000 || uv.y != -50000;
        }

        protected virtual Vector3 GetReticlePositionFromUV(Vector2 uv, float z)
        {
            return new Vector3(uv.x, uv.y, z);
        }

        protected virtual Vector2 ConvertPixelToLabel(Vector2 pos)
        {
            float coreHeight = 768f * Zoom.Value;
            Vector3 ts = ImageMgr.Instance.GetOneScale();

            pos.x = (((pos.x / _canvas.width) * coreHeight) - coreHeight / 2f) * ts.x;
            pos.y = ((((pos.y / _canvas.height) * coreHeight) - coreHeight / 2f) * ts.y + (imageOffset.y * coreHeight * ts.y));
            return pos;
        }

        protected Vector2 ConvertUVToPixel(Vector2 pos)
        {
            pos.x = (int)(pos.x * (float)(_canvas.width));
            pos.y = (int)(pos.y * (float)(_canvas.height));
            return pos;
        }

        protected void UpdateMarkupPercents(List<MarkupBase> userMarkups)
        {
            if (userMarkups == null || userMarkups.Count <= 0)
            {
                userMarkups = GetAllUserMarkups();
            }

            //update closest node computations
            foreach (var userMarkup in userMarkups)
            {
                if (userMarkup == null) { continue; }
                _userMarkupToGTMarkupMap[userMarkup] = ComputeClosestGTMarkup(userMarkup);
            }

            //safely update percentages w fast distance lookups
            foreach (var userMarkup in userMarkups)
            {
                if (userMarkup == null) { continue; }
                var closestGTMarkup = _userMarkupToGTMarkupMap[userMarkup];
                if (closestGTMarkup == null) { continue; }
                userMarkup.percent = ComputeScoreBetweenMarkup(userMarkup, closestGTMarkup);
            }
        }

        protected float DistanceBetweenMarkups(MarkupBase markupA, MarkupBase markupB)
        {
            if (markupA == null || markupB == null) { return float.MaxValue; }
            return Vector2.Distance(markupA.center, markupB.center);
        }

        protected void ClearCanvas(bool apply = true)
        {
            ClearTex2D(_canvas, UIMgr.Instance.canvasClearColor, apply);

            var cp = _canvas.width * _canvas.height;
            if (_indices == null || _indices.Length != cp) { _indices = new int[cp]; }
            for (int i = 0; i < cp; i++)
            {
                _indices[i] = -1;
            }
        }

        protected void ClearTex2D(Texture2D tex2D, Color clearColor, bool apply = true)
        {
            int cp = tex2D.width * tex2D.height;
			Color32[] cc = new Color32[cp];

            for (int i = 0; i < cp; i++)
            {
                cc[i] = clearColor;
            }

            tex2D.SetPixels32(0, 0, tex2D.width, tex2D.height, cc);
            
			if (apply)
            {
                tex2D.Apply();
            }
        }

        //====================  PRIVATE LIFE CYCLE  ====================//

        public void Awake()
        {
            _initialized = false;
        }

        public void OnEnable()
        {
            _initialized = false;
            StartCoroutine(ResetGraph());
        }

        public void OnDisable()
        {
            UnloadTool();
            _initialized = false;
        }

        virtual public void Update()
        {
            if (!_initialized) { return; }
            
			if (ImageMgr.Instance.dirty) 
			{ 
				StartCoroutine(ResetGraph()); 
				return; 
			}

            UpdateTool();
        }

        public void LateUpdate()
        {
            UpdateToolLate();

            if (!_initialized) { return; }

#if UNITY_EDITOR
            // Testing results screen on editor only
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RecordedAccuracy = 0.95f;
                EndLevel();
            }
#endif

            ProgressMgr progressMgr = ProgressMgr.Instance;

            // TODO: Fix sum for node progression.
            int sum = 0;
            float accuracy = 0f;

            int index = 0;
            foreach (MarkupBase userMarkup in GetAllUserMarkups())
            {
                if (mystery)
                {
                    sum++;
                    if (progressMgr.UpdateLabel(index, -1f, false))
                    {
                        progressMgr.UpdateLabelPosition(index, CalculateLabelPositionForMarkup(userMarkup));
                    }
                }
                else
                {
                    if (userMarkup.percent >= progressMgr.PassThreshold.Value)
                    {
                        // Add to accuracy amount.
                        accuracy += userMarkup.percent;
                        sum++;
                    }

                    if (progressMgr.UpdateLabel(index, userMarkup.percent, IsMarkupSelected(userMarkup)))
                    {
                        progressMgr.UpdateLabelPosition(index, CalculateLabelPositionForMarkup(userMarkup));
                    }
                }
                index++;
            }

            if (sum > 0)
            {
                // Calculate overall accuracy.
                RecordedAccuracy = accuracy / sum;
            }
            else
            {
                RecordedAccuracy = 0f;
            }

            for (int i = index; i < progressMgr.transform.childCount; i++)
            {
                progressMgr.UpdateLabel(i, 0, false);
            }

            // If unknown amount of ground truths, then just display sum.
            if (mystery)
            {
                //UIMgr.instance.status.text = "" + sum;
                UIMgr.Instance.UpdateNodeCounters(sum);

                if (sum >= 1)
                {
                    UIMgr.Instance.ShowFinishButton();
                }
            }
            // Otherwise display sum compared to ground truths.
            else
            {
                int groundTruthCount = GetGroundTruthCount();

                //UIMgr.instance.status.text = sum + " / " + groundTruthCount;
                UIMgr.Instance.UpdateNodeCounters(sum, groundTruthCount);
                //Debug.Log(sum + " " + groundTruthCount);

                // If sum is equal to ground truth amount then quit to results page.
                //if (sum >= groundTruthCount)
                if (sum >= 1)
                {
                    UIMgr.Instance.ShowFinishButton();
                }
                else
                {
                    UIMgr.Instance.HideFinishButton();
                }
            }

            UpdateProgressContainer();

            ForceLabelUpdate = false;
        }

        virtual protected void UpdateProgressContainer()
        {
            // TODO: Clean this up for much better progress container fixes
            _progressContainer.localPosition = new Vector3(
                (0.5f - Camera.main.transform.localPosition.x) * _progressContainer.rect.width * ImageMgr.Instance.GetOneScale().x,
                (0.5f - Camera.main.transform.localPosition.y) * _progressContainer.rect.height * ImageMgr.Instance.GetOneScale().y,
                0f);
        }

        virtual public void EndLevel()
        {
            // Stop timer at this point.
            UIMgr.Instance.timer.StopTimer();

            CalculateLevelScore();

            // This should save only if this is a mystery image...
            SaveCanvasImage(true);

            // Show level results here.
            GameMgr.Instance.ReturnToResultsPage(RecordedAccuracy);
        }

        virtual protected void CalculateLevelScore()
        {
            LevelScore.Value = 1000f;
        }

        public void SaveCanvasImage(bool submitSolution)
        {
            SavingCanvasFinished = false;
            StartCoroutine(DoSaveCanvasImage(submitSolution));
        }

        private IEnumerator DoSaveCanvasImage(bool submitSolution)
        {
            yield return null;

            byte[] bytes = _canvas.EncodeToPNG();
            _savedBytes = bytes;

            if (submitSolution)
            {
                // Submit score and answer results here...
                ConnectionMgr.Instance.SendUserSolution(GameMgr.Instance.CurrentLevel.LevelID, ConnectionMgr.Instance.CurrentAccount.id, bytes);
            }
            else
            {
                // For testing purposes, also write to a file in the project folder
                //File.WriteAllBytes(Application.dataPath + "/Saved Images/SavedPaintResult.png", bytes);
            }

            SavingCanvasFinished = true;
        }

        public byte[] GetSavedCanvasImage()
        {
            if (SavingCanvasFinished)
            {
                return _savedBytes;
            }
            return null;
        }

        protected IEnumerator ResetGraph()
        {
            UIMgr.Instance.Loading.SetActive(true);

            UIMgr.Instance.timer.ResetTimer();

            ProgressMgr.Instance.DestroyAllLabels();

            _canvas = new Texture2D((int)(GameMgr.CurrentSize().x), (int)(GameMgr.CurrentSize().y), TextureFormat.RGBA32, false);
            _canvas.filterMode = FilterMode.Point;
            UIMgr.Instance.imgDisplayMeshRenderer.material.SetTexture("_Overlay", _canvas);
            ClearCanvas();

            SetupTutorialCanvas();

            if (!GameMgr.CurrentIsMystery())
            {
                _userMarkupToGTMarkupMap = new Dictionary<MarkupBase, MarkupBase>();
                yield return StartCoroutine(ParseGroundTruths());
            }

            InitTool();
            _initialized = true;

            UIMgr.Instance.Loading.SetActive(false);
        }

        virtual protected void SetupTutorialCanvas()
        {
            //_tutorialCanvas = new Texture2D((int)(GameMgr.CurrentSize().x), (int)(GameMgr.CurrentSize().y), TextureFormat.RGBA32, false);
            //_tutorialCanvas.filterMode = FilterMode.Point;
            //UIMgr.Instance.tutorialDisplayMeshRenderer.material.SetTexture("_Overlay", _canvas);
            //ClearTex2D(_tutorialCanvas, UIMgr.Instance.canvasClearColor);
        }

        virtual public void OnZoom(int newZoomValue)
        {

        }

        virtual public void OnHorizontalPan(float newPanValue)
        {

        }

        virtual public void OnVerticalPan(float newPanValue)
        {

        }
    }

}