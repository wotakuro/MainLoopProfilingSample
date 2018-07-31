using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;
using UnityEngine.Profiling;
using UnityEngine.Experimental.PlayerLoop;


// プレイヤーループのカスタマイズ
// 参考URL https://www.patreon.com/posts/unity-2018-1-16336053
public class CustomPlayerLoop {

    public class ProfilingUpdate
    {
        private float startTime;
        private float executeTime;
        public void Start()
        {
            this.startTime = Time.realtimeSinceStartup;
        }
        public void End()
        {
            executeTime = Time.realtimeSinceStartup - this.startTime;
        }
        public float GetExecuteTime()
        {
            return executeTime;
        }
    }
    private static Dictionary<System.Type, ProfilingUpdate> profilingSubSystem;


    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        System.Type[] profilePoints = {
            typeof(PostLateUpdate.FinishFrameRendering),
            typeof(Update.ScriptRunBehaviourUpdate),
            typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
        };
        var playerLoop = PlayerLoop.GetDefaultPlayerLoop();
        AppendProfilingLoopSystem(ref playerLoop, profilePoints);
        PlayerLoop.SetPlayerLoop(playerLoop);
    }
    public static float GetProfilingTime<T>()
    {
        return GetProfilingTime(typeof(T));
    }
    public static float GetProfilingTime(System.Type t)
    {
        ProfilingUpdate obj = null;
        if( profilingSubSystem.TryGetValue(t, out obj))
        {
            return obj.GetExecuteTime();
        }
        return 0.0f;
    }

    private static void AppendProfilingLoopSystem(ref PlayerLoopSystem playerLoop,System.Type[] profilePoints)
    {
        profilingSubSystem = new Dictionary<System.Type, ProfilingUpdate>();
        // プロファイル対象のシステムを指定します
        for( int i = 0; i < profilePoints.Length; ++i)
        {
            profilingSubSystem.Add(profilePoints[i], new ProfilingUpdate());

        }
        // Note: this also resets the loop to its defalt state first.        
        var newSystems = new List<PlayerLoopSystem>();
        for (int i = 0; i < playerLoop.subSystemList.Length; ++i)
        {
            var subSystem = playerLoop.subSystemList[i];

            newSystems.Clear();
            for (int j = 0; j < subSystem.subSystemList.Length; ++j)
            {
                var subsub = subSystem.subSystemList[j];
                ProfilingUpdate updateObj;
                if (profilingSubSystem.TryGetValue(subsub.type, out updateObj))
                {
                    // 実際のSystemとの間で挟みます
                    newSystems.Add(new PlayerLoopSystem
                    {
                        type = typeof(ProfilingUpdate),
                        updateDelegate = updateObj.Start
                    });
                    newSystems.Add(subsub);
                    newSystems.Add(new PlayerLoopSystem
                    {
                        type = typeof(ProfilingUpdate),
                        updateDelegate = updateObj.End
                    });
                }
                else
                {
                    newSystems.Add(subsub);
                }
            }
            subSystem.subSystemList = newSystems.ToArray();
            playerLoop.subSystemList[i] = subSystem;
        }
    }
}
