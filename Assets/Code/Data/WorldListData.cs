using System.Collections.Generic;
using UnityEngine;

namespace ProjectQuorum.Data
{
    [CreateAssetMenu(menuName = "Data/World Data List", fileName = "New World List Data")]
    public class WorldListData : ScriptableObject
    {

        public List<WorldData> Worlds;

    }
}