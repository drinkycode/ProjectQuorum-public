using ProjectQuorum.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectQuorum.Data
{

    public class DataMgr : MonoBehaviour
    {

        public static DataMgr Instance;

        /// <summary>
		/// Reference to base manager that should exist in every scene.
		/// </summary>
		[SerializeField]
        private BaseMgr _manager;

        [Space]
        [Header("Data")]

        public WorldListData CurrentWorldList;

        public WorldData CurrentWorldData;

        public LevelListData CurrentLevelList;

        public GameLevelData CurrentGameLevelData;

        [Space]
        [Header("Scriptable Object References")]
        public WorldListData WorldListDataSO;

        public WorldData WorldDataSO;

        public LevelListData LevelListDataSO;

        public GameLevelData GameLevelDataSo;

        public List<Texture2D> DebugGroundTruths;

        public void OnValidate()
        {
            
        }

        public void Awake()
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            FindBaseManager();
        }

        public void Start()
        {
            // Make sure this is a persistent object for data purposes.
            DontDestroyOnLoad(this);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindBaseManager();
        }

        private void FindBaseManager()
        {
            // Find new base manager.
            _manager = FindObjectOfType<BaseMgr>();
        }

        public void UpdateWorldsList(List<ProjectWorldData> newWorlds)
        {
            if (CurrentWorldList == null)
            {
                CurrentWorldList = ScriptableObject.Instantiate(WorldListDataSO);
            }

            CurrentWorldList.Worlds.Clear();

            for (int i = 0; i < newWorlds.Count; i++)
            {
                ProjectWorldData world = newWorlds[i];

                if (world != null)
                {
                    WorldData worldData = ScriptableObject.Instantiate(WorldDataSO);

                    //worldData.Tool = WorldData.GetToolType(world.tool_type);
					worldData.Tool = ToolTypes.Paint;

                    worldData.WorldName = world.name;
                    worldData.WorldDescription = world.description;

                    LevelListData worldLevelList = ScriptableObject.Instantiate(LevelListDataSO);

                    for (int j = 0; j < world.levels.Length; j++)
                    {
                        WorldLevelData levelData = world.levels[j];

                        GameLevelData gameLevel = ScriptableObject.Instantiate(GameLevelDataSo);

                        Debug.Log(levelData.id + " " + levelData.name + " " + levelData.is_mystery + " " + levelData.image_url);

						gameLevel.Tool = worldData.Tool;
                        gameLevel.LevelID = levelData.id;
						gameLevel.Name = levelData.name;
						gameLevel.Description = levelData.description;

                        gameLevel.IsMystery = string.IsNullOrEmpty(levelData.is_mystery) ? true : false;
                        //gameLevel.IsMystery = false;

                        gameLevel.PhotoUrl = levelData.image_url;

                        gameLevel.PanAmount = new Vector2(0.1f, 0.1f);
                        gameLevel.ZoomedInPanAmount = new Vector2(0.5f, 0.5f);

                        if (levelData.id.Equals("54", System.StringComparison.OrdinalIgnoreCase))
                        {
                            gameLevel.IsMystery = false;
                            gameLevel.GroundTruthTex2D = DebugGroundTruths[0];
                        }

                        worldLevelList.Levels.Add(gameLevel);
                    }

                    worldData.LevelList = worldLevelList;

                    CurrentWorldList.Worlds.Add(worldData);
                }
            }
        }
    }

}