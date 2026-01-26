using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// AI Agent sử dụng Unity ML-Agents để học chơi game demo
/// Agent sẽ quan sát trạng thái game và đưa ra quyết định chọn hình nào
/// </summary>
public class DemoAIAgent : Agent
{
    [Header("Game References")]
    [SerializeField] private DemoGameController _gameController;
    [SerializeField] private DemoShape[] _shapes; // Cube, Sphere, Capsule
    
    [Header("Training Settings")]
    [SerializeField] private float _maxTimePerLevel = 30f;
    
    private float _levelStartTime;
    private int _lastCorrectIndex = -1;
    private bool _isWaitingForAction = false;
    
    // Cache để tránh gọi GetComponent nhiều lần
    private List<DemoSlot> _cachedSlots;
    
    public override void Initialize()
    {
        if (_gameController == null)
        {
            _gameController = FindObjectOfType<DemoGameController>();
        }
        
        if (_shapes == null || _shapes.Length == 0)
        {
            _shapes = FindObjectsOfType<DemoShape>();
        }
        
        // Sắp xếp shapes theo thứ tự: Cube (0), Sphere (1), Capsule (2)
        System.Array.Sort(_shapes, (a, b) => a.Shape.CompareTo(b.Shape));
    }
    
    public override void OnEpisodeBegin()
    {
        // Reset trạng thái khi bắt đầu episode mới
        _levelStartTime = Time.time;
        _lastCorrectIndex = -1;
        _isWaitingForAction = false;
        
        // Đảm bảo game controller sẵn sàng
        if (_gameController != null)
        {
            // Lấy danh sách slots hiện tại
            UpdateCachedSlots();
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        if (_gameController == null || _shapes == null || _shapes.Length == 0)
        {
            // Nếu thiếu references, gửi observations mặc định
            for (int i = 0; i < 20; i++)
            {
                sensor.AddObservation(0f);
            }
            return;
        }
        
        UpdateCachedSlots();
        
        // Observation 1-3: Vị trí hiện tại trong sequence (one-hot encoding cho slot hiện tại)
        int currentIndex = GetCurrentSlotIndex();
        for (int i = 0; i < 3; i++)
        {
            sensor.AddObservation(i == currentIndex ? 1f : 0f);
        }
        
        // Observation 4-6: Hình cần chọn tiếp theo (one-hot encoding)
        Shape nextShape = GetNextRequiredShape();
        for (int i = 0; i < 3; i++)
        {
            sensor.AddObservation((int)nextShape == i ? 1f : 0f);
        }
        
        // Observation 7-9: Số lượng mỗi loại hình còn lại trong sequence
        var remainingCounts = GetRemainingShapeCounts();
        sensor.AddObservation(remainingCounts[0] / 15f); // Normalize
        sensor.AddObservation(remainingCounts[1] / 15f);
        sensor.AddObservation(remainingCounts[2] / 15f);
        
        // Observation 10-12: Tỷ lệ tiến độ (số slot đã hoàn thành / tổng số slot)
        int totalSlots = _cachedSlots != null ? _cachedSlots.Count : 0;
        float progress = totalSlots > 0 ? (float)currentIndex / totalSlots : 0f;
        sensor.AddObservation(progress);
        sensor.AddObservation(totalSlots / 15f); // Normalize tổng số slot
        sensor.AddObservation((Time.time - _levelStartTime) / _maxTimePerLevel); // Thời gian đã trôi qua
        
        // Observation 13-15: Trạng thái các hình có thể chọn (0 = chưa chọn, 1 = đang được highlight)
        for (int i = 0; i < 3; i++)
        {
            if (i < _shapes.Length)
            {
                // Kiểm tra xem shape có đang được select không (có thể kiểm tra material)
                sensor.AddObservation(0f); // Simplified - có thể mở rộng sau
            }
            else
            {
                sensor.AddObservation(0f);
            }
        }
        
        // Observation 16-18: Khoảng cách từ hình hiện tại cần chọn đến các hình có sẵn
        // (để agent hiểu được mối quan hệ giữa yêu cầu và các lựa chọn)
        for (int i = 0; i < 3; i++)
        {
            sensor.AddObservation((int)nextShape == i ? 1f : 0f);
        }
        
        // Observation 19-20: Thông tin về level hiện tại
        int currentLevel = GetCurrentLevel();
        sensor.AddObservation(currentLevel / 20f); // Normalize level
        sensor.AddObservation(_lastCorrectIndex >= 0 ? 1f : 0f); // Đã từng chọn đúng chưa
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!_isWaitingForAction || _gameController == null)
        {
            return;
        }
        
        // Action space: Discrete với 4 actions
        // 0 = Chọn Cube
        // 1 = Chọn Sphere  
        // 2 = Chọn Capsule
        // 3 = Không làm gì (no-op)
        
        int action = actions.DiscreteActions[0];
        
        if (action >= 0 && action < 3 && action < _shapes.Length)
        {
            DemoShape selectedShape = _shapes[action];
            if (selectedShape != null)
            {
                // Lưu trạng thái trước khi chọn
                int previousIndex = GetCurrentSlotIndex();
                Shape nextRequired = GetNextRequiredShape();
                
                // Thực hiện action
                _gameController.Select(selectedShape);
                
                // Kiểm tra kết quả sau một frame
                StartCoroutine(CheckActionResult(previousIndex, nextRequired, action));
            }
        }
        
        _isWaitingForAction = false;
    }
    
