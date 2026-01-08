using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 连连看游戏主管理器
/// 负责整合所有系统，管理游戏流程、计分、点击处理、特殊元素消除逻辑
/// </summary>
public class LinkGameManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;           // 网格管理器
    [SerializeField] private DirectionSelector directionSelector; // 方向选择器
    [SerializeField] private Text scoreText;                    // 计分文本
    [SerializeField] private Text gameOverText;                 // 游戏结束文本（可选）
    
    private Tile selectedTile;                                  // 当前选中的Tile
    private Tile activeSpecialTile;                             // 当前激活的特殊元素（等待方向选择）
    private int score;                                           // 当前分数
    private bool isProcessing;                                   // 是否正在处理操作（防止重复点击）
    
    // 游戏状态
    private enum GameState
    {
        Playing,    // 游戏中
        Paused,     // 暂停
        GameOver    // 游戏结束
    }
    
    private GameState currentState = GameState.Playing;
    
    private void Start()
    {
        InitializeGame();
    }
    
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        score = 0;
        selectedTile = null;
        activeSpecialTile = null;
        isProcessing = false;
        currentState = GameState.Playing;
        
        // 生成网格
        if (gridManager != null)
        {
            gridManager.GenerateGrid();
            
            // 为所有Tile注册点击事件
            RegisterTileClickEvents();
        }
        
        // 注册方向选择事件
        if (directionSelector != null)
        {
            directionSelector.OnDirectionSelected += OnDirectionSelected;
        }
        
        UpdateScoreDisplay();
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 为所有Tile注册点击事件
    /// </summary>
    private void RegisterTileClickEvents()
    {
        if (gridManager == null) return;
        
        List<Tile> allTiles = gridManager.GetAllTiles();
        foreach (var tile in allTiles)
        {
            if (tile != null)
            {
                tile.OnTileClicked += OnTileClicked;
            }
        }
    }
    
    /// <summary>
    /// 处理Tile点击事件
    /// </summary>
    private void OnTileClicked(Tile tile)
    {
        if (isProcessing || currentState != GameState.Playing) return;
        if (tile == null || tile.IsMatched()) return;
        
        // 如果点击的是特殊元素
        if (tile.IsSpecial())
        {
            HandleSpecialTileClick(tile);
            return;
        }
        
        // 如果还没有选中任何Tile
        if (selectedTile == null)
        {
            SelectTile(tile);
        }
        // 如果点击的是已选中的Tile，取消选中
        else if (selectedTile == tile)
        {
            DeselectTile();
        }
        // 如果点击的是另一个Tile，尝试配对
        else
        {
            TryMatchTiles(selectedTile, tile);
        }
    }
    
    /// <summary>
    /// 选中Tile
    /// </summary>
    private void SelectTile(Tile tile)
    {
        // 取消之前的选中
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false);
        }
        
        selectedTile = tile;
        if (selectedTile != null)
        {
            selectedTile.SetSelected(true);
        }
    }
    
    /// <summary>
    /// 取消选中Tile
    /// </summary>
    private void DeselectTile()
    {
        if (selectedTile != null)
        {
            selectedTile.SetSelected(false);
            selectedTile = null;
        }
    }
    
    /// <summary>
    /// 尝试配对两个Tile
    /// </summary>
    private void TryMatchTiles(Tile tile1, Tile tile2)
    {
        if (tile1 == null || tile2 == null) return;
        
        // 检查是否为相同元素（普通元素需要ID相同）
        if (tile1.GetElementId() != tile2.GetElementId())
        {
            // 不匹配，取消选中
            DeselectTile();
            SelectTile(tile2);
            return;
        }
        
        // 检查路径是否可连接
        bool canConnect = PathFinder.CanConnect(
            tile1.GetGridPosition(),
            tile2.GetGridPosition(),
            gridManager.GetGridState(),
            gridManager.GetGridWidth(),
            gridManager.GetGridHeight()
        );
        
        if (canConnect)
        {
            // 可以连接，消除这两个Tile
            StartCoroutine(MatchTilesCoroutine(tile1, tile2));
        }
        else
        {
            // 不能连接，取消选中，选中新Tile
            DeselectTile();
            SelectTile(tile2);
        }
    }
    
    /// <summary>
    /// 消除两个Tile的协程
    /// </summary>
    private IEnumerator MatchTilesCoroutine(Tile tile1, Tile tile2)
    {
        isProcessing = true;
        
        // 取消选中
        DeselectTile();
        
        // 消除Tile
        tile1.Match();
        tile2.Match();
        
        // 增加分数
        AddScore(10);
        
        // 等待动画完成
        yield return new WaitForSeconds(0.5f);
        
        isProcessing = false;
        
        // 检查游戏是否完成
        CheckGameOver();
    }
    
    /// <summary>
    /// 处理特殊元素点击
    /// </summary>
    private void HandleSpecialTileClick(Tile specialTile)
    {
        if (specialTile == null || !specialTile.IsSpecial()) return;
        
        // 取消之前的选中
        DeselectTile();
        
        // 存储当前激活的特殊元素
        activeSpecialTile = specialTile;
        
        // 显示方向选择面板
        if (directionSelector != null)
        {
            // 获取特殊元素的世界坐标（用于定位面板）
            Vector3 worldPos = specialTile.transform.position;
            directionSelector.Show(worldPos);
        }
    }
    
    /// <summary>
    /// 处理方向选择事件
    /// </summary>
    private void OnDirectionSelected(Vector2Int direction)
    {
        if (isProcessing || currentState != GameState.Playing) return;
        
        // 使用存储的激活特殊元素
        if (activeSpecialTile != null && !activeSpecialTile.IsMatched())
        {
            StartCoroutine(HandleSpecialTileDirectionCoroutine(activeSpecialTile, direction));
            activeSpecialTile = null; // 清除引用
        }
    }
    
    /// <summary>
    /// 处理特殊元素方向消除的协程
    /// </summary>
    private IEnumerator HandleSpecialTileDirectionCoroutine(Tile specialTile, Vector2Int direction)
    {
        isProcessing = true;
        
        // 在指定方向查找两个普通元素
        List<Tile> tilesToRemove = gridManager.FindTilesInDirection(
            specialTile.GetGridPosition(),
            direction,
            2
        );
        
        // 如果找到了足够的元素
        if (tilesToRemove.Count >= 2)
        {
            // 消除特殊元素
            specialTile.Match();
            
            // 消除找到的两个普通元素
            foreach (var tile in tilesToRemove)
            {
                tile.Match();
            }
            
            // 增加分数（特殊元素消除得分更高）
            AddScore(30);
            
            // 等待动画完成
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // 该方向没有足够的元素，提示玩家
            Debug.Log($"方向 {DirectionSelector.GetDirectionName(direction)} 没有足够的元素可以消除");
        }
        
        isProcessing = false;
        
        // 检查游戏是否完成
        CheckGameOver();
    }
    
    
    /// <summary>
    /// 增加分数
    /// </summary>
    private void AddScore(int points)
    {
        score += points;
        UpdateScoreDisplay();
    }
    
    /// <summary>
    /// 更新分数显示
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"分数: {score}";
        }
    }
    
    /// <summary>
    /// 检查游戏是否结束
    /// </summary>
    private void CheckGameOver()
    {
        if (gridManager == null) return;
        
        List<Tile> allTiles = gridManager.GetAllTiles();
        int remainingTiles = 0;
        
        foreach (var tile in allTiles)
        {
            if (tile != null && !tile.IsMatched())
            {
                remainingTiles++;
            }
        }
        
        // 如果所有Tile都被消除了，游戏胜利
        if (remainingTiles == 0)
        {
            GameOver(true);
        }
    }
    
    /// <summary>
    /// 游戏结束
    /// </summary>
    private void GameOver(bool victory)
    {
        currentState = GameState.GameOver;
        
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = victory ? $"游戏胜利！\n最终分数: {score}" : "游戏结束";
        }
        
        Debug.Log($"游戏结束！最终分数: {score}");
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 清理旧网格
        if (gridManager != null)
        {
            gridManager.ClearGrid();
        }
        
        // 隐藏方向选择面板
        if (directionSelector != null)
        {
            directionSelector.Hide();
        }
        
        // 清除特殊元素引用
        activeSpecialTile = null;
        
        // 重新初始化
        InitializeGame();
    }
    
    private void OnDestroy()
    {
        // 取消事件订阅
        if (directionSelector != null)
        {
            directionSelector.OnDirectionSelected -= OnDirectionSelected;
        }
    }
}
