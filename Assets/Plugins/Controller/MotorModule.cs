using UnityEngine;

namespace NetControllerSystem
{
    public abstract class MotorModule : MonoBehaviour
    {
        public virtual void Initialize(Motor motor) { }
        public abstract void HandleMovement();
    }
}
