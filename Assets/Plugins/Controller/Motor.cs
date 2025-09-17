using InputManagement;
using UnityEngine;

namespace NetControllerSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Motor : MonoBehaviour
    {
        public EntityController Controller { get; private set; }
        public Rigidbody2D Rb { get; private set; }

        public abstract void HandleLocalControllerMovement();
        public abstract void SetRigidbodySettings(RigidbodySettings settings);
    }
}
