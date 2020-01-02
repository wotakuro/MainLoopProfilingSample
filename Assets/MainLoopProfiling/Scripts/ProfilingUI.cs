using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Profiling;
using UnityEngine.Experimental.PlayerLoop;
using System.Text;

namespace UTJ
{
    public class ProfilingUI : MonoBehaviour
    {
        private const int MeterIdxScript = 0;
        private const int MeterIdxPhysics = 1;
        private const int MeterIdxAnimator = 2;
        private const int MeterIdxRendeing = 3;
        private const int MeterIdxOther = 4;


        public Text frameRateText;
        public ProfilingMeter mainThreadMeter;
        public ProfilingMeter renderThreadMeter;

        private StringBuilder stringBuilderBuffer = new StringBuilder();
        private Recorder recordCamerRender;

        private float expectedExecuteTime;

        private float sumExecuteTime = 0.0f;
        private float minExecuteTime = float.MaxValue;
        private float maxExecuteTime = 0.0f;
        private int sumCount = 0;
        private int currentStartSec = 0;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                CustomPlayerLoop.Init();
            }
        }

        private void Start()
        {
            SetCameraForPreCulling();
        }

        private void SetCameraForPreCulling()
        {
#if UNITY_ANDROID  && !UNITY_EDITOR
            if (!SystemInfo.graphicsMultiThreaded)
            {
                return;
            }
            var cameras = Camera.allCameras;
            foreach( var camera in cameras)
            {
                if( !camera.gameObject.GetComponent<PrecullingNotificate>())
                {
                    camera.gameObject.AddComponent<PrecullingNotificate>();
                }
            }
#endif 
        }

        private void GotoNextSec(int sec)
        {
            sumExecuteTime = 0.0f;
            minExecuteTime = float.MaxValue;
            maxExecuteTime = 0.0f; 
            sumCount = 0;
            currentStartSec = sec;
        }
        private void AppendExecuteTime(float tm)
        {
            sumExecuteTime += tm;
            minExecuteTime = Mathf.Min(tm,minExecuteTime);
            maxExecuteTime = Mathf.Max(tm,maxExecuteTime);
            ++sumCount;
        }

        // Update is called once per frame
        private void Update()
        {
            UpdateExpectedExecuteTime();
            UpdateMainThreadMeter();
#if DEBUG || DEVELOPMENT_BUILD 
            UpdateRenderThreadMeter();
#endif
            int sec = (int)Time.realtimeSinceStartup;

            float allTime = CustomPlayerLoop.GetLastExecuteTime();
            // for android multiThread
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SystemInfo.graphicsMultiThreaded)
            {
                float waitForGfxPresent = CustomPlayerLoop.GetGfxWaitForPresent();
                allTime -= waitForGfxPresent;
            }
