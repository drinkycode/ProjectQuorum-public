using ProjectQuorum.Data;
using ProjectQuorum.Data.Variables;
using ProjectQuorum.UI;
using System.Collections;
using UnityEngine;

namespace ProjectQuorum.Managers
{

    public class GameMgr : BaseMgr
    {

        /// <summary>
        /// If this is being built using the CNV tool.
        /// </summary>
        static public bool CNV_TOOL = false;


        // Singleton for game manager access.
        static public GameMgr Instance;

		[Space]
		[Header("World/Level Data")]

		public WorldListData WorldList;
        public LevelListData Levels;

        [Space]
        public WorldListData CNVWorldList;
        public LevelListData CNVLevels;

        [Space]
        public GameLevelData CurrentLevel;
        
        [Space]
        public IntVariable WorldToLoad;

        public IntVariable LoadLevel;

        public FloatVariable LevelAccuracy;

        [Space]
        public GameObject ResultsPanel;

        public Vector2 defaultLevelSize = new Vector2(256f, 256f);


        [Space]
        [Header("Controllers")]

        public float PanArrowDelta = 0.25f;

        [SerializeField]
        private ZoomController _zoomController;

        [SerializeField]
        private PanController _panController;



        static public Texture2D CurrentTex2D()
        {
            return Instance.CurrentLevel.Photo;
        }

        static public string CurrentData()
        {
            return Instance.CurrentLevel.Data == null ? null : Instance.CurrentLevel.Data.text;
        }

        static public string CurrentDataName()
        {
            return Instance.CurrentLevel.Data == null ? string.Empty : Instance.CurrentLevel.Data.name;
        }

        static public Texture2D CurrentGroundTruthTex2D() { return Instance.CurrentLevel.GroundTruthTex2D; }

        static public string CurrentGroundTruthString()
        {
            return Instance.CurrentLevel.GroundTruthTextAsset == null ? null : Instance.CurrentLevel.GroundTruthTextAsset.text;
        }

        static public bool CurrentIsMystery() { return Instance.CurrentLevel.IsMystery; }

        static public bool CurrentIsInverted() { return Instance.CurrentLevel.InvertPhoto; }

        static public Vector2 CurrentSize() { return Instance.CurrentLevel.Size; }

        static public Vector2 ZoomSizes() { return Instance.CurrentLevel.ZoomSizes; }

        static public Vector2 PanAmount() { return Instance.CurrentLevel.PanAmount; }

        static public Vector2 ZoomedInPanAmount() { return Instance.CurrentLevel.ZoomedInPanAmount; }

        static public float CurrentSizeRatio()
        {
            return (float)(GameMgr.CurrentSize().x) / (float)(GameMgr.CurrentSize().y);
        }
        


        public override void Awake()
        {
			base.Awake();

            // Set singleton instance.
            Instance = this;

            // Set framerate to 60 for max cap.
            Application.targetFrameRate = 60;
        }

        public override void Start()
        {
            base.Start();

            // Assign zoom and pan controller if these values are null.
            if (_zoomController == null)
            {
                _zoomController = GameObject.FindObjectOfType<ZoomController>();
            }
            if (_panController == null)
            {
                _panController = GameObject.FindObjectOfType<PanController>();
            }

            // Load level based on set from menu.
            StartCoroutine(ChangeLevel(LoadLevel.Value));
        }

        public IEnumerator ChangeLevel(int index)
        {
            Debug.Log(string.Format("Loading level {0}", index));

            WorldData CurrentWorld = null;
            if (CNV_TOOL)
            {
                CurrentWorld = CNVWorldList.Worlds[WorldToLoad.Value];
            }
            else
            {
                if (ConnectionMgr.OFFLINE_DEBUG)
                {
                    CurrentWorld = WorldList.Worlds[WorldToLoad.Value];
                }
                else
                {
                    CurrentWorld = DataMgr.Instance.CurrentWorldData;
                }
            }

			Levels = CurrentWorld.LevelList;

            // Set current level based on selection.
            CurrentLevel = Levels.Levels[index];

            // Download texture for the game.
            if (!CNV_TOOL)
            {
                if (CurrentLevel.Photo == null)
                {
                    if (!string.IsNullOrEmpty(CurrentLevel.PhotoUrl))
                    {
                        // Format photo string URL here.
                    }
                    else
                    {
                        Debug.LogErrorFormat("Cannot load photo at URL: {0}!", CurrentLevel.Photo);
                    }
                }
            }

            ToolMgr.Instance.TurnOffAll();
            ImageMgr.Instance.FlagAsDirty();
            
            yield return null;

            ToolMgr.Instance.TurnOn(CurrentLevel.Tool);
        }

        public void ReturnToResultsPage(float accuracy)
        {
            LevelAccuracy.Value = accuracy;

            // TODO: Cleanup results page transition
            ResultsPanel.SetActive(true);

            //GotoNextScene();
        }

        public virtual void Update()
        {
            // Handle zoom levels
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                GotoZoomValue(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                GotoZoomValue(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GotoZoomValue(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                GotoZoomValue(3);
            }

            if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                _panController.ChangeVerticalPan(PanArrowDelta * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                _panController.ChangeVerticalPan(-PanArrowDelta * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                _panController.ChangeHorizontalPan(-PanArrowDelta * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                _panController.ChangeHorizontalPan(PanArrowDelta * Time.deltaTime);
            }
        }

        private void GotoZoomValue(int value)
        {
            Debug.LogFormat("Goto zoom value {0}", value);
            _zoomController.JumpToZoomValue(value);
            UIMgr.Instance.UpdateZoomUI();
        }
    }

}