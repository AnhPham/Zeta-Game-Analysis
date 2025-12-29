using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gesture : MonoBehaviour
{
    [SerializeField] Animation _animation;
    [SerializeField] RectTransform _rectTransform;

    public void Press()
    {
        _animation.Stop();
        _animation.Play("Press");
    }

    public void Tap(float x, float y)
    {
        _animation.Stop();
        _rectTransform.anchoredPosition = new Vector2(x, y);
        _animation.Play("TapNoLoop");
    }

    public void Release()
    {
        _animation.Stop();
        _animation.Play("Release");
    }
}
