using System;
using System.Collections.Generic;
using NetControllerSystem;
using UnityEngine;

public class EntityTrigger : MonoBehaviour
{
    public int NumEntitiesInside => entitiesInside.Count;
    public event EventHandler<EntityControllerEventArgs> OnEnter;
    public event EventHandler<EntityControllerEventArgs> OnExit;
    protected HashSet<EntityController> entitiesInside = new HashSet<EntityController>();

    public class EntityControllerEventArgs : EventArgs
    {
        public EntityController controller; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Layers.Entity)
            return;

        EntityController controller = collision.transform.root.GetComponentInChildren<EntityController>();

        if (controller == null)
            return;

        entitiesInside.Add(controller);
        OnEntityEnter(controller);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != Layers.Entity)
            return;

        EntityController controller = collision.transform.root.GetComponentInChildren<EntityController>();

        if (controller == null || !entitiesInside.Contains(controller))
            return;

        entitiesInside.Remove(controller); 
        OnEntityExit(controller);
    }

    protected virtual void OnEntityEnter(EntityController entityController)
    {
        OnEnter?.Invoke(this, new EntityControllerEventArgs
        {
            controller = entityController
        });
    }

    protected virtual void OnEntityExit(EntityController entityController)
    {
        OnExit?.Invoke(this, new EntityControllerEventArgs
        {
            controller = entityController
        });
    }
}

