using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPref;
    [SerializeField] private GameObject deathEffect;

    [SerializeField] private float respawnTime = 5f;

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


    public void Die(string damager)
    {
        UIController.instance.deathText.text = $"You were killed by {damager}";

        //PhotonNetwork.Destroy(_player);
        //SpawnPlayer();
        
        if(_player != null)
        {
            StartCoroutine(DieCo());
        }
    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, _player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(_player);

        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);

        SpawnPlayer();
    }

}
