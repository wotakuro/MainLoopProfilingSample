using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UTJ
{

    public class PrecullingNotificate : MonoBehaviour
    {
        void OnPreCull()
        {
            CustomPlayerLoop.OnPreCulling();
        }
    }

}