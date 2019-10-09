using ProjectQuorum.Managers;
using ProjectQuorum.UI;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.Tools
{

    public class PaintTool : ToolBase
    {
        [Space]
        [Header("Paint Radiuses")]
        public int SelectedRadius = 1;

        public float debugRadius;
        public float minRadius;
        public float maxRadius;
        public float LargeRadius;
        public float MediumRadius;
        public float SmallRadius;

        [Space]
        [Header("Cursor")]
        public Transform Cursor;
        public Renderer CursorColor;

        [Space]
        [Header("Spacing")]
        public int averaging = 5;
        public float minGap = 1f;
        public int maxGapIncrements = 64;

        [Space]
        [Header("Error Factors")]
        public int errorDeadzone = 1;
        public float errorFactor = 0.4f;
        public int distanceFactor = 1;

        private Dictionary<int, BlobData> _groundTruthBlobs;
        public Dictionary<int, BlobData> groundTruthBlobs
        {
            get
            {
                return _groundTruthBlobs;
            }
        }

        private Dictionary<int, BlobData> _userBlobs;

        private Vector3 _lastMouse;
        private float[] _lastR;
        private bool _painting;
        private bool _erasing;
        private int _ci;
        private int _tempCI;
        private Color _color;
        private Vector2 _lastTexCoord;
        private float _radius;
        private int _tempOld;

        //====================  BASIC INTERFACE  ====================//

        override protected void InitTool()
        {
            base.InitTool();

            _lastMouse = Input.mousePosition;

            _lastR = new float[averaging];
            for (int i = 0; i < averaging; i++)
            {
                _lastR[i] = minRadius;
            }

            _ci = 1;
            _tempCI = -1;

            _userBlobs = new Dictionary<int, BlobData>();
        }

        override protected void UpdateTool()
        {
            // Update brush size.
            //float speed = Vector3.Distance(Input.mousePosition, _lastMouse) / Time.deltaTime;
            //_radius = Filter(minRadius + InvSqr(debugRadius * speed));
            //if (_radius > maxRadius) { _radius = maxRadius; }

            if (SelectedRadius == 1)
            {
                _radius = LargeRadius;
            }
            else if (SelectedRadius == 2)
            {
                _radius = MediumRadius;
            }
            else
            {
                _radius = SmallRadius;
            }

            // Update cursor size.
            Cursor.localScale = new Vector3(_radius * 1f / transform.lossyScale.x,
                _radius * 1f / transform.lossyScale.y,
                _radius * 1f / transform.lossyScale.z);

            // Basic shared tool input stuff.
            Vector2 texCoord;
            if (BasicInputCheck(out texCoord))
            {
                // Show cursor if we're over the active image.
                Cursor.gameObject.SetActive(true);
                Cursor.localPosition = GetReticlePositionFromUV(texCoord, -0.1f);

                if (Input.GetMouseButtonDown(0))
                {
                    if (UIMgr.Instance.DeleteMode)
                    {
                        StartErase(texCoord);
                    }
                    else
                    {
                        StartPaint(texCoord);
                    }
                }
                else if (Input.GetMouseButton(0))
                {
                    ContinuePaint(texCoord);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    StopPaint();
                }

                UnityEngine.Cursor.visible = false;
            }
            else
            {
                // Hide cursor otherwise.
                Cursor.gameObject.SetActive(false);
                StopPaint();

                UnityEngine.Cursor.visible = true;
            }

            // Update cursor color.
            CursorColor.material.color = UIMgr.Instance.DeleteMode ? UIMgr.Instance.canvasClearColor : GetActiveColor(texCoord);

            _lastTexCoord = texCoord;
            _lastMouse = Input.mousePosition;
        }

        override protected List<MarkupBase> GetAllUserMarkups()
        {
            List<MarkupBase> allUserMarkups = new List<MarkupBase>();
            foreach (MarkupBase userBlob in _userBlobs.Values)
            {
                allUserMarkups.Add(userBlob);
            }
            return allUserMarkups;
        }

        override protected bool IsMarkupSelected(MarkupBase userMarkup)
        {
            return GetBestGuessIndex() == (userMarkup as BlobData).index;
        }

        override protected MarkupBase ComputeClosestGTMarkup(MarkupBase userMarkup)
        {
            if (_groundTruthBlobs == null || _groundTruthBlobs.Values == null) { return null; }

            //userBlob.center = GetWeightedBlobCenter( ref userBlob.pixels, _canvas.width );
            MarkupBase closeMarkup = userMarkup;
            float min = float.MaxValue;
            foreach (var gtBlob in _groundTruthBlobs.Values)
            {
                float dist = DistanceBetweenMarkups(userMarkup, gtBlob);
                if (dist < min)
                {
                    min = dist;
                    closeMarkup = gtBlob;
                }
            }
            return closeMarkup != userMarkup ? closeMarkup : null;
        }

        override protected float ComputeScoreBetweenMarkup(MarkupBase userMarkup, MarkupBase gtMarkup)
        {
            BlobData userBlob = userMarkup as BlobData;
            BlobData closeBlob = gtMarkup as BlobData;

            int csum = 0;
            int errors = 0;
            int examined = 0;

            var ubpl = userBlob.pixels.Length;
            for (int i = 0; i < closeBlob.pixels.Length; i++)
            {
                if (i >= ubpl) { continue; }

                examined++;

				int ubp = userBlob.pixels[i];
				int cbp = closeBlob.pixels[i];
                if (ubp == 0 && cbp == 0) { continue; }
                if (cbp != 0 && ubp != 0) { csum++; continue; }
                if (cbp != 0)
                {
                    csum++;
                    var cei = closeBlob.distIn[i];
                    if (cei > errorDeadzone)
                    {
                        errors += cei;
                    }
                    continue;
                }
                if (ubp != 0)
                {
                    var ceo = closeBlob.distOut[i];
                    if (ceo > errorDeadzone)
                    {
                        errors += ceo;
                    }
                    continue;
                }
            }

            return Mathf.Max(0, 1f - ((float)errors / (float)csum) * errorFactor);
        }

        override protected Vector2 CalculateLabelPositionForMarkup(MarkupBase userMarkup)
        {
            if (!mystery && IsMarkupSelected(userMarkup))
            {
                return ConvertPixelToLabel(ConvertUVToPixel(Cursor.localPosition));
            }
            return ConvertPixelToLabel(userMarkup.center);
        }

        override protected IEnumerator ParseGroundTruths()
        {
            var groundTruthTex2D = GameMgr.CurrentGroundTruthTex2D();
            if (groundTruthTex2D == null) { Debug.LogError("no image for ground truth generation"); yield break; }

            //compute is slow, this updates app status first
            DebugText.instance.Log("Generating distance maps...");
            yield return null;

            _groundTruthBlobs = new Dictionary<int, BlobData>();

            //pre-process image to extract indices from the pixels
            var gt = groundTruthTex2D;
            if (gt == null) { DebugText.instance.Log("ERROR: could not find ground truth"); yield break; }
            var gtPixels = gt.GetPixels32();
            var gp = gt.width * gt.height;
            var gtIndices = new int[gp];
            for (int i = 0; i < gp; i++)
            {
                gtIndices[i] = gtPixels[i].r;
            }

            int blobIndex = 0;
            int safety = 0;
            int rolling = 0;
            while (safety++ < 500)
            {
                //get the first non-zero index
                var pos = -1;
                for (; rolling < gp; rolling++)
                {
                    if (gtIndices[rolling] > 0)
                    {
                        pos = rolling;
                        break;
                    }
                }
                if (pos < 0) { DebugText.instance.Log("SUCCESS: loaded ground truth"); break; }

                // Get all connected indices
                var index = gtIndices[pos];
                if (index <= 0) { DebugText.instance.Log("ERROR: problem loading ground truth"); break; }

                // Just build a mask by scanning the image, don't lasso at all
                BlobData blob = new BlobData(gt.width, gt.height, index);
                for (int i = 0; i < blob.pixels.Length; i++)
                {
                    if (gtIndices[i] == index)
                    {
                        blob.pixels[i] = index;
                        gtIndices[i] = 0;
                    }
                    else
                    {
                        blob.pixels[i] = 0;
                    }
                }

                blob.Resize();
                blobIndex++;

                blob.center = GetWeightedBlobCenter(ref blob.pixels, gt.width);
                ComputeDistanceMaps(blob);
                _groundTruthBlobs.Add(index, blob);
            }
        }

        override protected int GetGroundTruthCount()
        {
            return _groundTruthBlobs.Count;
        }

        override protected Vector3 GetReticlePositionFromUV(Vector2 uv, float z)
        {
            return new Vector3(uv.x, uv.y + imageOffset.y, z);
        }


        //====================   BLOB STUFF   ====================//

        protected BlobData GetUserBlobForIndex(int index)
        {
            if (index < 0) { return null; }

            BlobData userBlob = null;

            // Try to retrieve value from dictionary using TryGetValue.
            if (!_userBlobs.TryGetValue(index, out userBlob))
            {
                userBlob = new BlobData(_canvas.width, _canvas.height, index);
                _userBlobs.Add(index, userBlob);
            }
            return userBlob;
        }

        protected Vector2 GetWeightedBlobCenter(ref int[] blob, int w)
        {
            int xSum = 0;
            int ySum = 0;
            int l = blob.Length;
            int s = 0;
            for (int i = 0; i < l; i++)
            {
                if (blob[i] <= 0) { continue; }
                xSum += i % w;
                ySum += i / w;
                s++;
            }
            if (s == 0) { return new Vector2(-50000, -50000); }
            return new Vector2(xSum / s, ySum / s);
        }


        //====================  MATH HELPERS  ====================//

        private float Filter(float r)
        {
            float s = 0;
            var a = _lastR.Length;
            for (int i = 0; i < a - 1; i++)
            {
                var x = _lastR[i + 1];
                _lastR[i] = x;
                s += x;
            }
            _lastR[a - 1] = r;
            s += r;
            return s / (float)a;
        }

        private float InvSqr(float i)
        {
            i = 1 - i;
            return 1 - (i * i * i);
        }

        private int GetBestGuessIndex()
        {
            if (_erasing) { return _tempOld; }
            return _tempCI >= 0 ? _tempCI : _ci;
        }


        //====================  PAINT METHODS  ====================//

        private void StartPaint(Vector2 position)
        {
            _painting = true;
            _tempCI = _indices[(int)(position.y * _canvas.height) * _canvas.width + (int)(position.x * _canvas.width)];
            _color = GetActiveColor(position);
        }

        private Color GetActiveColor(Vector2 position)
        {
            var colors = UIMgr.Instance.colors;
            if (!_painting)
            {
                var check = (int)(position.y * _canvas.height) * _canvas.width + (int)(position.x * _canvas.width);
                if (check >= 0 && check < _indices.Length)
                {
                    var ttci = _indices[check];
                    if (ttci >= 0)
                    {
                        return colors[(ttci >= 0 ? ttci : _ci) % colors.Length];
                    }
                }
            }
            if (_erasing) { return Color.white; }
            return colors[(_tempCI >= 0 ? _tempCI : _ci) % colors.Length];
        }

        private void StartErase(Vector2 position)
        {
            _tempOld = -1;
            _erasing = true;
            _color = UIMgr.Instance.canvasClearColor;
        }

        private void ContinuePaint(Vector2 position)
        {
            // Check to make sure last texture coordinate is set to a reasonable value.
            if (_lastTexCoord.x == -50000 || _lastTexCoord.y == -50000)
            {
                return;
            }

            var positions = new List<Vector2>();
            var gap = Vector2.Distance(_lastTexCoord, position) * (_canvas.width + _canvas.height) * 0.5f;
            if (gap > minGap)
            {
                int times = Mathf.FloorToInt(gap / minGap);

                // Increment only the maximum number of times for gaps.
                if (times > maxGapIncrements)
                {
                    times = maxGapIncrements;
                }

                for (float i = 1; i < times; i++)
                {
                    positions.Add(Vector2.Lerp(_lastTexCoord, position, i / times));
                }
            }
            positions.Add(position);

            var ci = GetCurrentIndex();
            BlobData userBlob = GetUserBlobForIndex(ci);
            var affectedIndices = new List<int>();
            foreach (var pos in positions)
            {
                foreach (var ai in DoStrokeAt(pos, ci, userBlob))
                {
                    if (!affectedIndices.Contains(ai)) { affectedIndices.Add(ai); }
                }
            }
            _canvas.Apply();

            var userMarkups = new List<MarkupBase>();
            foreach (var index in affectedIndices)
            {
                var affBlob = GetUserBlobForIndex(index);
                affBlob.center = GetWeightedBlobCenter(ref affBlob.pixels, _canvas.width);
                if (affBlob.center.x < -49000)
                {
                    _userBlobs.Remove(index);
                }
                else
                {
                    userMarkups.Add(affBlob);
                }
            }

            if (!mystery)
            {
                UpdateMarkupPercents(userMarkups);
            }
        }

        private List<int> DoStrokeAt(Vector2 position, int ci, BlobData userBlob)
        {
            HashSet<int> ret = new HashSet<int>();

            if (!_painting && !_erasing) { return ret.ToList(); }
            if (!_erasing) { ret.Add(ci); }

            int cw = _canvas.width;
            int ch = _canvas.height;
            float px = position.x * cw;
            float py = position.y * ch;

            int rw = Mathf.FloorToInt(_radius * cw);
            int rh = Mathf.FloorToInt(_radius * ch);
            int cx = Mathf.FloorToInt(px - rw * 0.5f);
            int cy = Mathf.FloorToInt(py + rh * 0.5f);

            float avgR = (Mathf.Max(rw, rh)) / 4f;

            var color = _color;
            if (!_erasing)
            {
                var colorWiggle = UIMgr.Instance.colorWiggle;
                color = color - new Color(UnityEngine.Random.Range(0, colorWiggle), UnityEngine.Random.Range(0, colorWiggle), UnityEngine.Random.Range(0, colorWiggle), 0);
            }

            var avgRSquared = avgR * avgR;

            // TODO: convert this to run on getpixels/setpixels jfc lol
            int l = _indices.Length;
            for (int x = cx; x < cx + rw; x++)
            {
                for (int y = cy; y >= cy - rh; y--)
                {
                    // Faster to compare squared distances.
                    if ((px - x) * (px - x) + (py - y) * (py - y) < avgRSquared)
                    {
                        _canvas.SetPixel(x, y, color);
                        var pos = y * cw + x;
                        if (pos >= 0 && pos < l)
                        {
                            var old = _indices[pos];
                            if (old > 0)
                            {
                                if (_erasing && _tempOld < 0) { _tempOld = old; }
                                GetUserBlobForIndex(old).pixels[pos] = 0;
                                // Can add at will since hashset by default does not allow for duplicates.
                                ret.Add(old);
                            }

                            _indices[pos] = ci;
                            if (_painting && userBlob != null) { userBlob.pixels[pos] = ci; }
                        }
                    }
                }
            }

            return ret.ToList();
        }

        private void StopPaint()
        {
            if (!_painting && !_erasing) 
			{ 
				return; 
			}

			if (_painting && (_tempCI < 0))
            {
                _ci++;
            }

            _painting = false;
            _erasing = false;
            _tempCI = -1;
            _tempOld = -1;
        }

        //====================  DISTANCE MAPPING STUFF  ====================//

        private void ComputeDistanceMaps(BlobData gt)
        {
            int CORE = 0;
            int NEW = -1;

            var gtpl = gt.pixels.Length;
            gt.distOut = new int[gtpl];
            gt.distIn = new int[gtpl];
            for (int i = 0; i < gtpl; i++)
            {
                var gtm = gt.pixels[i] != 0;
                gt.distOut[i] = gtm ? CORE : NEW;
                gt.distIn[i] = gtm ? NEW : CORE;
            }

            var w = _canvas.width;
            RemapFlaggedIntArrayAsDepth(ref gt.distOut, w, 0, distanceFactor);
            RemapFlaggedIntArrayAsDepth(ref gt.distIn, w, 0, distanceFactor);
        }

        private int RemapFlaggedIntArrayAsDepth(ref int[] mask, int w, int dead = 0, int distInc = 1)
        {
            int CORE = 0;
            int NEW = -1;

            int sum = 0;
            int nextLimit = 0;
            int ml = mask.Length;
			int[] progressMask = new int[ml];
			int[] pMask = new int[ml];
            
			for (int i = 0; i < ml; i++)
            {
                if (mask[i] == CORE) { progressMask[nextLimit++] = i; }
            }

            int n, index, newNextLimit, dist = 0;
            while (nextLimit > 0)
            {
                for (int i = 0; i < nextLimit; i++)
                {
                    pMask[i] = progressMask[i];
                }

                dist += distInc;
                newNextLimit = 0;
                for (int i = 0; i < nextLimit; i++)
                {
                    index = pMask[i];

                    n = index + 1;
                    if (n >= 0 && n < ml && mask[n] == NEW)
                    {
                        mask[n] = dist;
                        progressMask[newNextLimit++] = n;
                    }

                    n = index - 1;
                    if (n >= 0 && n < ml && mask[n] == NEW)
                    {
                        mask[n] = dist;
                        progressMask[newNextLimit++] = n;
                    }

                    n = index + w;
                    if (n >= 0 && n < ml && mask[n] == NEW)
                    {
                        mask[n] = dist;
                        progressMask[newNextLimit++] = n;
                    }

                    n = index - w;
                    if (n >= 0 && n < ml && mask[n] == NEW)
                    {
                        mask[n] = dist;
                        progressMask[newNextLimit++] = n;
                    }
                }
                if (dist > dead) { sum += newNextLimit * dist; }
                nextLimit = newNextLimit;
            }
            return sum;
        }

        private int GetCurrentIndex()
        {
            return _erasing ? -1 : ((_tempCI >= 0) ? _tempCI : _ci);
        }
    }

    //====================  DATA STRUCTURES  ====================//

    public class BlobData : MarkupBase
    {
        public int width;
        public int height;
        public int length;

        public int x;
        public int y;

        public int[] pixels;
        public int[] distOut;
        public int[] distIn;

        public BlobData(int width, int height, int index)
        {
            this.width = width;
            this.height = height;
            length = width * height;
            x = 0;
            y = 0;

            pixels = new int[length];
            this.index = index;
        }

        public int GetPixel(int atX, int atY)
        {
            if (atX < x || atY < y || atX > x + width || atY > y + height)
            {
                return 0;
            }

            return pixels[x + y * width];
        }

        public void Resize()
        {
            int minX = width - 1;
            int maxX = 0;

            // Find min and max x-values.
            for (int j = 0; j < height; j++)
            {
                // Find min x-value.
                for (int i = 0; i < width; i++)
                {
                    if (pixels[i + j * width] != 0)
                    {
                        if (i < minX)
                        {
                            minX = i;
                        }
                        break;
                    }
                }

                // Find max x-value.
                for (int i = width - 1; i >= 0; i--)
                {
                    if (pixels[i + j * width] != 0)
                    {
                        if (i > maxX)
                        {
                            maxX = i;
                        }
                        break;
                    }
                }
            }

            if (maxX <= minX)
            {
                Debug.LogError("Minimum x-value should never be greater or equal than maximum x-value!");
            }


            int minY = height - 1;
            int maxY = 0;

            // Find min and max y-values.
            for (int i = minX; i < maxX; i++)
            {
                // Find min y-value.
                for (int j = 0; j < height; j++)
                {
                    if (pixels[i + j * width] != 0)
                    {
                        if (j < minY)
                        {
                            minY = j;
                        }
                        break;
                    }
                }

                // Find max y-value.
                for (int j = height - 1; j >= 0; j--)
                {
                    if (pixels[i + j * width] != 0)
                    {
                        if (j > maxY)
                        {
                            maxY = j;
                        }
                        break;
                    }
                }
            }

            if (maxY <= minY)
            {
                Debug.LogErrorFormat("Minimum y-value {0} should never be greater or equal than maximum y-value {1}!", minY, maxY);
            }


            // Create new smaller pixel array and remap blob into that area.
            /*int newWidth = maxX - minX;
            int newHeight = maxY - minY;

            int[] oldPixels = pixels;

            pixels = new int[width * height];
            length = newWidth * newHeight;
            for (int i = 0; i < length; i++)
            {
                int innerX = i % newWidth;
                int innerY = Mathf.FloorToInt(i / newWidth);
                pixels[i] = oldPixels[(minX + innerX) + (minY + innerY) * width];
            }

            x = minX;
            y = minY;
            width = newWidth;
            height = newHeight;*/
        }
    }

}