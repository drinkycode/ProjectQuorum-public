using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class ScrollingBackground : MonoBehaviour
    {

        [SerializeField]
        private RawImage _rawImage;

        public float TileSize = 100f;

        /// <summary>
        /// Scroll speed in the x-direction.
        /// </summary>
        public float ScrollX = -0.25f;

        /// <summary>
        /// Scroll speed in the y-direction.
        /// </summary>
        public float ScrollY = -0.25f;

        public void OnValidate()
        {
            _rawImage = GetComponent<RawImage>();
        }

        public void Awake()
        {
            Rect r = _rawImage.uvRect;
            r.width = Screen.width / TileSize;
            r.height = Screen.height / TileSize;
            _rawImage.uvRect = r;
        }

        public void Update()
        {
            Rect r = _rawImage.uvRect;

            r.x += Time.deltaTime * ScrollX;
            if (r.x < -1)
            {
                r.x += 1;
            }
            else if (r.x > 1)
            {
                r.x -= 1;
            }

            r.y += Time.deltaTime * ScrollY;
            if (r.y < -1)
            {
                r.y += 1;
            }
            else if (r.y > 1)
            {
                r.y -= 1;
            }

            _rawImage.uvRect = r;
        }
    }

}
