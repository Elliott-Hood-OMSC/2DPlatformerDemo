using System;
using UnityEngine;
namespace NetControllerSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownMovement : RigidbodyController
    {
        private Rigidbody2D rb;

        #region Rigidbody Settings

        protected override void Awake()
        {
            base.Awake();
            rb = gameObject.GetComponent<Rigidbody2D>();
        }

        protected virtual void OnValidate()
        {
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.gravityScale = 0;
        }

        public override void SetRigidbodySettings(RigidbodySettings settings)
        {
            base.SetRigidbodySettings(settings);

            rb.linearDamping = settings.drag;
            rb.mass = settings.mass;
        }

        #endregion


        #region Movement

        [SerializeField] private MovementSettings movementSettings;
        [Serializable]
        private class MovementSettings
        {
            public float movementSpeed = 12.5f;
        }

        private void FixedUpdate()
        {
            rb.AddForce(Input.move.GetValue() * movementSettings.movementSpeed * rb.linearDamping);
        }

        private void AddForce(Vector2 force)
        {
            rb.AddForce(force);
        }

        #endregion

    }
}