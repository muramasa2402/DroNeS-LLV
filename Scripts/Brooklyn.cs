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
        if (Manhattan.tiles == null)
        {
            Manhattan manhattan = new Manhattan();
            manhattan.OnInitialized();
            manhattan.UpdateTileExtent();
        }
        HashSet<UnwrappedTileId> excludedSet = Manhattan.tiles;
        List<UnwrappedTileId> rightTiles = Manhattan.rightTiles;
        List<UnwrappedTileId> leftTiles = Manhattan.leftTiles;

        UnwrappedTileId tile;
        int j = 0;
        for (int i = 0; i < rightTiles.Count; i++) {

            if (i != rightTiles.Count - 1) { j = i + 1; }
            else { j = i; }

            for (int y = rightTiles[j].Y; y <= rightTiles[i].Y; y++) {
                for (int x = rightTiles[i].X; x <= rightTiles[i].X + 10; x++) {
                    tile = new UnwrappedTileId(_map.AbsoluteZoom, x, y);
                    if (!excludedSet.Contains(tile)) 
                    {
                        _currentExtent.activeTiles.Add(tile);
                    }
                }
            }


        }
        /* Additional tiles for southern part */

        for (int y = leftTiles[3].Y; y <= rightTiles[0].Y + 8; y++) 
        {
            for (int x = leftTiles[0].X; x <= rightTiles[0].X + 15; x++) 
            {
                tile = new UnwrappedTileId(_map.AbsoluteZoom, x, y);
                if (!excludedSet.Contains(tile)) 
                {
                    _currentExtent.activeTiles.Add(tile);
                }
            }
        }
        /* Additional tiles for northern part */
        var north = leftTiles[leftTiles.Count - 2];
        int z = 0;
        for (int y = north.Y; y >= north.Y - 12; y--)
        {
            z++;
            for (int x = north.X + z/4; x <= north.X + 15; x++)
            {
                tile = new UnwrappedTileId(_map.AbsoluteZoom, x, y);
                if (!excludedSet.Contains(tile))
                {
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
