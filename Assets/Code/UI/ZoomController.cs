using ProjectQuorum.Managers;
using ProjectQuorum.Data.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.UI
{
    public class ZoomController : MonoBehaviour
    {
		private static int ZOOM_SIZES = 4;

        public Camera MainCamera;

        public float[] ZoomSizes;

        public IntVariable ZoomLevel;
		public FloatVariable ZoomValue;

        public float DefaultZoomSize
        {
            get { return ZoomSizes[0]; }
        }

        public float ZoomedInSize
        {
            get { return ZoomSizes[ZoomSizes.Length - 1]; }
        }

		public float ZoomTime = 0.5f;

		private bool _isZooming = false;


        public void OnValidate()
        {
            MainCamera = GetComponent<Camera>();
        }

        public void Start()
        {
            // Set zoom sizes here.
            UpdateZoomSizes();
        }

        public void UpdateZoomSizes()
        {
            ZoomSizes = new float[ZOOM_SIZES]; 

			// Dynamically set zoom sizes here. These should be based on relative sizes for each image.

            ZoomSizes[0] = GameMgr.ZoomSizes().x;
			ZoomSizes[ZOOM_SIZES - 1] = GameMgr.ZoomSizes().y;

			if (ZOOM_SIZES > 1)
			{
				float zoomStep = 1 / (float)(ZOOM_SIZES - 1);
				float zoomDelta = zoomStep;

				for (int i = 1; i < ZOOM_SIZES - 1; i++)
				{
					ZoomSizes[i] = GameMgr.ZoomSizes().x + (GameMgr.ZoomSizes().y - GameMgr.ZoomSizes().x) * zoomDelta;
					zoomDelta += zoomStep;
				}
			}
            
			// Set to starting zoom size.
            MainCamera.orthographicSize = DefaultZoomSize;

            ZoomLevel.Value = 0;
            ZoomValue.Value = 1f;
        }

        public void JumpToZoomValue(int newZoomValue)
        {
            if (ZoomLevel.Value != newZoomValue)
            {
                GotoZoomValue(newZoomValue);
            }
            ZoomLevel.Value = newZoomValue;
        }

		public void GotoZoomValue(int newZoomValue)
		{
            if (GameMgr.CNV_TOOL)
            {
                
            }
            else
            {
                StartCoroutine(DoZoom(MainCamera.orthographicSize, ZoomSizes[newZoomValue], ZoomTime));
            }

            ToolMgr.Instance.activeTool.OnZoom(newZoomValue);
		}

		private IEnumerator DoZoom(float startZoom, float endZoom, float time)
		{
			_isZooming = true;

			float t = 0f;
			float delta = 1 / time;

			while (t < 1f)
			{
				MainCamera.orthographicSize = Mathf.Lerp(startZoom, endZoom, t);
				t += Time.deltaTime * delta;
				yield return null;
			}

			MainCamera.orthographicSize = endZoom;
            UpdateZoomValue();

			_isZooming = false;
		}

        public void ZoomIn()
        {
			if (_isZooming)
			{
				return;
			}

            if (GameMgr.CNV_TOOL)
            {
                if (ZoomLevel.Value >= 11)
                {
                    return;
                }
            }
            else
            { 
                if (ZoomLevel.Value >= 3)
                {
                    return;
                }
            }

            ZoomLevel.Value++;
			GotoZoomValue(ZoomLevel.Value);
        }

        public void ZoomOut()
        {
			if (_isZooming)
			{
				return;
			}

            if (ZoomLevel.Value <= 0)
            {
                return;
            }

            ZoomLevel.Value--;
			GotoZoomValue(ZoomLevel.Value);
        }

        private void UpdateZoomValue()
        {
            ZoomValue.Value = DefaultZoomSize / ZoomSizes[ZoomLevel.Value];
        }
    }
}