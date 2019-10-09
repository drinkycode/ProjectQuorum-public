using ProjectQuorum.Managers;
using ProjectQuorum.UI;
using UnityEngine;
using System.Collections.Generic;

namespace ProjectQuorum
{

    public class Edge2
    {
        public Vector2 a;
        public Vector2 b;
        public Edge2(Vector2 a, Vector2 b)
        {
            this.a = a;
            this.b = b;
        }
        public bool Equals(Edge2 edge)
        {
            return a.x == edge.a.x && a.y == edge.a.y && b.x == edge.b.x && b.y == edge.b.y;
        }
    }

    public class OutlineTool : MonoBehaviour
    {
        public MeshRenderer meshRenderer;
        public Texture2D imageData;
        public LayerMask imageDataLayer;

        public float z = -0.1f;
        public float adjust = 0.1f;
        public float adjustStep = 0.005f;

        public int previewWidth;
        public int previewHeight;
        public float previewZoom;

        public Color clearColor;

        public Color stepDown;
        public Color stepUp;
        public Color floodColor;
        public Color dryFloodColor;
        public Color tempIslandColor;
        public Color falseIslandColor;
        public Color outlineColor;
        public Color reticleColor;

        Texture2D _preview;

        Color[] _dataPixels;

        Vector3[] _neighborMatrix;

        Vector2 _lastScanPosition;
        Vector2 _lastScanSize;

        //========================= LIFE CYCLE =========================//

        void Awake()
        {
            _neighborMatrix = new Vector3[8];
            _neighborMatrix[0] = new Vector3(1, 0);
            _neighborMatrix[1] = new Vector3(-1, 0);
            _neighborMatrix[2] = new Vector3(0, 1);
            _neighborMatrix[3] = new Vector3(0, -1);
            _neighborMatrix[4] = new Vector3(1, 1);
            _neighborMatrix[5] = new Vector3(-1, -1);
            _neighborMatrix[6] = new Vector3(1, -1);
            _neighborMatrix[7] = new Vector3(-1, 1);
        }

        void Update()
        {
            FollowMouse();

            if (ImageMgr.Instance.dirty)
            {
                NodeMgr.instance.Clear();
                //imageData = ImageMgr.instance.image;
                RefreshTexture();
            }
            if (imageData == null) { return; }

            CheckScroll();
            UpdatePreview();
            if ( /*PreviewButtonHeld() &&*/ MarkButtonDown())
            {
                AddOutlineToNodeMgr();
            }
        }

        void OnEnable()
        {
            NodeMgr.instance.Clear();

            // /imageData = ImageMgr.instance.image;
            RefreshTexture();

            DebugText.instance.Log(name + " ready");
        }

        //========================= UTILITIES =========================//

        void AddOutlineToNodeMgr()
        {
            //var coords = MakeCoordListFromOutline();
            //NodeMgr.instance.AddGraph( Graph.MakeGraph( ref coords, true ) );
        }

        List<Vector3> MakeCoordListFromOutline()
        {
            var ret = new List<Vector3>();

            var pW = (int)(_lastScanSize.x);
            var pH = (int)(_lastScanSize.y);
            var coord = GetFirstXYWithColor(floodColor, ref _dataPixels, pW, pH);
            if (coord.x < 0 || coord.y < 0) { return ret; }
            var firstEdge = GetFirstBoundaryFromPixel(coord, ref _dataPixels, pW);
            if (firstEdge == null) { return ret; }

            var edge = firstEdge;
            int safety = 0;
            int max = 5000;
            do
            {
                ret.Add(Vector2.Lerp(edge.a, edge.b, 0.5f));
                edge = GetNextBoundaryFromEdge(edge, ref _dataPixels, pW);
            } while (safety++ < max && !edge.Equals(firstEdge));
            if (safety >= max) { Debug.LogError("OOPS"); ret.Clear(); return ret; }

            ConvertPreviewPixelCoordsToImageUVCoords(ref ret);

            return ret;
        }

        Edge2 GetFirstBoundaryFromPixel(Vector3 coord, ref Color[] pixels, int pWidth)
        {
            var x = (int)(coord.x);
            var y = (int)(coord.y);
            var color = pixels[y * pWidth + x];
            if (pixels[(y + 1) * pWidth + x] != color) { return new Edge2(new Vector2(x - 1, y), new Vector2(x, y)); }
            if (pixels[y * pWidth + x + 1] != color) { return new Edge2(new Vector2(x, y), new Vector2(x, y - 1)); }
            if (pixels[(y - 1) * pWidth + x] != color) { return new Edge2(new Vector2(x, y - 1), new Vector2(x - 1, y - 1)); }
            if (pixels[y * pWidth + x - 1] != color) { return new Edge2(new Vector2(x - 1, y - 1), new Vector2(x - 1, y)); }
            return null;
        }

