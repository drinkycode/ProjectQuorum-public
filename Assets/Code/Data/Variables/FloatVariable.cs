using UnityEngine;

namespace ProjectQuorum.Data.Variables
{
    [CreateAssetMenu(menuName = "Variables/Float Variable", fileName = "New Float Variable")]
    public class FloatVariable : GenericVariable<float, FloatVariable>
    {
        public override void SetValue(FloatVariable value)
        {
            Value = value.Value;
        }

        public override void ApplyChange(float amount)
        {
            Value += amount;
        }

        public override void ApplyChange(FloatVariable amount)
        {
            Value += amount.Value;
        }
    }
}