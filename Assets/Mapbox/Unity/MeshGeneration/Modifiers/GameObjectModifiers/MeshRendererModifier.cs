namespace Mapbox.Unity.MeshGeneration.Modifiers
{
    using Mapbox.Unity.MeshGeneration.Data;
    using UnityEngine;
    using UnityEngine.Rendering;

    [CreateAssetMenu(menuName = "Mapbox/Modifiers/Mesh Renderer Modifier")]
    public class MeshRendererModifier : GameObjectModifier
    {
        [SerializeField]
        Material[] _Materials;
        Material[] Materials
        {
            get
            {
                if (_Materials == null)
                {
                    _Materials = new Material[1];
                    _Materials[0] = null;
                }
                return _Materials;
            }
        }
        //public override void Run(VectorEntity ve, UnityTile tile)
        //{
        //  ve.MeshRenderer.enabled = false;

        //}
        public override void Run(VectorEntity ve, UnityTile tile)
        {

            tile.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            tile.MeshRenderer.allowOcclusionWhenDynamic = true;
            tile.MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            ve.MeshFilter.mesh.SetTriangles(ve.MeshFilter.mesh.triangles, 0);
            ve.MeshFilter.mesh.subMeshCount = 1;
            ve.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            ve.MeshRenderer.materials = Materials;
        }
    }
}