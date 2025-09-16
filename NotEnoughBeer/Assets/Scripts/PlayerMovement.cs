using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;

    [Header("Grid")]
    public Vector2Int startCell = new Vector2Int(0, 0);
    public float heightOffset = 0.5f;

    [Header("Movement")]
    [Min(0.01f)] public float moveTime = 0.12f;
    [Min(0.01f)] public float rotateTime = 0.1f;

    [Header("Hold-to-move")]
    [Min(0f)]   public float holdInitialDelay = 0.25f;
    [Min(0.01f)] public float holdRepeatInterval = 0.08f;

    [Header("Hold-to-rotate (optional)")]
    public bool enableRotateHold = true;
    [Min(0f)]   public float rotateHoldInitialDelay = 0.35f;
    [Min(0.05f)] public float rotateHoldRepeatInterval = 0.15f;

    Vector2Int cell;
    int facingIndex = 0;          // 0=N,1=E,2=S,3=W
    bool isMoving, isRotating;

    // Repeat timers
    float moveNextRepeatTime = -1f;
    Vector2Int lastHeldStep = Vector2Int.zero;

    float rotateNextRepeatTimeQ = -1f;
    float rotateNextRepeatTimeE = -1f;

    public Vector2Int CurrentCell => cell;
    public Vector2Int CurrentFacingDir => FacingDir();

    void Start()
    {
        if (!grid) { Debug.LogError("Assign GridManager."); enabled = false; return; }
        cell = startCell;
        SnapToCell();
        UpdateRotation();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // --- Rotation (Q/E), supports press + optional hold repeat ---
        if (!isMoving && !isRotating)
        {
            // Immediate press
            if (kb.qKey.wasPressedThisFrame) { StartCoroutine(Rotate(-1)); SetupRotateRepeat(ref rotateNextRepeatTimeQ, rotateHoldInitialDelay); }
            if (kb.eKey.wasPressedThisFrame) { StartCoroutine(Rotate(+1)); SetupRotateRepeat(ref rotateNextRepeatTimeE, rotateHoldInitialDelay); }
        }

        // Held rotation (optional)
        if (enableRotateHold && !isMoving && !isRotating)
        {
            if (kb.qKey.isPressed && Time.time >= rotateNextRepeatTimeQ && rotateNextRepeatTimeQ >= 0f)
            {
                StartCoroutine(Rotate(-1));
                rotateNextRepeatTimeQ = Time.time + rotateHoldRepeatInterval;
            }
            if (kb.eKey.isPressed && Time.time >= rotateNextRepeatTimeE && rotateNextRepeatTimeE >= 0f)
            {
                StartCoroutine(Rotate(+1));
                rotateNextRepeatTimeE = Time.time + rotateHoldRepeatInterval;
            }
            if (!kb.qKey.isPressed) rotateNextRepeatTimeQ = -1f;
            if (!kb.eKey.isPressed) rotateNextRepeatTimeE = -1f;
        }

        // --- Movement (WASD) with hold repeat ---
        // Determine desired step direction from current key state (with priority order)
        Vector2Int desiredStep = GetStepDirFromKeys(kb);

        // If no direction held, clear repeat
        if (desiredStep == Vector2Int.zero)
        {
            lastHeldStep = Vector2Int.zero;
            moveNextRepeatTime = -1f;
            return;
        }

        // On a *new* direction press this frame: move immediately (if free), then arm repeat
        bool pressedThisFrame = kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
                                kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame ||
                                kb.upArrowKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame ||
                                kb.downArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame;

        if (pressedThisFrame && !isMoving && !isRotating)
        {
            TryStep(desiredStep);
            lastHeldStep = desiredStep;
            moveNextRepeatTime = Time.time + holdInitialDelay;
            return;
        }

        // If direction changed while holding, reset repeat delay
        if (desiredStep != lastHeldStep)
        {
            lastHeldStep = desiredStep;
            moveNextRepeatTime = Time.time + holdInitialDelay;
        }

        // Repeats while holding
        if (!isMoving && !isRotating && moveNextRepeatTime >= 0f && Time.time >= moveNextRepeatTime)
        {
            if (TryStep(desiredStep))
                moveNextRepeatTime = Time.time + holdRepeatInterval; // schedule next
            else
                moveNextRepeatTime = Time.time + holdRepeatInterval; // still schedule; you might be blocked/bounds
        }
    }

    bool TryStep(Vector2Int step)
    {
        var target = cell + step;
        if (!grid.IsInside(target)) return false;
        StartCoroutine(MoveTo(target));
        return true;
    }

    IEnumerator MoveTo(Vector2Int target)
    {
        isMoving = true;
        Vector3 a = transform.position;
        Vector3 b = grid.GridToWorld(target) + Vector3.up * heightOffset;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;
            transform.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        cell = target;
        isMoving = false;
    }

    IEnumerator Rotate(int dir) // dir = -1 left, +1 right
    {
        isRotating = true;
        facingIndex = (facingIndex + dir + 4) % 4;

        Quaternion a = transform.rotation;
        Quaternion b = Quaternion.Euler(0f, facingIndex * 90f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / rotateTime;
            transform.rotation = Quaternion.Slerp(a, b, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.rotation = b;
        isRotating = false;
    }

    void SnapToCell()
    {
        transform.position = grid.GridToWorld(cell) + Vector3.up * heightOffset;
    }

    void UpdateRotation()
    {
        transform.rotation = Quaternion.Euler(0f, facingIndex * 90f, 0f);
    }

    Vector2Int FacingDir()
    {
        return facingIndex switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            3 => Vector2Int.left,
            _ => Vector2Int.up,
        };
    }

    // Priority: forward (W), backward (S), left (A), right (D). Arrows are aliases.
    Vector2Int GetStepDirFromKeys(Keyboard kb)
    {
        Vector2Int f = FacingDir();
        Vector2Int left = PerpLeft(f);
        Vector2Int right = PerpRight(f);

        bool w = kb.wKey.isPressed || kb.upArrowKey.isPressed;
        bool s = kb.sKey.isPressed || kb.downArrowKey.isPressed;
        bool a = kb.aKey.isPressed || kb.leftArrowKey.isPressed;
        bool d = kb.dKey.isPressed || kb.rightArrowKey.isPressed;

        if (w && !s) return f;
        if (s && !w) return -f;
        if (a && !d) return left;
        if (d && !a) return right;

        // conflicting or none
        return Vector2Int.zero;
    }

    static Vector2Int PerpLeft(Vector2Int v)  => new Vector2Int(-v.y, v.x);
    static Vector2Int PerpRight(Vector2Int v) => new Vector2Int(v.y, -v.x);

    void SetupRotateRepeat(ref float nextTime, float initialDelay)
    {
        if (!enableRotateHold) { nextTime = -1f; return; }
        nextTime = Time.time + initialDelay;
    }
}
