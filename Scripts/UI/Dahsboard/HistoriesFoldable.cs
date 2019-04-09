namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;
    public class HistoriesFoldable : FoldableMenu
    {
        private JobHistoryWindow _JobHistory;

        #region Properties
        public JobHistoryWindow JobHistory
        {
            get
            {
                if (!_JobHistory.gameObject.activeSelf)
                {
                    return null;
                }
                return _JobHistory;
            }
            private set
            {
                _JobHistory = value;
            }
        }
        #endregion

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(OpenWholeJobHistory);
            base.Start();
        }

        private void OpenWholeJobHistory()
        {
            if (JobHistory == null)
            {
                JobHistory = (JobHistoryWindow)UIPool.Get(WindowType.JobHistory, UICanvas);
                // TODO Need global Job Queue
                // JobQueue.Sources = 
            }
            else
            {
                JobHistory.transform.SetAsLastSibling();
            }
        }
    }
}
