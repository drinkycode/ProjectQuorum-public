using ProjectQuorum.Managers;

using UnityEngine;
using UnityEngine.UI;

namespace ProjectQuorum
{

    public class GuideText : MonoBehaviour
    {
        static public GuideText instance;

        public Text textField;

        public string z;

        public void Awake()
        {
            instance = this;
        }

        public void ResetForTool(GameObject tool)
        {
            Clear();
            Add(tool.name.ToUpper() + " CONTROLS");
            AddTool(tool);
            Add();
            Add("OTHER TOOLS");
            AddToolMenu();
            Add();
            Add("GLOBAL");
            Add(z);
        }

        public void AddTool(GameObject tool)
        {
            var ge = tool.GetComponent<GuideElement>();
            if (ge == null) { return; }
            foreach (var line in ge.lines)
            {
                Add(line);
            }
        }

        public void AddToolMenu()
        {
            int i = 1;
            foreach (var tool in ToolMgr.Instance.tools)
            {
                Add(i++ + " - " + tool.name);
            }
        }

        public void Clear()
        {
            textField.text = "";
        }

        public void Add(string text = "")
        {
            textField.text += text + "\n";
        }
    }

}