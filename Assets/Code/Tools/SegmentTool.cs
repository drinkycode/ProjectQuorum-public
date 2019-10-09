using ProjectQuorum.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

namespace ProjectQuorum
{

    public class SegmentTool : ToolBase
    {

        private string _currentFile = string.Empty;
        private int _totalLines;
        private int _halfLines;

        public int PointsOnScreen = 1000;

        public int dividerSize = 20;
        public int toplineSize = 40;

        public Vector2 error = new Vector2(10f, 10f);

        public Transform highlight;
        public Vector2 highlightSize = new Vector2(0.01f, 0.01f);

        public Transform cutter;
        private bool _cutOK;

        public Color vizBG;
        public Color vizFG;
        public float vizYRange = 0.5f;

        public string debugOutput;

        private List<Segment> _userSegments;
        private List<Segment> _groundTruthSegments;

        private Vector2 _dragStart;
        private Segment _dragSegment;
        private int _dragSegmentStartY;

        private Segment _dragDivider;
        private int _dragDividerStartX;

        // TODO: Remove these variables used for Android input hack fix
        private Vector2 _lastTexCoord;
        private bool _lastPress;

        private Texture2D _pointVisualization;

        /// <summary>
        /// Tracks the last known zoom size.
        /// </summary>
        private int _lastZoom = 0;

        //====================  BASIC INTERFACE  ====================//

        override protected void InitTool()
        {
            base.InitTool();

            Cursor.visible = true;

            //string pointsData = GameMgr.CurrentData();

            //string fileToFind = GameMgr.CurrentDataName();
            string fileToFind = "cnv";

            // Check to make sure if files need to be created...
            string documentsFolder = string.Empty;
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
            {
                documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            else
            {
                documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            // Makes sure the folders are created.

            string quorumFolder = Path.Combine(documentsFolder, "Quorum");
            Directory.CreateDirectory(quorumFolder);

            string cnvFolder = Path.Combine(quorumFolder, "CNV");
            Directory.CreateDirectory(cnvFolder);

            string filename = string.Format("{0}{1}{2}", cnvFolder, Path.DirectorySeparatorChar, fileToFind);
            Debug.LogFormat("Parsing points from file {0} {1}", GameMgr.CurrentDataName(), filename);

            string[] pointsData = null;

            /*using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {

                }
            }*/

            try
            {
                string[] filetypes = { "csv", "txt" };
                foreach (string filetype in filetypes)
                {
                    string testFile = string.Format("{0}.{1}", filename, filetype);
                    if (File.Exists(testFile))
                    {
                        //pointsData = File.ReadAllLines(testFile);

                        _currentFile = testFile;
                        _totalLines = CountLines(_currentFile);
                        _halfLines = Mathf.FloorToInt(_totalLines / 2);

                        pointsData = SampleFile(_currentFile, PointsOnScreen);

                        Debug.Log(_currentFile + " has " + _totalLines + " lines");
                        break;
                    }
                }
            }
            catch(IOException e)
            {
                Debug.Log(e.StackTrace);
            }

            if ((pointsData != null) && (pointsData.Length > 0))
            {
                //string[] lines = pointsData.Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                DrawPointStrings(pointsData);
            }
            else
            {
                Debug.LogErrorFormat("Could not find file {0}!", filename);
            }

            _userSegments = new List<Segment>();
            _userSegments.Add(GenSegmentByUV(1f, 0.5f));
            RedrawCanvas();

            // Reset last zoom to default starting at 0.
            _lastZoom = 0;

            // Old version of extracting data points for segment tool.

            /*string pointsData = GameMgr.CurrentData();
            if ((pointsData != null) && (pointsData.Length > 0))
            {
                float x = 0;
                List<Vector2> points = new List<Vector2>();
                string[] lines = pointsData.Split('\n');
                foreach (string line in lines)
                {
                    points.Add(new Vector2(x++, float.Parse(line)));
                }
                GenerateAndAssignVizTex2D(points);
            }

            _userSegments = new List<Segment>();
            _userSegments.Add(GenSegmentByUV(1f, 0.5f));
            RedrawCanvas();*/
        }

        private int CountLines(string file)
        {
            int lineCount = 0;

            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            lineCount++;
                        }
                    }
                }
            }

