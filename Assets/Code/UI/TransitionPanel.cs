using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.UI
{
    public class TransitionPanel : MonoBehaviour
    {

        public List<GameObject> Layers;

        [SerializeField]
        private Animator _animator;

        public void Start()
        {
            if (_animator == null)
            {
                _animator.GetComponent<Animator>();
            }
        }

        public void Show()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].SetActive(true);
            }

            _animator.SetTrigger("Show");
        }

        public void Hide()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].SetActive(true);
            }

            _animator.SetTrigger("Hide");
            StartCoroutine(DelayHide(0.55f));
        }

        private IEnumerator DelayHide(float delay)
        {
            yield return new WaitForSeconds(delay);

            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].SetActive(false);
            }
        }

    }
}