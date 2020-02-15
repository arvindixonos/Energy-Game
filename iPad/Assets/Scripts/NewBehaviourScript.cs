using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        print(Random.Range(1, 4));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
