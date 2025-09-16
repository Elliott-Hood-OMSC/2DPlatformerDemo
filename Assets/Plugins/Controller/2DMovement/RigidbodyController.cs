using UnityEngine;
using System;

namespace NetControllerSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class RigidbodyController : EntityController
    {
        public Rigidbody2D Rb { get; private set; }

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
        }

        #region Rigidbody Settings

        [SerializeField] private RigidbodySettings _defaultRigidbodySettings = RigidbodySettings.Default;

        public RigidbodySettings RigidbodySettings { get; protected set; }

        protected virtual void Start()
        {
            ResetRigidbodySettings();
        }

        /// <summary>
        /// Returns the rigidbodySettings to the default serialized value
        /// </summary>
        public void ResetRigidbodySettings()
        {
            SetRigidbodySettings(_defaultRigidbodySettings);
        }

        /// <summary>
        /// Actually settings the parameters is done by an override function in a child class
        /// </summary>
        /// <param name="settings"></param>
        public virtual void SetRigidbodySettings(RigidbodySettings settings)
        {
            RigidbodySettings = settings;
        }

        #endregion

    }


    [Serializable]
    public struct RigidbodySettings
    {
        public float drag;
        public float mass;
        public float gravity;

        public static RigidbodySettings Default => new RigidbodySettings
        {
            drag = 1,
            mass = 1,
            gravity = 1.5f,
        };

        public static RigidbodySettings HighDrag => new RigidbodySettings
        {
            drag = 5,
            mass = 1,
            gravity = 1.5f,
        };

        public static RigidbodySettings Weightless => new RigidbodySettings
        {
            drag = 5,
            mass = 1,
            gravity = 0,
        };
    }
}