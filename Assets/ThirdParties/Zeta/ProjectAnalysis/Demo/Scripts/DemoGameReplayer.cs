using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zeta.ProjectAnalysis;
using Newtonsoft.Json;
using System;

public class DemoGameReplayer : MonoBehaviour
{
    [SerializeField] DemoGameController _gameController;
    [SerializeField] TextAsset _behaviourJson;
    [SerializeField] DemoShape[] _shapes;
    [SerializeField] Gesture _gesture;
    [SerializeField] Text _userIdText;
    [SerializeField] GameObject _startReplayButton;
    [SerializeField] GameObject _stopReplayButton;

    BehaviourRequest _behaviourData;

    public void OnStartReplayButtonTap()
    {
        StartCoroutine(Play());
    }

    public void OnStopReplayButtonTap()
    {
        Stop();
    }

    void Start()
    {
        SetButtons();
    }

    void SetButtons()
    {
        #if !UNITY_EDITOR
        _startReplayButton.SetActive(false);
        _stopReplayButton.SetActive(false);
        #endif
    }

    IEnumerator Play()
    {
        _gesture.gameObject.SetActive(true);

        BehaviourAPIClient.CurrentState = BehaviourAPIClient.State.Replaying;

        var json = _behaviourJson.text;
        _behaviourData = JsonConvert.DeserializeObject<BehaviourRequest>(json);

        _userIdText.text = "User ID: " + _behaviourData.projectUserId;

        #if UNITY_EDITOR
        GameWindowEditor.Resize(_behaviourData.width, _behaviourData.height);
        #endif

        var index = 0;
        var dto = DateTimeOffset.Parse(_behaviourData.behaviours[index].time);
        var prevTime = dto.UtcDateTime;

        while (index < _behaviourData.behaviours.Count)
        {
            var behaviour = _behaviourData.behaviours[index];
            
            dto = DateTimeOffset.Parse(behaviour.time);
            var time = dto.UtcDateTime;
            var delay = (float)(time - prevTime).TotalSeconds;

            switch (behaviour.behaviourId)
            {
                case "shape_selected":
                case "button_clicked":
                    yield return new WaitForSeconds(delay - 0.33f);
                    _gesture.Tap(behaviour.x, behaviour.y);
                    yield return new WaitForSeconds(0.33f);
                    break;

                default:
                    yield return new WaitForSeconds(delay);
                    break;
            }

            var currentLevel = behaviour.level;

            switch (behaviour.behaviourId)
            {
                case "level_started":
                    _gameController.SetLevel(currentLevel);
                    _gameController.GenerateLevel();
                    break;

                case "shape_selected":
                    var shape = GetShape(behaviour.objectId);
                    if (shape != null)
                    {
                        _gameController.ResetShapes();
                        _gameController.Select(shape);
                    }
                    break;

                case "button_clicked":
                    switch (behaviour.objectId)
                    {
                        case "retry_button":
                            _gameController.OnRetryButtonTap();
                            break;
                    }
                    break;
            }

            index++;
            prevTime = time;
        }
    }

    void Stop()
    {
        _gesture.gameObject.SetActive(false);
        StopAllCoroutines();
        BehaviourAPIClient.CurrentState = BehaviourAPIClient.State.Default;
        _gameController.SetCurrentLevel();
        _gameController.GenerateLevel();
    }

    DemoShape GetShape(string name)
    {
        for (int i = 0; i < _shapes.Length; i++)
        {
            if (_shapes[i].Shape.ToString() == name)
            {
                return _shapes[i];
            }
        }

        return null;
    }
}