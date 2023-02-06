using System;
using UnityEngine;

namespace OuterWildsRandomSpeedrun
{
    public class Marshmallow : MonoBehaviour
    {
        public virtual void OnTriggerEnter(Collider collider)
        {
            if (collider == Locator.GetPlayerCollider())
                Collect();
        }

        public virtual void Collect()
        {
            OnCollected?.Invoke();
        }

        public event Action OnCollected;
    }
}