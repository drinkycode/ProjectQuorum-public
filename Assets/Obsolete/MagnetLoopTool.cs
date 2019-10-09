using ProjectQuorum.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum
{

    public class MagnetLoopTool : BasicLoopTool
    {
        public Transform origin;
        public List<Transform> controls;

        public Transform prefabControl;

        Graph _graph;
        bool _updateGraph;

        override protected void OnEnable()
        {
            base.OnEnable();
            _graph = null;

            foreach (var control in controls)
            {
                Destroy(control.gameObject);
            }
            controls.Clear();

            ShowHideControls();
        }

        override protected void Update()
        {
            if (_dragging) { base.Update(); return; }
            base.Update();

            preview.gameObject.SetActive(_graph == null);

            //TODO: check for manipulation of cores and control points

            if (_updateGraph) { UpdateGraph(); }
        }

        void ShowHideControls()
        {
            origin.gameObject.SetActive(_graph != null);
            foreach (var control in controls)
            {
                control.gameObject.SetActive(_graph != null);
            }
        }

        void UpdateGraph()
        {
            if (_graph == null) { return; }

            RegenNodes();

            int i;
            int cl = controls.Count;
            if (cl > 2)
            {
                var controlAngles = new float[cl];
                for (i = 0; i < cl; i++)
                {
                    controlAngles[i] = GetAngleBetweenPointAndOrigin(controls[i].position);
                }

                foreach (var node in _graph.nodes)
                {
                    var nodeAngle = GetAngleBetweenPointAndOrigin(node.center);
                    var rad = nodeAngle * Mathf.Deg2Rad;

                    //get index of left and right controls
                    var leftCI = 0;
                    for (i = 0; i < cl; i++)
                    {
                        if (nodeAngle < controlAngles[i])
                        {
                            leftCI = i;
                            break;
                        }
                    }
                    var rightCI = leftCI - 1;
                    if (rightCI < 0) { rightCI = cl - 1; }

                    //get the angles of the left and right controls
                    var leftCA = controlAngles[leftCI];
                    var rightCA = controlAngles[rightCI];
                    if (leftCA < rightCA) { leftCA += 360f; }

                    var align = (nodeAngle - leftCA) / (rightCA - leftCA);
                    if (align < 0.5)
                    {
                        var dist = Vector3.Distance(origin.position, controls[leftCI].position);
                        var ct = origin.position + new Vector3(Mathf.Cos(rad) * dist, Mathf.Sin(rad) * dist);
                        var l = (align - 0.5f) * 2f;
                        node.center = Vector3.Lerp(ct, node.center, l);
                    }
                    else
                    {
                        var dist = Vector3.Distance(origin.position, controls[rightCI].position);
                        var ct = origin.position + new Vector3(Mathf.Cos(rad) * dist, Mathf.Sin(rad) * dist);
                        var l = (align - 0.5f) * 2f;
                        node.center = Vector3.Lerp(node.center, ct, l);
                    }
                }
            }

            ShowHideControls();
            NodeMgr.instance.dirty = true;
            _updateGraph = false;
        }

        float GetAngleBetweenPointAndOrigin(Vector3 point)
        {
            var angle = Mathf.Atan2(point.y - origin.position.y, point.x - origin.position.x) * Mathf.Rad2Deg;
            if (angle < 0) { angle += 360; }
            return angle;
        }

        void RegenNodes()
        {
            //var adjustedGraph = Graph.MakeLoopGraph( origin.position, debugRadius );
            //_graph.nodes = adjustedGraph.nodes;
            //NodeMgr.instance.dirty = true;
        }

        override protected void CheckScroll()
        {
            var odr = debugRadius;
            base.CheckScroll();
            _updateGraph = odr != debugRadius;
        }

        override protected void AddLoop(Vector3 center, float radius)
        {
            _updateGraph = true;

            if (_graph != null)
            {
                var newControl = Instantiate(prefabControl).transform;
                newControl.SetParent(transform);
                newControl.position = center;
                controls.Add(newControl);
                return;
            }

            //_graph = Graph.MakeLoopGraph( center, radius );
            //NodeMgr.instance.AddGraph( _graph );
            //origin.position = center;
        }
    }

}