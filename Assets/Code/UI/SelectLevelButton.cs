using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{

    public class SelectLevelButton : MonoBehaviour
    {

        public GameObject MysteryIcon;

        public GameObject StarIcons;

        public List<GameObject> Stars;

        public void Show(bool isMystery)
        {
            gameObject.SetActive(true);

            if (isMystery)
            {
                MysteryIcon.SetActive(true);
                StarIcons.SetActive(false);
            }
            else
            {
                MysteryIcon.SetActive(false);
                StarIcons.SetActive(true);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

    }

}