using System.Collections.Generic;

namespace Drones.EventSystem
{
    using Drones.Utils;
    public static class SimulationEvent
    {
        public delegate void EventListener(IEvent info);

        private static Dictionary<EventType, HashSet<EventListener>> _Listeners;
        private static Dictionary<EventType, HashSet<EventListener>> Listeners
        {
            get
            {
                if (_Listeners == null)
                {
                    _Listeners = new Dictionary<EventType, HashSet<EventListener>>();
                }
                return _Listeners;
            }
        }

        // Listener is a (any) function that is executed when the event is fired
        public static void RegisterListener(EventType type, EventListener listener)
        {
            if (!Listeners.ContainsKey(type))
            {
                Listeners.Add(type, new HashSet<EventListener>());
            }

            Listeners[type].Add(listener);
        }

        public static void UnregisterListener(EventType type, EventListener listener)
        {

            if (Listeners.ContainsKey(type))
            {
                Listeners[type].Remove(listener);
            }
        }

        public static void Invoke(EventType type, IEvent info)
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
