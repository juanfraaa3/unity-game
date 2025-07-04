using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillSpecificEnemies : Objective
    {
        [Tooltip("Lista de enemigos que deben morir para completar el objetivo")]
        public List<GameObject> EnemiesToKill;

        private int m_KillCount = 0;

        protected override void Start()
        {
            base.Start();

            if (EnemiesToKill == null || EnemiesToKill.Count == 0)
            {
                Debug.LogWarning("No se han asignado enemigos al objetivo.");
                return;
            }

            foreach (GameObject enemy in EnemiesToKill)
            {
                if (enemy != null)
                {
                    Health health = enemy.GetComponentInChildren<Health>();
                    if (health != null)
                    {
                        health.OnDie += () => OnEnemyKilled(enemy);
                    }
                    else
                    {
                        Debug.LogWarning("El enemigo no tiene componente Health: " + enemy.name);
                    }
                }
            }

            // Texto inicial
            if (string.IsNullOrEmpty(Title))
                Title = "Elimina las torretas finales";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounter();
        }

        void OnEnemyKilled(GameObject enemy)
        {
            if (EnemiesToKill.Contains(enemy))
            {
                m_KillCount++;
                UpdateObjective("", GetUpdatedCounter(), "Torretas destruidas: " + m_KillCount + "/" + EnemiesToKill.Count);

                if (m_KillCount >= EnemiesToKill.Count)
                {
                    CompleteObjective("", GetUpdatedCounter(), "¡Has destruido todas las torretas!");
                }
            }
        }

        string GetUpdatedCounter()
        {
            return m_KillCount + " / " + EnemiesToKill.Count;
        }
    }
}
