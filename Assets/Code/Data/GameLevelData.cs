using ProjectQuorum.Managers;
using UnityEngine;

namespace ProjectQuorum.Data
{
    [CreateAssetMenu(menuName = "Data/Level Data", fileName = "New Level Data")]
    public class GameLevelData : ScriptableObject
    {

        public ToolTypes Tool;

        public string LevelID;

        [Space]
        [Header("Level Descriptors")]
        public string Name;

        [TextArea]
        public string Description;

        [Space]
        [Header("Level Properties")]
        public bool IsMystery;

        public TutorialMode TutorialMode;

        public Vector2 Size
        {
            get
            {
                if (Tool != ToolTypes.Segment)
                {
                    return new Vector2(Photo.width, Photo.height);
                }
                else if (Tool == ToolTypes.Segment)
                {
                    //return new Vector2(1355f, 255f);

                    // TODO: Investigate why this is hardbaked in?
                    return new Vector2(1355f, 962f);
                }
                return Vector2.zero;
            }
        } 

        // TODO: Replace with auto-detection for these values.
        public Vector2 ZoomSizes = new Vector2(0.5f, 0.3f);

        // TODO: Replace with auto-detection for these values.
        public Vector2 PanAmount = new Vector2(0f, 0f);
        public Vector2 ZoomedInPanAmount = new Vector2(0f, 0f);

        [Space]
        [Header("Source Photo")]
        public Texture2D Photo;

        public bool InvertPhoto;

		public string PhotoUrl;

        [Space]
        [Header("Segment Data")]
        public TextAsset Data;

        [Space]
        [Header("Ground Truths")]
        public Texture2D GroundTruthTex2D;

        public TextAsset GroundTruthTextAsset;

    }
}