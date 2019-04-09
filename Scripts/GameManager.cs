using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones
{
    using Drones.UI;
    using Drones.UI.Edit;
    using Drones.Utils;
    using Drones.Utils.Extensions;
    using static Drones.Utils.Constants;
    using static Drones.Utils.StaticFunc;
    using static Drones.Singletons;

    public class GameManager : MonoBehaviour
    {
        private static GameManager _Instance;
        private static SimulationStatus _SimStatus;
        private static SecureHashSet<IDronesObject> _AllDrones;
        private static SecureHashSet<IDronesObject> _AllHubs;
        //TODO Maybe hashset not required for Jobs?

        public static GameManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = ((GameObject)Instantiate(Resources.Load(ManagerPath))).GetComponent<GameManager>();
                }
                return _Instance;
            }
        }
        public static SimulationStatus SimStatus
        {
            get
            {
                return _SimStatus;
            }
            set
            {
                _SimStatus = value;
                if (_SimStatus != SimulationStatus.EditMode)
                {
                    EditModeSelection.Deselect();
                }

                if (_SimStatus == SimulationStatus.Paused || _SimStatus == SimulationStatus.EditMode)
                {
                    Time.timeScale = 0;
                }
                else
                {
                    Time.timeScale = 1;
                }
            }
        }

        public static SecureHashSet<IDronesObject> AllDrones
        {
            get
            {
                if (_AllDrones == null)
                {
                    _AllDrones = new SecureHashSet<IDronesObject>()
                    {
                        MemberCondition = (item) => item is Drone
                    };
                }
                return _AllDrones;
            }

        }
        public static SecureHashSet<IDronesObject> AllHubs
        {
            get
            {
                if (_AllHubs == null)
                {
                    _AllHubs = new SecureHashSet<IDronesObject>()
                    {
                        MemberCondition = (item) => item is Hub
                    };
                }
                return _AllHubs;
            }
        }



    }
}
