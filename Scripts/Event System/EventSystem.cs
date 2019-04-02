using System.Collections.Generic;

namespace Drones.EventSystem
{
    public class EventSystem<T1, T2>
    {
        public delegate void EventListener(T2 info);

        private Dictionary<T1, HashSet<EventListener>> _Listeners;
        Dictionary<T1, HashSet<EventListener>> Listeners
        {
            get
            {
                if (_Listeners == null)
                {
                    _Listeners = new Dictionary<T1, HashSet<EventListener>>();
                }
                return _Listeners;
            }
        }

        // Listener is a (any) function that is executed when the event is fired
        public void RegisterListener(T1 type, EventListener listener)
        {
            if (!Listeners.ContainsKey(type))
            {
                Listeners.Add(type, new HashSet<EventListener>());
            }

            Listeners[type].Add(listener);
        }

        public void UnregisterListener(T1 type, EventListener listener)
        {

            if (Listeners.ContainsKey(type))
            {
                Listeners[type].Remove(listener);
            }
        }

        public void Invoke(T1 type, T2 info)
        {
            Listeners.TryGetValue(type, out HashSet<EventListener> set);

            if (set == null) { return; }

            foreach (var invocation in set)
            {
                invocation(info);
            }
        }
    }
}
