

namespace Drones.UI
{
    using Drones.Managers;
    public class PriorityFocus : UIFocus
    {
        public static uint Count;
        private int _index;
        private void OnEnable()
        {
            SimManager.SimStatus = Utils.SimulationStatus.Paused;
            _index = (int)Count++;
        }

        private void LateUpdate()
        {
            Controlling = true;
            Window.transform.SetAsLastSibling();
            Window.transform.SetSiblingIndex(Window.transform.GetSiblingIndex() - (int)Count + _index + 1);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Count--;
        }

    }

}
