using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 连连看单个格子元素
/// 负责显示元素图标、处理点击事件、标识元素类型
/// </summary>
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;           // 显示元素图标的Image组件
    [SerializeField] private Button tileButton;          // 点击按钮组件
    [SerializeField] private GameObject selectedEffect; // 选中效果（可选）
    
    private int elementId;                              // 元素ID，相同ID的元素可以配对
    private bool isSpecial;                              // 是否为特殊元素
    private Vector2Int gridPosition;                     // 在网格中的位置坐标
    private bool isMatched;                              // 是否已被消除
    
    // 事件：当Tile被点击时触发
    public System.Action<Tile> OnTileClicked;
    
    /// <summary>
    /// 初始化Tile
    /// </summary>
    public void Initialize(int id, Sprite sprite, bool special, Vector2Int position)
    {
        elementId = id;
        isSpecial = special;
        gridPosition = position;
        isMatched = false;
        
        if (iconImage != null && sprite != null)
        {
            iconImage.sprite = sprite;
        }
        
        // 特殊元素可以有不同的视觉效果
        if (isSpecial && iconImage != null)
        {
            iconImage.color = Color.yellow; // 特殊元素用黄色标识
        }
        
        SetInteractable(true);
    }
    
    /// <summary>
    /// 获取元素ID
    /// </summary>
    public int GetElementId() => elementId;
    
    /// <summary>
    /// 是否为特殊元素
    /// </summary>
    public bool IsSpecial() => isSpecial;
    
    /// <summary>
    /// 获取网格位置
    /// </summary>
    public Vector2Int GetGridPosition() => gridPosition;
    
    /// <summary>
    /// 是否已被消除
    /// </summary>
    public bool IsMatched() => isMatched;
    
    /// <summary>
    /// 设置是否可交互
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (tileButton != null)
        {
            tileButton.interactable = interactable;
        }
    }
    
    /// <summary>
    /// 显示选中效果
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(selected);
        }
        
        // 也可以通过改变颜色来显示选中状态
        if (iconImage != null)
        {
            iconImage.color = selected ? Color.cyan : (isSpecial ? Color.yellow : Color.white);
        }
    }
    
    /// <summary>
    /// 消除Tile（播放动画后隐藏）
    /// </summary>
    public void Match()
    {
        isMatched = true;
        SetInteractable(false);
        
        // 简单的淡出动画
        StartCoroutine(FadeOut());
    }
    
    /// <summary>
    /// 淡出动画协程
    /// </summary>
    private System.Collections.IEnumerator FadeOut()
    {
        if (iconImage == null) yield break;
        
        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = iconImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            iconImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 处理点击事件（实现IPointerClickHandler接口）
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isMatched && OnTileClicked != null)
        {
            OnTileClicked.Invoke(this);
        }
    }
    
    /// <summary>
    /// 重置Tile状态（用于对象池）
    /// </summary>
    public void Reset()
    {
        isMatched = false;
        SetInteractable(true);
        SetSelected(false);
        
        if (iconImage != null)
        {
            iconImage.color = isSpecial ? Color.yellow : Color.white;
        }
        
        gameObject.SetActive(true);
    }
}
