using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum.UI.Buttons
{

    public class AccountButton : MonoBehaviour
    {
        [SerializeField]
        private Text _accountName;

        public void UpdateAccountName(string newName)
        {
            if (_accountName == null)
            {
                _accountName = GetComponentInChildren<Text>();
            }

            if (!string.IsNullOrEmpty(newName))
			{
                _accountName.text = newName;
			}
			else
			{
                _accountName.text = "No user";
			}
        }
    }

}