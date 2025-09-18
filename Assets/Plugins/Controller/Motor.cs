using System;
using InputManagement;
using UnityEngine;

namespace NetControllerSystem
{
    [RequireComponent(typeof(EntityController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Motor : MonoBehaviour
    {
        protected virtual void Awake()
        {
            Controller = GetComponent<EntityController>();
            Rb = GetComponent<Rigidbody2D>();
        }

        public EntityController Controller { get; private set; }
        public Rigidbody2D Rb { get; private set; }

        public abstract void HandleLocalControllerMovement();
    }
}
