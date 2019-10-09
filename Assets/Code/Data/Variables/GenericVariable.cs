using UnityEngine;

namespace ProjectQuorum.Data.Variables
{
    public class GenericVariable<T, This> : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif

        public T Value;

        public void SetValue(T value)
        {
            Value = value;
        }

        public virtual void SetValue(This value)
        {
        }

        public virtual void ApplyChange(T amount)
        {
        }

        public virtual void ApplyChange(This amount)
        {
        }
    }
}