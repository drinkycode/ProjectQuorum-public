using ProjectQuorum.Managers;

using UnityEngine;
using UnityEditor;

namespace ProjectQuorum
{
    [CustomEditor(typeof(ToolMgr))]
    public class ToolMgrEditor : Editor
    {
        public ToolMgr toolMgr
        {
            get
            {
                return (ToolMgr)target;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if ((toolMgr != null) && GUILayout.Button("Init From Children"))
            {
                var toolCount = toolMgr.transform.childCount;
                toolMgr.tools = new GameObject[toolCount];

                for (int i = 0; i < toolCount; i++)
                {
                    toolMgr.tools[i] = toolMgr.transform.GetChild(i).gameObject;
                }
            }
        }
    }
}