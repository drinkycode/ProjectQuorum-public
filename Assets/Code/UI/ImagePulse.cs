using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class ImagePulse : MonoBehaviour
    {

        [SerializeField]
        private Image _image;

        public float PulseAmount = 0.1f;

        public float PulseSpeed = 0.1f;

        private float _pulse = 0f;

        public void OnValidate()
        {
            _image = GetComponent<Image>();
        }

        public void Awake()
        {
            
        }

        public void Update()
        {
            _pulse += Time.deltaTime * PulseSpeed;

            Vector3 s = transform.localScale;
            s.x = 1 + Mathf.Sin(_pulse) * PulseAmount;
            s.y = 1 + Mathf.Sin(_pulse) * PulseAmount;
            transform.localScale = s;
        }
    }

}
