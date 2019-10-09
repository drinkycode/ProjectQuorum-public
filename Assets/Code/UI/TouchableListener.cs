using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectQuorum.UI
{
    public class TouchableListener : MonoBehaviour, IDragHandler, IPointerDownHandler,IPointerEnterHandler, IPointerExitHandler
    {

        public bool IsOver = false;

        public float VerticalScrollSetting = -55f;

        private RectTransform _rect;

        private float _defaultVerticalScrollSetting;

        public void OnValidate()
        {
            _rect = GetComponent<RectTransform>();
        }

        public void Start()
        {
            _defaultVerticalScrollSetting = _rect.offsetMax.x;
        }

        public void SetVerticalScroll(bool verticalScroll)
        {
            float verticalScrollSetting = (verticalScroll) ? VerticalScrollSetting : _defaultVerticalScrollSetting;
            _rect.offsetMax = new Vector2(verticalScrollSetting, _rect.offsetMax.y);
        }

        public void LateUpdate()
        {
            //if (Input.GetMouseButtonUp(0))
            //{
            //    IsOver = false;
            //}
        }

        public void OnDrag(PointerEventData eventData)
        {
            IsOver = true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsOver = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
#if !UNITY_MOBILE
            IsOver = true;
#endif
        }

        public void OnPointerExit(PointerEventData eventData)
        {
#if !UNITY_MOBILE
            IsOver = false;
#endif
        }

        public bool IsOverObject()
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
#else
            foreach (Touch touch in Input.touches)
            {
                int id = touch.fingerId;
                if (EventSystem.current.IsPointerOverGameObject(id))
                    return true;
            }
#endif
            return false;
        }
    }
}