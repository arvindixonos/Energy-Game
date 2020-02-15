using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnableMover : MonoBehaviour
{
    Tween moveTween;
    public  Vector3 targetPosition;
    public  float   duration;

    private Vector3 startPosition;

    void OnEnable()
    {
        if (moveTween == null)
        {
            startPosition = transform.localPosition;
            transform.DOLocalMove(targetPosition, duration);
        }
        else
            moveTween.PlayForward();
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localPosition = startPosition;
    }
}
