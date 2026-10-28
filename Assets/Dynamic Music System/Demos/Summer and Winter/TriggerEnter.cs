using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MordiAudio.MusicSystem.Demo
{
    public class TriggerEnter : MonoBehaviour
    {
        public Collider colliderObject;
        public UnityEvent onEnteredTrigger;

        private void OnTriggerEnter(Collider other) {
            if (other == colliderObject)
                onEnteredTrigger?.Invoke();
        }
    }
}