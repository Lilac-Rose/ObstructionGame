using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MirrorDash
{
    // builds the score/start/game-over UI at runtime and handles the
    // "press any key to start" / "press R to restart" input.
    // restart just calls back into GameBootstrap instead of reloading the scene.
    public class GameUI : MonoBehaviour
    {
        [HideInInspector] public GameBootstrap bootstrap;

        // set by GameBootstrap so the instructions text matches the actual hazard colors
        [HideInInspector] public Color characterAColor;
        [HideInInspector] public Color characterBColor;
        [HideInInspector] public Color phaseHazardColor;
        [HideInInspector] public Color switchHazardColor;

        private Text scoreText;
        private GameObject startPanel;
        private GameObject gameOverPanel;
        private Text gameOverText;

        private void Start()
        {
            // using Start() instead of Awake() so GameBootstrap has time to set the colors above first
            BuildCanvas();
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            scoreText.text = $"Score: {gm.Score}";

            if (!gm.IsStarted)
            {
                var kb = Keyboard.current;
                if (kb != null && kb.anyKey.wasPressedThisFrame)
                {
                    gm.BeginRun();
                    startPanel.SetActive(false);
                }
                return;
            }

            if (gm.IsGameOver)
            {
                if (!gameOverPanel.activeSelf)
                {
                    gameOverText.text = $"Game Over — Score: {gm.Score}\nPress R to Restart";
                    gameOverPanel.SetActive(true);
                }

                var kb = Keyboard.current;
                if (kb != null && kb.rKey.wasPressedThisFrame && bootstrap != null)
                {
                    bootstrap.StartNewRun();
                }
            }
        }

        private void BuildCanvas()
        {
            var canvasGO = new GameObject("GameCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);

            canvasGO.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // score in the corner
            var scoreGO = new GameObject("ScoreText");
            scoreGO.transform.SetParent(canvasGO.transform, false);
            scoreText = scoreGO.AddComponent<Text>();
            scoreText.font = font;
            scoreText.fontSize = 32;
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.UpperLeft;
            scoreText.text = "Score: 0";
            RectTransform scoreRT = scoreText.rectTransform;
            scoreRT.anchorMin = new Vector2(0f, 1f);
            scoreRT.anchorMax = new Vector2(0f, 1f);
            scoreRT.pivot = new Vector2(0f, 1f);
            scoreRT.anchoredPosition = new Vector2(20f, -20f);
            scoreRT.sizeDelta = new Vector2(400f, 60f);

            BuildStartPanel(canvasGO.transform, font);
            BuildGameOverPanel(canvasGO.transform, font);
        }

        private void BuildStartPanel(Transform canvasTransform, Font font)
        {
            startPanel = new GameObject("StartPanel");
            startPanel.transform.SetParent(canvasTransform, false);
            var panelImage = startPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);
            RectTransform panelRT = panelImage.rectTransform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            string aHex = ColorUtility.ToHtmlStringRGB(characterAColor);
            string bHex = ColorUtility.ToHtmlStringRGB(characterBColor);
            string phaseHex = ColorUtility.ToHtmlStringRGB(phaseHazardColor);
            string switchHex = ColorUtility.ToHtmlStringRGB(switchHazardColor);

            string instructions =
                "MIRROR DASH\n\n" +
                $"WASD / Arrow Keys — move the <color=#{aHex}>Phaser</color>\n" +
                $"The <color=#{bHex}>Switcher</color> mirrors your every move through the arena's center\n\n" +
                $"<color=#{phaseHex}>Blue hazards</color> — harmless to you, deadly to your mirror\n" +
                $"<color=#{switchHex}>Orange hazards</color> — your mirror destroys them for points, deadly to you\n\n" +
                "Survive as long as you can!\n\n" +
                "Press any key to start";

            var textGO = new GameObject("StartText");
            textGO.transform.SetParent(startPanel.transform, false);
            var startText = textGO.AddComponent<Text>();
            startText.font = font;
            startText.fontSize = 28;
            startText.color = Color.white;
            startText.alignment = TextAnchor.MiddleCenter;
            startText.supportRichText = true;
            startText.text = instructions;
            RectTransform textRT = startText.rectTransform;
            textRT.anchorMin = new Vector2(0.1f, 0.1f);
            textRT.anchorMax = new Vector2(0.9f, 0.9f);
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
        }

        private void BuildGameOverPanel(Transform canvasTransform, Font font)
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvasTransform, false);
            var panelImage = gameOverPanel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.75f);
            RectTransform panelRT = panelImage.rectTransform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            var goTextGO = new GameObject("GameOverText");
            goTextGO.transform.SetParent(gameOverPanel.transform, false);
            gameOverText = goTextGO.AddComponent<Text>();
            gameOverText.font = font;
            gameOverText.fontSize = 40;
            gameOverText.color = Color.white;
            gameOverText.alignment = TextAnchor.MiddleCenter;
            RectTransform goRT = gameOverText.rectTransform;
            goRT.anchorMin = Vector2.zero;
            goRT.anchorMax = Vector2.one;
            goRT.offsetMin = Vector2.zero;
            goRT.offsetMax = Vector2.zero;

            gameOverPanel.SetActive(false);
        }
    }
}
