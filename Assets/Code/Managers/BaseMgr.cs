using ProjectQuorum.UI;
using ProjectQuorum.UI.Buttons;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectQuorum.Managers
{

    public class BaseMgr : MonoBehaviour
    {
		[Header("Scene/Transition Variables")]
        [SerializeField]
        private TransitionPanel _transitionPanel;

        public bool ShowHideTransition = true;

        public string NextScene = "";

        public string ReturnScene = "";

        [Space]
		[Header("Login Variables")]
		public bool AllowLogin = true;

		public bool ShowLogin = true;

		[SerializeField]
		protected LoginPanel _loginPanel;

		[SerializeField]
		protected AccountButton _accountButton;

		[Space]
		[Header("Login Variables")]
		public bool AllowOptions = true;

        [SerializeField]
		protected OptionsPanel _optionsPanel;

		public virtual void Awake()
		{
            if (_transitionPanel == null)
            {
                _transitionPanel = FindObjectOfType<TransitionPanel>();
            }

            if (_accountButton == null)
            {
                _accountButton = FindObjectOfType<AccountButton>();
            }
		}

        public virtual void Start()
        {
            if (AllowLogin)
            {
                LoginPanel[] panels = Resources.FindObjectsOfTypeAll<LoginPanel>();
                if (panels.Length > 0)
                {
                    _loginPanel = panels[0]; // Assume only one type of this object exists.

                    if (!ConnectionMgr.Instance.IsLoggedIn)
                    {
                        if (ShowLogin)
                        {
                            Debug.Log("Showing login panel");
                            _loginPanel.Show();
                        }
                    }
                    else
                    {
                        _loginPanel.Hide();

                        UpdateAccountInfo(ConnectionMgr.Instance.CurrentAccount);
                    }
                }
                else
                {
                    Debug.LogError("Missing valid LoginPanel prefab in this scene!");
                }
            }

            if (AllowOptions)
            {
                OptionsPanel[] panels = Resources.FindObjectsOfTypeAll<OptionsPanel>();
                if (panels.Length > 0)
                {
                    _optionsPanel = panels[0]; // Assume only one type of this object exists.
                    _optionsPanel.Hide();
                }
                else
                {
                    Debug.LogError("Missing valid OptionPanel prefab in this scene!");
                }
            }

            if (ShowHideTransition)
            {
                if (_transitionPanel != null)
                {
                    _transitionPanel.Hide();
                }
            }
        }

		public virtual void UpdateAccountInfo(AccountData newUserAccount)
		{
			if (_accountButton != null)
			{
				_accountButton.UpdateAccountName(newUserAccount.username);
			}
            else
            {
                Debug.LogError("No account button found!");
            }
		}

		public virtual void GotoNextScene()
        {
            StartCoroutine(LoadScene(NextScene, 0.48f));
        }

        public virtual void GotoReturnScene()
        {
            StartCoroutine(LoadScene(ReturnScene, 0.48f));
        }

        private IEnumerator LoadScene(string loadScene, float delay)
        {
            if (_transitionPanel != null)
            {
                _transitionPanel.Show();
            }

            yield return new WaitForSeconds(delay);

            SceneManager.LoadScene(loadScene);
        }
    }

}