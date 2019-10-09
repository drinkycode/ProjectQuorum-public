using System;
using UnityEngine;

namespace ProjectQuorum.Data.Variables
{
    [CreateAssetMenu(menuName = "Variables/Int Variable", fileName = "New Int Variable")]
    public class IntVariable : GenericVariable<int, IntVariable>
    {
        public override void SetValue(IntVariable value)
        {
            Value = value.Value;
        }

        public override void ApplyChange(int amount)
        {
            Value += amount;
        }

        public override void ApplyChange(IntVariable amount)
        {
            Value += amount.Value;
        }
    }
}