using UnityEngine;
using System.Collections;

namespace ProjectQuorum
{

    public class MouseTool : DriveTool
    {
        public LayerMask imageLayer;

        override protected void Update()
        {
            if (JustClicked()) { NewGraph(); }
            if (Tick() && Clicking()) { DropNode(); }
            //TODO: close the loop when mouse is released?

            TrackMouse();
            CapVelocity(ref _velocity);
            Move();

            var s = Clicking();
            reticleOn.SetActive(s);
            reticleOff.SetActive(!s);
        }

        void TrackMouse()
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 2f, imageLayer))
            {
                var newCoord = new Vector3(hit.textureCoord.x, hit.textureCoord.y) + new Vector3(-0.5f, -0.5f, -0.2f);
                _velocity = newCoord - transform.position;
            }
        }

        bool JustClicked()
        {
            return Input.GetMouseButtonDown(0);
        }

        bool Clicking()
        {
            return Input.GetMouseButton(0);
        }

        bool JustUnClicked()
        {
            return Input.GetMouseButtonUp(0);
        }
    }

}