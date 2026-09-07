using UnityEngine;

namespace MirrorDash
{
    // main entry point - just drop this on an empty GameObject and hit play.
    // builds the camera, arena, both characters, spawner and UI at runtime.
    [DisallowMultipleComponent]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Arena")]
        public Vector2 arenaSize = new Vector2(16f, 9f);
        public float arenaBorderThickness = 0.15f;

        [Header("Characters")]
        public float playerMoveSpeed = 6f;
        public float characterRadius = 0.35f;
        public Color characterAColor = new Color(0.25f, 0.95f, 0.55f); // Phaser - green
        public Color characterBColor = new Color(0.85f, 0.35f, 0.95f); // Switcher - magenta

        [Header("Hazards")]
        public float hazardSpawnIntervalStart = 1.6f;
        public float hazardSpawnIntervalMin = 0.5f;
        public float difficultyRampDuration = 60f;
        public float hazardDriftSpeed = 2.5f;
        public float hazardRadius = 0.3f;
        public int pointsPerSwitchKill = 10;
        public Color phaseHazardColor = new Color(0.25f, 0.55f, 1f);   // A ignores, B dies
        public Color switchHazardColor = new Color(1f, 0.55f, 0.15f);  // B destroys, A dies

        [Header("Scoring")]
        public float survivalScorePerSecond = 2f;

        [Header("Visuals")]
        public Color backgroundColor = new Color(0.07f, 0.07f, 0.11f);
        public Color arenaBorderColor = new Color(0.85f, 0.85f, 0.9f);

        private GameObject runRoot;

        private void Awake()
        {
            // pin the frame rate so movement/drift speed is consistent between platforms
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            SetupCamera();
            StartNewRun();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.orthographicSize = (arenaSize.y / 2f) * 1.15f;
            cam.backgroundColor = backgroundColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // tears down the old run (if there is one) and builds a fresh one.
        // used for both the first start and restarting after game over.
        public void StartNewRun()
        {
            if (runRoot != null) Destroy(runRoot);
            runRoot = new GameObject("MirrorDash_Run");

            Vector2 arenaCenter = Vector2.zero;
            Vector2 arenaHalfSize = arenaSize / 2f;

            var gmGO = new GameObject("GameManager");
            gmGO.transform.SetParent(runRoot.transform);
            var gm = gmGO.AddComponent<GameManager>();
            gm.ArenaCenter = arenaCenter;
            gm.ArenaHalfSize = arenaHalfSize;
            gm.SurvivalScorePerSecond = survivalScorePerSecond;

            BuildArenaBorder(arenaHalfSize);

            // B's start position uses the same mirror formula as the runtime update
            Vector2 startA = arenaCenter + new Vector2(-arenaHalfSize.x * 0.5f, 0f);
            Vector2 startB = (2f * arenaCenter) - startA;

            GameObject charA = CreateCharacter("CharacterA_Phaser", characterAColor, startA);
            var playerController = charA.AddComponent<PlayerController>();
            playerController.moveSpeed = playerMoveSpeed;
            playerController.characterRadius = characterRadius;

            GameObject charB = CreateCharacter("CharacterB_Switcher", characterBColor, startB);
            playerController.mirrorTarget = charB.transform;

            gm.CharacterA = charA.transform;
            gm.CharacterB = charB.transform;

            var spawnerGO = new GameObject("HazardSpawner");
            spawnerGO.transform.SetParent(runRoot.transform);
            var spawner = spawnerGO.AddComponent<HazardSpawner>();
            spawner.spawnIntervalStart = hazardSpawnIntervalStart;
            spawner.spawnIntervalMin = hazardSpawnIntervalMin;
            spawner.difficultyRampDuration = difficultyRampDuration;
            spawner.hazardDriftSpeed = hazardDriftSpeed;
            spawner.hazardRadius = hazardRadius;
            spawner.pointsPerSwitchKill = pointsPerSwitchKill;
            spawner.phaseHazardColor = phaseHazardColor;
            spawner.switchHazardColor = switchHazardColor;
            spawner.arenaCenter = arenaCenter;
            spawner.arenaHalfSize = arenaHalfSize;

            var uiGO = new GameObject("GameUI");
            uiGO.transform.SetParent(runRoot.transform);
            var ui = uiGO.AddComponent<GameUI>();
            ui.bootstrap = this;
            ui.characterAColor = characterAColor;
            ui.characterBColor = characterBColor;
            ui.phaseHazardColor = phaseHazardColor;
            ui.switchHazardColor = switchHazardColor;
        }

        private GameObject CreateCharacter(string name, Color color, Vector2 startPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(runRoot.transform);
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * (characterRadius * 2f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrimitiveSprites.Circle;
            sr.color = color;
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            return go;
        }

        private void BuildArenaBorder(Vector2 halfSize)
        {
            var borderRoot = new GameObject("ArenaBorder");
            borderRoot.transform.SetParent(runRoot.transform);

            float t = arenaBorderThickness;
            CreateBorderSegment(borderRoot.transform, new Vector2(0f, halfSize.y), new Vector2(arenaSize.x + t, t));
            CreateBorderSegment(borderRoot.transform, new Vector2(0f, -halfSize.y), new Vector2(arenaSize.x + t, t));
            CreateBorderSegment(borderRoot.transform, new Vector2(-halfSize.x, 0f), new Vector2(t, arenaSize.y + t));
            CreateBorderSegment(borderRoot.transform, new Vector2(halfSize.x, 0f), new Vector2(t, arenaSize.y + t));
        }

        private void CreateBorderSegment(Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("BorderSegment");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = PrimitiveSprites.Square;
            sr.color = arenaBorderColor;
            sr.sortingOrder = 1;
        }
    }
}
