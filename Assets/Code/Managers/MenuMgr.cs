using ProjectQuorum.Data;
using ProjectQuorum.Data.Variables;
using ProjectQuorum.UI.Buttons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.Managers
{
    public class MenuMgr : BaseMgr
    {

		[Space]
		[Header("World Variables")]
		public WorldListData WorldList;
        
		public IntVariable WorldToLoad;

		[Space]
		[Header("UI Variables")]
		public Slider WorldSlider;

		public RectTransform SliderViewport;

		public Text WorldDescription;

        [Space]
        [Header("World Menu")]
        public Transform WorldMenu;

        public GameObject WorldButtonPrefab;

        public override void Start()
		{
            base.Start();

			WorldToLoad.Value = 0;
			UpdateWorldSelection();

            List<ProjectWorldData> userWorlds;

            if (!ConnectionMgr.OFFLINE_DEBUG)
            {
                userWorlds = ConnectionMgr.Instance.GetProjectWorldsByUser(ConnectionMgr.Instance.CurrentAccount.id);

                DataMgr.Instance.UpdateWorldsList(userWorlds);

                for (int i = 0; i < userWorlds.Count; i++)
                {
                    ProjectWorldData world = userWorlds[i];

                    if (world != null)
                    {
                        Debug.LogFormat("Found user world {0} {1}!", world.name, world.levels.Length);
                        GameObject newWorldButton = GameObject.Instantiate(WorldButtonPrefab);

                        WorldButton button = newWorldButton.GetComponent<WorldButton>();
                        button.UpdateWorldButton(world.name, world.levels.Length);

                        newWorldButton.transform.SetParent(WorldMenu);
                        newWorldButton.transform.localScale = new Vector3(1f, 1f, 1f); // Need to reset scale back to 1.
                    }
                }
            }
            else
            {
                for (int i = 0; i < WorldList.Worlds.Count; i++)
                {
                    WorldData world = WorldList.Worlds[i];

                    if (world != null)
                    {
                        Debug.LogFormat("Found user world {0} {1}!", world.name, world.LevelList.Levels.Count);
                        GameObject newWorldButton = GameObject.Instantiate(WorldButtonPrefab);

                        WorldButton button = newWorldButton.GetComponent<WorldButton>();
                        button.UpdateWorldButton(world.name, world.LevelList.Levels.Count);

                        newWorldButton.transform.SetParent(WorldMenu);
                        newWorldButton.transform.localScale = new Vector3(1f, 1f, 1f); // Need to reset scale back to 1.
                    }
                }
            }
		}

        public override void GotoNextScene()
        {
			DataMgr.Instance.CurrentWorldData = DataMgr.Instance.CurrentWorldList.Worlds[WorldToLoad.Value];

            base.GotoNextScene();
        }

        public void UpdateWorldSelection()
		{
			//Debug.LogFormat ("Updating world {0}", WorldSlider.value);

			WorldToLoad.Value = Mathf.RoundToInt(WorldSlider.value);
			SliderViewport.anchoredPosition = new Vector2(WorldToLoad.Value * -350f, SliderViewport.anchoredPosition.y);
			WorldDescription.text = WorldList.Worlds[WorldToLoad.Value].WorldDescription;
		}

    }
}