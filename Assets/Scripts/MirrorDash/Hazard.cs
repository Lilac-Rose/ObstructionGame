using UnityEngine;

namespace MirrorDash
{
    public enum HazardType
    {
        Phase,  // blue - A passes through harmlessly, kills B
        Switch  // orange - B destroys it for points, kills A
    }

    // one hazard - drifts toward the center, win/lose rules depend on its type
    [RequireComponent(typeof(Rigidbody2D))]
    public class Hazard : MonoBehaviour
    {
        public HazardType type;

        [HideInInspector] public float driftSpeed = 2.5f;
        [HideInInspector] public Vector2 targetCenter;
        [HideInInspector] public int scoreValue = 10;

        private void Update()
        {
            Vector2 pos = transform.position;
            Vector2 toCenter = targetCenter - pos;
            float dist = toCenter.magnitude;
            if (dist < 0.1f)
            {
                // despawn instead of just sitting at the center forever
                Destroy(gameObject);
                return;
            }

            Vector2 step = (toCenter / dist) * driftSpeed * Time.deltaTime;
            transform.position = pos + step;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver) return;

            bool isCharacterA = other.transform == gm.CharacterA;
            bool isCharacterB = other.transform == gm.CharacterB;
            if (!isCharacterA && !isCharacterB) return;

            if (type == HazardType.Phase)
            {
                // A just passes through, only B can die to these
                if (isCharacterB) gm.TriggerGameOver();
            }
            else // HazardType.Switch
            {
                if (isCharacterA)
                {
                    gm.TriggerGameOver();
                }
                else if (isCharacterB)
                {
                    gm.AddScore(scoreValue);
                    Destroy(gameObject);
                }
            }
        }
    }
}
