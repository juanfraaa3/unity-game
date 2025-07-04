using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveRun : Objective
    {
        float m_RunTimer = 0f;
        float m_RequiredRunTime = 5f;
        bool m_Jumped = false;

        protected override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Title))
                Title = "Corre y salta";

            if (string.IsNullOrEmpty(Description))
                Description = "Corre durante 5 segundos y salta al menos una vez";
        }

        void Update()
        {
            if (IsCompleted)
                return;

            // Detectar si está corriendo
            if (Input.GetButton("Sprint"))
            {
                m_RunTimer += Time.deltaTime;

                // Detectar si saltó (con botón de salto configurado)
                if (Input.GetButtonDown("Jump"))
                {
                    m_Jumped = true;
                }

                // Si cumplió ambos requisitos
                if (m_RunTimer >= m_RequiredRunTime && m_Jumped)
                {
                    CompleteObjective(string.Empty, string.Empty, "¡Objetivo completado!");
                }
            }
            else
            {
                // Si deja de correr, reinicia todo
                m_RunTimer = 0f;
                m_Jumped = false;
            }
        }
    }
}