    private IEnumerator CheckActionResult(int previousIndex, Shape expectedShape, int actionTaken)
    {
        yield return new WaitForSeconds(0.1f); // Đợi game controller xử lý
        
        int currentIndex = GetCurrentSlotIndex();
        bool wasCorrect = currentIndex > previousIndex;
        
        if (wasCorrect)
        {
            // Phần thưởng cho việc chọn đúng
            AddReward(1.0f);
            _lastCorrectIndex = currentIndex;
            
            // Phần thưởng thêm nếu hoàn thành level
            if (IsLevelComplete())
            {
                AddReward(10.0f);
                EndEpisode();
                yield break;
            }
        }
        else
        {
            // Phạt khi chọn sai
            AddReward(-2.0f);
            EndEpisode();
            yield break;
        }
        
        // Phần thưởng nhỏ cho việc tiến bộ
        float progressReward = (float)currentIndex / Mathf.Max(GetTotalSlots(), 1);
        AddReward(progressReward * 0.1f);
        
        // Phạt nhẹ nếu mất quá nhiều thời gian
        float timePenalty = (Time.time - _levelStartTime) / _maxTimePerLevel;
        if (timePenalty > 0.8f)
        {
            AddReward(-0.5f);
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Heuristic cho testing: chọn hình đúng theo thứ tự
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        Shape nextShape = GetNextRequiredShape();
        discreteActionsOut[0] = (int)nextShape;
    }
    
    // Các helper methods
    private void UpdateCachedSlots()
    {
        // Sử dụng extension method để lấy slots từ game controller
        if (_gameController != null)
        {
            _cachedSlots = _gameController.GetSlots();
        }
        else
        {
            // Fallback: tìm trong scene
            _cachedSlots = new List<DemoSlot>(FindObjectsOfType<DemoSlot>());
            _cachedSlots.Sort((a, b) => {
                // Sắp xếp theo vị trí (từ trái sang phải, từ trên xuống dưới)
                float aX = a.transform.position.x;
                float bX = b.transform.position.x;
                if (Mathf.Abs(aX - bX) < 0.1f)
                {
                    return a.transform.position.y.CompareTo(b.transform.position.y);
                }
                return aX.CompareTo(bX);
            });
        }
    }
    
    private int GetCurrentSlotIndex()
    {
        // Sử dụng extension method để lấy current index từ game controller
        return GetCurrentIndexFromController();
    }
    
    private int GetCurrentIndexFromController()
    {
        if (_gameController != null)
        {
            return _gameController.GetCurrentIndex();
        }
        return 0;
    }
    
    private Shape GetNextRequiredShape()
    {
        if (_gameController != null)
        {
            return _gameController.GetNextRequiredShape();
        }
        
        if (_cachedSlots == null) UpdateCachedSlots();
        
        int currentIndex = GetCurrentIndexFromController();
        if (currentIndex < _cachedSlots.Count && _cachedSlots[currentIndex] != null)
        {
            return _cachedSlots[currentIndex].Shape;
        }
        
        return Shape.Cube; // Default
    }
    
    private int[] GetRemainingShapeCounts()
    {
        int[] counts = new int[3];
        
        if (_cachedSlots == null) UpdateCachedSlots();
        
        int currentIndex = GetCurrentIndexFromController();
        for (int i = currentIndex; i < _cachedSlots.Count; i++)
        {
            if (_cachedSlots[i] != null)
            {
                counts[(int)_cachedSlots[i].Shape]++;
            }
        }
        
        return counts;
    }
    
    private int GetTotalSlots()
    {
        if (_cachedSlots == null) UpdateCachedSlots();
        return _cachedSlots != null ? _cachedSlots.Count : 0;
    }
    
    private bool IsLevelComplete()
    {
        int currentIndex = GetCurrentIndexFromController();
        return currentIndex >= GetTotalSlots();
    }
    
    private int GetCurrentLevel()
    {
        if (_gameController != null)
        {
            return _gameController.GetCurrentLevel();
        }
        return 1;
    }
    
    // Method để game controller gọi khi cần action từ AI
    public void RequestAIDecision()
    {
        if (!_isWaitingForAction)
        {
            _isWaitingForAction = true;
            RequestDecision();
        }
    }
    
    // Method để kiểm tra xem agent có đang chờ action không
    public bool IsWaitingForAction()
    {
        return _isWaitingForAction;
    }
}
