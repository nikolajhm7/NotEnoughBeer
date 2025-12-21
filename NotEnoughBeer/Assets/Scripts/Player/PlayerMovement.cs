using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    public GridManager grid;

    [Header("Grid")]
    public Vector2Int startCell = new(2, 0);
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
    int facingIndex = 0;        
    bool isMoving, isRotating;

    
    float moveNextRepeatTime = -1f;
    Vector2Int lastHeldStep = Vector2Int.zero;

    float rotateNextRepeatTimeQ = -1f;
    float rotateNextRepeatTimeE = -1f;

    public Vector2Int CurrentCell => cell;
    public Vector2Int CurrentFacingDir => FacingDir();
    public int FacingIndex => facingIndex;
    bool PositionInitializedFromSave = false;

    void Start()
    {
        if (!grid) { Debug.LogError("Assign GridManager."); enabled = false; return; }

        if (!PositionInitializedFromSave)
        {
            cell = startCell;
            SnapToCell();
        }

        UpdateRotation();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (!isMoving && !isRotating)
        {
            if (kb.qKey.wasPressedThisFrame) { grid.StartCoroutine(Rotate(-1)); SetupRotateRepeat(ref rotateNextRepeatTimeQ, rotateHoldInitialDelay); }
            if (kb.eKey.wasPressedThisFrame) { grid.StartCoroutine(Rotate(+1)); SetupRotateRepeat(ref rotateNextRepeatTimeE, rotateHoldInitialDelay); }
        }

        if (enableRotateHold && !isMoving && !isRotating)
        {
            if (kb.qKey.isPressed && Time.time >= rotateNextRepeatTimeQ && rotateNextRepeatTimeQ >= 0f)
            {
                grid.StartCoroutine(Rotate(-1));
                rotateNextRepeatTimeQ = Time.time + rotateHoldRepeatInterval;
            }
            if (kb.eKey.isPressed && Time.time >= rotateNextRepeatTimeE && rotateNextRepeatTimeE >= 0f)
            {
                grid.StartCoroutine(Rotate(+1));
                rotateNextRepeatTimeE = Time.time + rotateHoldRepeatInterval;
            }
            if (!kb.qKey.isPressed) rotateNextRepeatTimeQ = -1f;
            if (!kb.eKey.isPressed) rotateNextRepeatTimeE = -1f;
        }

        Vector2Int desiredStep = GetStepDirFromKeys(kb);

        if (desiredStep == Vector2Int.zero)
        {
            lastHeldStep = Vector2Int.zero;
            moveNextRepeatTime = -1f;
            return;
        }

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

        if (desiredStep != lastHeldStep)
        {
            lastHeldStep = desiredStep;
            moveNextRepeatTime = Time.time + holdInitialDelay;
        }

        if (!isMoving && !isRotating && moveNextRepeatTime >= 0f && Time.time >= moveNextRepeatTime)
        {
            if (TryStep(desiredStep))
                moveNextRepeatTime = Time.time + holdRepeatInterval;
            else
                moveNextRepeatTime = Time.time + holdRepeatInterval;
        }
    }

    bool TryStep(Vector2Int step)
    {
        var target = cell + step;

        if (!grid.IsWalkable(target)) return false;

        grid.StartCoroutine(MoveTo(target));

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

    IEnumerator Rotate(int dir) 
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

        
        return Vector2Int.zero;
    }

    static Vector2Int PerpLeft(Vector2Int v) => new(-v.y, v.x);
    static Vector2Int PerpRight(Vector2Int v) => new(v.y, -v.x);

    void SetupRotateRepeat(ref float nextTime, float initialDelay)
    {
        if (!enableRotateHold) { nextTime = -1f; return; }
        nextTime = Time.time + initialDelay;
    }
   
    public void TeleportToCell(Vector2Int c)
    {
        cell = c;
        SnapToCell();
        PositionInitializedFromSave = true;
    }

    
    /// <param name="index"
    public void SetFacingIndex(int index)
    {
        facingIndex = ((index % 4) + 4) % 4; // ensure 0-3
        UpdateRotation();
    }
}
