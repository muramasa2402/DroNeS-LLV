namespace Drones.UI
{
    using Utils;
    using Managers;

    public class TimeControlFoldable : FoldableMenu
    {
        protected override void Start()
        {
            Buttons[0].onClick.AddListener(Pause);
            Buttons[1].onClick.AddListener(Slow);
            Buttons[2].onClick.AddListener(Play);
            Buttons[3].onClick.AddListener(Fast);
            Buttons[4].onClick.AddListener(Faster);
        }

        void Pause()
        {
            SimManager.SimStatus = SimulationStatus.Paused;
            TimeKeeper.TimeSpeed = TimeSpeed.Pause;
        }

        void Slow()
        {
            SimManager.SimStatus = SimulationStatus.Running;
            TimeKeeper.TimeSpeed = TimeSpeed.Slow;
        }

        void Play()
        {
            SimManager.SimStatus = SimulationStatus.Running;
            TimeKeeper.TimeSpeed = TimeSpeed.Normal;
        }

        void Fast()
        {
            SimManager.SimStatus = SimulationStatus.Running;
            TimeKeeper.TimeSpeed = TimeSpeed.Fast;
        }

        void Faster()
        {
            SimManager.SimStatus = SimulationStatus.Running;
            TimeKeeper.TimeSpeed = TimeSpeed.Ultra;
        }
    }
}
