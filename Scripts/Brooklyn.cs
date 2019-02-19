using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Utils;
using UnityEngine;
using Mapbox.Unity.Map.TileProviders;

public class Brooklyn : AbstractTileProvider {
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
        HashSet<UnwrappedTileId> excludedSet = Manhattan.tiles;
        List<UnwrappedTileId> rightTiles = Manhattan.rightTiles;
        List<UnwrappedTileId> leftTiles = Manhattan.leftTiles;

        UnwrappedTileId tile;
        int j = 0;
        for (int i = 0; i < rightTiles.Count; i++) {

            if (i != rightTiles.Count - 1) { j = i + 1; }
            else { j = i; }

            for (int y = rightTiles[j].Y; y <= rightTiles[i].Y; y++) {
                for (int x = rightTiles[i].X; x <= rightTiles[i].X + ((i<3)?12:7); x++) {
                    tile = new UnwrappedTileId(_map.AbsoluteZoom, x, y);
                    if (!excludedSet.Contains(tile)) {
                        _currentExtent.activeTiles.Add(tile);
                    }
                }
            }

        }

        var south = TileCover.CoordinateToTileId(new Vector2d (40.681f, -74.031f), _map.AbsoluteZoom);

        for (int y = rightTiles[0].Y; y <= south.Y ; y++) {
            for (int x = south.X; x <= south.X + 13; x++) {
                tile = new UnwrappedTileId(_map.AbsoluteZoom, x, y);
                if (!excludedSet.Contains(tile)) {
                    _currentExtent.activeTiles.Add(tile);
                }
            }
        }
        OnExtentChanged();
    }

    public override bool Cleanup(UnwrappedTileId tile) {
        return (!_currentExtent.activeTiles.Contains(tile));
    }

}
