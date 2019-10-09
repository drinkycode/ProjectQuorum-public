using ProjectQuorum.Managers;
using ProjectQuorum.Tools;
using ProjectQuorum.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum
{

    public class EditTool : MonoBehaviour
    {
        public LayerMask nodeLayer;
        public LayerMask imageLayer;
        public LayerMask edgeLayer;

        protected bool _dragging;
        protected VisualNode _lastNode;

        protected bool _lockXDrag;
        protected bool _lockYDrag;

        protected Vector3 _dragOffset;

        Vector3 _dragStart;

        protected List<Node> _dontDelete;

        virtual protected void OnEnable()
        {
            DebugText.instance.Log(name + " ready");
            _dragging = false;
            _dontDelete = new List<Node>();
        }

        virtual protected void Update()
        {
            if (ImageMgr.Instance.dirty) { NodeMgr.instance.Clear(); }

            var newNode = _dragging ? _lastNode : GetNewNode();
            UpdateHighlights(newNode);
            if (newNode == null) { _dragging = false; return; }

            if (DeleteKey() && !_dontDelete.Contains(newNode.node))
            {
                OnDelete(newNode);
                _lastNode = null;
                NodeMgr.instance.DeleteNode(newNode.node);
                //DebugText.instance.Log( "deleted node" );
                _dragging = false;
                return;
            }

            TryDraggingNode(newNode);
        }

        virtual protected void OnDelete(VisualNode node)
        {
            //
        }

        protected void TryDraggingNode(VisualNode node)
        {
            if (!_dragging && Input.GetMouseButtonDown(0))
            {
                _dragStart = node.node.center;
                _dragging = true;
            }
            if (_dragging)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, imageLayer))
                {
                    var newCoord = new Vector3(hit.textureCoord.x * transform.localScale.x, hit.textureCoord.y * transform.localScale.y) +
                                    new Vector3(-0.5f * transform.localScale.x, -0.5f * transform.localScale.y, -0.2f);
                    if (_lockXDrag) { newCoord.x = _dragStart.x; }
                    if (_lockYDrag) { newCoord.y = _dragStart.y; }
                    node.UpdateCoord(newCoord + _dragOffset);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _dragging = false;
                    UpdateHighlights(node);
                }
            }
        }

        VisualNode GetNewNode()
        {
            VisualNode ret = null;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, nodeLayer))
            {
                ret = hit.transform.GetComponent<VisualNode>();
                if (ret == null && hit.transform.parent != null)
                {
                    ret = hit.transform.parent.GetComponent<VisualNode>();
                }

                if (ret != null)
                {
                    _dragOffset = new Vector3(ret.transform.position.x - hit.point.x, ret.transform.position.y - hit.point.y, 0);
                }
            }
            return ret;
        }

        protected void UpdateHighlights(VisualNode newNode)
        {
            if (_lastNode == newNode) { return; }
            if (_lastNode != null) { _lastNode.SetHighlighted(false); }
            if (newNode != null) { newNode.SetHighlighted(true); }
            _lastNode = newNode;
        }

        bool DeleteKey()
        {
            return Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete) || Input.GetMouseButtonDown(1);
        }
    }

}