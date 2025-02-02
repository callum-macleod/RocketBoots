using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ResetOnCollision : MonoBehaviour
{
    SceneMgr sceneMgr;
    void Awake()
    {
        sceneMgr = GameObject.Find("SceneMgr").GetComponent<SceneMgr>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject.layer == (int)Layers.Player)
            sceneMgr.ResetPlayer();
    }
}
