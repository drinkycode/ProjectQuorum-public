using ProjectQuorum.Data;
using ProjectQuorum.Data.Variables;
using ProjectQuorum.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectQuorum.Managers
{

    public class LevelMenuMgr : BaseMgr
    {

		public WorldListData WorldList;

		public IntVariable WorldToLoad;

		public IntVariable LoadLevel;

		public Text WorldName;

		public Text WorldDescription;

		public List<SelectLevelButton> LevelButtons;

		public override void Start()
		{
            base.Start();

			//WorldData world = WorldList.Worlds[WorldToLoad.Value];
			WorldData world = DataMgr.Instance.CurrentWorldData;

			WorldName.text = world.WorldName.ToUpper();
			WorldDescription.text = world.WorldDescription;

			int numLevels = world.LevelList.Levels.Count;
			for (int i = 0; i < LevelButtons.Count; i++) 
			{
				if (i < numLevels) 
				{
                    LevelButtons[i].Show(world.LevelList.Levels[i].IsMystery);
				} 
				else 
				{
                    LevelButtons[i].Hide();
				}
			}
		}

        public void SelectLevel(int level)
        {
            LoadLevel.SetValue(level);
            GotoNextScene();
        }
    }

}