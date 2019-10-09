using UnityEngine;

namespace ProjectQuorum.Audio
{

    public class ButtonFX : MonoBehaviour
    {
        // TODO: Place volume for button clip elsewhere?
        public float Volume = 1f;

        public void PlayFX(AudioClip clip)
        {
            AudioMgr.Instance.PlayFX(clip, Volume);
        }
    }

}