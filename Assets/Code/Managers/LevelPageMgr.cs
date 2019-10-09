using ProjectQuorum.Data;
using ProjectQuorum.Data.Variables;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectQuorum.Managers
{
    public class LevelPageMgr : BaseMgr
    {
        [Space]
        [Header("World/Level Data")]
        public WorldListData WorldList;

        public IntVariable WorldToLoad;

        public IntVariable LoadLevel;

        public LevelListData Levels;

        [SerializeField]
        private GameLevelData _selectedLevel;

        [Space]
        [Header("Text Fields")]
        public Text WorldName;

        public Text LevelName;

        public Image LevelImage;

        public Text LevelDescription;

        public override void Start()
        {
            base.Start();

            //WorldData worldData = WorldList.Worlds[WorldToLoad.Value];
			WorldData worldData = DataMgr.Instance.CurrentWorldData;

            Levels = worldData.LevelList;

            _selectedLevel = Levels.Levels[LoadLevel.Value];

            WorldName.text = worldData.WorldName.ToUpper();
            LevelName.text = string.Format("Level {0}", LoadLevel.Value + 1);

            if (_selectedLevel != null)
            {
				if (_selectedLevel.Photo == null)
				{
					if (!string.IsNullOrEmpty(_selectedLevel.PhotoUrl))
					{
						// Format photo string URL here.
					}
					else
					{
						Debug.LogErrorFormat("Cannot load photo at URL: {0}!", _selectedLevel.Photo);
					}
				}

                LevelImage.sprite = Sprite.Create(_selectedLevel.Photo, new Rect(0, 0, _selectedLevel.Photo.width, _selectedLevel.Photo.height), Vector2.zero);

                LevelDescription.text = _selectedLevel.Description;
            }
        }
    }
}
