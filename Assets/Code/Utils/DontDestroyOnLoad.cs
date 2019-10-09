using UnityEngine;

namespace ProjectQuorum.Utils
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        public void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
