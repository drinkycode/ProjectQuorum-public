using ProjectQuorum.Tools;
using ProjectQuorum.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum
{

    public class NodeMgr : MonoBehaviour
    {
        public static NodeMgr instance;

        public Transform visualNodePrefab;
        public Transform visualNodeParent;

        [HideInInspector]
        public bool dirty;

        List<Graph> _graphs;

        void Awake()
        {
            instance = this;
            _graphs = new List<Graph>();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Z)) { PopLastGraph(); }
        }

        void LateUpdate()
        {
            if (dirty)
            {
                DestroyVisualGraphs();
                RefreshVisualGraphs();
                dirty = false;
            }
        }

        public void Clear()
        {
            DestroyVisualGraphs();
            _graphs.Clear();
        }

        public void AddGraph(Graph graph)
        {
            if (graph == null) { return; }
            _graphs.Add(graph);
            dirty = true;
        }

        public void DeleteNode(Node node)
        {
            var graph = GetGraphForNode(node);
            if (graph != null)
            {
                graph.DeleteNode(node);
                dirty = true;
            }
        }

        public Graph GetGraphByIndex(int index)
        {
            return _graphs[index];
        }

        public Graph GetGraphForNode(Node node)
        {
            foreach (var graph in _graphs)
            {
                if (graph.nodes.Contains(node))
                {
                    return graph;
                }
            }
            return null;
        }

        void PopLastGraph()
        {
            if (_graphs.Count <= 0) { return; }
            _graphs.Remove(_graphs[_graphs.Count - 1]);
            dirty = true;
        }

        void RefreshVisualGraphs()
        {
            foreach (var graph in _graphs)
            {
                foreach (var node in graph.nodes)
                {
                    var visualNode = Instantiate(visualNodePrefab).GetComponent<VisualNode>();
                    visualNode.transform.SetParent(visualNodeParent, false);
                    visualNode.node = node;
                }
            }
        }

        void DestroyVisualGraphs()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

}