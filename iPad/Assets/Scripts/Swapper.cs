using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swapper : MonoBehaviour
{
    public RectTransform left;
    public RectTransform right;

    public RectTransform leftAra;
    public RectTransform rightAra;

    public bool toSwap;

    void OnEnable()
    {
        //left = transform.GetChild(0).GetChild(1).GetComponent<RectTransform>();
        //right = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        //leftAra = transform.GetChild(1).GetChild(1).GetComponent<RectTransform>();
        //rightAra = transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();

        Swap();
    }

    void OnDisable ()
    {
        Swap();
        toSwap = false;
    }

    public void Swap ()
    {
        if (!toSwap)
            return;
        float leftX = left.anchoredPosition.x;

        left.anchoredPosition = new Vector2(right.anchoredPosition.x, right.anchoredPosition.y);
        right.anchoredPosition = new Vector2(leftX, left.anchoredPosition.y);

        leftAra.anchoredPosition = new Vector2(rightAra.anchoredPosition.x, rightAra.anchoredPosition.y);
        rightAra.anchoredPosition = new Vector2(leftX, leftAra.anchoredPosition.y);
    }
}
