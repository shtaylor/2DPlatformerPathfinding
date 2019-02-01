using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingDixieSpawner : DixieSpawner
{
    

    protected override void Awake()
    {
        // We don't want the FollowingSpawner to spawn as often
        timeBetweenSpawnWave *= 1.5f;
        base.Awake();
    }

    

    protected override IEnumerator Spawn()
    {
        if (player != null)
        {
            transform.position = player.transform.position;
        }
        float respawnTime = Random.Range(.5f, timeBetweenSpawnWave);
        yield return new WaitForSeconds(respawnTime);
        if (numberOfDixies < maxNumberOfDixies 
            && Vector2.Distance(transform.position, player.transform.position) <= maxDistanceToPlayerToEnableSpawn)
        {
            
            GameObject effect = Instantiate<GameObject>(SpawnEffect, EffectLocation.position, Quaternion.identity, DixieParent);
            yield return new WaitForSeconds(SpawnEffectDuration * .8f);
            Instantiate<GameObject>(DixiePrefab, transform.position, Quaternion.identity, DixieParent);
            numberOfDixies++;
            yield return new WaitForSeconds(SpawnEffectDuration * .2f);
            Destroy(effect);
        }
        StartCoroutine(Spawn());
    }
}
