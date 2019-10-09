using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI.Buttons
{

    public class WorldButton : MonoBehaviour
    {

        public Text WorldName;

        public Text LevelsCompleted;

        public Image WorldImage;

        [SerializeField]
        private Button _button;

        public void UpdateWorldButton(string newWorldName, int levels)
        {
            WorldName.text = newWorldName;
            LevelsCompleted.text = string.Format("{0} Levels Available", levels);
        }
    }

}