#endif
            AppendExecuteTime(allTime);
            if ( this.currentStartSec != sec)
            {
                stringBuilderBuffer.Length = 0;
                stringBuilderBuffer.Append("FPS:").Append(this.sumCount).Append(" ");
                stringBuilderBuffer.Append("(Avg:")
                    .AddMsecFromSec(this.sumExecuteTime / (float)this.sumCount)
                    .Append("ms)\n");

                stringBuilderBuffer.Append("min-max:")
                    .AddMsecFromSec(this.minExecuteTime)
                    .Append("ms");

                stringBuilderBuffer.Append(" - ")
                    .AddMsecFromSec(this.maxExecuteTime)
                    .Append("ms");
                
                frameRateText.text = stringBuilderBuffer.ToString();
                this.GotoNextSec(sec);
            }
        }

        private void UpdateExpectedExecuteTime()
        {
            // set barFrame
            float targetFrame = (float)Application.targetFrameRate;
            if (targetFrame <= 0.0f)
            {
                targetFrame = 60.0f;
            }
            this.expectedExecuteTime = 1.0f / targetFrame;
        }
        private void UpdateMainThreadMeter()
        {
            float allTime = CustomPlayerLoop.GetLastExecuteTime();
            float scriptUpdateTime = CustomPlayerLoop.GetProfilingTime<Update.ScriptRunBehaviourUpdate>() +
                CustomPlayerLoop.GetProfilingTime<PreLateUpdate.ScriptRunBehaviourLateUpdate>() +
                CustomPlayerLoop.GetProfilingTime<FixedUpdate.ScriptRunBehaviourFixedUpdate>() +
                CustomPlayerLoop.GetProfilingTime<Update.ScriptRunDelayedDynamicFrameRate>();
            float animatorTime = CustomPlayerLoop.GetProfilingTime<PreLateUpdate.DirectorUpdateAnimationBegin>() +
                CustomPlayerLoop.GetProfilingTime<PreLateUpdate.DirectorUpdateAnimationEnd>();
            float renderTime = CustomPlayerLoop.GetProfilingTime<PostLateUpdate.FinishFrameRendering>();
            float physicsTime = CustomPlayerLoop.GetProfilingTime<FixedUpdate.PhysicsFixedUpdate>();
            // for android multiThread
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SystemInfo.graphicsMultiThreaded)
            {
                float waitForGfxPresent = CustomPlayerLoop.GetGfxWaitForPresent();
                renderTime -= waitForGfxPresent;
                allTime -= waitForGfxPresent;
            }
#endif
            float otherTime = allTime - scriptUpdateTime - animatorTime - renderTime - physicsTime;


            mainThreadMeter.SetParameter(MeterIdxScript, scriptUpdateTime / this.expectedExecuteTime);
            mainThreadMeter.SetParameter(MeterIdxAnimator, animatorTime / this.expectedExecuteTime);
            mainThreadMeter.SetParameter(MeterIdxRendeing, renderTime / this.expectedExecuteTime);
            mainThreadMeter.SetParameter(MeterIdxPhysics, physicsTime / this.expectedExecuteTime);
            mainThreadMeter.SetParameter(MeterIdxOther, otherTime / this.expectedExecuteTime);
        }

#if DEBUG || DEVELOPMENT_BUILD 
        private void UpdateRenderThreadMeter()
        {
            if(!SystemInfo.graphicsMultiThreaded)
            {
                return;
            }
            if(recordCamerRender == null)
            {
                recordCamerRender = Recorder.Get("Camera.Render");
            }
            float cameraRenderTime = recordCamerRender.elapsedNanoseconds * 0.000000001f;
            float mainThreadTime = CustomPlayerLoop.GetProfilingTime<PostLateUpdate.FinishFrameRendering>();
#if UNITY_ANDROID && !UNITY_EDITOR
            float waitForGfxPresent = CustomPlayerLoop.GetGfxWaitForPresent();
            mainThreadTime -= waitForGfxPresent;
#endif
            float renderThreadTime = cameraRenderTime - mainThreadTime;
            renderThreadMeter.SetParameter(0, renderThreadTime / this.expectedExecuteTime);
        }
#endif

    }

    public static class StringBuilderExtention
    {
        public static StringBuilder AddMsecFromSec(this StringBuilder sb, float tm)
        {
            const int num = 4;
            int output = (int)(tm * 1000.0f * GetPow10(num));

            sb.Append(output / GetPow10(num));
            sb.Append(".");
            for (int i = 1; i < num; ++i)
            {
                int tmp = output / GetPow10(num - i) % 10;
                sb.Append(tmp);
            }
            return sb;
        }

        public static StringBuilder AddMsecFromNanosec(this StringBuilder sb, long nanosec)
        {
            float sec = (nanosec / 1000000000.0f);

            return sb.AddMsecFromSec(sec);
        }
        private static int GetPow10(int p)
        {
            int param = 1;
            for (int i = 0; i < p; ++i)
            {
                param *= 10;
            }
            return param;
        }
    }
}
