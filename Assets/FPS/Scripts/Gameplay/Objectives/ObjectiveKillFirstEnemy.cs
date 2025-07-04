using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillSpecificEnemy : Objective
    {
        // Enemigo específico a eliminar
        public GameObject EnemyToKill;

        // Siguiente objetivo a activar
        public GameObject NextObjective;

        private bool m_EnemyKilled = false;

        protected override void Start()
        {
            base.Start();

            // Asegura que el objetivo se complete cuando el enemigo sea destruido
            if (EnemyToKill != null)
            {
                EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
            }

            if (string.IsNullOrEmpty(Title))
                Title = "Eliminar al enemigo específico";

            if (string.IsNullOrEmpty(Description))
                Description = "Elimina al primer enemigo para continuar el entrenamiento.";
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted)
                return;

            // Comprobamos si el enemigo muerto es el enemigo específico
            if (evt.Enemy == EnemyToKill)
            {
                m_EnemyKilled = true;
            }

            // Si el enemigo ha sido eliminado, completar objetivo
            if (m_EnemyKilled)
            {
                CompleteObjective(string.Empty, string.Empty, "¡Objetivo completado! Enemigo eliminado.");

                // Activar el siguiente objetivo
                if (NextObjective != null)
                {
                    NextObjective.SetActive(true);
                }

                // Elimina el listener para evitar que sigamos escuchando la muerte de enemigos
                EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
            }
        }

        void OnDestroy()
        {
            // Elimina el listener en caso de que se destruya el objeto sin completar el objetivo
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}
