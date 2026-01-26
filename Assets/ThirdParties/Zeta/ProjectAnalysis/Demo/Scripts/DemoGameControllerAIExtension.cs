using System.Collections;
using UnityEngine;

/// <summary>
/// Extension cho DemoGameController để hỗ trợ AI Agent
/// Thêm các method public để AI có thể truy cập thông tin game state
/// </summary>
public static class DemoGameControllerAIExtension
{
    /// <summary>
    /// Lấy index hiện tại trong sequence
    /// </summary>
    public static int GetCurrentIndex(this DemoGameController controller)
    {
        var field = typeof(DemoGameController).GetField("_currentIndex", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (int)field.GetValue(controller);
        }
        return 0;
    }
    
    /// <summary>
    /// Lấy danh sách slots hiện tại
    /// </summary>
    public static System.Collections.Generic.List<DemoSlot> GetSlots(this DemoGameController controller)
    {
        var field = typeof(DemoGameController).GetField("_slots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (System.Collections.Generic.List<DemoSlot>)field.GetValue(controller);
        }
        return new System.Collections.Generic.List<DemoSlot>();
    }
    
    /// <summary>
    /// Lấy level hiện tại
    /// </summary>
    public static int GetCurrentLevel(this DemoGameController controller)
    {
        var field = typeof(DemoGameController).GetField("_currentLevel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (int)field.GetValue(controller);
        }
        return 1;
    }
    
    /// <summary>
    /// Kiểm tra xem game có đang playable không
    /// </summary>
    public static bool IsPlayable(this DemoGameController controller)
    {
        var field = typeof(DemoGameController).GetField("_playable", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(controller);
        }
        return false;
    }
    
    /// <summary>
    /// Lấy hình cần chọn tiếp theo
    /// </summary>
    public static Shape GetNextRequiredShape(this DemoGameController controller)
    {
        var slots = controller.GetSlots();
        int currentIndex = controller.GetCurrentIndex();
        
        if (currentIndex < slots.Count && slots[currentIndex] != null)
        {
            return slots[currentIndex].Shape;
        }
        
        return Shape.Cube; // Default
    }
}
