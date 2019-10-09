using ProjectQuorum.Data.Variables;
using ProjectQuorum.Tools;
using ProjectQuorum.UI;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.Managers
{

    public class UIMgr : MonoBehaviour
    {
        static public UIMgr Instance;

        public ZoomController Zoom;
        public PanController Pan;
        public TouchableListener TouchableArea;

        public LayerMask nodeLayer;
        public LayerMask imageLayer;
        public LayerMask edgeLayer;

        public Text status;

        [Space]
        [Header("Mesh Renderers")]
        public MeshRenderer imgDisplayMeshRenderer;
        public MeshRenderer tutorialDisplayMeshRenderer;

        [Space]
        [Header("Node Settings")]
        public int maxNodes = 7;
        public List<NodeCounter> nodeCounters;

        [Space]
        [Header("Game Timer")]
        public LevelTimer timer;

        [Space]
        [Header("Paint Colors")]
        public Color canvasClearColor;
        public Color[] colors;
        public float colorWiggle = 0.1f;
        public int PaintSize = 2;

        [Space]
        [Header("Boolean Flags")]
        public bool DeleteMode;

        [Space]
        [Header("Buttons")]
        public ToggleButton ZoomInButton;
        public ToggleButton ZoomOutButton;
        public ToggleButton editButton;
        public ToggleButton deleteButton;
        public ToggleButton PaintLargeBrush;
        public ToggleButton PaintMediumBrush;
        public ToggleButton PaintSmallBrush;

        public Button FinishButton;
        public Text FinishButtonText;
        
        [Space]
        [Header("UI Variables")]
        public GameObject deleteModeLabel;
        public GameObject DeleteModeBorder;

        public GameObject Loading;

        public Text ZoomText;
        public IntVariable ZoomLevel;

        public Text DebugText;


        private bool _first = true;

        private IEnumerator _updateZoomText;


        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            UpdateZoomToggles(false);
            UpdateDeleteToggles(false);
            HideFinishButton();
        }

        public void Update()
        {
            if (_first && ToolMgr.Instance.activeTool != null)
            {
                _first = false;

                // Make sure paintbrush tool is set to medium size;
                if (ToolMgr.Instance.activeTool.tool == ToolTypes.Paint)
                {
                    ShowPaintButtons();
                    PaintSize = 2;
                    UpdatePaintbrushToggles(false);
                }
                else
                {
                    HidePaintButtons();
                }

                // Hide zoom text.
                ZoomText.gameObject.SetActive(false);
            }
        }

        public void DebugLog(string s)
        {
            DebugText.text += s + "\n";
        }

        public void UpdateNodeCounters(int num, int total = 0)
        {
            // TODO: Implement better updating for nodes
            HideNodeCounters();

            // Clamp to maximum number of node counters.
            // TODO: Allow flexible amount of node counters.
            if (num > nodeCounters.Count)
            {
                num = nodeCounters.Count;
            }
            if (total > nodeCounters.Count)
            {
                total = nodeCounters.Count;
            }

            int i;

            if (total <= 0)
            {
                HideNodeCounters();

                for (i = 0; i < num; i++)
                {
                    nodeCounters[i].Show();
                    nodeCounters[i].SetProgress(true);
                }
            }
            else
            {
                //ShowAllNodeCounters();

                for (i = 0; i < num; i++)
                {
                    nodeCounters[i].Show();
                    nodeCounters[i].SetProgress(true);
                }

                while (i < total)
                {
                    nodeCounters[i].Show();
                    nodeCounters[i].SetProgress(false);
                    i++;
                }
            }
        }

        public void HideNodeCounters()
        {
            for (int i = 0; i < nodeCounters.Count; i++)
            {
                nodeCounters[i].Hide();
            }
        }

        public void ShowAllNodeCounters()
        {
            for (int i = 0; i < nodeCounters.Count; i++)
            {
                nodeCounters[i].Show();
                nodeCounters[i].SetProgress(false);
            }
        }



        public void OnZoomOutButton()
        {
            Zoom.ZoomOut();
            UpdateZoomUI();
        }

        public void OnZoomInButton()
        {
            Zoom.ZoomIn();
            UpdateZoomUI();
        }

        public void UpdateZoomUI()
        {
            UpdateZoomToggles();
            UpdateZoomText(ZoomLevel.Value);

            if (ZoomLevel.Value <= 0)
            {
                TouchableArea.SetVerticalScroll(false);
                ProgressMgr.Instance.DefaultSize();

                /*Pan.HideHorizontalSlider();
                Pan.HideVerticalSlider();

                if (Mathf.Abs(GameMgr.Instance.CurrentLevel.PanAmount.x) > 0.01f)
                {
                    Pan.ShowHorizontalSlider();
                }
                if (Mathf.Abs(GameMgr.Instance.CurrentLevel.PanAmount.y) > 0.01f)
                {
                    Pan.ShowVerticalSlider();
                }*/

                // Force showing all sliders right now.
                if (!GameMgr.CNV_TOOL)
                {
                    Pan.ShowHorizontalSlider();
                    Pan.ShowVerticalSlider();
                }
                else
                {
                    Pan.ShowHorizontalSlider();
                    Pan.HideVerticalSlider();
                }

                Pan.SetDefaultValues();
            }
            else
            {
                TouchableArea.SetVerticalScroll(true);
                ProgressMgr.Instance.ZoomedInSize();

                // Force showing all sliders right now.
                if (!GameMgr.CNV_TOOL)
                {
                    Pan.ShowHorizontalSlider();
                    Pan.ShowVerticalSlider();
                }
                else
                {
                    Pan.ShowHorizontalSlider();
                    Pan.HideVerticalSlider();
                }

                Pan.SetZoomedInValues();
            }
        }

        private void UpdateZoomToggles(bool playFX = true)
        {
            if (ZoomLevel.Value <= 0)
            {
                ZoomInButton.Toggle(false, false);
                ZoomOutButton.Toggle(true, false);
            }
            else if (ZoomLevel.Value >= 3)
            {
                ZoomInButton.Toggle(true, false);
                ZoomOutButton.Toggle(false, false);
            }
            else
            {
                ZoomInButton.Toggle(false, false);
                ZoomOutButton.Toggle(false, false);
            }
        }

        private void UpdateZoomText(int zoom)
        {
            if (_updateZoomText != null)
            {
                StopCoroutine(_updateZoomText);
            }

            StartCoroutine(_updateZoomText = DoUpdateZoomText(zoom));
        }

        private IEnumerator DoUpdateZoomText(int zoom)
        {
            ZoomText.gameObject.SetActive(true);
            ZoomText.text = string.Format("{0}x ZOOM", (zoom + 1));

            yield return new WaitForSeconds(3f);

            ZoomText.gameObject.SetActive(false);
            _updateZoomText = null;
        }


        public void OnDeleteButton()
        {
            DeleteMode = true;
            UpdateDeleteToggles();
        }

        public void OnEditButton()
        {
            DeleteMode = false;
            UpdateDeleteToggles();
        }

        private void UpdateDeleteToggles(bool playFX = true)
        {
            deleteButton.Toggle(DeleteMode, playFX);
            editButton.Toggle(!DeleteMode, playFX);

            // Set visual graphic for erase/delete mode.
            deleteModeLabel.SetActive(DeleteMode);
            DeleteModeBorder.SetActive(DeleteMode);
        }


        public void ShowPaintButtons()
        {
            PaintLargeBrush.gameObject.SetActive(true);
            PaintMediumBrush.gameObject.SetActive(true);
            PaintSmallBrush.gameObject.SetActive(true);
        }

        public void HidePaintButtons()
        {
            PaintLargeBrush.gameObject.SetActive(false);
            PaintMediumBrush.gameObject.SetActive(false);
            PaintSmallBrush.gameObject.SetActive(false);
        }

        public void ToggleLargePaintbrush()
        {
            PaintSize = 1;
            UpdatePaintbrushToggles();
        }

        public void ToggleMediumPaintbrush()
        {
            PaintSize = 2;
            UpdatePaintbrushToggles();
        }

        public void ToggleSmallPaintbrush()
        {
            PaintSize = 3;
            UpdatePaintbrushToggles();
        }

        private void UpdatePaintbrushToggles(bool playFX = true)
        {
            PaintLargeBrush.Toggle(false, playFX);
            PaintMediumBrush.Toggle(false, playFX);
            PaintSmallBrush.Toggle(false, playFX);

            // Large size paintbrush.
            if (PaintSize == 1)
            {
                PaintLargeBrush.Toggle(true, playFX);
            }
            // Medium size paintbrush.
            else if (PaintSize == 2)
            {
                PaintMediumBrush.Toggle(true, playFX);
            }
            // Otherwise small size paintbrush.
            else
            {
                PaintSmallBrush.Toggle(true, playFX);
            }

            if (ToolMgr.Instance.activeTool.tool == ToolTypes.Paint)
            {
                PaintTool paintTool = (PaintTool)ToolMgr.Instance.activeTool;
                paintTool.SelectedRadius = PaintSize;
            }
        }


        public void ShowFinishButton()
        {
            FinishButton.gameObject.SetActive(true);
        }

        public void HideFinishButton()
        {
            FinishButton.gameObject.SetActive(false);
        }

        public void OnFinish()
        {
            ToolMgr.Instance.activeTool.EndLevel();
        }
    }

}