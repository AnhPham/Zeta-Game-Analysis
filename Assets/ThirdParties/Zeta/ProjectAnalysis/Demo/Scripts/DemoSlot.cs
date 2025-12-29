using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Shape
{
    Cube,
    Sphere,
    Capsule
}

public class DemoSlot : MonoBehaviour
{
    [SerializeField] Transform[] _slotShapes;
    [SerializeField] Material _activeMaterial;

    private Shape _shape;
    public Shape Shape
    {
        get
        {
            return _shape;
        }
        set
        {
            _shape = value;
            _slotShapes[(int)_shape].gameObject.SetActive(true);
        }
    }

    public void Show()
    {
        _slotShapes[(int)_shape].gameObject.GetComponent<MeshRenderer>().material = _activeMaterial;
    }
}
