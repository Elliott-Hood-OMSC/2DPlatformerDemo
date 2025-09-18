using System;
using System.Collections.Generic;
using NetControllerSystem;
using UnityEngine;

public class EntityTrigger : MonoBehaviour
{
    public int NumEntitiesInside => _entitiesInside.Count;

    public event Action<EntityController> OnEnter;
    public event Action<EntityController> OnExit;

    private readonly HashSet<EntityController> _entitiesInside = new HashSet<EntityController>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Layers.Entity)
            return;

        EntityController controller = collision.transform.root.GetComponentInChildren<EntityController>();
        if (controller == null)
            return;

        if (_entitiesInside.Add(controller))
        {
            OnEntityEnter(controller);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Layers.Entity)
            return;

        EntityController controller = collision.transform.root.GetComponentInChildren<EntityController>();
        if (controller == null)
            return;

        if (_entitiesInside.Remove(controller))
        {
            OnEntityExit(controller);
        }
    }

    protected virtual void OnEntityEnter(EntityController entityController)
    {
        OnEnter?.Invoke(entityController);
    }

    protected virtual void OnEntityExit(EntityController entityController)
    {
        OnExit?.Invoke(entityController);
    }
}
