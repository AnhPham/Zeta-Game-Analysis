using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zeta.ProjectAnalysis;

public class DemoGameController : MonoBehaviour
{
    [SerializeField] DemoSlot _slotPrefab;
    [SerializeField] Transform _slotContainer;
    [SerializeField] Text _levelText;
    [SerializeField] GameObject _result;
    [SerializeField] Text _resultText;
    [SerializeField] DemoShape[] _shapes;
    [SerializeField] Text _userIdText;

    List<DemoSlot> _slots = new List<DemoSlot>();
    int _currentIndex = 0;
    int _currentLevel = 1;
    bool _playable = true;
    EventSystem _eventSystem;
    PointerEventData _ped;
    readonly List<RaycastResult> _hits = new();

    public void OnRetryButtonTap()
    {
        BehaviourAPIClient.Add(behaviourId: "button_clicked", level: _currentLevel, screen: "game", objectId: "retry_button");
        GenerateLevel();
    }

    public void SetLevel(int level)
    {
        _currentLevel = level;
    }

    public void SetCurrentLevel()
    {
        _currentLevel = PlayerPrefs.GetInt("LEVEL", 1);
    }

    public void Select(DemoShape demoShape)
    {
        demoShape.Select();
        BehaviourAPIClient.Add(behaviourId: "shape_selected", level: _currentLevel, screen: "game", objectId: demoShape.Shape.ToString());

        if (demoShape.Shape == _slots[_currentIndex].Shape)
        {
            _slots[_currentIndex].Show();
            _currentIndex++;

            if (_currentIndex >= _slots.Count)
            {
                StartCoroutine(Win());
            }
        }
        else
        {
            StartCoroutine(Lose());
        }       
    }

    public void GenerateLevel()
    {
        ResetShapes();
        ClearLevel();

        _currentIndex = 0;
        _playable = true;

        _result.SetActive(false);
        _levelText.text = "Level " + _currentLevel;
        
        var seed = _currentLevel;
        var rnd = new System.Random(seed);
        var shapeCount = Mathf.Min(_currentLevel, 15);

        for (int i = 0; i < shapeCount; i++)
        {
            var slot = Instantiate(_slotPrefab, _slotContainer);

            var shape = rnd.Next(0, 3);
            slot.Shape = (Shape)shape;
            slot.transform.localPosition = new Vector3((i % 5) * 0.75f, - (i / 5) * 0.75f, 0);

            _slots.Add(slot);
        }

        BehaviourAPIClient.Add(behaviourId: "level_started", level: _currentLevel, screen: "game");
    }

    public void ResetShapes()
    {
        for (int i = 0; i < _shapes.Length; i++)
        {
            _shapes[i].DeSelect();
        }
    }

    void Awake()
    {
        _eventSystem = EventSystem.current;
        _ped = new PointerEventData(_eventSystem);
    }

    void Start()
    {
        SetCurrentLevel();
        GenerateLevel();
        SetUserIdText();
    }

    void Update()
    {
        if (_playable && Input.GetMouseButtonDown(0))
        {
            _ped.Reset();
            _ped.position = Input.mousePosition;
            _hits.Clear();

            _eventSystem.RaycastAll(_ped, _hits);
            if (_hits.Count == 0) 
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    var demoShape = hit.collider.gameObject.GetComponent<DemoShape>();
                    if (demoShape != null)
                    {
                        Select(demoShape);
                    }
                }
                else
                {
                    BehaviourAPIClient.Add(behaviourId: "miss_clicked", level: _currentLevel, screen: "game");
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetShapes(); 
        }
    }

    void SetUserIdText()
    {
        _userIdText.text = "User ID: " + PlayerPrefs.GetString("ZETA_PROJECT_USER_ID", string.Empty);
    }

    void ClearLevel()
    {
        foreach (var slot in _slots)
        {
            Destroy(slot.gameObject);
        }
        _slots.Clear();
    }

    IEnumerator Win()
    {
        BehaviourAPIClient.Add(behaviourId: "level_completed", level: _currentLevel, screen: "game");
        BehaviourAPIClient.Send((response) => { SetUserIdText(); });

        _playable = false;
        _currentLevel++;

        if (BehaviourAPIClient.CurrentState == BehaviourAPIClient.State.Default)
        {
            PlayerPrefs.SetInt("LEVEL", _currentLevel);
        }

        yield return new WaitForSeconds(0.2f);

        _result.SetActive(true);
        _resultText.text = "Win!";
        yield return new WaitForSeconds(1);

        if (BehaviourAPIClient.CurrentState == BehaviourAPIClient.State.Default)
        {
            GenerateLevel();
        }
    }

    IEnumerator Lose()
    {
        BehaviourAPIClient.Add(behaviourId: "level_failed", level: _currentLevel, screen: "game");
        BehaviourAPIClient.Send((response) => { SetUserIdText(); });

        _playable = false;

        yield return new WaitForSeconds(0.2f);

        _result.SetActive(true);
        _resultText.text = "Lose!";

        yield return new WaitForSeconds(1);

        if (BehaviourAPIClient.CurrentState == BehaviourAPIClient.State.Default)
        {
            GenerateLevel();
        }
    }
}