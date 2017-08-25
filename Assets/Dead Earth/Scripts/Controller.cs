using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    private Animator _animator = null;
    private int _hHash = 0;
    private int _vHash = 0;
    private int _aHash = 0;
	// Use this for initialization
	void Start () {
        _animator = GetComponent<Animator>();
        _hHash = Animator.StringToHash("Horizontal");
        _vHash = Animator.StringToHash("Vertical");
        _aHash = Animator.StringToHash("Attack");
    }
	
	// Update is called once per frame
	void Update () {
        float xAxis = Input.GetAxis("Horizontal") * 2.32f;
        float yAxis = Input.GetAxis("Vertical") * 5.66f;

        if (Input.GetMouseButtonDown(0)) _animator.SetTrigger(_aHash);

        _animator.SetFloat(_hHash, xAxis, 0.30f, Time.deltaTime);
        _animator.SetFloat(_vHash, yAxis, 1.0f, Time.deltaTime);
	}
}
