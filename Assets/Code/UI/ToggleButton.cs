using ProjectQuorum.Audio;
using UnityEngine;

namespace ProjectQuorum.UI
{

    public class ToggleButton : MonoBehaviour
    {

        public GameObject Active;

        public GameObject Inactive;

        [Space]
        [Header("Audio Clip")]
        [SerializeField]
        private AudioSource _clip;

        public void OnValidate()
        {
            if (_clip == null)
            {
                _clip = GetComponent<AudioSource>();
            }
        }

        public void Toggle(bool toggle, bool playFX = true)
        {
            Active.SetActive(toggle);
            Inactive.SetActive(!toggle);

            // Things to do if being toggled to active.
            if (toggle)
            {
                if (playFX && (_clip != null))
                {
                    AudioMgr.Instance.PlayFX(_clip.clip);
                }
            }
        }

    }

}
