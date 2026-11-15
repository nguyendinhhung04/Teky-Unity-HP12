using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float health;

    public float speed = 2f;
    public float runSpeed = 4f;
    public Sprite[] runFrames;
    public float runFrameDuration = 0.08f;

    private TealFalconEnemySeries.DarkKnightController darkKnight;
    private SpriteRenderer spriteRenderer;
    private Sprite idleSprite;
    private float runFrameTimer;
    private int runFrameIndex;
    private bool isRunAnimationPlaying;

    private void Start()
    {
        darkKnight = GetComponent<TealFalconEnemySeries.DarkKnightController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            idleSprite = spriteRenderer.sprite;
        }
    }

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentMoveSpeed = isRunning ? runSpeed : speed;

        if (darkKnight != null)
        {
            bool isMoving = Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f;
            darkKnight.animationSpeedOverride = isMoving ? 3.3f : 0f;
        }

        UpdateRunAnimation(Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveY) > 0.01f);

        if (Mathf.Abs(moveX) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveX);
            transform.localScale = scale;
        }

        Vector3 moveDirection = new Vector3(moveX, moveY, 0f) * currentMoveSpeed * Time.deltaTime;
        transform.Translate(moveDirection, Space.World);

        if (darkKnight != null && Mathf.Abs(moveX) > 0.01f &&
            darkKnight.CurrentFightingState == TealFalconEnemySeries.DarkKnightController.FightingState.OnGuard)
        {
            darkKnight.ActivateGuard();
        }

        if (darkKnight == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (darkKnight.CurrentFightingState == TealFalconEnemySeries.DarkKnightController.FightingState.Idle)
            {
                darkKnight.ActivateGuard();
            }

            if (darkKnight.CurrentFightingState == TealFalconEnemySeries.DarkKnightController.FightingState.OnGuard)
            {
                darkKnight.ActivateAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.L) &&
            (darkKnight.CurrentFightingState == TealFalconEnemySeries.DarkKnightController.FightingState.Idle ||
             darkKnight.CurrentFightingState == TealFalconEnemySeries.DarkKnightController.FightingState.OnGuard))
        {
            darkKnight.ActivateBeamAttack();
        }
    }

    private void UpdateRunAnimation(bool isMoving)
    {
        if (spriteRenderer == null || runFrames == null || runFrames.Length == 0)
        {
            return;
        }

        if (!isMoving)
        {
            if (isRunAnimationPlaying)
            {
                spriteRenderer.sprite = idleSprite;
                runFrameTimer = 0f;
                runFrameIndex = 0;
                isRunAnimationPlaying = false;
            }

            return;
        }

        if (!isRunAnimationPlaying)
        {
            runFrameIndex = 0;
            runFrameTimer = 0f;
            isRunAnimationPlaying = true;
            spriteRenderer.sprite = runFrames[runFrameIndex];
        }

        if (runFrameDuration <= 0f)
        {
            return;
        }

        runFrameTimer += Time.deltaTime;
        while (runFrameTimer >= runFrameDuration)
        {
            runFrameTimer -= runFrameDuration;
            runFrameIndex = (runFrameIndex + 1) % runFrames.Length;
            spriteRenderer.sprite = runFrames[runFrameIndex];
        }
    }
}
