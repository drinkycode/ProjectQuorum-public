using ProjectQuorum.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ProjectQuorum.UI
{
    public class LoginPanel : MonoBehaviour
    {

		public GameObject UserSubPanel;

		public GameObject LoginSubPanel;

        [Header("User Info")]

        public Text UserName;

		public Text UserFullName;

		public Text UserEmail;

        public Text UserRole;

        [Header("Login Fields")]
		public InputField UsernameInput;

        public InputField PasswordInput;

		public Text ErrorMessage;

		[Header("Login Events")]
		public UnityEvent OnLoginEvent;

		public UnityEvent OnLogoutEvent;
        
        public void OnEnable()
        {
            
        }

        public void Show()
        {
            gameObject.SetActive(true);

			if (ConnectionMgr.Instance.IsLoggedIn)
			{
				UserSubPanel.SetActive(true);
				LoginSubPanel.SetActive(false);
			}
			else
			{
				LoginSubPanel.SetActive(true);
				UserSubPanel.SetActive(false);

				// Clear password input.
				PasswordInput.text = string.Empty;
			}
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Update()
        {
            if (LoginSubPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {

                }
            }
        }

        public void Login()
		{
			if (ConnectionMgr.Instance.Login(UsernameInput.text, PasswordInput.text))
			{
				UpdateInfo(ConnectionMgr.Instance.CurrentAccount);

				ErrorMessage.gameObject.SetActive(false);
				Hide();

				if (OnLoginEvent != null)
				{
					OnLoginEvent.Invoke();
				}
			}
			else
			{
				ErrorMessage.gameObject.SetActive(true);
			}
		}

		public void UpdateInfo(AccountData newUser)
		{
            UserName.text = newUser.username;
            UserFullName.text = string.Format("{0} {1}", newUser.first_name, newUser.last_name);
            UserEmail.text = newUser.email;
            UserRole.text = newUser.role;
        }

		public void Logout()
		{
			ConnectionMgr.Instance.Logout();
			Hide();
		}

    }
}