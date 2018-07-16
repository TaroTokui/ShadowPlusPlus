using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMotion : MonoBehaviour {

    [SerializeField]
    private float _MotionAmptilude = 1.0f;

    [SerializeField]
    private float _MotionSpeed = 1.0f;

    private Vector3 base_pos;

    // Use this for initialization
    void Start () {
        base_pos = gameObject.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        float x = Mathf.Sin(Time.realtimeSinceStartup * _MotionSpeed) * _MotionAmptilude;
        var pos = new Vector3(x, 0.0f, 0.0f);
        gameObject.transform.position = base_pos + pos;
	}
}
