using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoShape : MonoBehaviour
{
    [SerializeField] Shape _shape;
    [SerializeField] Material _defaultMaterial;
    [SerializeField] Material _selectedMaterial;

    MeshRenderer _meshRenderer;

    public Shape Shape
    {
        get
        {
            return _shape;
        }
        set
        {
            _shape = value;
        }
    }

    public void Select()
    {
        _meshRenderer.material = _selectedMaterial;
    }

    public void DeSelect()
    {
        _meshRenderer.material = _defaultMaterial;
    }

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
}
