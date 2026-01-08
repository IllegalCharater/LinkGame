using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 方向选择UI控制器
/// 当点击特殊元素时显示，提供上下左右四个方向按钮
/// </summary>
public class DirectionSelector : MonoBehaviour
{
    [SerializeField] private Button upButton;      // 上方向按钮
    [SerializeField] private Button downButton;    // 下方向按钮
    [SerializeField] private Button leftButton;    // 左方向按钮
    [SerializeField] private Button rightButton;   // 右方向按钮
    [SerializeField] private GameObject panel;     // 方向选择面板（用于显示/隐藏）
    
    // 方向向量定义
    public static readonly Vector2Int Up = new Vector2Int(0, 1);
    public static readonly Vector2Int Down = new Vector2Int(0, -1);
    public static readonly Vector2Int Left = new Vector2Int(-1, 0);
    public static readonly Vector2Int Right = new Vector2Int(1, 0);
    
    // 事件：当选择方向时触发，参数为选择的方向向量
    public System.Action<Vector2Int> OnDirectionSelected;
    
    private Vector2Int selectedDirection;
    private bool isActive;
    
    private void Awake()
    {
        // 绑定按钮事件
        if (upButton != null)
        {
            upButton.onClick.AddListener(() => SelectDirection(Up));
        }
        
        if (downButton != null)
        {
            downButton.onClick.AddListener(() => SelectDirection(Down));
        }
        
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(() => SelectDirection(Left));
        }
        
        if (rightButton != null)
        {
            rightButton.onClick.AddListener(() => SelectDirection(Right));
        }
        
        // 初始隐藏面板
        Hide();
    }
    
    /// <summary>
    /// 显示方向选择面板
    /// </summary>
    /// <param name="worldPosition">特殊元素的世界坐标位置（用于定位面板）</param>
    public void Show(Vector3 worldPosition)
    {
        isActive = true;
        
        if (panel != null)
        {
            panel.SetActive(true);
            
            // 将世界坐标转换为UI坐标（如果需要）
            // 这里假设面板已经通过RectTransform正确设置位置
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 可以在这里设置面板位置，例如跟随特殊元素
                // rectTransform.position = worldPosition;
            }
        }
        
        // 启用所有按钮
        SetButtonsInteractable(true);
    }
    
    /// <summary>
    /// 隐藏方向选择面板
    /// </summary>
    public void Hide()
    {
        isActive = false;
        
        if (panel != null)
        {
            panel.SetActive(false);
        }
        
        SetButtonsInteractable(false);
    }
    
    /// <summary>
    /// 选择方向
    /// </summary>
    private void SelectDirection(Vector2Int direction)
    {
        if (!isActive) return;
        
        selectedDirection = direction;
        
        // 触发事件
        if (OnDirectionSelected != null)
        {
            OnDirectionSelected.Invoke(direction);
        }
        
        // 选择后隐藏面板
        Hide();
    }
    
    /// <summary>
    /// 设置按钮是否可交互
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (upButton != null) upButton.interactable = interactable;
        if (downButton != null) downButton.interactable = interactable;
        if (leftButton != null) leftButton.interactable = interactable;
        if (rightButton != null) rightButton.interactable = interactable;
    }
    
    /// <summary>
    /// 检查是否正在显示
    /// </summary>
    public bool IsActive() => isActive;
    
    /// <summary>
    /// 根据方向向量获取方向名称（用于调试）
    /// </summary>
    public static string GetDirectionName(Vector2Int direction)
    {
        if (direction == Up) return "上";
        if (direction == Down) return "下";
        if (direction == Left) return "左";
        if (direction == Right) return "右";
        return "未知";
    }
}
