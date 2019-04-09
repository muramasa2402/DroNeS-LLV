namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;
    public class MapFoldable : FoldableMenu
    {
        protected override void Start()
        {
            Buttons[0].onClick.AddListener(OpenEagleEye);
            Buttons[1].onClick.AddListener(OpenRTS);
            Buttons[2].onClick.AddListener(OpenNavigationWindow);
            base.Start();
        }

        private void OpenRTS()
        {
            CameraTransform.gameObject.SetActive(true);
            EagleEye.gameObject.SetActive(false);
        }

        private void OpenEagleEye()
        {
            EagleEye.gameObject.SetActive(true);
            CameraTransform.gameObject.SetActive(false);
        }

        public static void OpenNavigationWindow()
        {
            if (Navigation != null && Navigation.gameObject.activeSelf)
            {
                Navigation.transform.SetAsLastSibling();
            } 
            else
            {
                UIObjectPool.Get(WindowType.Navigation, UICanvas);
            }
        }

    }
}
