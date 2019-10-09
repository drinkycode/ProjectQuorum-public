using ProjectQuorum.Tools;
using System.Collections;
using UnityEngine;

namespace ProjectQuorum
{

    public class DriveTool : MonoBehaviour
    {
        public GameObject reticleOn;
        public GameObject reticleOff;

        public float speed = 1f;
        public float shift = 0.1f;
        public float maxSpeed = 2f;
        public float releaseBrake = 0.5f;
        public float dropDelay = 0.5f;

        protected Vector3 _velocity;

        Graph _graph;

        float _tick;

        void OnEnable()
        {
            NodeMgr.instance.Clear();
        }

        virtual protected void Update()
        {
            if (JustShifted()) { NewGraph(); }
            if (Tick() && Shift()) { DropNode(); }

            //TODO: close the loop when space is released?

            if (JustUnShifted()) { CutSpeed(); }

            CheckArrowKeys();
            CapVelocity(ref _velocity);
            Move();

            var s = Shift();
            reticleOn.SetActive(s);
            reticleOff.SetActive(!s);
        }

        protected bool Tick()
        {
            _tick += Time.deltaTime;
            if (_tick > dropDelay)
            {
                _tick -= dropDelay;
                return true;
            }
            return false;
        }

        protected void Move()
        {
            transform.position += _velocity * (Shift() ? shift : 1f) * Time.deltaTime;
        }

        protected void CapVelocity(ref Vector3 velocity)
        {
            if (Vector3.Distance(Vector3.zero, velocity) > maxSpeed)
            {
                velocity = Vector3.Normalize(velocity) * maxSpeed;
            }
        }

        protected void NewGraph()
        {
            _graph = new Graph();
            NodeMgr.instance.AddGraph(_graph);
        }

        protected void DropNode()
        {
            var newNode = new Node(transform.position);
            if (_graph.nodes.Count > 0)
            {
                var tail = _graph.nodes[_graph.nodes.Count - 1];
                tail.neighbors[1] = newNode;
                newNode.neighbors.Add(tail);
            }
            else
            {
                newNode.neighbors.Add(null);
            }
            newNode.neighbors.Add(null);
            _graph.nodes.Add(newNode);

            NodeMgr.instance.dirty = true;
        }

        void CutSpeed()
        {
            _velocity *= releaseBrake;
        }

        void CheckArrowKeys()
        {
            var fs = speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.UpArrow)) { _velocity.y += fs; }
            if (Input.GetKey(KeyCode.DownArrow)) { _velocity.y -= fs; }
            if (Input.GetKey(KeyCode.LeftArrow)) { _velocity.x -= fs; }
            if (Input.GetKey(KeyCode.RightArrow)) { _velocity.x += fs; }
        }

        bool JustArrowed()
        {
            return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
        }

        bool Space()
        {
            return Input.GetKey(KeyCode.Space);
        }

        bool JustSpaced()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        bool Shift()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        bool JustShifted()
        {
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
        }

        bool JustUnShifted()
        {
            return Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift);
        }
    }

}