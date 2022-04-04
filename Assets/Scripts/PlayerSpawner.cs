using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPref;
    public static PlayerSpawner instance;

    private GameObject _player;

    private void Awake() {
        instance = this;
    }


    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }


    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
        _player = PhotonNetwork.Instantiate(playerPref.name, spawnPoint.position, spawnPoint.rotation);
    }

}
