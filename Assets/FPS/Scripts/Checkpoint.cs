using UnityEngine;
using Unity.FPS.Game;


public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(transform.position);

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.RegisterCheckpoint(gameObject.name);
            }

            //Debug.Log("Checkpoint activado en: " + transform.position);
        }
    }

}
