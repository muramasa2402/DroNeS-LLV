using UnityEngine.EventSystems;
using UnityEngine;
using Drones.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace Drones.UI.Edit
{
    public class NoFlyZone : MonoBehaviour
    {
        private Dictionary<System.Type, int> _EntryCount;
        public Dictionary<System.Type, int> EntryCount
        {
            get
            {
                if (_EntryCount == null)
                {
                    _EntryCount = new Dictionary<System.Type, int>
                    {
                        {typeof(Drone), 0},
                        {typeof(Hub), 0}
                    };
                }
                return _EntryCount;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            EntryCount[other.GetComponent<IDronesObject>().GetType()]++;
            //TODO Invoke simulation event
        }


    }

}