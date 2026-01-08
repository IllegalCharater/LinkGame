using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 网格管理系统
/// 负责生成8x8网格、随机布局元素、确保有解
/// </summary>
public class GridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 8;              // 网格宽度
    [SerializeField] private int gridHeight = 8;             // 网格高度
    [SerializeField] private GameObject tilePrefab;          // Tile预制体
    [SerializeField] private Transform gridContainer;        // 网格容器（需要GridLayoutGroup组件）
    [SerializeField] private Sprite[] elementSprites;        // 元素图标数组
    [SerializeField] private Sprite specialElementSprite;    // 特殊元素图标
    [SerializeField] [Range(0f, 1f)] private float specialElementChance = 0.1f; // 特殊元素生成概率（10%）
    
    private Tile[,] grid;                                     // 网格数组
    private List<Tile> allTiles;                              // 所有Tile的列表
    
    /// <summary>
    /// 生成网格
    /// </summary>
    public void GenerateGrid()
    {
        // 清理旧网格
        ClearGrid();
        
        // 初始化数组
        grid = new Tile[gridWidth, gridHeight];
        allTiles = new List<Tile>();
        
        // 计算需要的元素对数（8x8 = 64格，需要32对）
        int totalPairs = (gridWidth * gridHeight) / 2;
        
        // 生成元素ID列表（每个ID出现2次）
        List<int> elementIds = new List<int>();
        for (int i = 0; i < totalPairs; i++)
        {
            elementIds.Add(i);
            elementIds.Add(i); // 每对元素添加两次
        }
        
        // 随机打乱元素ID列表
        ShuffleList(elementIds);
        
        // 确定特殊元素的数量（根据概率计算）
        int specialElementCount = Mathf.RoundToInt((gridWidth * gridHeight) * specialElementChance);
        specialElementCount = Mathf.Max(1, specialElementCount); // 至少1个特殊元素
        
        // 创建Tile并布局
        int elementIndex = 0;
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                
                // 判断是否为特殊元素
                bool isSpecial = elementIndex < specialElementCount;
                
                // 获取元素ID和图标
                int elementId = isSpecial ? -1 : elementIds[elementIndex - specialElementCount]; // 特殊元素ID为-1
                Sprite sprite = isSpecial ? specialElementSprite : 
                    (elementSprites != null && elementId < elementSprites.Length ? elementSprites[elementId] : null);
                
                // 创建Tile
                GameObject tileObj = Instantiate(tilePrefab, gridContainer);
                Tile tile = tileObj.GetComponent<Tile>();
                
                if (tile == null)
                {
                    tile = tileObj.AddComponent<Tile>();
                }
                
                // 初始化Tile
                tile.Initialize(elementId, sprite, isSpecial, position);
                
                // 存储到网格
                grid[x, y] = tile;
                allTiles.Add(tile);
                
                elementIndex++;
            }
        }
        
        // 确保网格有解（简单验证：至少有一对普通元素可以连接）
        // 注意：这里只是基本验证，完整的有解性检查比较复杂，可以在游戏运行时处理
    }
    
    /// <summary>
    /// 获取指定位置的Tile
    /// </summary>
    public Tile GetTile(Vector2Int position)
    {
        if (IsValidPosition(position))
        {
            return grid[position.x, position.y];
        }
        return null;
    }
    
    /// <summary>
    /// 获取网格状态（用于路径查找）
    /// true表示该位置有元素（被阻挡），false表示空
    /// </summary>
    public bool[,] GetGridState()
    {
        bool[,] state = new bool[gridWidth, gridHeight];
        
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // 如果Tile存在且未被消除，则标记为有元素
                state[x, y] = grid[x, y] != null && !grid[x, y].IsMatched();
            }
        }
        
        return state;
    }
    
    /// <summary>
    /// 检查位置是否有效
    /// </summary>
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < gridWidth && 
               position.y >= 0 && position.y < gridHeight;
    }
    
    /// <summary>
    /// 获取网格宽度
    /// </summary>
    public int GetGridWidth() => gridWidth;
    
    /// <summary>
    /// 获取网格高度
    /// </summary>
    public int GetGridHeight() => gridHeight;
    
    /// <summary>
    /// 获取所有Tile
    /// </summary>
    public List<Tile> GetAllTiles() => allTiles;
    
    /// <summary>
    /// 清理网格
    /// </summary>
    public void ClearGrid()
    {
        if (allTiles != null)
        {
            foreach (var tile in allTiles)
            {
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
            allTiles.Clear();
        }
        
        grid = null;
    }
    
    /// <summary>
    /// 洗牌算法（Fisher-Yates）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 在指定方向查找元素
    /// </summary>
    /// <param name="startPos">起始位置</param>
    /// <param name="direction">方向向量</param>
    /// <param name="count">需要查找的元素数量</param>
    /// <returns>找到的Tile列表</returns>
    public List<Tile> FindTilesInDirection(Vector2Int startPos, Vector2Int direction, int count)
    {
        List<Tile> foundTiles = new List<Tile>();
        Vector2Int currentPos = startPos + direction;
        
        while (IsValidPosition(currentPos) && foundTiles.Count < count)
        {
            Tile tile = GetTile(currentPos);
            
            // 如果Tile存在且未被消除
            if (tile != null && !tile.IsMatched())
            {
                // 只查找普通元素（跳过特殊元素）
                if (!tile.IsSpecial())
                {
                    foundTiles.Add(tile);
                }
            }
            
            currentPos += direction;
        }
        
        return foundTiles;
    }
}
