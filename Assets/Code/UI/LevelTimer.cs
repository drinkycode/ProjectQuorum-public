using ProjectQuorum.Managers;
using ProjectQuorum.Data.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class LevelTimer : MonoBehaviour
    {

        public static string FormatIntoTimer(float time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            return (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
        }


        public FloatVariable CurrentTime;

        public Text TimerText;

        private bool _running;

        public void Start()
        {
			ResetTimer();
        }

        public void ResetTimer()
        {
			CurrentTime.Value = 0;
            _running = true;
        }

        public void StopTimer()
        {
			_running = false;
        }

        public void Update()
        {
			if (_running)
            {
				CurrentTime.Value += Time.deltaTime;
				TimerText.text = FormatIntoTimer(CurrentTime.Value);
            }
        }
    }
}
