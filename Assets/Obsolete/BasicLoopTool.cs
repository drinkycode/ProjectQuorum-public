using ProjectQuorum.Managers;
using UnityEngine;

namespace ProjectQuorum
{

    public class BasicLoopTool : EditTool
    {
        public float debugRadius;
        public float minRadius;
        public Transform preview;
        Vector3 _previewScale = Vector3.one;

        override protected void OnEnable()
        {
            base.OnEnable();
            ResetGraph();
        }

        override protected void Update()
        {
            if (_dragging) { base.Update(); return; }

            base.Update();
            if (ImageMgr.Instance.dirty) { ResetGraph(); }

            CheckScroll();
            preview.gameObject.SetActive(_lastNode == null);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, imageLayer))
            {
                var point = new Vector3(hit.textureCoord.x, hit.textureCoord.y) + new Vector3(-0.5f, -0.5f, -0.1f);
                preview.position = point;

                if (Input.GetMouseButtonDown(0))
                {
                    //TODO: mark the center of the loop
                }

                if (Input.GetMouseButton(0))
                {
                    //TODO: update radius / loop preview
                }

                if (Input.GetMouseButtonUp(0)) //check for dragging
                {
                    AddLoop(point, debugRadius);
                }
            }
            else
            {
                //might need to pause / cancel the loop?
            }
        }

        virtual protected void CheckScroll()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0) { return; }

            debugRadius += debugRadius * Mathf.Clamp(scroll, -1f, 1f) * 0.25f;

            if (debugRadius < minRadius) { debugRadius = minRadius; }

            _previewScale.x = _previewScale.y = _previewScale.z = debugRadius * 2f;
            preview.localScale = _previewScale;
        }

        virtual protected void AddLoop(Vector3 center, float radius)
        {
            //NodeMgr.instance.AddGraph( Graph.MakeLoopGraph( center, radius ) );
        }

        void ResetGraph()
        {
            NodeMgr.instance.Clear();
        }
    }

}