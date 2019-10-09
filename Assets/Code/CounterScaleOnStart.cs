using ProjectQuorum.Managers;
using UnityEngine;

namespace ProjectQuorum
{

    public class CounterScaleOnStart : MonoBehaviour
    {
        public void Start()
        {
            Vector3 ttls = ToolMgr.Instance.activeTool.transform.lossyScale;
            transform.localScale = new Vector3(transform.localScale.x * 1f / ttls.x,
                                                transform.localScale.y * 1f / ttls.y,
                                                transform.localScale.z * 1f / ttls.z);
        }
    }

}