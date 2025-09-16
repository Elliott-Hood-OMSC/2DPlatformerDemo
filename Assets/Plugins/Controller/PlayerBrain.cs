using InputManagement;
using UnityEngine;

namespace NetControllerSystem
{
    /// <summary>
    /// Handles input and other persistant player data.
    /// Separated from the EntityController in case the player can control ever multiple entities at once.
    /// </summary>
    [RequireComponent(typeof(InputManager))]
    public class PlayerBrain : MonoBehaviour
    {
        public InputManager InputManager { get; protected set; }
        public EntityController PossessedController { get; protected set; } 

        public void PossessController(EntityController spawnedController)
        {
            if (PossessedController != null)
            {
                PossessedController.PlayerBrain = null;
            }
            if (spawnedController != null)
            {
                spawnedController.PlayerBrain = this;
            }
            
            PossessedController = spawnedController;
        }
    }
}