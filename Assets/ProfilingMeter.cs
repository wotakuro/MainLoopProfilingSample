using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilingMeter : MonoBehaviour {

    public RectTransform[] meters;
    private int meterLength;
    private float[] parameters;

	// Use this for initialization
	void Start () {
        meterLength = meters.Length;
        parameters = new float[meterLength];
    }
    private void Update()
    {
        float sum = 0.0f;
        for(int i = 0; i < meterLength; ++i)
        {
            meters[i].sizeDelta = new Vector2( parameters[i] * 100.0f , meters[i].sizeDelta.y);
            meters[i].anchoredPosition = new Vector2(sum * 100.0f, meters[i].anchoredPosition.y);
            sum += parameters[i];
        }
    }

    public void SetParameter(int idx , float param)
    {
        this.parameters[idx] = param;
    }
}
