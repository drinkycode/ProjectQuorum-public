using ProjectQuorum.Data.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ProjectQuorum.Audio;

namespace ProjectQuorum.UI
{
    public class OptionsPanel : MonoBehaviour
    {

        [Header("Sound Options")]
        public ToggleButton MusicOn;
        public ToggleButton MusicOff;
        public ToggleButton FXOn;
        public ToggleButton FXOff;

        public void OnEnable()
        {
            if (AudioMgr.Instance != null)
            {
                if (AudioMgr.Instance.MusicMuted)
                {
                    ToggleMusicButtons(false, false);
                }
                else
                {
                    ToggleMusicButtons(true, false);
                }

                if (AudioMgr.Instance.FXMuted)
                {
                    ToggleFXButtons(false, false);
                }
                else
                {
                    ToggleFXButtons(true, false);
                }
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void TurnMusicOn(bool playFX = true)
        {
            if (AudioMgr.Instance != null)
            {
                AudioMgr.Instance.MuteMusic(false);
            }
            ToggleMusicButtons(true, playFX);
        }

        public void TurnMusicOff(bool playFX = true)
        {
            if (AudioMgr.Instance != null)
            {
                AudioMgr.Instance.MuteMusic(true);
            }
            ToggleMusicButtons(false, playFX);
        }

        private void ToggleMusicButtons(bool toggleOn, bool playFX = true)
        {
            MusicOn.Toggle(toggleOn, playFX);
            MusicOff.Toggle(!toggleOn, playFX);
        }

        public void TurnFXOn(bool playFX = true)
        {
            if (AudioMgr.Instance != null)
            {
                AudioMgr.Instance.MuteFX(false);
            }
            ToggleFXButtons(true, playFX);
        }

        public void TurnFXOff(bool playFX = true)
        {
            if (AudioMgr.Instance != null)
            {
                AudioMgr.Instance.MuteFX(true);
            }
            ToggleFXButtons(false, playFX);
        }

        private void ToggleFXButtons(bool toggleOn, bool playFX = true)
        {
            FXOn.Toggle(toggleOn, playFX);
            FXOff.Toggle(!toggleOn, playFX);
        }

    }
}