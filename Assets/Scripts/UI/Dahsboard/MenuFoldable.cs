using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;

namespace Drones.UI
{
    using Drones.Utils;
    using Managers;

    public class MenuFoldable : FoldableMenu
    {

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
            EditPanel.Instance.gameObject.SetActive(true);
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
            StartCoroutine(LoadMainMenu());
        }

        IEnumerator LoadMainMenu()
        {
            yield return SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            yield return new WaitUntil(() => Time.unscaledDeltaTime < 1 / 30f);
            var remove = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        }

    }
}