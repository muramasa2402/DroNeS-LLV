using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.Utils.Extensions;
    using Utils;
    using static Singletons;
    public class EditPanel : DashboardPanel
    {
        #region Fields
        [SerializeField]
        private Button _Play;
        [SerializeField]
        private Button _Navigation;
        [SerializeField]
        private Button _Lists;
        [SerializeField]
        private Button _Histories;
        [SerializeField]
        private Button _Add;
        [SerializeField]
        private Button _Delete;
        [SerializeField]
        private Button _Menu;
        #endregion

        #region Properties
        private Button Play
        {
            get
            {
                if (_Play == null)
                {
                    _Play = transform.Find("Play").GetComponent<Button>();
                }
                return _Play;
            }
        }

        private Button Navigation
        {
            get
            {
                if (_Navigation == null)
                {
                    _Navigation = transform.Find("Navigation").GetComponent<Button>();
                }
                return _Navigation;
            }
        }

        private Button Lists
        {
            get
            {
                if (_Lists == null)
                {
                    _Lists = transform.Find("Lists").GetComponent<Button>();
                }
                return _Lists;
            }
        }

        private Button Histories
        {
            get
            {
                if (_Histories == null)
                {
                    _Histories = transform.Find("Histories").GetComponent<Button>();
                }
                return _Histories;
            }
        }

        private Button Add
        {
            get
            {
                if (_Add == null)
                {
                    _Add = transform.Find("Add").GetComponent<Button>();
                }
                return _Add;
            }
        }

        private Button Delete
        {
            get
            {
                if (_Delete == null)
                {
                    _Delete = transform.Find("Delete").GetComponent<Button>();
                }
                return _Delete;
            }
        }

        private Button Menu
        {
            get
            {
                if (_Menu == null)
                {
                    _Menu = transform.Find("Menu").GetComponent<Button>();
                }
                return _Menu;
            }
        }

        protected override Dictionary<Transform, Button> OwnerToButton
        {
            get
            {
                if (_OwnerToButton == null)
                {
                    _OwnerToButton = new Dictionary<Transform, Button>
                    {
                        {Play.transform, Play},
                        {Navigation.transform, Navigation},
                        {Lists.transform, Lists},
                        {Histories.transform, Histories},
                        {Add.transform, Add},
                        {Add.transform, Delete},
                        {Menu.transform, Menu}
                    };
                }
                return _OwnerToButton;
            }
        }
        #endregion

        private void OnEnable()
        {
            transform.parent.ToRect().sizeDelta = PanelSize[DashboardMode.EditMode];
            Control.gameObject.SetActive(false);
            SimulationInfo.SetActive(false);
            SimManager.SimStatus = SimulationStatus.EditMode;
            CameraSwitch.OnEagleEye();
        }

        private void OnDisable()
        {
            transform.parent.ToRect().sizeDelta = PanelSize[DashboardMode.Simulation];
            Control.gameObject.SetActive(true);
            SimulationInfo.SetActive(true);
        }

        private void Awake()
        {
            Play.onClick.AddListener(ExitEditMode);
            Navigation.onClick.AddListener(MapFoldable.OpenNavigationWindow);

            Lists.onClick.AddListener(delegate { EnableFoldable(Lists); });
            Histories.onClick.AddListener(delegate { EnableFoldable(Histories); });
            Add.onClick.AddListener(delegate { EnableFoldable(Add); });
            Delete.onClick.AddListener(DeleteSelection);
            Menu.onClick.AddListener(delegate { EnableFoldable(Menu); });
        }

        private void ExitEditMode()
        {
            SimManager.SimStatus = SimulationStatus.Running;
            CameraSwitch.OnRTS();
            gameObject.SetActive(false);
        }

        private void DeleteSelection()
        {
            Selectable.DeleteMode = true;
        }

    }
}
