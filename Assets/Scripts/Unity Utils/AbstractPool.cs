using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    using Drones.Interface;
    public abstract class AbstractPool
    {
        protected Dictionary<Type, string> _Paths;

        protected Dictionary<Type, uint> _StartSize;

        protected Dictionary<Type, GameObject> _Templates;

        protected Dictionary<Type, Queue<IPoolable>> _Pool;

        protected Dictionary<Type, bool> _IsBuilding;
    }
}