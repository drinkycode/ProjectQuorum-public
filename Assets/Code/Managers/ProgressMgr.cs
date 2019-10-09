using ProjectQuorum.Data.Variables;
using ProjectQuorum.UI;
using UnityEngine;

namespace ProjectQuorum.Managers
{

    public class ProgressMgr : MonoBehaviour
    {

        public static string GetPercentile(float num, bool addPercentSign = true)
        {
            return (int)(num * 100f) + (addPercentSign ? "%" : "");
        }


        static public ProgressMgr Instance;

        public RectTransform prefabProgress;

        public FloatVariable PassThreshold;
        public FloatVariable AlmostThreshold;

        public FloatVariable Zoom;

        private RectTransform _rectTransform;
        private float _defaultSize;

        public void OnValidate()
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultSize = _rectTransform.sizeDelta.x;
        }

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            // TODO: Cleanup hack for reposition with ToolBase late update.
            transform.localPosition = new Vector3(transform.localPosition.x + 0.01f, transform.localPosition.y + 0.01f, 0f);
        }

        public void DefaultSize()
        {
            _rectTransform.sizeDelta = new Vector2(_defaultSize, _defaultSize);
        }

        public void ZoomedInSize()
        {
            float zoomSize = _defaultSize * Zoom.Value;
            //Debug.Log("Zoom size " + zoomSize + " " + _defaultSize + " " + Zoom.Value);
            _rectTransform.sizeDelta = new Vector2(zoomSize, zoomSize);
        }

        public bool UpdateLabel(int index, float percent, bool selected)
        {
            RectTransform progressLabel = GetProgressLabel(index);
            ProgressLabel label = progressLabel.GetComponent<ProgressLabel>();
            label.SetPercent(percent);

            // TODO: Cleanup input usage to better handle input updates.
            if (selected)
            {
                label.mode = PLMode.Mega;
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                label.mode = PLMode.Off;
            }

            return label.mode != PLMode.Off;
        }

        public void UpdateLabelPosition(int index, Vector2 position)
        {
            GetProgressLabel(index).anchoredPosition = position;
        }

        public RectTransform GetProgressLabel(int index)
        {
            RectTransform progressLabel = null;
            if (index < transform.childCount)
            {
                progressLabel = transform.GetChild(index).GetComponent<RectTransform>();
            }
            else
            {
                progressLabel = Instantiate(prefabProgress).GetComponent<RectTransform>();
                progressLabel.SetParent(transform, false);
            }
            return progressLabel;
        }

        public void DestroyAllLabels()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }

}