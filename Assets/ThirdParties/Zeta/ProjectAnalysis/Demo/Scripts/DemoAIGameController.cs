using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeta.ProjectAnalysis;

/// <summary>
/// Wrapper cho DemoGameController để tích hợp với ML-Agents
/// Quản lý việc tương tác giữa AI Agent và game logic
/// </summary>
public class DemoAIGameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DemoGameController _gameController;
    [SerializeField] private DemoAIAgent _aiAgent;
    
    [Header("AI Settings")]
    [SerializeField] private bool _useAI = false;
    [SerializeField] private float _actionDelay = 0.5f; // Thời gian delay giữa các action
    
    private bool _isAIPlaying = false;
    private Coroutine _aiPlayCoroutine;
    
    void Start()
    {
        if (_gameController == null)
        {
            _gameController = FindObjectOfType<DemoGameController>();
        }
        
        if (_aiAgent == null)
        {
            _aiAgent = FindObjectOfType<DemoAIAgent>();
        }
        
        // Subscribe vào events nếu có
        if (_useAI && _aiAgent != null)
        {
            StartAIPlaying();
        }
    }
    
    public void SetUseAI(bool useAI)
    {
        _useAI = useAI;
        
        if (_useAI && !_isAIPlaying)
        {
            StartAIPlaying();
        }
        else if (!_useAI && _isAIPlaying)
        {
            StopAIPlaying();
        }
    }
    
    private void StartAIPlaying()
    {
        if (_isAIPlaying) return;
        
        _isAIPlaying = true;
        if (_aiPlayCoroutine != null)
        {
            StopCoroutine(_aiPlayCoroutine);
        }
        _aiPlayCoroutine = StartCoroutine(AIPlayingLoop());
    }
    
    private void StopAIPlaying()
    {
        _isAIPlaying = false;
        if (_aiPlayCoroutine != null)
        {
            StopCoroutine(_aiPlayCoroutine);
            _aiPlayCoroutine = null;
        }
    }
    
    private IEnumerator AIPlayingLoop()
    {
        while (_isAIPlaying && _aiAgent != null && _gameController != null)
        {
            // Kiểm tra xem game có đang playable không
            bool isPlayable = GetPlayableState();
            
            if (isPlayable && !_aiAgent.IsWaitingForAction())
            {
                // Yêu cầu AI đưa ra action
                _aiAgent.RequestAIDecision();
                
                // Đợi một chút trước khi action tiếp theo
                yield return new WaitForSeconds(_actionDelay);
            }
            else
            {
                // Đợi game sẵn sàng
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    private bool GetPlayableState()
    {
        // Sử dụng reflection để lấy _playable từ DemoGameController
        var field = typeof(DemoGameController).GetField("_playable", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null && _gameController != null)
        {
            return (bool)field.GetValue(_gameController);
        }
        return false;
    }
    
    void OnDestroy()
    {
        StopAIPlaying();
    }
}
