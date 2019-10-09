using ProjectQuorum.Audio;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectQuorum.UI
{
    public class ButtonListener : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
    {

        public AudioClip DownFX;

        public AudioClip UpFX;

        [SerializeField]
        private AudioSource _source;

        private bool _pressed = false;

        public void OnValidate()
        {
            _source = GetComponent<AudioSource>();
        }

        public void Update()
        {
            if (_pressed)
            {
                // TODO: Check to make sure this is handled with touch inputs.
                if (Input.GetMouseButtonUp(0))
                {
                    _pressed = false;
                    if ((_source != null) && (UpFX != null))
                    {
                        _source.clip = UpFX;
                        AudioMgr.Instance.PlayFX(_source.clip);
                    }
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pressed = true;
            if ((_source != null) && (DownFX != null))
            {
                _source.clip = DownFX;
                _source.Play();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            
        }
    }
}