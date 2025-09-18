using DamageSystem;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private Hitbox _hitbox;

    private void Start()
    {
        _hitbox.Activate();
    }
}
