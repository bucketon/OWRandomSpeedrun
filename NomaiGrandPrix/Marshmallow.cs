using System;
using UnityEngine;

namespace TeamOptimism
{
    public class Marshmallow : MonoBehaviour
    {
        float x;
        float y;
        float z;
        float xSpeed;
        float ySpeed;
        float zSpeed;
        const float MAX_SPEED = 48f;

        public void OnTriggerEnter(Collider collider)
        {
            if (collider == Locator.GetPlayerCollider())
            {
                Collect();
            }
        }

        public void Collect()
        {
            OnCollected?.Invoke();
        }

        public void Start()
        {
            var random = new System.Random((int)DateTime.Now.Ticks);

            xSpeed = (float)random.NextDouble() * MAX_SPEED;
            ySpeed = (float)random.NextDouble() * MAX_SPEED;
            zSpeed = (float)random.NextDouble() * MAX_SPEED;
        }

        public void Update()
        {
            x += Time.deltaTime * xSpeed;
            y += Time.deltaTime * ySpeed;
            z += Time.deltaTime * zSpeed;
            transform.localRotation = Quaternion.Euler(x, y, z);
        }

        public event Action OnCollected;
    }
}
