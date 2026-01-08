using UnityEngine;

/// <summary>
/// 游戏入口脚本
/// 负责初始化游戏管理器
/// </summary>
public class Main : MonoBehaviour
{
    [SerializeField] private LinkGameManager gameManager; // 游戏管理器引用
    
    void Start()
    {
        // 如果未在Inspector中指定，尝试自动查找
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<LinkGameManager>();
        }
        
        if (gameManager == null)
        {
            Debug.LogWarning("未找到 LinkGameManager，请确保场景中存在游戏管理器！");
        }
        else
        {
            Debug.Log("连连看游戏初始化完成");
        }
    }
}
