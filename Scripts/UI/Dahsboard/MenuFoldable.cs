namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;
    public class MenuFoldable : FoldableMenu
    {
        #region Properties
        //TODO
        #endregion

        protected override void Start()
        {
            Buttons[0].onClick.AddListener(QuitToMainMenu);
            Buttons[1].onClick.AddListener(SaveSimulation);
            Buttons[2].onClick.AddListener(LoadSimulation);
            Buttons[3].onClick.AddListener(ExportToCSV);
            Buttons[4].onClick.AddListener(OpenEditMode);
            base.Start();
        }

        private void OpenEditMode()
        {
            Edit.gameObject.SetActive(true);
        }

        private void ExportToCSV()
        {
            // TODO
        }

        private void LoadSimulation()
        {
            // TODO
        }

        private void SaveSimulation()
        {
            // TODO
        }

        private void QuitToMainMenu()
        {
            // TODO
        }

    }
}