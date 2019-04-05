using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Drones.UI
{
    using Drones.Utils;
    using static Singletons;
    public class MinimapInteractionComponent : UIFocus
    {

        private Collider _BoundaryCollider;
        public Collider BoundaryCollider
        {
            get
            {
                if (_BoundaryCollider == null)
                {
                    _BoundaryCollider = Boundary.GetComponent<Collider>();
                }
                return _BoundaryCollider;
            }
        }

        private RawImage _MapImage;
        public RawImage MapImage
        {
            get
            {
                if (_MapImage == null)
                {
                    _MapImage = GetComponent<RawImage>();
                }
                return _MapImage;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            Window.SetAsLastSibling();
            Vector2 localCursor = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(MapImage.rectTransform, eventData.pressPosition, eventData.pressEventCamera, out localCursor))
            {
                var tex = MapImage.texture;
                var r = MapImage.rectTransform.rect;
                //Using the size of the texture and the local cursor, clamp the X,Y coords between 0 and width - height of texture
                var coordX = Mathf.Clamp((((localCursor.x - r.x) * tex.width) / r.width), 0, tex.width);
                var coordY = Mathf.Clamp((((localCursor.y - r.y) * tex.height) / r.height), 0, tex.height);

                //Convert coordX and coordY to % (0.0-1.0) with respect to texture width and height
                localCursor = new Vector2(coordX / tex.width, coordY / tex.height);

                CastMiniMapRayToWorld(localCursor);
            }

        }
        private void CastMiniMapRayToWorld(Vector2 localCursor)
        {
            var horizontalScale = MinimapCamera.pixelWidth * MapImage.uvRect.width;
            var xDir = localCursor.x * horizontalScale + MapImage.uvRect.x * MinimapCamera.pixelWidth;
            var verticalScale = MinimapCamera.pixelHeight * MapImage.uvRect.height;
            var yDir = localCursor.y * verticalScale + MapImage.uvRect.y * MinimapCamera.pixelHeight;
            var targetCoordinates = new Vector2(xDir, yDir);
            var miniMapRay = MinimapCamera.ScreenPointToRay(targetCoordinates);

            if (_BoundaryCollider.Raycast(miniMapRay, out RaycastHit miniMapHit, 3000))
            {
                Vector3 target = miniMapHit.point;
                target.y = 0;
                Functions.LookHere(target);
            }
        }
    }

}
