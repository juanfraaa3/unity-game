using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    Health m_Health;
    CharacterController m_Controller;

    void Start()
    {
        m_Health = GetComponent<Health>();
        m_Controller = GetComponent<CharacterController>();

        // Guardar posición inicial como primer checkpoint
        CheckpointManager.Instance.SetCheckpoint(transform.position);

        if (m_Health != null)
            m_Health.OnDie += RespawnAtCheckpoint;
    }

    void Update()
    {
        // Matar al jugador si cae fuera del mapa
        if (transform.position.y < -100f && m_Health.CurrentHealth > 0)
        {
            m_Health.Kill();
        }
    }

    void RespawnAtCheckpoint()
    {
        if (m_Controller != null)
            m_Controller.enabled = false;

        // Mover al checkpoint, con leve elevación para evitar bugs de colisión
        transform.position = CheckpointManager.Instance.GetCheckpoint() + Vector3.up * 1f;

        if (m_Controller != null)
            m_Controller.enabled = true;

        // Reactivar el arma
        var weaponsManager = GetComponent<PlayerWeaponsManager>();
        if (weaponsManager != null)
        {
            weaponsManager.SwitchToWeaponIndex(weaponsManager.ActiveWeaponIndex);

            // Desactivar el apuntado (IsAiming) después de revivir
            weaponsManager.SetAiming(false);  // Usamos el método SetAiming para desactivar el apuntado
        }

        // Reactivar HUD si está desactivado
        GameObject hud = GameObject.Find("PlayerHUD");
        if (hud != null)
        {
            hud.SetActive(true);
        }

        // Resetear el estado de muerte
        typeof(Health)
            .GetField("m_IsDead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(m_Health, false);

        // Reactivar el arma si está desactivada
        Transform weaponParent = transform.Find("Main Camera/FirstPersonSocket/WeaponParentSocket");
        if (weaponParent != null && weaponParent.childCount > 0)
        {
            Transform weapon = weaponParent.GetChild(0);
            if (weapon != null && !weapon.gameObject.activeSelf)
            {
                weapon.gameObject.SetActive(true);
            }
        }

        // Volver a suscribirse a OnDie (por seguridad)
        m_Health.OnDie -= RespawnAtCheckpoint;
        m_Health.OnDie += RespawnAtCheckpoint;

        // Restaurar salud
        m_Health.Heal(m_Health.MaxHealth);

        // Resetear las animaciones de la cámara y el arma (si está en ADS)
        ResetWeaponAndCamera();
    }

    void ResetWeaponAndCamera()
    {
        // Si existe el 'PlayerWeaponsManager' y el 'WeaponController', reseteamos el FOV
        var weaponsManager = GetComponent<PlayerWeaponsManager>();
        if (weaponsManager != null)
        {
            weaponsManager.SetAiming(false); // Desactivamos el estado de apuntado
        }

        // Aquí puedes añadir código para resetear cualquier animación, FOV o estado visual del arma
        var playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = 60f;  // Restaurar el FOV al valor por defecto
        }
    }
}
