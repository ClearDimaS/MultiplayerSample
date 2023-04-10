using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MS.Core
{
    public class CameraLooker : MonoBehaviour
    {
        [SerializeField] private bool invertDirection;

        private Camera m_Camera;
        private void Start()
        {
            m_Camera = Camera.main;
        }
        private void Update()
        {
            var camDir = ((invertDirection ? transform.position - m_Camera.transform.position:
                m_Camera.transform.position - transform.position)).normalized;
            if(m_Camera)
                transform.rotation = Quaternion.LookRotation(camDir, Vector3.forward);
        }
    }
}
