using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMgr : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject playerPrefab;
    GameObject playerRef;

    // Start is called before the first frame update
    void Awake()
    {
        playerRef = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
