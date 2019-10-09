using ProjectQuorum.Data.Variables;
using ProjectQuorum.Managers;
using ProjectQuorum.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class ResultsPanel : MonoBehaviour
    {

        public FloatVariable CurrentAcurracy;
        
        public FloatVariable CurrentTime;

        public FloatVariable LevelScore;

        [Space]
        [Header("UI Elements")]

        public Text TimerText;

        public Text AccuracyText;

        public List<Image> Stars;

        public Text ScoreText;

        public GameObject ReturnButton;

        public void OnEnable()
        {
            TimerText.text = "Time: 0:00";
            AccuracyText.text = "Accuracy: 0%";
            ScoreText.text = "Score: 0";

            SetStars(0);

            ReturnButton.SetActive(false);

            // TODO: Clean up and make ratings scoring an adjustable thing
            int rating = 0;
            if (CurrentAcurracy.Value > 0.9f)
            {
                rating = 3;
            }
            else if (CurrentAcurracy.Value > 0.8f)
            {
                rating = 2;
            }
            else
            {
                rating = 1;
            }

            StartCoroutine(DoReveal(rating));
        }

        public void SetStars(int rating)
        {
            // Clear stars
            for (int i = 0; i < Stars.Count; i++)
            {
                Stars[i].fillAmount = 0f;
            }

            for (int i = 0; i < rating; i++)
            {
                Stars[i].fillAmount = 1f;
            }
        }

        private IEnumerator DoReveal(int rating)
        {
            // First tally up time amount.

            float time = CurrentTime.Value;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                TimerText.text = string.Format("Time: {0}", LevelTimer.FormatIntoTimer(time * t));
                yield return null;
            }

            TimerText.text = string.Format("Time: {0}", LevelTimer.FormatIntoTimer(time));

            yield return new WaitForSeconds(0.5f);

            // Tally up accuracy now.

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                AccuracyText.text = string.Format("Accuracy: {0}%", Mathf.Floor(CurrentAcurracy.Value * t * 100f));
                yield return null;
            }

            AccuracyText.text = string.Format("Accuracy: {0}%", Mathf.Floor(CurrentAcurracy.Value * 100f));

            yield return new WaitForSeconds(0.5f);

            // Show star rating.

            int stars = 0;
            float fillTime = 0.5f;

            while (stars < rating)
            {
                Image star = Stars[stars];

                t = 0f;
                while (t < fillTime)
                {
                    t += Time.deltaTime;
                    star.fillAmount = t / fillTime;
                    yield return null;
                }

                star.fillAmount = 1f;
                stars++;
            }

            yield return new WaitForSeconds(1f);

            // Tally up final score

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                ScoreText.text = string.Format("Score: {0}", Mathf.Floor(LevelScore.Value * t));
                yield return null;
            }

            ScoreText.text = string.Format("Score: {0}", Mathf.Floor(LevelScore.Value));

            yield return new WaitForSeconds(1f);

            ReturnButton.SetActive(true);
        }
    }
}