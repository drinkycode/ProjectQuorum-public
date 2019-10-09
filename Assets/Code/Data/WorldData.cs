using ProjectQuorum.Managers;
using System;
using UnityEngine;

namespace ProjectQuorum.Data
{
    [CreateAssetMenu(menuName = "Data/World Data", fileName = "New World Data")]
    public class WorldData : ScriptableObject
    {

        public static ToolTypes GetToolType(string tool)
        {
            if (tool.Equals("Paint", StringComparison.OrdinalIgnoreCase))
            {
                return ToolTypes.Paint;
            }
            else if (tool.Equals("Neuron", StringComparison.OrdinalIgnoreCase))
            {
                return ToolTypes.Neuron;
            }
            else if (tool.Equals("Segment", StringComparison.OrdinalIgnoreCase))
            {
                return ToolTypes.Segment;
            }

            return ToolTypes.None;
        }

        public ToolTypes Tool;

        public string WorldName;

        [TextArea]
        public string WorldDescription;

        public Sprite WorldImage;

        public LevelListData LevelList;

    }
}