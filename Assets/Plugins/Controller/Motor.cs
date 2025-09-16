using InputManagement;
using UnityEngine;

namespace NetControllerSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Motor : MonoBehaviour
    {
        public EntityController Controller { get; private set; }
        public Rigidbody2D Rb { get; private set; }

        protected virtual void Awake()
        {
            Controller = GetComponent<EntityController>();
            Controller.OnPlayerBrainChanged += Controller_OnPossessionEvent;
        }

        public virtual void OnDestroy()
        {
            if (Controller != null)
            {
                Controller.OnPlayerBrainChanged -= Controller_OnPossessionEvent;

                OnInputManagerRemoved(Controller.InputManager);
            }
        }

        private void Controller_OnPossessionEvent(PlayerBrain oldPlayerBrain, PlayerBrain newPlayerBrain)
        {
            if (oldPlayerBrain != null)
            {
                OnInputManagerRemoved(oldPlayerBrain.InputManager);
            }

            if (newPlayerBrain != null)
            {
                OnInputManagerAdded(newPlayerBrain.InputManager);
            }
        }

        protected virtual void OnInputManagerRemoved(InputManager controllerInputManager) { }

        protected virtual  void OnInputManagerAdded(object inputManager) { }

        public abstract void HandleLocalControllerMovement();
        public abstract void SetRigidbodySettings(RigidbodySettings settings);
    }
}
