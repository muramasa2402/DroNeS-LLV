using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Utils;
using UnityEngine;
using Mapbox.Unity.Map.TileProviders;

public class Manhattan : AbstractTileProvider {
    private bool _initialized = false;

    //private List<UnwrappedTileId> _toRemove;
    //private HashSet<UnwrappedTileId> _tilesToRequest;

    public override void OnInitialized() {

        _initialized = true;
        _currentExtent.activeTiles = new HashSet<UnwrappedTileId>();
    }

    public override void UpdateTileExtent() {
        if (!_initialized) {
            return;
        }

        _currentExtent.activeTiles.Clear();

        var firstTile = TileCover.CoordinateToTileId(new Vector2d(40.696, -74.025), _map.AbsoluteZoom);
        var finalTile = TileCover.CoordinateToTileId(new Vector2d(40.83, -73.934), _map.AbsoluteZoom);

        float[,] coords = {{40.699f,-74.025f},
                            {40.699f,-74.003f},
                            {40.702f,-74.025f},
                            {40.702f,-74.003f},
                            {40.705f,-74.023f},
                            {40.705f,-74.003f},
                            {40.707f,-74.025f},
                            {40.707f,-73.992f},
                            {40.71f,-74.02f},
                            {40.71f,-73.971f},
                            {40.734f,-74.018f},
                            {40.734f,-73.972f},
                            {40.745f,-74.018f},
                            {40.745f,-73.967f},
                            {40.753f,-74.01f},
                            {40.753f,-73.964f},
                            {40.757f,-74.01f},
                            {40.757f,-73.97f},
                            {40.76f,-74.01f},
                            {40.76f,-73.963f},
                            {40.762f,-74.01f},
                            {40.762f,-73.958f},
                            {40.767f,-74.00f},
                            {40.767f,-73.953f},
                            {40.775f,-73.995f},
                            {40.775f,-73.940f},
                            {40.783f,-73.990f},
                            {40.783f,-73.940f},
                            {40.788f,-73.986f},
                            {40.788f,-73.945f},
                            {40.792f,-73.986f},
                            {40.792f,-73.936f},
                            {40.796f,-73.981f},
                            {40.796f,-73.928f},
                            {40.804f,-73.974f},
                            {40.804f,-73.937f},
                            {40.807f,-73.974f},
                            {40.807f,-73.937f},
                            {40.814f,-73.963f},
                            {40.814f,-73.934f},
                            {40.83f,-73.958f},
                            {40.83f,-73.934f}};

        List<UnwrappedTileId> leftTiles = new List<UnwrappedTileId>();
        List<UnwrappedTileId> rightTiles = new List<UnwrappedTileId>();

        for (int k = 0; k <= coords.GetUpperBound(0); k += 2) {
            leftTiles.Add(TileCover.CoordinateToTileId(new Vector2d(coords[k, 0], coords[k, 1]), _map.AbsoluteZoom));
        }
        for (int k = 1; k <= coords.GetUpperBound(0); k += 2) {
            rightTiles.Add(TileCover.CoordinateToTileId(new Vector2d(coords[k, 0], coords[k, 1]), _map.AbsoluteZoom));
        }

        int j = 0;
        for (int i = 0; i < leftTiles.Count; i++) {
            if (i != leftTiles.Count - 1) j = i + 1;
            else j = i;
            for (int y = leftTiles[j].Y; y <= leftTiles[i].Y; y++) {
                for (int x = leftTiles[i].X; x <= rightTiles[i].X; x++) {
                    _currentExtent.activeTiles.Add(new UnwrappedTileId(_map.AbsoluteZoom, x, y));
                }
            }
        }
        OnExtentChanged();
    }

    public override bool Cleanup(UnwrappedTileId tile) {
        return (!_currentExtent.activeTiles.Contains(tile));
    }

}
