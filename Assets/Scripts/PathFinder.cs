using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 连连看路径查找算法
/// 检查两个点是否可以通过最多3条线段（2次转弯）连接
/// 原理：从起点向4个方向扩展，检查每个方向上的所有点，判断是否能到达终点
/// </summary>
public static class PathFinder
{
    // 四个方向：上、下、右、左
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // 上
        new Vector2Int(0, -1),  // 下
        new Vector2Int(1, 0),   // 右
        new Vector2Int(-1, 0)   // 左
    };
    
    /// <summary>
    /// 检查两个点是否可以通过路径连接
    /// </summary>
    /// <param name="start">起点坐标</param>
    /// <param name="end">终点坐标</param>
    /// <param name="grid">网格状态，true表示该位置有元素（被阻挡），false表示空</param>
    /// <param name="gridWidth">网格宽度</param>
    /// <param name="gridHeight">网格高度</param>
    /// <returns>是否可连接</returns>
    public static bool CanConnect(Vector2Int start, Vector2Int end, bool[,] grid, int gridWidth, int gridHeight)
    {
        // 起点和终点相同，直接返回false
        if (start == end) return false;
        
        // 检查直线连接（0次转弯）
        if (CheckStraightLine(start, end, grid, gridWidth, gridHeight))
        {
            return true;
        }
        
        // 检查一次转弯（L型连接）
        if (CheckOneTurn(start, end, grid, gridWidth, gridHeight))
        {
            return true;
        }
        
        // 检查两次转弯（Z型连接）
        if (CheckTwoTurns(start, end, grid, gridWidth, gridHeight))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查直线连接（0次转弯）
    /// </summary>
    private static bool CheckStraightLine(Vector2Int start, Vector2Int end, bool[,] grid, int gridWidth, int gridHeight)
    {
        // 水平线
        if (start.y == end.y)
        {
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            
            // 检查起点和终点之间的所有点（不包括起点和终点）
            for (int x = minX + 1; x < maxX; x++)
            {
                if (IsValidPosition(x, start.y, gridWidth, gridHeight) && grid[x, start.y])
                {
                    return false; // 路径被阻挡
                }
            }
            return true;
        }
        
        // 垂直线
        if (start.x == end.x)
        {
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            
            // 检查起点和终点之间的所有点（不包括起点和终点）
            for (int y = minY + 1; y < maxY; y++)
            {
                if (IsValidPosition(start.x, y, gridWidth, gridHeight) && grid[start.x, y])
                {
                    return false; // 路径被阻挡
                }
            }
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查一次转弯（L型连接）
    /// 路径：起点 -> 拐点 -> 终点
    /// </summary>
    private static bool CheckOneTurn(Vector2Int start, Vector2Int end, bool[,] grid, int gridWidth, int gridHeight)
    {
        // 尝试两个可能的拐点
        Vector2Int corner1 = new Vector2Int(start.x, end.y);
        Vector2Int corner2 = new Vector2Int(end.x, start.y);
        
        // 检查拐点1：先水平后垂直
        if (IsValidPosition(corner1.x, corner1.y, gridWidth, gridHeight) && 
            !grid[corner1.x, corner1.y]) // 拐点必须为空
        {
            // 检查起点到拐点的路径
            bool path1Clear = CheckStraightLine(start, corner1, grid, gridWidth, gridHeight);
            // 检查拐点到终点的路径
            bool path2Clear = CheckStraightLine(corner1, end, grid, gridWidth, gridHeight);
            
            if (path1Clear && path2Clear)
            {
                return true;
            }
        }
        
        // 检查拐点2：先垂直后水平
        if (IsValidPosition(corner2.x, corner2.y, gridWidth, gridHeight) && 
            !grid[corner2.x, corner2.y]) // 拐点必须为空
        {
            // 检查起点到拐点的路径
            bool path1Clear = CheckStraightLine(start, corner2, grid, gridWidth, gridHeight);
            // 检查拐点到终点的路径
            bool path2Clear = CheckStraightLine(corner2, end, grid, gridWidth, gridHeight);
            
            if (path1Clear && path2Clear)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查两次转弯（Z型连接）
    /// 从起点向4个方向扩展，找到所有可达的边界点，然后检查这些点是否能连接到终点
    /// </summary>
    private static bool CheckTwoTurns(Vector2Int start, Vector2Int end, bool[,] grid, int gridWidth, int gridHeight)
    {
        // 从起点向4个方向扩展，找到所有可达的边界点
        List<Vector2Int> reachablePoints = new List<Vector2Int>();
        
        foreach (var dir in Directions)
        {
            Vector2Int current = start + dir;
            
            // 沿着这个方向一直走，直到遇到阻挡或边界
            while (IsValidPosition(current.x, current.y, gridWidth, gridHeight))
            {
                // 如果这个位置有元素阻挡，停止
                if (grid[current.x, current.y])
                {
                    break;
                }
                
                // 将这个点加入可达点列表
                reachablePoints.Add(current);
                current += dir;
            }
        }
        
        // 检查每个可达点是否能通过一次转弯连接到终点
        foreach (var point in reachablePoints)
        {
            if (CheckOneTurn(point, end, grid, gridWidth, gridHeight))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查坐标是否在有效范围内
    /// </summary>
    private static bool IsValidPosition(int x, int y, int gridWidth, int gridHeight)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }
}
