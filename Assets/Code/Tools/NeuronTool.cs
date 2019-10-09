using ProjectQuorum.Managers;
using ProjectQuorum.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.Tools
{

    public class NeuronTool : ToolBase
    {
        protected bool _dragging;
        protected VisualNode _lastNode;
        protected bool _lockXDrag;
        protected bool _lockYDrag;
        protected Vector3 _dragOffset;
        protected Vector3 _dragStart;

        protected List<Node> _dontDelete;

        public VisualEdge fakeEdgeA;
        public Transform sourceVisual;

        protected Node _ghostNode;
        protected Node _sourceNode;

        protected Node _justAdded;

        protected Graph _groundTruthGraph;
        public Graph groundTruthGraph
        {
            get
            {
                return _groundTruthGraph;
            }
        }

        public float weightPos = 0.3f;
        public float distanceFactor = 0.1f;

        //public string debugGraph;

        [Header("NodeMgr Elements")]

        public Transform visualNodePrefab;
        public Transform visualNodeParent;

        public float ScaleAdjustment = 0.8f;

        [HideInInspector]
        public bool dirty;

        List<Graph> _graphs;

        [SerializeField]
        private Vector2 _imageOffsetPixels;

        // TODO: Remove these variables used for Android input hack fix
        private Vector2 _lastTexCoord;
        private bool _lastPress;

        //====================  BASIC INTERFACE  ====================//

        override protected void InitTool()
        {
            base.InitTool();

            _imageOffsetPixels = new Vector2(imageOffset.x * _canvas.width, imageOffset.y * _canvas.height);

            _dragging = false;
            _dontDelete = new List<Node>();

            _ghostNode = new Node(new Vector3());
            fakeEdgeA.neighbor = _ghostNode;
            fakeEdgeA.gameObject.SetActive(false);

            _sourceNode = null;

            Cursor.visible = true;

            ClearGraphs();
            AddGraph(new Graph());
        }

        override protected void UnloadTool()
        {
            if (fakeEdgeA != null) fakeEdgeA.gameObject.SetActive(false);
            if (sourceVisual != null) sourceVisual.gameObject.SetActive(false);
            ClearGraphs();
        }

        private void SubUpdateIDK()
        {
            var newNode = _dragging ? _lastNode : GetNewNode();
            UpdateHighlights(newNode);
            if (newNode == null) { _dragging = false; return; }

            if (UIMgr.Instance.DeleteMode && Input.GetMouseButtonUp(0) && !_dontDelete.Contains(newNode.node))
            {
                foreach (var dn in newNode.node.neighbors)
                {
                    if (dn != null)
                    {
                        _justAdded = dn;
                        break;
                    }
                }

                if (_sourceNode == newNode.node)
                {
                    _sourceNode = null;
                }

                _lastNode = null;
                DeleteNode(newNode.node);
                _dragging = false;

                ProgressMgr.Instance.DestroyAllLabels();

                UpdateNodeMarkup();

                return;
            }

            TryDraggingNode(newNode);
        }

        override protected int GetGroundTruthCount()
        {
            return _groundTruthGraph.nodes.Count;
        }

        override protected void UpdateTool()
        {
            SubUpdateIDK();

            SourceNodeUpdates();

            if (UIMgr.Instance.DeleteMode) { return; }

            Vector2 texCoord;

            // TODO: Fix this input hack to get mouse button working on Android
            bool inputCheck = BasicInputCheck(out texCoord);
            if (!inputCheck && _lastPress && Input.GetMouseButtonUp(0))
            {
                inputCheck = true;
                texCoord = _lastTexCoord;
            }
            _lastPress = false;

            // if (BasicInputCheck(out texCoord))
            if (inputCheck)
            {
                // TODO: Cache reticle offset vector3 - Mike Lee
                _ghostNode.center = GetReticlePositionFromUV(texCoord, -0.1f) + new Vector3(0f, imageOffset.y, 0f);

                RaycastHit hit;
                if (_lastNode == null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, UIMgr.Instance.edgeLayer))
                {
                    var visEdge = hit.transform.parent.GetComponent<VisualEdge>();
                    var fromNode = visEdge.transform.parent.GetComponent<VisualNode>().node;
                    var toNode = visEdge.neighbor;

                    var wfnc = visualNodeParent.transform.TransformPoint(fromNode.center);
                    var segmentLength = Vector3.Distance(wfnc, visualNodeParent.transform.TransformPoint(toNode.center));
                    var distFromNode = Vector3.Distance(wfnc, hit.point);
                    var newNodeCoord = Vector3.Lerp(fromNode.center, toNode.center, distFromNode / segmentLength);

                    fakeEdgeA.gameObject.SetActive(false);
                    sourceVisual.transform.localPosition = newNodeCoord;
                    sourceVisual.gameObject.SetActive(true);

                    if (Input.GetMouseButtonUp(0))
                    {
                        var newNode = new Node(newNodeCoord);
                        InsertNewNodeAppropes(newNode, fromNode, toNode);

                        UpdateNodeMarkup(newNode);
                    }
                }
                else
                {
                    UpdateFakeEdges();
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (_lastNode == null && _sourceNode != null || _graphs[0].nodes.Count <= 0)
                        {
                            Vector3 newNodeCoord = new Vector3(_ghostNode.center.x, _ghostNode.center.y, -0.1f);
                            Node newNode = new Node(newNodeCoord);
                            InsertNewNodeAppropes(newNode);

                            UpdateNodeMarkup(newNode);
                        }
                        else if (_lastNode != null)
                        {
                            _justAdded = _lastNode.node;
                        }
                    }
                }

                _lastTexCoord = texCoord;
                _lastPress = true;
            }
            else
            {
                fakeEdgeA.gameObject.SetActive(false);
            }
        }

        private void TryDraggingNode(VisualNode node)
        {
            if (!_dragging && Input.GetMouseButtonDown(0))
            {
                _dragStart = node.node.center;
                _dragging = true;
            }

            if (_dragging)
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, UIMgr.Instance.imageLayer))
                {
                    Vector3 newCoord = new Vector3(hit.textureCoord.x, hit.textureCoord.y + imageOffset.y, -0.2f);
                    if (_lockXDrag) { newCoord.x = _dragStart.x; }
                    if (_lockYDrag) { newCoord.y = _dragStart.y; }

                    newCoord += _dragOffset;
                    node.UpdateCoord(newCoord);

                    UpdateNodeMarkup(node.node, false);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    _dragging = false;
                    UpdateHighlights(node);
                }
            }
        }

        private void UpdateFakeEdges()
        {
            if (_sourceNode == null || IsAnyNodeHighlighted() || !Input.GetMouseButton(0))
            {
                fakeEdgeA.gameObject.SetActive(false);
                return;
            }
            fakeEdgeA.transform.position = transform.TransformPoint(_sourceNode.center); //bounce it thru world space??
            fakeEdgeA.gameObject.SetActive(true);
        }

        private void SourceNodeUpdates()
        {
            if (_dragging || _lastNode != null)
            {
                if (Input.GetMouseButton(0)) { _sourceNode = _lastNode.node; }
                fakeEdgeA.gameObject.SetActive(false);
                sourceVisual.gameObject.SetActive(false);
                return;
            }

            if (_sourceNode == null)
            {
                sourceVisual.gameObject.SetActive(false);
            }
            else
            {
                sourceVisual.localPosition = new Vector3(_sourceNode.center.x, _sourceNode.center.y, visualNodeParent.transform.position.z + 0.05f);
                sourceVisual.gameObject.SetActive(true);
            }
        }

        override protected void UpdateToolLate()
        {
            if (dirty)
            {
                DestroyVisualGraphs();
                RefreshVisualGraphs();
                dirty = false;
            }

            if (_dragging || _justAdded != null)
            {
                RedrawCanvas();
            }

            if (_justAdded != null)
            {
                _sourceNode = _justAdded;
                _justAdded = null;
                SourceNodeUpdates();
            }
        }

        override protected Vector2 ConvertPixelToLabel(Vector2 pos)
        {
            float coreHeight = 768f * Zoom.Value;
            Vector3 ts = ImageMgr.Instance.GetOneScale();
            float scaleAdjustment = ScaleAdjustment;

            pos.x = (((pos.x / _canvas.width) * coreHeight) - coreHeight / 2f) * scaleAdjustment * ts.x;
            pos.y = ((((pos.y / _canvas.height) * coreHeight) - coreHeight / 2f) * scaleAdjustment * ts.y + (imageOffset.y * coreHeight * ts.y));
            return pos;
        }

        override protected Vector2 CalculateLabelPositionForMarkup(MarkupBase userMarkup)
        {
            return ConvertPixelToLabel(ConvertUVToPixel(userMarkup.center - imageOffset));
        }

        private void RedrawCanvas()
        {
            ClearCanvas(false);

            int c = 1;
            var colors = UIMgr.Instance.colors;
            var cl = colors.Length;
            var nodes = _graphs[0].nodes;
            foreach (var node in nodes)
            {
                if (node.neighbors[0] == null) { continue; }

                var color = colors[c++ % cl];
                var size = 6;
                var bSize = size * size;
                var brush = new Color[bSize];
                for (int i = 0; i < bSize; i++)
                {
                    brush[i] = color;
                }

                int skip = 4;
                Vector2 fromCanvasCoord = ConvertUVToPixel(node.neighbors[0].center) - _imageOffsetPixels;
                Vector2 toCanvasCoord = ConvertUVToPixel(node.center) - _imageOffsetPixels;
                float dist = Vector3.Distance(fromCanvasCoord, toCanvasCoord);
                if (dist < skip) { dist = skip; }
                for (int i = 0; i < dist; i += skip)
                {
                    var strokeCenter = Vector3.Lerp(fromCanvasCoord, toCanvasCoord, (float)i / dist);
                    _canvas.SetPixels((int)(strokeCenter.x) - size / 2, (int)(strokeCenter.y) - size / 2, size, size, brush);
                }
            }
            _canvas.Apply();
        }

        override protected void UpdateProgressContainer()
        {
            // TODO: Clean this up for much better progress container fixes
            _progressContainer.localPosition = new Vector3((0.5f - Camera.main.transform.localPosition.x) * _progressContainer.rect.width * ImageMgr.Instance.GetOneScale().x * ScaleAdjustment,
                (0.5f - Camera.main.transform.localPosition.y) * _progressContainer.rect.height * ImageMgr.Instance.GetOneScale().y * ScaleAdjustment,
                0f);
        }

        public void UpdateNodeMarkup(Node node = null, bool updateAll = true)
        {
            //debugGraph = _graphs[0].SerializeAsString(); //*********

            if (mystery) { return; }

            List<MarkupBase> userMarkups = null;
            if (node != null && !updateAll)
            {
                userMarkups = new List<MarkupBase>();
                userMarkups.Add(node);
            }

            UpdateMarkupPercents(userMarkups);
        }

        override protected bool IsMarkupSelected(MarkupBase userMarkup)
        {
            return _dragging && _lastNode.node == userMarkup;
        }

        override protected float ComputeScoreBetweenMarkup(MarkupBase userMarkup, MarkupBase gtMarkup)
        {
            Node userNode = userMarkup as Node;
            if (userNode.IsHelper()) { return -1f; } //stub

            Node gtNode = gtMarkup as Node;
            if (gtNode == null) { return 0; } //???

            int divideBy = userNode.neighbors.Count;
            if (divideBy <= 0) { return -1f; } //stub

            float linkScore = 0;
            int gtCount = gtNode.neighbors.Count;
            int badLinks = Math.Abs(divideBy - gtCount);
            for (int i = 0; i < divideBy && i < gtCount; i++)
            {
                Node run = userNode.neighbors[i];
                while (run != null && run.IsHelper())
                {
                    run = run.neighbors[Math.Min(i, 1)];
                }

                bool matched = false;
                foreach (Node gtnn in gtNode.neighbors)
                {
                    if (DoesUserNodeMatchGTNode(run, gtnn)) { matched = true; }
                }
                if (!matched) { badLinks++; }
            }
            linkScore = ((float)(divideBy - badLinks)) / ((float)divideBy);

            float distScore = 1f - Vector3.Distance(userNode.center, gtNode.center) * distanceFactor;

            return Mathf.Max(linkScore, 0) * (1f - weightPos) + Mathf.Max(distScore, 0) * weightPos;
        }

        bool DoesUserNodeMatchGTNode(Node userNode, Node gtNode)
        {
            if (userNode == null && gtNode == null) { return true; }
            if (userNode == null || gtNode == null) { return false; }
            return _userMarkupToGTMarkupMap[userNode] == gtNode;
        }

        override protected MarkupBase ComputeClosestGTMarkup(MarkupBase userMarkup)
        {
            if (_groundTruthGraph == null) { return null; }

            var closeMarkup = userMarkup;
            var min = float.MaxValue;
            foreach (var gtNode in _groundTruthGraph.nodes)
            {
                var dist = DistanceBetweenMarkups(userMarkup, gtNode);
                if (dist < min)
                {
                    min = dist;
                    closeMarkup = gtNode;
                }
            }
            return closeMarkup != userMarkup ? closeMarkup : null;
        }

        override protected List<MarkupBase> GetAllUserMarkups()
        {
            var allUserMarkups = new List<MarkupBase>();
            foreach (var userNode in _graphs[0].nodes)
            {
                allUserMarkups.Add(userNode);
            }
            return allUserMarkups;
        }

        override protected IEnumerator ParseGroundTruths()
        {
            string groundTruthString = GameMgr.CurrentGroundTruthString();
            if (groundTruthString == null || groundTruthString.Length <= 0)
            {
                Debug.LogError("No data for ground truth generation!");
                yield break;
            }

            _groundTruthGraph = Graph.Parse(groundTruthString);

            /****** PERFECT USER DATA TEST ******
		    var testUserGraph = Graph.Parse( text );
		    ClearGraphs();
		    AddGraph( testUserGraph );
		    UpdateNodeMarkup();
		    //************/

            yield break;
        }

        //====================  NODE METHODS  ====================//

        bool IsAnyNodeHighlighted()
        {
            foreach (var vnode in FindObjectsOfType<VisualNode>())
            {
                if (vnode.isHighlighted) { return true; }
            }
            return false;
        }

        void InsertNewNodeAppropes(Node node, Node fromNode = null, Node toNode = null)
        {
            if (fromNode == null)
            {
                fromNode = _sourceNode;
            }
            if (fromNode != null)
            {
                if (fromNode.neighbors[fromNode.neighbors.Count - 1] == null)
                {
                    fromNode.neighbors[fromNode.neighbors.Count - 1] = node;
                }
                else if (toNode != null && fromNode.neighbors.Contains(toNode))
                {
                    fromNode.neighbors[fromNode.neighbors.IndexOf(toNode)] = node;
                }
                else
                {
                    fromNode.neighbors.Add(node);
                }
            }

            if (toNode != null)
            {
                if (fromNode != null && toNode.neighbors.Contains(fromNode))
                {
                    toNode.neighbors[toNode.neighbors.IndexOf(fromNode)] = node;
                }
            }

            node.neighbors.Add(fromNode);
            node.neighbors.Add(toNode);

            //var gn = GetGraph().nodes;
            //gn.InsertAt( fromNode != null ? gn.IndexOf( fromNode ) : null );
            _graphs[0].nodes.Add(node);

            dirty = true;

            _justAdded = node;
            //DebugText.instance.Log( "added new node" );
        }

        public VisualNode GetNewNode()
        {
            VisualNode ret = null;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, UIMgr.Instance.nodeLayer))
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

        void UpdateHighlights(VisualNode newNode)
        {
            if (_lastNode == newNode) { return; }
            if (_lastNode != null) { _lastNode.SetHighlighted(false); }
            if (newNode != null) { newNode.SetHighlighted(true); }
            _lastNode = newNode;
        }

        //====================  NODEMGR METHODS  ====================//

        public void ClearGraphs()
        {
            DestroyVisualGraphs();
            if (_graphs == null)
            {
                _graphs = new List<Graph>();
            }
            else
            {
                _graphs.Clear();
            }
        }

        public void AddGraph(Graph graph)
        {
            if (graph == null) { return; }
            _graphs.Add(graph);
            dirty = true;
        }

        public void DeleteNode(Node node)
        {
            var graph = _graphs[0];
            if (graph != null)
            {
                graph.DeleteNode(node);
                dirty = true;
            }
        }

        public void PopLastGraph()
        {
            if (_graphs.Count <= 0) { return; }
            _graphs.Remove(_graphs[_graphs.Count - 1]);
            dirty = true;
        }

        public void RefreshVisualGraphs()
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

        public void DestroyVisualGraphs()
        {
            foreach (Transform child in visualNodeParent)
            {
                Destroy(child.gameObject);
            }
        }
    }

    //====================  DATA STRUCTURES  ====================//

    public class Node : MarkupBase
    {
        static public char DELIM = '|';

        public List<Node> neighbors;
        public List<int> tempRefs;

        public Node(Vector3 center)
        {
            this.center = center;
            neighbors = new List<Node>();
        }

        static public Node Parse(string data)
        {
            var chunks = data.Split(Node.DELIM);
            if (chunks == null || chunks.Length <= 0) { return null; }

            var node = new Node(ParseV3(chunks[0]));

            node.tempRefs = new List<int>();
            for (int i = 1; i < chunks.Length; i++)
            {
                node.tempRefs.Add(int.Parse(chunks[i]));
            }

            return node;
        }

        public void DeRef(Graph graph)
        {
            foreach (var tempRef in tempRefs)
            {
                neighbors.Add(tempRef < 0 ? null : graph.nodes[tempRef]);
            }
            tempRefs.Clear();
            tempRefs = null;
        }

        public string SerializeAsString(Graph fromGraph)
        {
            var ret = SerializeV3AsString(center);
            foreach (var neighbor in neighbors)
            {
                var ns = "" + DELIM + fromGraph.nodes.IndexOf(neighbor);
                ret += ns;
            }
            return ret;
        }

        static public Vector3 ParseV3(string data)
        {
            var e = data.Split(',');
            var x = float.Parse(e[0]);
            var y = float.Parse(e[1]);
            var z = float.Parse(e[2]);
            return new Vector3(x, y, z);
        }

        static public string SerializeV3AsString(Vector3 v3)
        {
            return v3.x + "," + v3.y + "," + v3.z;
        }

        public bool IsHelper()
        {
            return neighbors.Count == 2 && RealNeighborCount() == 2;
        }

        public int RealNeighborCount()
        {
            int count = 0;
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null) { count++; }
            }
            return count;
        }
    }

    public class Graph
    {
        public List<Node> nodes;

        public Graph()
        {
            nodes = new List<Node>();
        }

        static public Graph Parse(string data)
        {
            var graph = new Graph();

            var lines = data.Split('\n');
            foreach (var line in lines)
            {
                graph.nodes.Add(Node.Parse(line.TrimEnd('\r', '\n')));
            }
            foreach (var node in graph.nodes)
            {
                node.DeRef(graph);
            }

            return graph;
        }

        public string SerializeAsString()
        {
            var ret = "";
            foreach (var node in nodes)
            {
                ret += node.SerializeAsString(this) + "\n";
            }
            return ret;
        }

        public void DeleteNode(Node deleteMe)
        {
            if (deleteMe.neighbors.Count > 0)
            {
                var backwardNode = deleteMe.neighbors[0];
                var forwardNodes = new List<Node>(deleteMe.neighbors);
                forwardNodes.Remove(backwardNode);
                foreach (var node in nodes)
                {
                    if (node.neighbors[0] == deleteMe)
                    {
                        node.neighbors[0] = backwardNode;

                        if (backwardNode.neighbors.Contains(deleteMe))
                        {
                            backwardNode.neighbors[backwardNode.neighbors.IndexOf(deleteMe)] = node;
                        }
                        else
                        {
                            backwardNode.neighbors.Add(node);
                        }
                    }
                }

                if (backwardNode != null && backwardNode.neighbors.Contains(deleteMe)) //clean up the tips?
                {
                    if (backwardNode.neighbors.Count <= 2)
                    {
                        backwardNode.neighbors[backwardNode.neighbors.IndexOf(deleteMe)] = null;
                    }
                    else
                    {
                        backwardNode.neighbors.Remove(deleteMe);
                    }
                }
            }

            nodes.Remove(deleteMe);
        }
    }

}