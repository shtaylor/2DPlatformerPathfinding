using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DixieSpawner : MonoBehaviour
{

    public static int numberOfDixies = 0;

    public GameObject DixiePrefab;
    public GameObject SpawnEffect;
    public Transform EffectLocation;
    public int maxNumberOfDixies = 10;
    public float SpawnEffectDuration = .796f;

    public float maxDistanceToPlayerToEnableSpawn = 80;

    public Transform DixieParent;

    public float timeBetweenSpawnWave = 5;

    public GameObject player;

    protected virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        maxNumberOfDixies = 10;
        if (DixiePrefab == null)
        {
            return;
        }
        Destroy(GetComponent<MeshRenderer>());
        StartCoroutine(Spawn());
    }

    private void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    protected virtual IEnumerator Spawn()
    {
        if (player == null)
        {
            yield return null;
        }
        else
        {
            Vector3 playerPos = player.transform.position;
            float respawnTime = Random.Range(1.5f, timeBetweenSpawnWave);
            yield return new WaitForSeconds(respawnTime);
            if (numberOfDixies < maxNumberOfDixies
                && Vector2.Distance(transform.position, playerPos) <= maxDistanceToPlayerToEnableSpawn)
            {
                GameObject effect = Instantiate<GameObject>(SpawnEffect, EffectLocation.position, Quaternion.identity, DixieParent);
                yield return new WaitForSeconds(SpawnEffectDuration * .8f);
                Instantiate<GameObject>(DixiePrefab, transform.position, Quaternion.identity, DixieParent);
                numberOfDixies++;
                yield return new WaitForSeconds(SpawnEffectDuration * .2f);
                Destroy(effect);
            }
        }
        StartCoroutine(Spawn());
    }
}
