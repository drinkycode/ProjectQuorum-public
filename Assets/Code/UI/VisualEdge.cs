using ProjectQuorum.Managers;
using ProjectQuorum.Tools;

using UnityEngine;

namespace ProjectQuorum.UI
{

    public class VisualEdge : MonoBehaviour
    {

        private Node _neighbor;
        public Node neighbor
        {
            get
            {
                return _neighbor;
            }
            set
            {
                _neighbor = value;
            }
        }

        public void LateUpdate()
        {
            if (neighbor == null)
            {
                return;
            }

            Transform tt = ToolMgr.Instance.activeTool.transform;
            Vector3 wnc = tt.TransformPoint(neighbor.center);

            wnc.z = transform.position.z;
            transform.LookAt(wnc);
            transform.localScale = new Vector3(1f, 1f, Vector3.Distance(transform.position, wnc));
        }
    }

}