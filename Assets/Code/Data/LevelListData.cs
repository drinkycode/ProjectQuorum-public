using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.Data
{
    [CreateAssetMenu(menuName = "Data/Level List Data", fileName = "New Level List Data")]
    public class LevelListData : ScriptableObject
    {

        public List<GameLevelData> Levels;

    }
}