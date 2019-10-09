using UnityEngine;

namespace ProjectQuorum.Managers
{

    public enum ToolTypes
    {
        None = 0,
        Paint,
        Neuron,
        Segment
    }

    public enum TutorialMode
    {
        Off = 0,
        Simple,
        Full
    }

    public class ToolMgr : MonoBehaviour
    {
        public static ToolMgr Instance;

        public ToolBase activeTool;

        public GameObject[] tools;

        public void Awake()
        {
            Instance = this;
        }

        public void TurnOn(ToolTypes tool)
        {
            TurnOffAll();
            foreach (var tgo in tools)
            {
                if (tgo == null) { continue; }
                var toolBase = tgo.GetComponentInChildren<ToolBase>(true);
                if (toolBase == null) { continue; }

                if (toolBase.tool == tool)
                {
                    toolBase.mystery = GameMgr.CurrentIsMystery();
                    activeTool = toolBase;
                    tgo.SetActive(true);

                    if (ToolText.instance != null) { ToolText.instance.Log(tgo.name); }
                    if (GuideText.instance != null) { GuideText.instance.ResetForTool(tgo); }

                    break;
                }
            }
        }

        public void TurnOffAll()
        {
            foreach (var tool in tools)
            {
                tool.SetActive(false);
            }
        }
    }

}