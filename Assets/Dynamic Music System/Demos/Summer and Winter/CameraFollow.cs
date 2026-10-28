using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MordiAudio.MusicSystem.Demo
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform targetPosition, targetLookAt;

        Vector3 velocity;

        void Update() {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition.position, ref velocity, 0.1f);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((targetLookAt.position - targetPosition.position).normalized), Time.deltaTime * 5f);
        }
    }
}

