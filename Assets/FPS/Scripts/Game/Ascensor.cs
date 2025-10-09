using Unity.FPS.Game;   // para Health y EventManager
using UnityEngine;
using System.Collections;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveElevatorPoints : MonoBehaviour
    {
        [Header("Platform Setup")]
        public Transform PlatformToMove;
        public Transform PointA; // abajo
        public Transform PointB; // arriba
        public float MoveDuration = 1.0f;
        public AnimationCurve Easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool PlaceAtAOnPlay = true;

        [Header("Player Setup")]
        public Health PlayerHealth; // arrastra aquí el Health del jugador

        [Header("Kill Zone Setup")]
        public GameObject KillZone;
        public float KillZoneActivationDelay = 0.75f;

        bool _movingUp;
        bool _movingDown;
        float _time;
        bool _alreadyUsed;   // 👈 si quieres que solo suba una vez
        bool _canGoDown;     // 👈 nuevo: habilita bajar cuando acaben las waves

        // 👇 propiedad pública por si la necesita un trigger externo
        public bool HasAlreadyUsed => _alreadyUsed;

        void Start()
        {
            if (PlatformToMove == null) PlatformToMove = transform;

            if (PlaceAtAOnPlay && PointA != null)
                PlatformToMove.position = PointA.position;

            if (KillZone != null)
                KillZone.SetActive(false);

            if (PlayerHealth != null)
                PlayerHealth.OnDie += OnPlayerDeath;

            // 👉 escuchar evento de WaveManager
            EventManager.AddListener<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        void OnDestroy()
        {
            if (PlayerHealth != null)
                PlayerHealth.OnDie -= OnPlayerDeath;

            EventManager.RemoveListener<AllWavesCompletedEvent>(OnAllWavesCompleted);
        }

        void OnPlayerDeath()
        {
            ResetElevatorAndKillZone();
        }

        void OnAllWavesCompleted(AllWavesCompletedEvent evt)
        {
            Debug.Log("✔ Todas las waves completadas → ascensor puede bajar");
            _canGoDown = true;
        }

        // ===== API pública =====
        public void StartMoveUp()
        {
            if (!_alreadyUsed && PointA != null && PointB != null)
            {
                _alreadyUsed = true;
                _movingUp = true;
                _movingDown = false;
                _time = 0f;
            }
        }

        public void StartMoveDown()
        {
            if (_canGoDown && PointA != null && PointB != null)
            {
                _movingDown = true;
                _movingUp = false;
                _time = 0f;

                if (KillZone != null)
                {
                    KillZone.SetActive(false);
                    Debug.Log("KillZone DESACTIVADA al comenzar descenso");
                }
            }
            else
            {
                Debug.Log("⛔ Intento de bajar, pero aún no se puede (waves no completadas)");
            }
        }

        void Update()
        {
            if (_movingUp && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);
                PlatformToMove.position = Vector3.Lerp(PointA.position, PointB.position, e);

                if (t >= 1f)
                {
                    _movingUp = false;
                    _time = 0f;
                    OnReachedTop();
                }
            }
            else if (_movingDown && PointA != null && PointB != null)
            {
                _time += Time.deltaTime;
                float t = Mathf.Clamp01(_time / MoveDuration);
                float e = Easing.Evaluate(t);
                PlatformToMove.position = Vector3.Lerp(PointB.position, PointA.position, e);

                if (t >= 1f)
                {
                    _movingDown = false;
                    _time = 0f;
                    Debug.Log("✔ Ascensor volvió a bajar");
                }
            }
        }

        // ===== Lógica KillZone =====
        void OnReachedTop()
        {
            if (KillZone != null)
                StartCoroutine(EnableKillZoneAfterDelay());

            //Avisar cuando llegue arriba
            EventManager.Broadcast(new ElevatorReachedTopEvent());
            Debug.Log("📢 Evento lanzado: Ascensor llegó al punto B");
        }

        IEnumerator EnableKillZoneAfterDelay()
        {
            yield return new WaitForSeconds(KillZoneActivationDelay);
            if (KillZone != null)
            {
                KillZone.SetActive(true);
                Debug.Log("KillZone ACTIVADA");
            }
        }

        public void ResetElevatorAndKillZone()
        {
            if (PlatformToMove != null && PointA != null)
                PlatformToMove.position = PointA.position;

            _movingUp = _movingDown = false;
            _time = 0f;

            if (KillZone != null)
                KillZone.SetActive(false);

            _alreadyUsed = false;
            _canGoDown = false;   // 👈 reset también bloquea el bajar

            Debug.Log("Ascensor reseteado y KillZone desactivada");
        }
    }
}
