using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectQuorum.Managers
{

    public class TitleMgr : BaseMgr
    {
        
		[Header("Title Scene")]
		public Text LoginButtonText;

		public Text UserText;

        public GameObject AboutPanel;

        public override void Start()
		{
			base.Start();

			CheckLoginStatus();

			_loginPanel.OnLoginEvent.AddListener(CheckLoginStatus);
        }

		public void CheckLoginStatus()
		{
			if (ConnectionMgr.Instance.IsLoggedIn)
			{
				LoginButtonText.text = "Start";
				UserText.text = string.Format("Current user: {0}", ConnectionMgr.Instance.CurrentAccount.email);
			}
			else
			{
				LoginButtonText.text = "Log In";
				UserText.text = "Not logged in!";
			}
		}

		public void OnLoginClick()
		{
			if (ConnectionMgr.Instance.IsLoggedIn)
			{
				GotoNextScene();
			}
			else
			{
				_loginPanel.Show();
			}
		}

        public void OnForgotPassword()
        {

        }

        public void OnRegisterClick()
        {
            Application.OpenURL("https://projectquorum.org/signup");
        }

		public void OnLogoutClick()
		{
			if (ConnectionMgr.Instance.Logout())
            {
                CheckLoginStatus();
            }
		}

        public void ShowAbout()
        {
            if (AboutPanel != null)
            {
                AboutPanel.SetActive(true);
            }
        }

        public void CloseAbout()
        {
            if (AboutPanel != null)
            {
                AboutPanel.SetActive(false);
            }
        }

        public void OnWebsiteClick()
        {
            Application.OpenURL("https://projectquorum.org");
        }

    }

}