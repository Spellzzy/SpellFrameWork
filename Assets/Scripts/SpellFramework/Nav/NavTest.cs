using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Node
{
    public Node Parent;
    public int PosX;
    public int PosY;

    public float ValueG;
    public float ValueF;
    public float ValueH;

    public bool IsWall = true;

    public GameObject ShowGo;

    public Node(int x, int y, Node parent = null)
    {
        this.PosX = x;
        this.PosY = y;
        this.Parent = parent;
        this.IsWall = false;
    }

    public void UpdateParent(Node parent, float g)
    {
        this.Parent = parent;
        SetValueG(g);
        UpdateValueF();
    }

    public void SetValueH(float h)
    {
        this.ValueH = h;
    }

    public void SetValueG(float g)
    {
        this.ValueG = g;
    }

    public void UpdateValueF()
    {
        ValueF = ValueG + ValueH;
    }

    public override string ToString()
    {
        return string.Format("{0} -- {1}", this.PosX, this.PosY);
    }

    public void SetIsWall(bool isWall, Material material)
    {
        this.IsWall = isWall;
        SetGoMat(material);
    }

    public void SetGoMat(Material material)
    {
        var render = ShowGo.GetComponentInChildren<Renderer>();
        render.material = material;
    }
}


public class NavTest : MonoBehaviour
{
    public int width = 8;
    public int high = 6;

    public GameObject cellPrefab;

    private Node[,] mapCells;
    public Material WallMat;
    public Material StartMat;
    public Material EndMat;
    public Material PathMat;

    void Start()
    {
        mapCells = new Node[width, high];
        InitCells();
    }

    void InitCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < high; j++)
            {
                mapCells[i, j] = new Node(i, j);
                
                var go = (GameObject)GameObject.Instantiate(cellPrefab);
                go.transform.position = new Vector3(i * 1.25f, 0, j * 1.25f);
                mapCells[i, j].ShowGo = go;
            }
        }
        mapCells[4, 0].SetIsWall(true, WallMat);
        mapCells[4, 1].SetIsWall(true, WallMat);
        mapCells[4, 2].SetIsWall(true, WallMat);
        mapCells[4, 3].SetIsWall(true, WallMat);
        mapCells[4, 4].SetIsWall(true, WallMat);
        mapCells[4, 5].SetIsWall(true, WallMat);


        Node start = mapCells[0, 2];
        start.SetGoMat(StartMat);
        
        Node end = mapCells[5, 3];
        end.SetGoMat(EndMat);
        
        FindPath(start, end);

        Node pathOut = end;
        Debug.Log("------------------------------");
        Debug.Log("输出路径--");
        Debug.Log(end.ToString());
        while (pathOut.Parent != null)
        {
            pathOut = pathOut.Parent;
            pathOut.SetGoMat(PathMat);
            Debug.Log(pathOut.ToString());
        }
    }


    void FindPath(Node start, Node end)
    {
        // 开启列表
        List<Node> openList = new List<Node>();
        // 关闭列表
        List<Node> closedList = new List<Node>();
        // 先将起点加入开启列表
        openList.Add(start);

        while (openList.Count > 0)
        {
            // 取列表中F值最小的结点
            Node node = FindMinFofNode(openList);
            // 开放列表移除
            openList.Remove(node);
            // 加入关闭列表 
            closedList.Add(node);
            
            // 取结点周围结点
            List<Node> surroundNodes = GetSurroundNodes(node);

            NodesFilter(surroundNodes, closedList);

            foreach (Node surroundNode in surroundNodes)
            {
                if (openList.IndexOf(surroundNode) > -1)
                {
                    // 存在于开启列表内
                    // 检查G值
                    float nowG = CalcG(surroundNode, node);
                    if (nowG < surroundNode.ValueG)
                    {
                        surroundNode.UpdateParent(node, nowG);
                    }

                }
                else
                {
                    surroundNode.Parent = node;
                    // 计算F值
                    CalcF(surroundNode, end);
                    // 加入开启列表
                    openList.Add(surroundNode);
                }   
            }

            if (openList.IndexOf(end) > - 1)
            {
                // 终点已到
                break;
            }
        }
        
    }


    float CalcG(Node node, Node parent)
    {
        return Vector2.Distance(new Vector2(node.PosX, node.PosY), new Vector2(parent.PosX, parent.PosY)) + parent.ValueG;
    }


    // F = G + H
    void CalcF(Node node, Node end)
    {
        // 曼哈顿距离
        float h = Mathf.Abs((end.PosX - node.PosX)) + Mathf.Abs((end.PosY - node.PosY));
        float g = 0;
        if (node.Parent == null)
        {
            // 此时为起始结点 无父节点 所以g为0
            g = 0;
        }
        else
        {
            // 存在父节点 重新计算G
            g = CalcG(node, node.Parent);
            // g = Vector2.Distance(new Vector2(node.PosX, node.PosY), new Vector2(node.Parent.PosX, node.Parent.PosY))
            //     + node.Parent.ValueG;
        }

        float f = g + h;
        node.ValueF = f;
        node.ValueG = g;
        node.ValueH = h;
    }


    // 对结点列表进行过滤 剔除存在与关闭列表的结点
    void NodesFilter(List<Node> src, List<Node> closedList)
    {
        foreach (var node in closedList)
        {
            if (src.IndexOf(node) > -1)
            {
                src.Remove(node);
            }
        }
    }

    List<Node> GetSurroundNodes(Node node)
    {
        Node up = null, down = null, left = null, right = null;
        Node lu = null, ld = null, ru = null, rd = null;
        if (node.PosY < high - 1)
        {
            up = mapCells[node.PosX, node.PosY + 1];
        }

        if (node.PosY > 0)
        {
            down = mapCells[node.PosX, node.PosY - 1];
        }

        if (node.PosX > 0)
        {
            left = mapCells[node.PosX - 1, node.PosY];
        }

        if (node.PosX < width - 1)
        {
            right = mapCells[node.PosX + 1, node.PosY];
        }

        if (up != null && left != null)
        {
            lu = mapCells[node.PosX - 1, node.PosY + 1];
        }

        if (up != null && right != null)
        {
            ru = mapCells[node.PosX + 1, node.PosY + 1];
        }

        if (down != null && left != null)
        {
            ld = mapCells[node.PosX - 1, node.PosY - 1];
        }

        if (down != null && right != null)
        {
            rd = mapCells[node.PosX + 1, node.PosY - 1];
        }

        List<Node> res = new List<Node>();
        if (up != null && !up.IsWall)
        {
            res.Add(up);
        }
        if (down != null && !down.IsWall)
        {
            res.Add(down);
        }
        if (left != null && !left.IsWall)
        {
            res.Add(left);
        }
        if (right != null && !right.IsWall)
        {
            res.Add(right);
        }

        if (lu != null && !lu.IsWall && !left.IsWall && !up.IsWall )
        {
            res.Add(lu);
        }
        
        if (ld != null && !ld.IsWall && !left.IsWall && !down.IsWall )
        {
            res.Add(ld);
        }
        
        if (ru != null && !ru.IsWall && !right.IsWall && !up.IsWall )
        {
            res.Add(ru);
        }
        
        if (rd != null && !rd.IsWall && !right.IsWall && !down.IsWall )
        {
            res.Add(rd);
        }

        return res;
    }

    Node FindMinFofNode(List<Node> openList)
    {
        float f = float.MaxValue;
        Node nodeTemp = null;
        foreach (var node in openList)
        {
            if (node.ValueF < f)
            {
                f = node.ValueF;
                nodeTemp = node;
            }
        }
        return nodeTemp;
    }


}