        Edge2 GetNextBoundaryFromEdge(Edge2 edge, ref Color[] pixels, int pWidth)
        {
            var x = (int)(edge.b.x);
            var y = (int)(edge.b.y);
            var hop = edge.b - edge.a;
            if (hop.y >= 0 && pixels[(y + 1) * pWidth + x] != pixels[(y + 1) * pWidth + x + 1]) { return new Edge2(edge.b, new Vector2(x, y + 1)); }
            if (hop.x >= 0 && pixels[(y + 1) * pWidth + x + 1] != pixels[y * pWidth + x + 1]) { return new Edge2(edge.b, new Vector2(x + 1, y)); }
            if (hop.y <= 0 && pixels[y * pWidth + x + 1] != pixels[y * pWidth + x]) { return new Edge2(edge.b, new Vector2(x, y - 1)); }
            if (hop.x <= 0 && pixels[y * pWidth + x] != pixels[(y + 1) * pWidth + x]) { return new Edge2(edge.b, new Vector2(x - 1, y)); }
            return null;
        }

        void ConvertPreviewPixelCoordsToImageUVCoords(ref List<Vector3> coords)
        {
            var fw = (float)(imageData.width);
            var fh = (float)(imageData.height);
            var lastScan = new Vector3(_lastScanPosition.x, _lastScanPosition.y);
            var half = new Vector3(-0.5f, -0.5f);
            for (int i = 0; i < coords.Count; i++)
            {
                var fix = coords[i];
                fix += lastScan;
                fix.x /= fw;
                fix.y /= fh;
                coords[i] = fix + half;
            }
        }

        Vector3 GetFirstXYWithColor(Color color, ref Color[] pixels, int pWidth, int pHeight)
        {
            for (int x = 0; x < pWidth; x++)
            {
                for (int y = 0; y < pHeight; y++)
                {
                    if (pixels[y * pWidth + x] == color)
                    {
                        return new Vector3(x, y);
                    }
                }
            }
            return new Vector3(-1f, -1f);
        }

        bool PreviewButtonHeld()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        bool MarkButtonDown()
        {
            return Input.GetMouseButtonDown(0);
        }

        void CheckScroll()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0) { return; }

