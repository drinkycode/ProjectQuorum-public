using ProjectQuorum.UI;
using UnityEngine;

namespace ProjectQuorum.Managers
{

    public class ImageMgr : MonoBehaviour
    {
        static private float MAX_SCALE_Y = 0.621f;

        static public ImageMgr Instance;

        public Material material;
        public Material materialInverted;

        public float maxScale = 1f;

		[Tooltip("The amount of buffer space we should put around the image.")]
		public float bufferAmount = 20f;

		[Tooltip("The smallest percent the largest zoom should be relative to " + 
			"the starting position (goes from 0 - 1).")]
		public float smallestZoomAmount = 0.25f;

        [Header("Screen UI Area")]
        public RectTransform ScreenArea;

		/// <summary>
		/// If the image is "dirty" and needs to be recalculated.
		/// </summary>
        [HideInInspector]
        public bool dirty;

        private ZoomController _zoomController;

        private PanController _panController;

        private bool _assigned;

        public void Awake()
        {
            Instance = this;
        }

		public void Start()
		{
			// Find components if possible at this point.
			_zoomController = Camera.main.GetComponent<ZoomController>();
			_panController = Camera.main.GetComponent<PanController>();
		}

        public void Update()
        {
            if (dirty) 
			{ 
				AssignImage(); 
			}
        }

        public void LateUpdate()
        {
            dirty = !_assigned;
        }

        public void FlagAsDirty()
        {
            dirty = true;
            _assigned = false;
        }

        public Vector3 GetOneScale()
        {
            // TODO: Clean up this function to work more nicely with other zoom functions
            float sizeRatio = GameMgr.CurrentSizeRatio();
            float scaleX = maxScale;
            float scaleY = maxScale;

			if (sizeRatio >= 1f)
			{
				scaleY /= sizeRatio;
			}
			else
			{
				scaleX *= sizeRatio;
			}

            return new Vector3(scaleX, scaleY, 1f);
        }

        public void AssignImage()
        {
            transform.parent.localScale = GetOneScale(); // Scale parent instead to autoscale everything else

			// Set the image material here.
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = GameMgr.CurrentIsInverted() ? materialInverted : material;
            renderer.material.SetTexture("_MainTex", GameMgr.CurrentTex2D());

			// Now we need to calculate the image and make a best fit to the current screen dimensions.
            float screenRatio = ((float)Screen.width) / ((float)Screen.height);
            float imageRatio = GameMgr.CurrentSizeRatio();

            // Calculate zoom sizes here.
            Debug.LogFormat("Screen ratio versus image ratio {0} {1}", screenRatio, imageRatio);

            float actualScreenRatio = (ScreenArea.rect.width / ScreenArea.rect.height);
            Debug.LogFormat("Screen area {0} {1} {2}", ScreenArea.rect.width, ScreenArea.rect.height, actualScreenRatio);


			if (_zoomController == null)
			{
				_zoomController = Camera.main.GetComponent<ZoomController>();
			}

            if (_zoomController != null)
            {
                // The max orthographic size to fill the Y-axis is 0.621f.
                // Currently assumes test image is 1:1 square.
                float defaultZoom = MAX_SCALE_Y;

                // If actual screen ratio is landscape, we need to fill in the
                // X-axis.
                if (actualScreenRatio > 1f)
                {
                    defaultZoom = MAX_SCALE_Y / actualScreenRatio;
                }

                //float defaultZoom = actualScreenRatio * MAX_SCALE_Y;
                //if (defaultZoom > MAX_SCALE_Y)
                //    defaultZoom = MAX_SCALE_Y;

				GameMgr.Instance.CurrentLevel.ZoomSizes = new Vector2(defaultZoom, defaultZoom * smallestZoomAmount);

                _zoomController.UpdateZoomSizes();
            }


			if (_panController == null)
			{
				_panController = Camera.main.GetComponent<PanController>();
			}

            if (_panController != null)
            {
                _panController.ResetSliders();

                // Set to initial pan amounts.
                float defaultPanX = 0.1f;
                float defaultPanY = 0.1f;

                // If screen area is landscape, then there should be no horizontal
                // panning and only vertical panning.
                if (actualScreenRatio > 1f)
                {
                    defaultPanX = 0.1f;
                    defaultPanY = (actualScreenRatio - MAX_SCALE_Y) * 0.25f;
                }
                else
                {
                    defaultPanX = (actualScreenRatio - MAX_SCALE_Y) * 0.5f;
                    defaultPanY = 0.1f;
                }

                GameMgr.Instance.CurrentLevel.PanAmount = new Vector2(defaultPanX, defaultPanY);

                float zoomedPanX = 0.5f;
                float zoomedPanY = 0.5f;
                GameMgr.Instance.CurrentLevel.ZoomedInPanAmount = new Vector2(zoomedPanX, zoomedPanY);

                _panController.HorizontalPanRange = GameMgr.PanAmount().x;
                _panController.VerticalPanRange = GameMgr.PanAmount().y;

                //_panController.HideHorizontalSlider();
                //_panController.HideVerticalSlider();

                if (Mathf.Abs(defaultPanX) > 0.01f)
                {
                    _panController.ShowHorizontalSlider();
                }

                if (Mathf.Abs(defaultPanY) > 0.01f)
                {
                    _panController.ShowVerticalSlider();
                }

                // Force showing pan slider.
                if (GameMgr.CNV_TOOL)
                {
                    _panController.ShowHorizontalSlider();
                    _panController.HideVerticalSlider();
                }
            }

            _assigned = true;
        }
    }

}