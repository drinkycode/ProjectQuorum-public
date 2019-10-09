using ProjectQuorum.Data.Variables;
using ProjectQuorum.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public enum PLMode
    {
        Null = 0,
        Off,
        Stub,
        Normal,
        Mega
    }

    public enum PLNormalMode
    {
        Null = 0,
        Fail,
        Almost,
        Pass
    }
    
    public class ProgressLabel : MonoBehaviour
    {
        public PLMode mode;
        public PLNormalMode normalMode;

        [Space]
        [Header("Thresholds")]

        public FloatVariable FailThreshold;
        public FloatVariable OkayThreshold;
        public FloatVariable GoodThreshold;
        public FloatVariable SuccessThreshold;

        public Color FailThresholdColor;
        public Color BadThresholdColor;
        public Color OkayThresholdColor;
        public Color GoodThresholdColor;
        public Color SuccessThresholdColor;

        [Space]
        [Header("Game Objects")]
        public GameObject stub;
        public GameObject normal;
        public GameObject mega;

        [Space]
        [Header("Icons")]
        public Image normalIcon;
        public Sprite normalFail;
        public Sprite normalAlmost;
        public Sprite normalPass;

        [Space]
        [Header("Colors")]
        public Color colorPass;
        public Color colorFail;
        public Color colorAlmost;

        [Space]
        [Header("Dial")]
        public Image Wheel;
        public Image Arrow;
        public Image Check;

        public float WheelOffset = 0f;

        private bool _dirty = false;
        private float _percent;

        public void OnValidate()
        {

        }

        public void SetPercent(float percent)
        {
            _percent = percent;
            if (percent < 0)
            {
                SetMode(PLMode.Stub);
            }
            else if (percent == 0)
            {
                SetMode(PLMode.Off);
            }
            else if (percent > 0)
            {
                SetMode(PLMode.Normal);
            }
        }

        public void SetMode(PLMode mode)
        {
            this.mode = mode;
            _dirty = true;
        }

        void SetNormalModeByPercent()
        {
            if (_percent >= ProgressMgr.Instance.PassThreshold.Value)
            {
                normalMode = PLNormalMode.Pass;
            }
            else if (_percent >= ProgressMgr.Instance.AlmostThreshold.Value)
            {
                normalMode = PLNormalMode.Almost;
            }
            else
            {
                normalMode = PLNormalMode.Fail;
            }
        }

        void LateUpdate()
        {
            if (!_dirty) { return; }
            _dirty = false;

            SetNormalModeByPercent();

            switch (mode)
            {
                case PLMode.Null:
                case PLMode.Off:
                    stub.SetActive(false);
                    normal.SetActive(false);
                    mega.SetActive(false);
                    break;
                case PLMode.Stub:
                    stub.SetActive(true);
                    normal.SetActive(false);
                    mega.SetActive(false);
                    break;
                case PLMode.Normal:
                    stub.SetActive(false);
                    normal.SetActive(true);
                    mega.SetActive(false);

                    normalIcon.sprite = GetNormalSprite();
                    normalIcon.color = GetNormalColor();
                    break;
                case PLMode.Mega:
                    stub.SetActive(false);
                    normal.SetActive(false);
                    mega.SetActive(_percent > 0);

                    UpdatePercent(_percent);
                    //var text = GetComponentInChildren<Text>(true);
                    //text.text = GetPercentile(_percent, false);
                    //text.color = GetNormalColor();
                    
                    break;
                default:
                    break;
            }
        }

        private Sprite GetNormalSprite()
        {
            switch (normalMode)
            {
                case PLNormalMode.Fail:
                    return normalFail;
                case PLNormalMode.Almost:
                    return normalAlmost;
                case PLNormalMode.Pass:
                    return normalPass;
                default:
                    break;
            }
            return null;
        }

        private Color GetNormalColor()
        {
            switch (normalMode)
            {
                case PLNormalMode.Fail:
                    return colorFail;
                case PLNormalMode.Almost:
                    return colorAlmost;
                case PLNormalMode.Pass:
                    return colorPass;
                default:
                    break;
            }
            return Color.white;
        }

        private void UpdatePercent(float percent)
        {
            float clampedPercent = Mathf.Clamp(percent, 0, 1);
            Wheel.fillAmount = clampedPercent;

            float rotationAngle = Mathf.Clamp(180f - 360f * clampedPercent + WheelOffset, -150, 150f);
            Arrow.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

            // By default, assume the arrow is shown.
            Arrow.enabled = true;
            Check.enabled = false;

            if (clampedPercent >= SuccessThreshold.Value)
            {
                Wheel.color = SuccessThresholdColor;
                Arrow.enabled = false;
                Check.enabled = true;
            }
            else if (clampedPercent >= GoodThreshold.Value)
            {
                Wheel.color = GoodThresholdColor;
            }
            else if (clampedPercent >= OkayThreshold.Value)
            {
                Wheel.color = OkayThresholdColor;
            }
            else if (clampedPercent >= FailThreshold.Value)
            {
                Wheel.color = BadThresholdColor;
            }
            else 
            {
                Wheel.color = FailThresholdColor;
            }
        }

        private string GetPercentile(float num, bool addPercentSign = true)
        {
            return (int)(num * 100f) + (addPercentSign ? "%" : "");
        }
    }

}