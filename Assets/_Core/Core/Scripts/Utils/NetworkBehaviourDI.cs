using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace MS.Core
{
    public class NetworkBehaviourDI : NetworkBehaviour
    {

        private void Awake()
        {
            var ctx = transform.GetComponent<RunnableContext>();
            RunnableContext parentCtx = transform.parent ? transform.parent.GetComponentInParent<GameObjectContext>() : null;
            if (parentCtx == null)
                parentCtx = FindObjectOfType<SceneContext>(true);

            if (ctx is GameObjectContext goCtx)
            {
                Debug.Log($"{goCtx.name} has parent context: {parentCtx.gameObject.name}");
                var container = parentCtx.Container;
                if (!goCtx.Initialized)
                    container.Inject(goCtx);
            }

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}
