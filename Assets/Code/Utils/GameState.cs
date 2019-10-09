using ProjectQuorum.Audio;
using ProjectQuorum.Managers;
using UnityEngine;

namespace ProjectQuorum.Utils
{
    public class GameState : MonoBehaviour
    {
		
        public GameObject AudioMgrPrefab;

		public GameObject ConnectionMgrPrefab;

        public void Awake()
        {
            // Check to see if audio manager exists.
            AudioMgr audio = FindObjectOfType<AudioMgr>();
			if (audio == null)
            {
                GameObject.Instantiate(AudioMgrPrefab);
            }

			// Check to see if audio manager exists.
			ConnectionMgr connection = FindObjectOfType<ConnectionMgr>();
			if (connection == null)
			{
				GameObject.Instantiate(ConnectionMgrPrefab);
			}
        }
    }
}
