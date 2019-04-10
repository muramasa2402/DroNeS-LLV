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
            RTS.gameObject.SetActive(true);
            EagleEye.gameObject.SetActive(false);
        }

        private void OpenEagleEye()
        {
            RTS.gameObject.SetActive(false);
            EagleEye.gameObject.SetActive(true);
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
