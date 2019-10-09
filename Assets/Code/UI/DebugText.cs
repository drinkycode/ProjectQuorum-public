using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ProjectQuorum.UI
{

    public class DebugText : MonoBehaviour
    {
        static public DebugText instance;

        public Text textField;

        public static void LogText(string log)
        {
            if (instance != null)
            {
                instance.Log(log);
            }
        }

        public void Awake()
        {
            instance = this;
        }

        public void Log(string log)
        {
            textField.text = log;
        }
    }

}