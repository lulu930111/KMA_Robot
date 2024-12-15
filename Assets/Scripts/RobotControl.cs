using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotControl : MonoBehaviour
{
    public Transform target;
    public float speed = 0.5f;
    public CanvasGroup systemCanvas;
    void Start()
    {
        
    }

    void Update()
    {
        if(systemCanvas.alpha > 0){
            return;
        }
        if(Input.GetKey(KeyCode.W)){
            target.position += new Vector3(0, speed * Time.deltaTime, 0);
        }
        if(Input.GetKey(KeyCode.S)){
            target.position -= new Vector3(0, speed * Time.deltaTime, 0);
        }   
        if(Input.GetKey(KeyCode.A)){
            target.position -= new Vector3(speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.D)){
            target.position += new Vector3(speed * Time.deltaTime, 0, 0);
        }
        if(Input.GetKey(KeyCode.Q)){
            target.position += new Vector3(0, 0, speed * Time.deltaTime);
        }
        if(Input.GetKey(KeyCode.E)){
            target.position -= new Vector3(0, 0, speed * Time.deltaTime);
        }
    }
}
