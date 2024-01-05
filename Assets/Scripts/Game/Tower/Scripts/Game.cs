using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public enum GameTileContentType {
    Empty, Destination, Wall, SpawnPoint, Tower
}

public enum TowerType {
    Laser, Mortar
}

public enum EnemyType {
	Small, Medium, Large
}

[System.Serializable]
public class GameBehaviorCollection
{
    private List<GameBehavior> behaviors = new List<GameBehavior>();
    
    public bool IsEmpty => behaviors.Count == 0;
    
    public void Add(GameBehavior behavior)
    {
        behaviors.Add(behavior);
    }

    public void GameUpdate()
    {
        for (int i = 0; i < behaviors.Count; i++)
        {
            if (!behaviors[i].GameUpdate())
            {
                int lastIndex = behaviors.Count - 1;
                behaviors[i] = behaviors[lastIndex];
                behaviors.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < behaviors.Count; i++)
        {
            behaviors[i].Recycle();
        }
        behaviors.Clear();
    }
}


public class Game : MonoBehaviour
{
    [SerializeField]
    GameScenario scenario = default;
    GameScenario.State activeScenario;
    
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField] private GameBoard board = default;

    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField, Range(0.1f, 10f)]
    private float spawnSpeed = 1f;
    
    [SerializeField, Range(0, 100)]
    private int startingPlayerHealth = 10;
    private int playerHealth;

    [SerializeField] private float playSpeed = 1f;
    
    private GameBehaviorCollection enemies = new GameBehaviorCollection();
    private GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    private float spawnProgress;
    
    private TowerType selectedTowerType;

    private static Game instance;

    private const float pausedTimeScale = 0f;

    void Awake ()
    {
        playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
    }
    
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    
    private void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }

        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void OnEnable()
    {
        instance = this;
    }

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }
    
    public static Explosion SpawnExplosion () {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : 1f;
        }else if (Time.timeScale > pausedTimeScale) {
            Time.timeScale = playSpeed;
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            BeginNewGame();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }else if (Input.GetMouseButtonDown(1)) {
            HandleAlternativeTouch();
        }
        
        if (Input.GetKeyDown(KeyCode.V)) {
            board.ShowPaths = !board.ShowPaths;
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            board.ShowGrid = !board.ShowGrid;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        // 转移到scenario state Progress
        // spawnProgress += Time.deltaTime * spawnSpeed;
        // while (spawnProgress >= 1f)
        // {
        //     spawnProgress -= 1f;
        //     SpawnEnemy();
        // }

        if (playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Game Over --> 失败");
            BeginNewGame();
        }

        if (!activeScenario.Progress() && enemies.IsEmpty)
        {
            Debug.Log("Game Over --> 胜利");
            BeginNewGame();
            activeScenario.Progress();
        }
        
        
        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
        Enemy enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        instance.enemies.Add(enemy);
    }

    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            // tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }
            
        }
    }
    void HandleAlternativeTouch () {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null) {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }
    
    void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }
    
    public static void EnemyReachedDestination () {
        instance.playerHealth -= 1;
    }
}
