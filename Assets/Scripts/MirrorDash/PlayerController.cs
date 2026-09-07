using UnityEngine;
using UnityEngine.InputSystem;

namespace MirrorDash
{
    // handles WASD/arrow movement for character A, and mirrors B through the
    // arena center every frame. B never gets controlled on its own.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;

        [HideInInspector] public float characterRadius = 0.35f;
        [HideInInspector] public Transform mirrorTarget; // Character B

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver) return;

            Vector2 input = ReadMovementInput();
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 movedPosition = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
            transform.position = ClampToArena(movedPosition, gm);
        }

        private void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || mirrorTarget == null) return;

            // B = 2 * arenaCenter - A  (perfect point-reflection through the center)
            Vector2 mirrored = (2f * gm.ArenaCenter) - (Vector2)transform.position;
            mirrorTarget.position = new Vector3(mirrored.x, mirrored.y, mirrorTarget.position.z);
        }

        private static Vector2 ReadMovementInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            Vector2 dir = Vector2.zero;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1f;
            return dir;
        }

        private Vector3 ClampToArena(Vector3 pos, GameManager gm)
        {
            Vector2 half = gm.ArenaHalfSize - Vector2.one * characterRadius;
            pos.x = Mathf.Clamp(pos.x, gm.ArenaCenter.x - half.x, gm.ArenaCenter.x + half.x);
            pos.y = Mathf.Clamp(pos.y, gm.ArenaCenter.y - half.y, gm.ArenaCenter.y + half.y);
            return pos;
        }
    }
}
