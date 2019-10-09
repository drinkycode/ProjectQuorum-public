using ProjectQuorum.Managers;
using ProjectQuorum.Tools;

using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.UI
{

    public class VisualNode : MonoBehaviour
    {
        public Transform visualEdgePrefab;

        public Vector2 imageOffset;

        //public GameObject normal;
        public GameObject highlighted;

        public bool isHighlighted;

        Node _node;
        public Node node
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;
                OnNodeUpdated();
            }
        }

        List<VisualEdge> _edges;

        void Awake()
        {
            _edges = new List<VisualEdge>();
        }

        public void SetHighlighted(bool highlight)
        {
            //normal.SetActive( !highlight );
            highlighted.SetActive(highlight);
            isHighlighted = highlight;
        }

        public void UpdateCoord(Vector3 coord)
        {
            node.center = coord;
            transform.localPosition = node.center;
        }

        void OnNodeUpdated()
        {
            transform.localPosition = node.center;

            ClearEdges();
            for (int i = 1; i < node.neighbors.Count; i++)
            {
                AddEdgeForNeighbor(node.neighbors[i]);
            }

            var ttls = ToolMgr.Instance.activeTool.transform.lossyScale;
            transform.localScale = new Vector3(1f / ttls.x, 1f / ttls.y, 1f / ttls.z);
        }

        void ClearEdges()
        {
            if (_edges == null) { return; }
            foreach (var edge in _edges)
            {
                Destroy(edge.gameObject);
            }
            _edges.Clear();
        }

        void AddEdgeForNeighbor(Node neighbor)
        {
            if (neighbor == null) { return; }
            var visualEdge = Instantiate(visualEdgePrefab).GetComponent<VisualEdge>();
            visualEdge.transform.SetParent(transform, false);
            visualEdge.neighbor = neighbor;
            _edges.Add(visualEdge);
        }
    }

}