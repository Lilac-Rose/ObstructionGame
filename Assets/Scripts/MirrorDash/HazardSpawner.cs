using UnityEngine;

namespace MirrorDash
{
    // spawns hazards on a timer from random points on the arena edge.
    // spawn rate ramps up over difficultyRampDuration, from spawnIntervalStart down to spawnIntervalMin.
    public class HazardSpawner : MonoBehaviour
    {
        [Header("Spawn Timing")]
        public float spawnIntervalStart = 1.6f;
        public float spawnIntervalMin = 0.5f;
        public float difficultyRampDuration = 60f;

        [Header("Hazard Settings")]
        public float hazardDriftSpeed = 2.5f;
        public float hazardRadius = 0.3f;
        public int pointsPerSwitchKill = 10;
        public Color phaseHazardColor = new Color(0.25f, 0.55f, 1f);
        public Color switchHazardColor = new Color(1f, 0.55f, 0.15f);

        [HideInInspector] public Vector2 arenaCenter;
        [HideInInspector] public Vector2 arenaHalfSize;

        private float timer;
        private float elapsed;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver || !gm.IsStarted) return;

            elapsed += Time.deltaTime;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SpawnHazard();
                timer = CurrentSpawnInterval();
            }
        }

        private float CurrentSpawnInterval()
        {
            float t = Mathf.Clamp01(elapsed / difficultyRampDuration);
            return Mathf.Lerp(spawnIntervalStart, spawnIntervalMin, t);
        }

        private void SpawnHazard()
        {
            HazardType type = Random.value < 0.5f ? HazardType.Phase : HazardType.Switch;
            Vector2 spawnPos = RandomEdgePosition();

            var go = new GameObject(type == HazardType.Phase ? "PhaseHazard" : "SwitchHazard");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
            go.transform.localScale = Vector3.one * (hazardRadius * 2f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            if (type == HazardType.Phase)
            {
                sr.sprite = PrimitiveSprites.Circle;
                sr.color = phaseHazardColor;
                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }
            else
            {
                sr.sprite = PrimitiveSprites.Square;
                sr.color = switchHazardColor;
                var col = go.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = Vector2.one;
            }

            var hazard = go.AddComponent<Hazard>();
            hazard.type = type;
            hazard.driftSpeed = hazardDriftSpeed;
            hazard.targetCenter = arenaCenter;
            hazard.scoreValue = pointsPerSwitchKill;
        }

        private Vector2 RandomEdgePosition()
        {
            int side = Random.Range(0, 4); // 0=top, 1=right, 2=bottom, 3=left
            float x, y;
            switch (side)
            {
                case 0:
                    x = Random.Range(-arenaHalfSize.x, arenaHalfSize.x);
                    y = arenaHalfSize.y;
                    break;
                case 1:
                    x = arenaHalfSize.x;
                    y = Random.Range(-arenaHalfSize.y, arenaHalfSize.y);
                    break;
                case 2:
                    x = Random.Range(-arenaHalfSize.x, arenaHalfSize.x);
                    y = -arenaHalfSize.y;
                    break;
                default:
                    x = -arenaHalfSize.x;
                    y = Random.Range(-arenaHalfSize.y, arenaHalfSize.y);
                    break;
            }
            return arenaCenter + new Vector2(x, y);
        }
    }
}
