using ProjectQuorum.Managers;
using UnityEngine;

namespace ProjectQuorum
{

    public class DebugStartup : MonoBehaviour
    {
        public KeyCode ChangeLevelKey = KeyCode.Space;

        public void Start()
        {

        }

        public void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(ChangeLevelKey))
            {
                // Change level
                UIMgr.Instance.OnEditButton();
            }
#endif
        }
    }

}
