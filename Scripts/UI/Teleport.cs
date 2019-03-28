using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Drones.UI
{
    using static SceneAttributes;
    public class Teleport : MonoBehaviour, IPointerClickHandler
    {
        public Camera miniMapCam;
        public GameObject boundary;
        private Collider boundaryCollider;
        private RawImage rawimage;
        // Update is called once per frame
        private void Start()
        {
            if (miniMapCam == null)
            {
                miniMapCam = GameObject.Find("Minimap Camera").GetComponent<Camera>();
            }
            if (boundary == null)
            {
                boundary = GameObject.Find("City Boundary");
            }
            boundaryCollider = boundary.GetComponent<Collider>();
            rawimage = GetComponent<RawImage>();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 localCursor = new Vector2(0, 0);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RawImage>().rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localCursor))
            {
                Texture tex = rawimage.texture;
                Rect r = rawimage.rectTransform.rect;

                //Using the size of the texture and the local cursor, clamp the X,Y coords between 0 and width - height of texture
                float coordX = Mathf.Clamp((((localCursor.x - r.x) * tex.width) / r.width), 0, tex.width);
                float coordY = Mathf.Clamp((((localCursor.y - r.y) * tex.height) / r.height), 0, tex.height);

                //Convert coordX and coordY to % (0.0-1.0) with respect to texture width and height
                float recalcX = coordX / tex.width;
                float recalcY = coordY / tex.height;

                localCursor = new Vector2(recalcX, recalcY);

                CastMiniMapRayToWorld(localCursor);
            }

        }
        private void CastMiniMapRayToWorld(Vector2 localCursor)
        {
            float horizontalScale = miniMapCam.pixelWidth * rawimage.uvRect.width;
            float xDir = localCursor.x * horizontalScale + rawimage.uvRect.x * miniMapCam.pixelWidth;
            float verticalScale = miniMapCam.pixelHeight * rawimage.uvRect.height;
            float yDir = localCursor.y * verticalScale + rawimage.uvRect.y * miniMapCam.pixelHeight;
            Vector2 targetCoordinates = new Vector2(xDir, yDir);
            Ray miniMapRay = miniMapCam.ScreenPointToRay(targetCoordinates);
            if (boundaryCollider.Raycast(miniMapRay, out RaycastHit miniMapHit, 3000))
            {
                Vector3 target = miniMapHit.point;
                target.y = CameraControl.controller.Ceiling;
                CameraContainer.position = target;
            }
        }
    }

}
