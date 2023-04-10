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
            var ctx = GetComponent<RunnableContext>();
            if (ctx is GameObjectContext goCtx)
            {
                var sceneContext = FindObjectOfType<SceneContext>();
                var container = sceneContext.Container;
                container.Inject(goCtx);
            }
        }
    }
}
