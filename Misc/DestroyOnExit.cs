using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExit : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            Destroy(other.gameObject);
        }
    }
}
