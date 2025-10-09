using UnityEngine;
using Unity.FPS.Game;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillSpecificEnemyType_FromSpawners : Objective
    {
        [Header("Configuración")]
        [Tooltip("Nombre parcial o tag del tipo de enemigo a eliminar (por ejemplo, 'Turret' o 'EnemyTurret')")]
        public string EnemyIdentifier = "Turret";

        [Tooltip("Cantidad total de enemigos de este tipo que deben eliminarse para completar el objetivo")]
        public int EnemiesToKillCount = 3;

        [Tooltip("Siguiente objetivo a activar tras completar este (opcional)")]
        public GameObject NextObjective;

        private int m_KillCount = 0;
        private bool m_ObjectiveCompleted = false;

        protected override void Start()
        {
            base.Start();

            // Suscribirse a los eventos globales
            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.AddListener<PlayerDeathEvent>(OnPlayerDied); // 👈 Nuevo

            if (string.IsNullOrEmpty(Title))
                Title = "Elimina las torretas finales";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounter();
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (m_ObjectiveCompleted)
                return;

            if (evt.Enemy == null)
                return;

            // Verificamos si el enemigo muerto coincide con el tipo buscado
            bool isMatch =
                evt.Enemy.name.Contains(EnemyIdentifier) ||
                evt.Enemy.CompareTag(EnemyIdentifier);

            if (isMatch)
            {
                m_KillCount++;
                UpdateObjective("", GetUpdatedCounter(), "Torretas destruidas: " + m_KillCount + "/" + EnemiesToKillCount);

                if (m_KillCount >= EnemiesToKillCount)
                {
                    CompleteObjective("", GetUpdatedCounter(), "¡Has destruido todas las torretas!");
                    m_ObjectiveCompleted = true;

                    if (NextObjective != null)
                        NextObjective.SetActive(true);

                    CleanupListeners();
                }
            }
        }

        // 👇 Nuevo: si el jugador muere, se reinicia el progreso
        void OnPlayerDied(PlayerDeathEvent evt)
        {
            if (m_ObjectiveCompleted)
                return;

            m_KillCount = 0;
            UpdateObjective("", GetUpdatedCounter(), "Has muerto. Se reinicia el conteo de torretas.");
        }

        string GetUpdatedCounter()
        {
            return m_KillCount + " / " + EnemiesToKillCount;
        }

        void CleanupListeners()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDied);
        }

        void OnDestroy()
        {
            CleanupListeners();
        }
    }
}
