using UnityEngine;

public class GameTile : MonoBehaviour {
	static Quaternion
		northRotation = Quaternion.Euler(90f, 0f, 0f),
		eastRotation = Quaternion.Euler(90f, 90f, 0f),
		southRotation = Quaternion.Euler(90f, 180f, 0f),
		westRotation = Quaternion.Euler(90f, 270f, 0f);
	
    [SerializeField]
    Transform arrow = default;
    
    GameTile north, east, south, west, nextOnPath;

    private int distance;

    public bool HasPath => distance != int.MaxValue;
    
    public bool IsAlternative { get; set; }

    private GameTileContent _content;
    
    public Direction PathDirection { get; private set; }
    
    public GameTile NextTileOnPath => nextOnPath;
	// 当前格子与下个格子的中间位坐标
    public Vector3 ExitPoint
    {
	    get;
	    private set;
    }

    public GameTileContent Content
    {
	    get => _content;
	    set
	    {
		    Debug.Assert(value != null, "Null assigned to content!");
		    if (_content != null)
		    {
			    _content.Recycle();
		    }
		    _content = value;
		    _content.transform.localPosition = transform.localPosition;
	    }
    }

    private GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
	    Debug.Assert(HasPath, "No path!");
	    if (neighbor == null || neighbor.HasPath) {
		    return null;
	    }
	    neighbor.distance = distance + 1;
	    neighbor.nextOnPath = this;
	    // 计算格子中间位置坐标
	    // neighbor.ExitPoint = (neighbor.transform.localPosition + transform.localPosition) * 0.5f;
	    neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();
	    neighbor.PathDirection = direction;
	    return neighbor.Content.BlocksPath ? null : neighbor;
    }
    
    public GameTile GrowPathNorth () => GrowPathTo(north, Direction.South);

    public GameTile GrowPathEast () => GrowPathTo(east, Direction.West);

    public GameTile GrowPathSouth () => GrowPathTo(south, Direction.North);

    public GameTile GrowPathWest () => GrowPathTo(west, Direction.East);
    
    // 展示路径Arrow图
    public void ShowPath () {
	    if (distance == 0) {
		    arrow.gameObject.SetActive(false);
		    return;
	    }
	    arrow.gameObject.SetActive(true);
	    arrow.localRotation =
		    nextOnPath == north ? northRotation :
		    nextOnPath == east ? eastRotation :
		    nextOnPath == south ? southRotation :
		    westRotation;
    }

    // 终点地块
    public void DoDestination()
    {
	    distance = 0;
	    nextOnPath = null;
	    ExitPoint = transform.localPosition;
    }

    public void ClearPath()
    {
	    distance = int.MaxValue;
	    nextOnPath = null;
    }


    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
	    Debug.Assert(west.east == null && east.west == null, "存在重定义邻居");
	    west.east = east;
	    east.west = west;
    }
    
    public static void MakeNorthSouthNeighbors (GameTile north, GameTile south) {
	    Debug.Assert(
		    south.north == null && north.south == null, "存在重定义邻居"
	    );
	    south.north = north;
	    north.south = south;
    }

    public void HidePath()
    {
	    arrow.gameObject.SetActive(false);
    }

}