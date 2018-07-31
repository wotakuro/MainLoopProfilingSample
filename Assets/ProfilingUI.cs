using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Profiling;
using UnityEngine.Experimental.PlayerLoop;
public class ProfilingUI : MonoBehaviour {
    Text text;

    private void Start()
    {
        text = this.GetComponent<Text>();
    }
    // Update is called once per frame
    void Update () {
        if (Time.frameCount % 60 == 0)
        {
            text.text = "ScriptRunBehaviourUpdate:" + (CustomPlayerLoop.GetProfilingTime<Update.ScriptRunBehaviourUpdate>() * 1000.0f)
                + "\nFinishFrameRendering:" + (CustomPlayerLoop.GetProfilingTime<PostLateUpdate.FinishFrameRendering>() * 1000.0f);
        }

		
	}
}
