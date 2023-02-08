using System;
using UnityEngine;

namespace OuterWildsRandomSpeedrun
{
    public class Marshmallow : MonoBehaviour
    {
        float z;

        public void OnTriggerEnter(Collider collider)
        {
            if (collider == Locator.GetPlayerCollider())
                Collect();
        }

        public void Collect()
        {
            OnCollected?.Invoke();
        }

        public void Update()
        {
            z += Time.deltaTime * 48f;
            transform.localRotation = Quaternion.Euler(0, 0, z);
        }

        public event Action OnCollected;
    }
}