            return lineCount;
        }

        private string[] SampleFile(string file, int numPoints, int start = -1, int end = -1)
        {
            string[] lines = new string[numPoints];

            if (start == -1)
            {
                start = 0;
            }
            if (end == -1)
            {
                end = _totalLines;
            }

            int sampleRate = Mathf.FloorToInt((float)(end - start) / (float)numPoints);
            if (sampleRate < 1)
            {
                // Sample rate cannot be less than one.
                sampleRate = 1;
            }

            using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string line;
                        int i = 0;
                        int l = 0;
                        int index = 0;

                        while ((line = sr.ReadLine()) != null)
                        {
                            if ((i >= start) && (i < end))
                            {
                                if (l == 0)
                                {
                                    lines[index] = line;
                                    index++;
                                }

                                l++;
                                if (l > sampleRate)
                                {
                                    l = 0;
                                }
                            }
                            i++;
                        }
                    }
                }
            }

            // Check to make sure last line array is not null.
            if (lines[lines.Length - 1] == null)
            {
                
            }

            return lines;
        }

        private void DrawPointStrings(string[] pointsString)
        {
            float x = 0;
            List<Vector2> points = new List<Vector2>();

            foreach (string line in pointsString)
            {
                if (line == null)
                {
                    break;
                }

                string[] split = line.Split(',');
                points.Add(new Vector2(x++, float.Parse(split[1])));
            }

            if (_pointVisualization != null)
            {
                DrawVixTex2DPoints(_pointVisualization, points);
            }
            else
            { 
                GenerateAndAssignVizTex2D(points);
            }
            Debug.LogFormat("Number of lines {0}", x);
        }

        override protected int GetGroundTruthCount()
        {
            return _groundTruthSegments.Count;
        }

        override protected void UpdateTool()
        {
            cutter.gameObject.SetActive(false);

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
                Vector2 mousePixel = ConvertUVToPixel(texCoord);

                if (UIMgr.Instance.DeleteMode)
                {
                    if (Input.GetMouseButtonDown(0) && _userSegments.Count > 1)
                    {
                        var deleteMe = new List<Segment>();

                        foreach (var userSegment in _userSegments)
                        {
                            if (SegmentContainsPixelX(userSegment, mousePixel))
                            {
                                var xsi = _userSegments.IndexOf(userSegment);
                                if (xsi > 0)
                                {
                                    var prev = _userSegments[xsi - 1];
                                    prev.center.x = userSegment.center.x;
                                }
                                deleteMe.Add(userSegment);

                            }
                        }

                        foreach (var delete in deleteMe)
                        {
                            _userSegments.Remove(delete);
                        }
                        RedrawCanvas();
                    }
                    return;
                }

                if (_dragDivider != null)
                {
                    _dragDivider.center.x = _dragDividerStartX + mousePixel.x - _dragStart.x;
                    RedrawCanvas();
                    HighlightUserSegmentDivider(_dragDivider);
                }
                else if (_dragSegment != null)
                {
                    _dragSegment.center.y = _dragSegmentStartY + mousePixel.y - _dragStart.y;
                    RedrawCanvas();
                    HighlightUserSegmentTopline(_dragSegment);
                }
                else
                {
                    highlight.gameObject.SetActive(false);

                    foreach (var userSegment in _userSegments)
                    {
                        if (!_cutOK && DividerContainsPixel(userSegment, mousePixel))
                        {
                            HighlightUserSegmentDivider(userSegment);

                            if (Input.GetMouseButtonDown(0))
                            {
                                _dragDivider = userSegment;
                                _dragDividerStartX = (int)(_dragDivider.center.x);
                                _dragStart = mousePixel;
                            }
                            break;
                        }

                        if (!_cutOK && ToplineContainsPixel(userSegment, mousePixel))
                        {
                            HighlightUserSegmentTopline(userSegment);

                            if (Input.GetMouseButtonDown(0))
                            {
                                _dragSegment = userSegment;
                                _dragSegmentStartY = (int)(_dragSegment.center.y);
                                _dragStart = mousePixel;
                            }
                            break;
                        }

                        if (SegmentContainsPixelX(userSegment, mousePixel))
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                _cutOK = mousePixel.y > 0;
                            }

                            if (_cutOK)
                            {
                                if (Input.GetMouseButtonUp(0))
                                {
                                    float osx = userSegment.center.x;
                                    userSegment.center.x = mousePixel.x;

                                    Segment newSeg = new Segment(new Vector2(osx, mousePixel.y));
                                    _userSegments.Insert(_userSegments.IndexOf(userSegment) + 1, newSeg);

                                    RedrawCanvas();
                                }
                                else if (Input.GetMouseButton(0))
                                {
                                    PositionDarkCutter(mousePixel.x);
                                }
                            }

                            // Passive cut highlighter
                            if (!Input.GetMouseButton(0))
                            {
                                PositionCutter(mousePixel.x);
                            }

                            break;
                        }
                    }
                }

                _lastTexCoord = texCoord;
                _lastPress = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _dragSegment = null;
                _dragDivider = null;
                _cutOK = false;
            }
        }

        override public void OnZoom(int newZoomValue)
        {
            Debug.Log("CNV Tool zoom " + newZoomValue);

            if (_lastZoom != newZoomValue)
            {
                int z = newZoomValue + 1;

                int range = Mathf.RoundToInt(_totalLines / (z * 2));
                int start = _halfLines - range;
                if (start < 0)
                {
                    start = 0;
                }
                int end = _halfLines + range;
                if (end >= _totalLines)
                {
                    end = _totalLines - 1;
                }

                //Debug.Log("Search from " + start + " " + end + " " + range + " " + _totalLines + " " + _halfLines);

                string[] pointsData = SampleFile(_currentFile, PointsOnScreen, start, end);

                if ((pointsData != null) && (pointsData.Length > 0))
                {
                    DrawPointStrings(pointsData);
                }
            }

            _lastZoom = newZoomValue;
        }

        public override void OnHorizontalPan(float newPanValue)
        {
            // No need to recalculate on highest zoom.
            if (_lastZoom == 0)
            {
                return;
            }

            float p = (newPanValue + 1) / 2;
            
            int z = _lastZoom + 1;
            int range = Mathf.RoundToInt(_totalLines / (z * 2));

            int half = range + Mathf.FloorToInt((_totalLines - range * 2) * p);

            //Debug.Log("New pan value " + p + " " + newPanValue + " " + half);

            int start = half - range;
            if (start < 0)
            {
                start = 0;
            }
            int end = half + range;
            if (end >= _totalLines)
            {
                end = _totalLines - 1;
            }

            Debug.Log("Search from " + start + " " + end + " " + range + " " + half);

            string[] pointsData = SampleFile(_currentFile, PointsOnScreen, start, end);

            if ((pointsData != null) && (pointsData.Length > 0))
            {
                DrawPointStrings(pointsData);
            }
        }

        override protected bool IsMarkupSelected(MarkupBase userMarkup)
        {
            if (_dragSegment != null)
            {
                return _dragSegment == userMarkup;
            }
            if (_dragDivider != null)
            {
                if (_dragDivider == userMarkup) { return true; }
                return userMarkup == _dragDivider.GetNextSegment(_userSegments);
            }
            return false;
        }

        override protected float ComputeScoreBetweenMarkup(MarkupBase userMarkup, MarkupBase gtMarkup)
        {
            var userStartX = (userMarkup as Segment).GetStartX(_userSegments);
            var gtStartX = (gtMarkup as Segment).GetStartX(_groundTruthSegments);
            var startError = Mathf.Abs(userStartX - gtStartX) / error.x;
            var stopError = Mathf.Abs(userMarkup.center.x - gtMarkup.center.x) / error.x;
            var devError = Mathf.Abs(userMarkup.center.y - gtMarkup.center.y) / error.y;
            return (Mathf.Max(0, 1f - startError) + Mathf.Max(0, 1f - stopError) + Mathf.Max(0, 1f - devError)) / 3f;
        }

        override protected MarkupBase ComputeClosestGTMarkup(MarkupBase userMarkup)
        {
            var closeMarkup = userMarkup;
            var min = float.MaxValue;
            foreach (var gtSegment in _groundTruthSegments)
            {
                var userMidpoint = GetUserMarkupMidpoint(userMarkup);
                var gtMidpoint = gtSegment.CalcMidpoint(_groundTruthSegments);
                var dist = Mathf.Abs(userMidpoint.x - gtMidpoint.x);
                if (dist < min)
                {
                    min = dist;
                    closeMarkup = gtSegment;
                }
            }
            return closeMarkup != userMarkup ? closeMarkup : null;
        }

        override protected List<MarkupBase> GetAllUserMarkups()
        {
            var allUserMarkups = new List<MarkupBase>();
            foreach (var segment in _userSegments)
            {
                allUserMarkups.Add(segment);
            }
            return allUserMarkups;
        }

        override protected Vector2 CalculateLabelPositionForMarkup(MarkupBase userMarkup)
        {
            return ConvertPixelToLabel(GetUserMarkupMidpoint(userMarkup));
        }

        override protected IEnumerator ParseGroundTruths()
        {
            var groundTruthString = GameMgr.CurrentGroundTruthString();
            if (groundTruthString == null || groundTruthString.Length <= 0) { Debug.LogError("no data for ground truth generation"); yield break; }

            _groundTruthSegments = new List<Segment>();
            var lines = groundTruthString.Split('\n');
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                _groundTruthSegments.Add(new Segment(new Vector2(float.Parse(parts[1]), ConvertDataYToPixel(float.Parse(parts[2])))));
            }

            yield break;
        }

        //====================  DEBUG / TEST TOOLS  ====================//

        /*void GenerateDebugGTSegments()
        {
            _groundTruthSegments.Add( GenSegmentByUV( 0.40f, 0.5f ) );
            _groundTruthSegments.Add( GenSegmentByUV( 0.50f, 0.6f ) );
            _groundTruthSegments.Add( GenSegmentByUV( 0.70f, 0.5f ) );
            _groundTruthSegments.Add( GenSegmentByUV( 0.80f, 0.4f ) );
            _groundTruthSegments.Add( GenSegmentByUV( 1.00f, 0.5f ) );
        }

        List<Vector2> GenerateDebugGTPoints()
        {
            var points = new List<Vector2>();
            foreach( var gtSeg in _groundTruthSegments )
            {
                for( int p = gtSeg.GetStartX( _groundTruthSegments ); p < gtSeg.center.x; p++ )
                {
                    var v = gtSeg.center.y / GameMgr.CurrentSize().y;
                    points.Add( new Vector2( p, ( v - 0.5f ) + UnityEngine.Random.Range( -0.02f, 0.02f ) ) );
                }
            }
            return points;
        }

        string ConvertPointsToFileString( List<Vector2> points )
        {
            var output = "";
            foreach( var point in points )
            {
                output += point.y + "\n";
            }
            return output;
        }//*/

        //====================  ACTUAL UTILITIES  ====================//

        private int ConvertDataYToPixel(float pointY)
        {
            //return (int)((pointY / (1f / vizYRange) + 0.5f) * GameMgr.CurrentSize().y);
            return (int)(pointY * GameMgr.CurrentSize().y);
        }

        private void GenerateAndAssignVizTex2D(List<Vector2> points)
        {
            Debug.Log("Creating texture with size " + GameMgr.CurrentSize().x + " " + GameMgr.CurrentSize().y);
            _pointVisualization = new Texture2D((int)(GameMgr.CurrentSize().x), (int)(GameMgr.CurrentSize().y), TextureFormat.RGBA32, false);
            _pointVisualization.filterMode = FilterMode.Point;

            DrawVixTex2DPoints(_pointVisualization, points);

            UIMgr.Instance.imgDisplayMeshRenderer.material.SetTexture("_MainTex", _pointVisualization);
        }

        private void DrawVixTex2DPoints(Texture2D visualization, List<Vector2> points)
        {
            ClearTex2D(visualization, vizBG);

            foreach (var point in points)
            {
                int px = (int)(point.x / PointsOnScreen * 1355);
                int py = ConvertDataYToPixel(point.y);
                visualization.SetPixel(px, py, vizFG);

                visualization.SetPixel(px - 1, py, vizFG);
                visualization.SetPixel(px + 1, py, vizFG);
                visualization.SetPixel(px, py - 1, vizFG);
                visualization.SetPixel(px, py + 1, vizFG);
            }

            visualization.Apply();
        }

        private Segment GenSegmentByUV(float x, float y)
        {
            return new Segment(ConvertUVToPixel(new Vector2(x, y)));
        }

        private Vector2 GetUserMarkupMidpoint(MarkupBase userMarkup)
        {
            return (userMarkup as Segment).CalcMidpoint(_userSegments);
        }

        private float GetPixelScalingRatio()
        {
            return GameMgr.CurrentSize().y / 768f;
        }

        private bool DividerContainsPixel(Segment userSegment, Vector2 pixel)
        {
            if (userSegment.center.x >= _canvas.width) { return false; }
            var padding = (dividerSize / 2) * GetPixelScalingRatio();
            return pixel.x >= userSegment.center.x - padding && pixel.x <= userSegment.center.x + padding;
        }

        private bool ToplineContainsPixel(Segment userSegment, Vector2 pixel)
        {
            var padding = (toplineSize / 2) * GetPixelScalingRatio();
            return SegmentContainsPixelX(userSegment, pixel) && pixel.y >= userSegment.center.y - padding && pixel.y <= userSegment.center.y + padding;
        }

        private bool SegmentContainsPixelX(Segment userSegment, Vector2 pixel)
        {
            return pixel.x >= userSegment.GetStartX(_userSegments) && pixel.x <= userSegment.center.x;
        }

        private void HighlightUserSegmentTopline(Segment userSegment)
        {
            var sx = userSegment.GetStartX(_userSegments);
            var hw = (userSegment.center.x - sx) / GameMgr.CurrentSize().x;
            highlight.localScale = new Vector3(hw, highlightSize.y / transform.lossyScale.y, highlightSize.y / transform.lossyScale.y);
            highlight.localPosition = new Vector2(sx / GameMgr.CurrentSize().x + hw / 2f, userSegment.center.y / GameMgr.CurrentSize().y + imageOffset.y);
            highlight.gameObject.SetActive(true);
        }

        private void HighlightUserSegmentDivider(Segment userSegment)
        {
            var highestY = Mathf.Max(userSegment.center.y, userSegment.GetNextSegment(_userSegments).center.y);
            var highestYinUV = highestY / GameMgr.CurrentSize().y;
            highlight.localScale = new Vector3(highlightSize.x / transform.lossyScale.x, highestYinUV, highlightSize.x / transform.lossyScale.x);
            highlight.localPosition = new Vector2(userSegment.center.x / GameMgr.CurrentSize().x, highestYinUV / 2f + imageOffset.y);
            highlight.gameObject.SetActive(true);
        }

        private void PositionCutter(float pixelX)
        {
            cutter.localScale = new Vector3(highlightSize.x / transform.lossyScale.x, 1f, highlightSize.x / transform.lossyScale.x);
            cutter.localPosition = new Vector2(pixelX / GameMgr.CurrentSize().x, 0.5f + imageOffset.y);
            cutter.gameObject.SetActive(true);
        }

        private void PositionDarkCutter(float pixelX)
        {
            highlight.localScale = new Vector3(highlightSize.x / transform.lossyScale.x, 1f, highlightSize.x / transform.lossyScale.x);
            highlight.localPosition = new Vector2(pixelX / GameMgr.CurrentSize().x, 0.5f + imageOffset.y);
            highlight.gameObject.SetActive(true);
        }

        private void RedrawCanvas()
        {
            //TODO: only redraw the necessary bars? usually only one or two at a time and we know their Xs

            ClearCanvas(false);

            int c = 1;
            var colors = UIMgr.Instance.colors;
            var cl = colors.Length;

            foreach (var userSegment in _userSegments)
            {
                if (userSegment == null) { continue; }

                var startX = userSegment.GetStartX(_userSegments);
                var endX = (int)(userSegment.center.x);
                var segWidth = endX - startX;
                var segY = (int)(userSegment.center.y);
                var segSize = segWidth * segY;

                var color = colors[c++ % cl];
                var brush = new Color[segSize];
                for (int i = 0; i < segSize; i++)
                {
                    brush[i] = color;
                }

                _canvas.SetPixels(startX, 0, segWidth, segY, brush);
            }

            _canvas.Apply();

            if (mystery) { return; }

            if (_dragDivider != null)
            {
                var userMarkups = new List<MarkupBase>();
                userMarkups.Add(_dragDivider);
                userMarkups.Add(_dragDivider.GetNextSegment(_userSegments));
                UpdateMarkupPercents(userMarkups);
            }
            else if (_dragSegment != null)
            {
                var userMarkups = new List<MarkupBase>();
                userMarkups.Add(_dragSegment);
                UpdateMarkupPercents(userMarkups);
            }
            else
            {
                UpdateMarkupPercents(GetAllUserMarkups());
            }
        }
    }

    //====================  DATA STRUCTURES  ====================//

    public class Segment : MarkupBase
    {
        public Segment(Vector2 coord)
        {
            center = coord;
        }

        public Vector2 CalcMidpoint(List<Segment> segmentList)
        {
            var coord = center;
            var startX = GetStartX(segmentList);
            coord.x = startX + (coord.x - startX) / 2;
            return coord;
        }

        public Segment GetNextSegment(List<Segment> segmentList)
        {
            var umi = segmentList.IndexOf(this);
            return (umi >= segmentList.Count - 1) ? null : segmentList[umi + 1];
        }

        public int GetStartX(List<Segment> segmentList)
        {
            var pi = segmentList.IndexOf(this) - 1;
            if (pi < 0) { return 0; }
            return (int)(segmentList[pi].center.x);
        }
    }

}