            adjust += Mathf.Clamp(scroll, -1f, 1f) * adjustStep;
        }

        void FollowMouse()
        {
            var mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition - new Vector3(0, 0, Camera.main.transform.position.z - z));
            //var my = 0.5f - transform.localScale.y * 0.5f;
            //mouseWorldPoint.y = Mathf.Clamp( mouseWorldPoint.y, -my, my );
            transform.position = mouseWorldPoint;
        }

        void UpdatePreview()
        {
            ClearPreview(clearColor);

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, imageDataLayer))
            {
                GeneratePreviewFromDataAt(hit.textureCoord);
            }
            else { _preview.Apply(); }
        }

        void RefreshTexture()
        {
            _preview = new Texture2D(previewWidth, previewHeight, TextureFormat.RGB24, false);
            _preview.filterMode = FilterMode.Point;
            meshRenderer.material.SetTexture("_MainTex", _preview);
            transform.localScale = new Vector3((float)previewWidth * previewZoom / imageData.width, (float)previewHeight * previewZoom / imageData.height, 1f);
        }

        void GeneratePreviewFromDataAt(Vector2 imagePos)
        {
            if (imagePos.x < 0 || imagePos.x > 1f || imagePos.y < 0 || imagePos.y > 1f) { return; }
            imagePos.x = imagePos.x * imageData.width - _preview.width / 2f;
            imagePos.y = imagePos.y * imageData.height - _preview.height / 2f;

            var sourceCoords = new Vector2(0, 0);
            var sourceSize = new Vector2(_preview.width, _preview.height);
            var targetSize = new Vector2(imageData.width, imageData.height);
            ClipCoordsAgainstTarget(ref sourceCoords, ref sourceSize, ref imagePos, targetSize);

            _dataPixels = imageData.GetPixels((int)(imagePos.x), (int)(imagePos.y), (int)(sourceSize.x), (int)(sourceSize.y));

            Step(ref _dataPixels);

            var fx = -sourceCoords.x + previewWidth / 2;
            var fy = -sourceCoords.y + previewHeight / 2;
            FloodStroke(ref _dataPixels, (int)(sourceSize.x), (int)(sourceSize.y), (int)fx, (int)fy);

            //DrawReticle( ref _dataPixels, (int)(sourceSize.x), (int)(sourceSize.y), (int)fx, (int)fy );

            if (CheckPreviewPixelsForEdgeFlood(ref _dataPixels, (int)(sourceSize.x), (int)(sourceSize.y)))
            {
                ConditionalPaint(ref _dataPixels, floodColor, dryFloodColor); //abort, break the scan, etc
            }

            //ClearPreview( Camera.main.backgroundColor );
            _preview.SetPixels((int)(sourceCoords.x), (int)(sourceCoords.y), (int)(sourceSize.x), (int)(sourceSize.y), _dataPixels);
            _preview.Apply();

            _lastScanPosition = imagePos;
            _lastScanSize = sourceSize;
        }

        void ClearPreview(Color clearColor, bool apply = false)
        {
            var c = _preview.width * _preview.height;
            var clearPixels = new Color[c];
            for (int i = 0; i < c; i++)
            {
                clearPixels[i] = clearColor;
            }
            _preview.SetPixels(0, 0, _preview.width, _preview.height, clearPixels);
            if (apply) { _preview.Apply(); }
        }

        void Step(ref Color[] pixels)
        {
            var threshold = GetAverageGrayscale(ref pixels) + adjust;

            var l = pixels.Length;
            for (int i = 0; i < l; i++)
            {
                pixels[i] = (pixels[i].grayscale < threshold) ? stepDown : stepUp;
            }
        }

        float GetAverageGrayscale(ref Color[] pixels)
        {
            float ag = 0;
            var l = pixels.Length;
            for (int i = 0; i < l; i++)
            {
                ag += pixels[i].grayscale;
            }
            return ag / (float)l;
        }

        bool CheckPreviewPixelsForEdgeFlood(ref Color[] pixels, int pWidth, int pHeight)
        {
            for (int x = 0; x < pWidth; x++)
            {
                for (int y = 0; y < pHeight; y++)
                {
                    if (x > 0 && x < pWidth - 1 && y > 0 && y < pHeight - 1) { continue; }
                    if (pixels[y * pWidth + x] == floodColor) { return true; }
                }
            }
            return false;
        }

        void FloodStroke(ref Color[] pixels, int pWidth, int pHeight, int rX, int rY)
        {
            RecursiveFlood(ref pixels, pWidth, pHeight, rX, rY);
            /*if( PreviewButtonHeld() )
            {
                RejectIslands( ref pixels, pWidth, pHeight );
            }//*/
            //KeyStroke( ref pixels, pWidth, pHeight, floodColor, dryFloodColor, outlineColor );
        }

        void RecursiveFlood(ref Color[] pixels, int pWidth, int pHeight, int pX, int pY)
        {
            if (pX < 0 || pX >= pWidth || pY < 0 || pY >= pHeight) { return; }
            int i = pY * pWidth + pX;
            if (pixels[i] == stepDown)
            {
                pixels[i] = floodColor;

                RecursiveFlood(ref pixels, pWidth, pHeight, pX - 1, pY);
                RecursiveFlood(ref pixels, pWidth, pHeight, pX + 1, pY);
                RecursiveFlood(ref pixels, pWidth, pHeight, pX, pY - 1);
                RecursiveFlood(ref pixels, pWidth, pHeight, pX, pY + 1);
            }
        }

        void RejectIslands(ref Color[] pixels, int pWidth, int pHeight)
        {
            for (int x = 0; x < pWidth; x++)
            {
                for (int y = 0; y < pHeight; y++)
                {
                    if (pixels[y * pWidth + x] == stepUp)
                    {
                        if (RecursivePaintIsland(ref pixels, pWidth, pHeight, x, y))
                        {
                            ConditionalPaint(ref pixels, tempIslandColor, floodColor);
                        }
                        else
                        {
                            RecursiveFalseIsland(ref pixels, pWidth, pHeight, x, y);
                        }
                    }
                }
            }
            ConditionalPaint(ref pixels, falseIslandColor, stepUp);
        }

        bool RecursivePaintIsland(ref Color[] pixels, int pWidth, int pHeight, int pX, int pY)
        {
            if (pX < 0 || pX >= pWidth || pY < 0 || pY >= pHeight) { return true; }
            int i = pY * pWidth + pX;
            if (pixels[i] == tempIslandColor || pixels[i] == floodColor) { return true; }
            if (pixels[i] == stepUp)
            {
                pixels[i] = tempIslandColor;
                return RecursivePaintIsland(ref pixels, pWidth, pHeight, pX - 1, pY) && RecursivePaintIsland(ref pixels, pWidth, pHeight, pX + 1, pY) &&
                        RecursivePaintIsland(ref pixels, pWidth, pHeight, pX, pY - 1) && RecursivePaintIsland(ref pixels, pWidth, pHeight, pX, pY + 1);
            }
            return false;
        }

        void RecursiveFalseIsland(ref Color[] pixels, int pWidth, int pHeight, int pX, int pY)
        {
            if (pX < 0 || pX >= pWidth || pY < 0 || pY >= pHeight) { return; }
            int i = pY * pWidth + pX;
            if (pixels[i] == stepUp || pixels[i] == tempIslandColor)
            {
                pixels[i] = falseIslandColor;

                RecursiveFalseIsland(ref pixels, pWidth, pHeight, pX - 1, pY);
                RecursiveFalseIsland(ref pixels, pWidth, pHeight, pX + 1, pY);
                RecursiveFalseIsland(ref pixels, pWidth, pHeight, pX, pY - 1);
                RecursiveFalseIsland(ref pixels, pWidth, pHeight, pX, pY + 1);
            }
        }

        void ConditionalPaint(ref Color[] pixels, Color ifColor, Color thenColor)
        {
            int i, l = pixels.Length;
            for (i = 0; i < l; i++)
            {
                if (pixels[i] == ifColor) { pixels[i] = thenColor; }
            }
        }

        void DrawReticle(ref Color[] pixels, int pWidth, int pHeight, int rX, int rY)
        {
            SafeSetPixel(ref pixels, pWidth, pHeight, rX, rY, reticleColor);
            SafeSetPixel(ref pixels, pWidth, pHeight, rX + 1, rY, reticleColor);
            SafeSetPixel(ref pixels, pWidth, pHeight, rX - 1, rY, reticleColor);
            SafeSetPixel(ref pixels, pWidth, pHeight, rX, rY + 1, reticleColor);
            SafeSetPixel(ref pixels, pWidth, pHeight, rX, rY - 1, reticleColor);
        }

        void SafeSetPixel(ref Color[] pixels, int pWidth, int pHeight, int rX, int rY, Color pixelColor)
        {
            if (rX < 0 || rX >= pWidth || rY < 0 || rY >= pHeight) { return; }
            pixels[rY * pWidth + rX] = pixelColor;
        }

        void KeyStroke(ref Color[] pixels, int pWidth, int pHeight, Color keyColor, Color clearColor, Color edgeColor)
        {
            var edges = new List<int>();
            int i, l = pixels.Length;
            for (i = 0; i < l; i++)
            {
                if (pixels[i] != keyColor) { continue; }
                if (i + pWidth < l && pixels[i + pWidth] != keyColor) { edges.Add(i); continue; }
                if (i - pWidth > 0 && pixels[i - pWidth] != keyColor) { edges.Add(i); continue; }
                if (i % pWidth < pWidth - 1 && pixels[i + 1] != keyColor) { edges.Add(i); continue; }
                if (i % pWidth > 0 && pixels[i - 1] != keyColor) { edges.Add(i); continue; }
            }
            ConditionalPaint(ref pixels, keyColor, clearColor);
            foreach (var edge in edges)
            {
                pixels[edge] = edgeColor;
            }
        }

        void ClipCoordsAgainstTarget(ref Vector2 sourceCoords, ref Vector2 sourceSize, ref Vector2 targetCoords, Vector2 targetSize)
        {
            if (targetCoords.x < 0)
            {
                sourceSize.x += targetCoords.x;
                sourceCoords.x = -targetCoords.x;
                targetCoords.x = 0;
            }
            if (targetCoords.x + sourceSize.x > targetSize.x)
            {
                sourceSize.x = targetSize.x - targetCoords.x;
            }

            if (targetCoords.y < 0)
            {
                sourceSize.y += targetCoords.y;
                sourceCoords.y = -targetCoords.y;
                targetCoords.y = 0;
            }
            if (targetCoords.y + sourceSize.y > targetSize.y)
            {
                sourceSize.y = targetSize.y - targetCoords.y;
            }
        }
    }

}