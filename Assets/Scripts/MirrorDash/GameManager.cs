using UnityEngine;

namespace MirrorDash
{
    // holds the shared state for the current run (score, game over, arena bounds).
    // gets rebuilt from scratch by GameBootstrap every restart.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [HideInInspector] public Vector2 ArenaCenter;
        [HideInInspector] public Vector2 ArenaHalfSize;
        [HideInInspector] public Transform CharacterA;
        [HideInInspector] public Transform CharacterB;
        [HideInInspector] public float SurvivalScorePerSecond = 2f;

        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsStarted { get; private set; }

        private float scoreAccumulator;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (IsGameOver || !IsStarted) return;

            // score just for staying alive
            scoreAccumulator += SurvivalScorePerSecond * Time.deltaTime;
            if (scoreAccumulator >= 1f)
            {
                int whole = Mathf.FloorToInt(scoreAccumulator);
                Score += whole;
                scoreAccumulator -= whole;
            }
        }

        public void AddScore(int amount)
        {
            if (IsGameOver) return;
            Score += amount;
        }

        public void TriggerGameOver()
        {
            IsGameOver = true;
        }

        // called from GameUI once the player dismisses the start screen
        public void BeginRun()
        {
            IsStarted = true;
        }
    }
}
