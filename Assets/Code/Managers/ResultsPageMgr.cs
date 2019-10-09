using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ProjectQuorum.Managers
{
    public class ResultsPageMgr : MonoBehaviour
    {
        public string levelMenuScene = "LevelMenu";

        public void OnButton()
        {
            SceneManager.LoadScene(levelMenuScene);
        }
    }
}
