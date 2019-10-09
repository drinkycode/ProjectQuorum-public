using ProjectQuorum.Managers;
using ProjectQuorum.Data.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class PanController : MonoBehaviour
    {
        /// <summary>
        /// Slider used to control pan movement.
        /// </summary>
        public Slider HorizontalPanSlider;

        /// <summary>
        /// Range for horizontal panning.
        /// </summary>
        public float HorizontalPanRange = 1f;

        /// <summary>
        /// Slider used to control vertical pan movement.
        /// </summary>
        public Slider VerticalPanSlider;

        /// <summary>
        /// Range for vertical panning.
        /// </summary>
        public float VerticalPanRange = 1f;
        
        /// <summary>
        /// Zoom level for increasing pan amount based on zoom size.
        /// </summary>
        public FloatVariable ZoomLevel;

        /// <summary>
        /// Starting position for pan position.
        /// </summary>
        private Vector3 _startPos;

        private float _panX;
        private float _panY;

        private float _oldPanX;

        public void Start()
        {
            _startPos = transform.localPosition;
        }

        public void Update()
        {
            if (GameMgr.CNV_TOOL)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _oldPanX = _panX;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    if (_oldPanX != _panX)
                    {
                        ToolMgr.Instance.activeTool.OnHorizontalPan(HorizontalPanSlider.value);
                    }
                }
            }
        }

        public void ResetSliders()
        {
            HorizontalPanSlider.value = 0;
            VerticalPanSlider.value = 0;

            _panX = 0f;
            _panY = 0f;

            // Reset back to center.
            transform.localPosition = _startPos;
        }

        public void SetDefaultValues()
        {
            float oldHorizontalPanRage = HorizontalPanRange;
            float oldVerticalPanRage = VerticalPanRange;

            HorizontalPanRange = GameMgr.PanAmount().x;
            if (HorizontalPanRange <= 0.1f)
            {
                HorizontalPanRange = 0.1f;
            }

            VerticalPanRange = GameMgr.PanAmount().y;
            if (VerticalPanRange <= 0.1f)
            {
                VerticalPanRange = 0.1f;
            }

            Debug.Log("zoom out " + HorizontalPanRange + " " + oldHorizontalPanRage + "   " + VerticalPanRange + " " + oldVerticalPanRage);

            // Reset sliders for CNV tool.
            if (GameMgr.CNV_TOOL)
            {
                _panX = _panY = 0f;
                ResetSliders();
            }
        }

        public void SetZoomedInValues()
        {
			float oldHorizontalPanRage = HorizontalPanRange;
			float oldVerticalPanRage = VerticalPanRange;

            HorizontalPanRange = GameMgr.ZoomedInPanAmount().x;
            if (HorizontalPanRange <= 0.1f)
            {
                HorizontalPanRange = 0.1f;
            }

            VerticalPanRange = GameMgr.ZoomedInPanAmount().y;
            if (VerticalPanRange <= 0.1f)
            {
                VerticalPanRange = 0.1f;
            }

            Debug.Log("zoom in " + HorizontalPanRange + " " + oldHorizontalPanRage + "   " + VerticalPanRange + " " + oldVerticalPanRage);

            // Reset sliders for CNV tool.
            if (GameMgr.CNV_TOOL)
            {
                _panX = _panY = 0f;
                ResetSliders();
            }
        }

        public void HideHorizontalSlider()
        {
            HorizontalPanSlider.gameObject.SetActive(false);
        }

        public void HideVerticalSlider()
        {
            VerticalPanSlider.gameObject.SetActive(false);
        }

        public void ShowHorizontalSlider()
        {
            HorizontalPanSlider.gameObject.SetActive(true);
        }

        public void ShowVerticalSlider()
        {
            VerticalPanSlider.gameObject.SetActive(true);
        }

        public void OnChangedHorizontalPan()
        {
            if (!GameMgr.CNV_TOOL)
            {
                _panX = HorizontalPanSlider.value * HorizontalPanRange * 5f;
                UpdatePanPosition();
            }
            else
            {
                _panX = HorizontalPanSlider.value;
            }
        }

        public void OnChangedVerticalPan()
        {
            if (!GameMgr.CNV_TOOL)
            {
                _panY = VerticalPanSlider.value * VerticalPanRange * 5f;
                UpdatePanPosition();
            }
            else
            {

            }
        }

        public void ChangeHorizontalPan(float delta)
        {
            if (!HorizontalPanSlider.gameObject.activeSelf)
            {
                return;
            }

            if (!GameMgr.CNV_TOOL)
            {
                HorizontalPanSlider.value += delta;
                OnChangedHorizontalPan();
            }
            else
            {

            }

            ToolMgr.Instance.activeTool.OnHorizontalPan(HorizontalPanSlider.value);
        }

        public void ChangeVerticalPan(float delta)
        {
            if (!VerticalPanSlider.gameObject.activeSelf)
            {
                return;
            }

            if (!GameMgr.CNV_TOOL)
            {
                VerticalPanSlider.value += delta;
                OnChangedVerticalPan();
            }
            else
            {

            }

            ToolMgr.Instance.activeTool.OnVerticalPan(VerticalPanSlider.value);
        }

        private void UpdatePanPosition()
        {
            transform.localPosition = _startPos + new Vector3(_panX, _panY, 0f);

            // Force the labels to update to track positions.
            ToolMgr.Instance.activeTool.ForceLabelUpdate = true;
        }
    }
}