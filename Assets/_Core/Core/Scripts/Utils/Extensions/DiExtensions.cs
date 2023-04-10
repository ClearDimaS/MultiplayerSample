using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace MS.Core
{
    public static class DiExtensions
    {
        public static void InjectContexts(this DiContainer diContainer, GameObject gameObject)
        {
            IEnumerable<MonoBehaviour> monos = gameObject.GetComponentsInChildren<GameObjectContext>(true);
/*            foreach (var item in monos)
            {
                diContainer.Inject(item);
            }*/
        }

        public static void InjectMonos(this DiContainer diContainer, GameObject gameObject)
        {
            IEnumerable<MonoBehaviour> monos = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

            var ctx = typeof(Context);
            monos = monos.OrderBy(x => ctx.IsAssignableFrom(x.GetType()) ? 0 : 1);

            foreach (var item in monos)
            {
                diContainer.Inject(item);
            }
        }
    }
}
