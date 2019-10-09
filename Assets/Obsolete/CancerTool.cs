using ProjectQuorum.Managers;
using ProjectQuorum.Tools;
using ProjectQuorum.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum
{

    public class CancerTool : EditTool
    {
        public VisualEdge fakeEdgeA;
        public VisualEdge fakeEdgeB;

        Node _ghostNode;

        override protected void OnEnable()
        {
            base.OnEnable();

            _ghostNode = new Node(new Vector3());
            fakeEdgeA.neighbor = _ghostNode;

            fakeEdgeA.gameObject.SetActive(false);
            fakeEdgeB.gameObject.SetActive(false);

            ResetGraph();

            _lockXDrag = true;
        }

        override protected void Update()
        {
            base.Update();
            if (ImageMgr.Instance.dirty) { ResetGraph(); }
            if (_dragging || _lastNode != null)
            {
                fakeEdgeA.gameObject.SetActive(false);
                fakeEdgeB.gameObject.SetActive(false);
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, imageLayer))
            {
                _ghostNode.center = new Vector3(hit.textureCoord.x, hit.textureCoord.y) + new Vector3(-0.5f, -0.5f, -0.1f);

                if (Input.GetMouseButton(0))
                {
                    UpdateFakeEdges();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    var newNode = GenerateNewNode();
                    InsertNewNodeAppropes(newNode, GetXIndex(newNode));
                }
            }
            else
            {
                fakeEdgeA.gameObject.SetActive(false);
                fakeEdgeB.gameObject.SetActive(false);
            }
        }

        void UpdateFakeEdges()
        {
            var index = GetXIndex(_ghostNode);
            var graph = GetGraph();
            if (index < 1 || index >= graph.nodes.Count) { return; }

            fakeEdgeA.transform.position = graph.nodes[index - 1].center;
            fakeEdgeA.gameObject.SetActive(true);
            fakeEdgeB.transform.position = _ghostNode.center;
            fakeEdgeB.neighbor = graph.nodes[index];
            fakeEdgeB.gameObject.SetActive(true);
        }

        int GetXIndex(Node node)
        {
            var graph = GetGraph();
            for (int i = 1; i < graph.nodes.Count; i++)
            {
                if (node.center.x < graph.nodes[i].center.x)
                {
                    return i;
                }
            }
            return -1;
        }

        void InsertNewNodeAppropes(Node node, int index)
        {
            var graph = GetGraph();
            if (index < 1 || index >= graph.nodes.Count) { return; }

            node.neighbors[0] = graph.nodes[index - 1];
            node.neighbors[1] = graph.nodes[index];
            graph.nodes[index].neighbors[0] = node;
            graph.nodes[index - 1].neighbors[1] = node;

            graph.nodes.Insert(index, node);
            NodeMgr.instance.dirty = true;

            DebugText.instance.Log("inserted new node");
        }

        Graph GetGraph()
        {
            return NodeMgr.instance.GetGraphByIndex(0);
        }

        Node GenerateNewNode()
        {
            var newNode = new Node(new Vector3(_ghostNode.center.x, _ghostNode.center.y, -0.1f));
            newNode.neighbors.Add(newNode);
            newNode.neighbors.Add(newNode);
            return newNode;
        }

        void ResetGraph()
        {
            NodeMgr.instance.Clear();
            var starterGraph = GenerateStarterGraph();
            NodeMgr.instance.AddGraph(starterGraph);

            _dontDelete.Clear();
            _dontDelete.Add(starterGraph.nodes[0]);
            _dontDelete.Add(starterGraph.nodes[starterGraph.nodes.Count - 1]);
        }

        Graph GenerateStarterGraph()
        {
            var half = new Vector3(0.5f, 0.5f);
            var coords = new List<Vector3>();
            coords.Add(new Vector3(0, 0.5f) - half);
            //coords.Add( new Vector3( 0.2f, 0.5f ) - half );
            //coords.Add( new Vector3( 0.4f, 0.5f ) - half );
            //coords.Add( new Vector3( 0.6f, 0.5f ) - half );
            //coords.Add( new Vector3( 0.8f, 0.5f ) - half );
            coords.Add(new Vector3(1f, 0.5f) - half);
            return null;//Graph.MakeGraph( ref coords, false, false );
        }
    }

}