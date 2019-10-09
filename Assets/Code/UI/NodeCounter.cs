using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{

    public class NodeCounter : MonoBehaviour
    {

        public GameObject Half;
        public GameObject Finished;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetProgress(bool finished)
        {
            Half.SetActive(false);
            Finished.SetActive(finished);
        }

    }

}
