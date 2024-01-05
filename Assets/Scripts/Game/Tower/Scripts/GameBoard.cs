using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    private Transform ground = default;
    
    [SerializeField]
    GameTile tilePrefab = default;
    
    [SerializeField] 
    private Texture2D gridTexture = default;
    // 缩放指数
    private Vector2Int _size;
    GameTile[] _tiles;
    
    GameTileContentFactory _contentFactory;

    List<GameTileContent> _updatingContent = new List<GameTileContent>();
    // 搜索最短路径队列
    private readonly Queue<GameTile> _searchFrontier = new Queue<GameTile>();

    private List<GameTile> _spawnPoints = new List<GameTile>();

    private bool _showPaths, _showGrid;
    public bool ShowPaths
    {
        get => _showPaths;
        set
        {
            _showPaths = value;
            if (_showPaths)
            {
                foreach (GameTile tile in _tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in _tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => _showGrid;
        set
        {
            _showGrid = value;
            Material mat = ground.GetComponent<MeshRenderer>().material;
            if (_showGrid)
            {
                mat.mainTexture = gridTexture;
                mat.SetTextureScale("_MainTex", _size);
            }
            else
            {
                mat.mainTexture = null;
            }
        }
    }

    public GameTile GetSpawnPoint(int index)
    {
        return _spawnPoints[index];
    }

    public int SpawnPointCount => _spawnPoints.Count;

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1))
        {
            // 用x z来定位
            int x = (int)(hit.point.x + _size.x * 0.5f);
            int y = (int)(hit.point.z + _size.y * 0.5f);
            if (x >= 0 && x < _size.x && y >= 0 && y < _size.y) {
                return _tiles[x + y * _size.x];
            }
        }
        return null;
    }

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        _spawnPoints = new List<GameTile>();
        this._contentFactory = contentFactory;
        this._size = size;
        ground.localScale = new Vector3(size.x, size.y, 1f);

        Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
        _tiles = new GameTile[size.x * size.y];
        
        for (int i = 0, y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++, i++)
            {
                GameTile tile = _tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);
                if (x > 0)
                {
                    // 非最左侧地块 绑定横向关系
                    GameTile.MakeEastWestNeighbors(tile, _tiles[i - 1]);
                }
                if (y > 0)
                {
                    // 非最顶层地块 绑定纵向关系
                    GameTile.MakeNorthSouthNeighbors(tile, _tiles[i - size.x]);
                }
                // 用是否为偶数 设置选择分类
                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0) {
                    tile.IsAlternative = !tile.IsAlternative;
                }
                // tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        // ToggleDestination(_tiles[_tiles.Length / 2]);
        // ToggleSpawnPoint(_tiles[0]);

        Clear();
    }

    public void Clear()
    {
        foreach (var tile in _tiles)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }
        _spawnPoints.Clear();
        _updatingContent.Clear();
        
        ToggleDestination(_tiles[_tiles.Length / 2]);
        ToggleSpawnPoint(_tiles[0]);
    }

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths())
            {
                // 避免没有终点
                tile.Content = _contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty) 
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Wall);
            // 避免造成死路
            if (!FindPaths())
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }
    
    public void ToggleTower(GameTile tile, TowerType towerType)
    {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            _updatingContent.Remove(tile.Content);
            if (((Tower)tile.Content).TowerType == towerType)
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else
            {
                tile.Content = _contentFactory.Get(towerType);
                _updatingContent.Add(tile.Content);
            }
            
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(towerType);
            // 避免造成死路
            if (FindPaths())
            {
               _updatingContent.Add(tile.Content);
            }
            else
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Wall)
        {
            // 墙变塔
            tile.Content = _contentFactory.Get(towerType);
            _updatingContent.Add(tile.Content);
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (_spawnPoints.Count > 1)
            {
                _spawnPoints.Remove(tile);
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            }
        }else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.SpawnPoint);
            _spawnPoints.Add(tile);
        }
    }


    bool FindPaths()
    {
        foreach (GameTile tile in _tiles)
        {
            // 设置终点
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.DoDestination();
                // 终点先入队
                _searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if (_searchFrontier.Count == 0)
        {
            return false;
        }
        // int destIndex = tiles.Length / 2;
        // tiles[destIndex].DoDestination();
        // searchFrontier.Enqueue(tiles[destIndex]);
        
        while (_searchFrontier.Count > 0)
        {
            GameTile curTile = _searchFrontier.Dequeue();
            if (curTile != null)
            {
                if (curTile.IsAlternative)
                {
                    _searchFrontier.Enqueue(curTile.GrowPathNorth());
                    _searchFrontier.Enqueue(curTile.GrowPathSouth());
                    _searchFrontier.Enqueue(curTile.GrowPathEast());
                    _searchFrontier.Enqueue(curTile.GrowPathWest());
                }
                else
                {
                    _searchFrontier.Enqueue(curTile.GrowPathWest());
                    _searchFrontier.Enqueue(curTile.GrowPathEast());
                    _searchFrontier.Enqueue(curTile.GrowPathSouth());
                    _searchFrontier.Enqueue(curTile.GrowPathNorth());
                }
            }
        }

        foreach (GameTile tile in _tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }

        if (_showPaths)
        {
            foreach (GameTile tile in _tiles) {
                tile.ShowPath();
            }
        }

        return true;
    }
    
    public void GameUpdate () {
        for (int i = 0; i < _updatingContent.Count; i++) {
            _updatingContent[i].GameUpdate();
        }
    }
    
}
