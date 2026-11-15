using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public Sprite[] runFrames;
    public float runFrameDuration = 0.08f;

    [Header("Combat")]
    public float health = 100f;
    public float slashDamage = 25f;
    public float slashRange = 1f;
    public float bowDamage = 15f;
    public float arrowSpeed = 10f;
    public Sprite energyOrbSprite;
    public LayerMask damageableLayers = ~0;

    [Header("Shield")]
    [Range(0f, 1f)]
    public float shieldDamageMultiplier = 0.25f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Sprite idleSprite;
    private Vector2 movement;
    private Vector2 facingDirection = Vector2.right;
    private int runFrameIndex;
    private float runFrameTimer;
    private bool isShielding;
    private bool isAttacking;
    private LineRenderer slashLine;
    private LineRenderer slashCoreLine;
    private LineRenderer shieldLine;
    private LineRenderer shieldInnerLine;
    private ParticleSystem combatSparks;
    private Material effectsMaterial;
    private Sprite generatedOrbSprite;
    private Texture2D generatedOrbTexture;
    private float shieldParticleTimer;
    private float effectTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
        {
            idleSprite = spriteRenderer.sprite;
        }

        Shader effectsShader = Shader.Find("Sprites/Default");
        if (effectsShader == null)
        {
            Debug.LogError("Player1 could not find the Sprites/Default shader for combat effects.", this);
            return;
        }

        effectsMaterial = new Material(effectsShader);
        slashLine = CreateLine("SlashBlueGlow", new Color(0.05f, 0.48f, 1f, 0.8f), 0.16f);
        slashCoreLine = CreateLine("SlashRedCore", new Color(1f, 0.12f, 0.3f, 0.95f), 0.055f);
        shieldLine = CreateLine("ShieldBlueGlow", new Color(0.08f, 0.55f, 1f, 0.6f), 0.14f);
        shieldInnerLine = CreateLine("ShieldRedCore", new Color(1f, 0.12f, 0.32f, 0.9f), 0.045f);
        combatSparks = CreateParticleSystem("CombatSparks", new Color(1f, 0.78f, 0.28f));
        if (energyOrbSprite == null)
        {
            generatedOrbSprite = CreateEnergyOrbSprite(out generatedOrbTexture);
        }

        if (shieldLine != null)
        {
            shieldLine.positionCount = 49;
            shieldLine.enabled = false;
        }

        if (shieldInnerLine != null)
        {
            shieldInnerLine.positionCount = 49;
            shieldInnerLine.enabled = false;
        }
    }

    private void Update()
    {
        effectTime += Time.deltaTime;
        movement = new Vector2(
            (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
            (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f)
        ).normalized;

        if (movement.x != 0f)
        {
            facingDirection = movement.x > 0f ? Vector2.right : Vector2.left;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = movement.x < 0f;
            }
        }

        isShielding = Input.GetKey(KeyCode.J);
        UpdateShieldEffect();
        UpdateRunAnimation();

        if (Input.GetKeyDown(KeyCode.K) && !isAttacking && !isShielding)
        {
            StartCoroutine(Slash());
        }

        if (Input.GetKeyDown(KeyCode.L) && !isAttacking && !isShielding)
        {
            StartCoroutine(ShootArrow());
        }
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = (Vector2)transform.position + movement * moveSpeed * Time.fixedDeltaTime;
        if (body != null)
        {
            body.MovePosition(nextPosition);
        }
        else
        {
            transform.position = nextPosition;
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        if (isShielding)
        {
            EmitSparks(transform.position + (Vector3)(facingDirection * 0.45f), 10, 2f,
                new Color(0.42f, 0.86f, 1f));
            health = Mathf.Max(0f, health - damage * shieldDamageMultiplier);
            return;
        }

        health = Mathf.Max(0f, health - damage);
    }

    private IEnumerator Slash()
    {
        isAttacking = true;
        ApplyDamageInFront(slashDamage);

        const float duration = 0.24f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            DrawSlashArc(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        if (slashLine != null)
        {
            slashLine.enabled = false;
        }

        if (slashCoreLine != null)
        {
            slashCoreLine.enabled = false;
        }

        isAttacking = false;
    }

    private IEnumerator ShootArrow()
    {
        isAttacking = true;

        const float drawDuration = 0.2f;
        GameObject chargingOrb = CreateEnergyOrb(
            "ChargingEnergyOrb",
            transform.position + (Vector3)(facingDirection * 0.55f),
            0.45f
        );
        if (chargingOrb == null)
        {
            isAttacking = false;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            if (chargingOrb != null)
            {
                chargingOrb.transform.position = transform.position + (Vector3)(facingDirection * 0.55f);
                chargingOrb.transform.Rotate(0f, 0f, 360f * Time.deltaTime);
                float pulse = 0.85f + Mathf.Sin(elapsed * 35f) * 0.12f;
                chargingOrb.transform.localScale = Vector3.one * (0.45f * pulse);
            }

            yield return null;
        }

        if (chargingOrb != null)
        {
            Destroy(chargingOrb);
        }

        StartCoroutine(FlyArrow());
        isAttacking = false;
    }

    private IEnumerator FlyArrow()
    {
        GameObject orb = CreateEnergyOrb(
            "BlueRedEnergyOrb",
            transform.position + (Vector3)(facingDirection * 0.55f),
            0.6f
        );
        if (orb == null)
        {
            yield break;
        }

        TrailRenderer trail = orb.AddComponent<TrailRenderer>();
        trail.time = 0.22f;
        trail.minVertexDistance = 0.025f;
        trail.material = effectsMaterial;
        trail.startWidth = 0.32f;
        trail.endWidth = 0f;
        trail.startColor = new Color(0.45f, 0.25f, 1f, 0.8f);
        trail.endColor = new Color(1f, 0.12f, 0.2f, 0f);
        trail.sortingOrder = 9;
        trail.numCapVertices = 4;

        const float maxFlightTime = 2f;
        float elapsed = 0f;
        Vector2 projectileDirection = facingDirection;
        while (elapsed < maxFlightTime)
        {
            float step = arrowSpeed * Time.deltaTime;
            orb.transform.position += (Vector3)(projectileDirection * step);
            orb.transform.Rotate(0f, 0f, 540f * Time.deltaTime);
            float pulse = 1f + Mathf.Sin(elapsed * 24f) * 0.12f;
            orb.transform.localScale = Vector3.one * (0.6f * pulse);

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                orb.transform.position,
                0.22f,
                damageableLayers
            );
            foreach (Collider2D hit in hits)
            {
                if (hit.transform.root == transform.root)
                {
                    continue;
                }

                if (ApplyDamage(hit, bowDamage))
                {
                    EmitSparks(orb.transform.position, 12, 2.8f, new Color(0.2f, 0.65f, 1f));
                    EmitSparks(orb.transform.position, 12, 2.8f, new Color(1f, 0.16f, 0.28f));
                    Destroy(orb);
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(orb);
    }

    private void ApplyDamageInFront(float damage)
    {
        if (damage <= 0f || slashRange <= 0f)
        {
            return;
        }

        Vector2 center = (Vector2)transform.position + facingDirection * (slashRange * 0.5f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            new Vector2(slashRange, 0.9f),
            0f,
            damageableLayers
        );

        HashSet<int> damagedTargets = new HashSet<int>();
        foreach (Collider2D hit in hits)
        {
            if (hit.transform.root == transform.root)
            {
                continue;
            }

            int targetId = hit.transform.root.gameObject.GetInstanceID();
            if (damagedTargets.Add(targetId))
            {
                ApplyDamage(hit, damage);
            }
        }
    }

    private static bool ApplyDamage(Collider2D hit, float damage)
    {
        if (hit == null || damage <= 0f)
        {
            return false;
        }

        Monster monster = hit.GetComponentInParent<Monster>();
        if (monster != null)
        {
            monster.health -= damage;
            if (monster.health <= 0f)
            {
                monster.gameObject.SetActive(false);
            }

            return true;
        }

        Enemy enemy = hit.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.health -= damage;
            if (enemy.health <= 0f)
            {
                enemy.Die();
            }

            return true;
        }

        return false;
    }

    private void UpdateRunAnimation()
    {
        if (spriteRenderer == null || runFrames == null || runFrames.Length == 0)
        {
            return;
        }

        if (movement.sqrMagnitude == 0f || isAttacking)
        {
            runFrameIndex = 0;
            runFrameTimer = 0f;
            spriteRenderer.sprite = idleSprite;
            return;
        }

        runFrameTimer += Time.deltaTime;
        if (runFrameDuration > 0f && runFrameTimer >= runFrameDuration)
        {
            runFrameTimer %= runFrameDuration;
            runFrameIndex = (runFrameIndex + 1) % runFrames.Length;
        }

        spriteRenderer.sprite = runFrames[runFrameIndex];
    }

    private void UpdateShieldEffect()
    {
        if (shieldLine == null || shieldInnerLine == null)
        {
            return;
        }

        shieldLine.enabled = isShielding;
        shieldInnerLine.enabled = isShielding;
        if (!isShielding)
        {
            shieldParticleTimer = 0f;
            return;
        }

        float pulse = 1f + Mathf.Sin(effectTime * 6f) * 0.035f;
        DrawCircle(shieldLine, transform.position, 0.62f * pulse, 48, effectTime * 0.3f);
        DrawCircle(shieldInnerLine, transform.position, 0.49f * pulse, 48, -effectTime * 0.42f);

        shieldParticleTimer += Time.deltaTime;
        if (shieldParticleTimer >= 0.09f)
        {
            shieldParticleTimer = 0f;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 position = transform.position +
                               new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.62f;
            EmitSparks(position, 1, 0.35f,
                Random.value > 0.5f ? new Color(0.2f, 0.65f, 1f) : new Color(1f, 0.16f, 0.32f));
        }
    }

    private void DrawSlashArc(float progress)
    {
        if (slashLine == null || slashCoreLine == null)
        {
            return;
        }

        slashLine.enabled = true;
        slashCoreLine.enabled = true;
        DrawSlashArcLine(slashLine, 0.9f, progress);
        DrawSlashArcLine(slashCoreLine, 0.84f, progress);

        if (progress < 0.12f)
        {
            float angle = (facingDirection.x > 0f ? -25f : 205f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector3 sparkPosition = transform.position + (Vector3)(direction * 0.72f);
            EmitSparks(sparkPosition, 10, 3.8f, new Color(0.2f, 0.65f, 1f));
            EmitSparks(sparkPosition, 10, 3.8f, new Color(1f, 0.16f, 0.32f));
        }
    }

    private void DrawSlashArcLine(LineRenderer line, float radius, float progress)
    {
        const int segments = 22;
        line.positionCount = segments + 1;
        float startAngle = facingDirection.x > 0f ? -72f : 108f;
        float sweep = 144f * progress;
        Vector3 center = transform.position + (Vector3)(facingDirection * 0.2f);
        for (int i = 0; i <= segments; i++)
        {
            float angle = (startAngle + sweep * i / segments) * Mathf.Deg2Rad;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private GameObject CreateEnergyOrb(string objectName, Vector3 position, float scale)
    {
        Sprite orbSprite = energyOrbSprite != null ? energyOrbSprite : generatedOrbSprite;
        if (orbSprite == null)
        {
            Debug.LogError("Player1 could not create the energy orb sprite.", this);
            return null;
        }

        GameObject orb = new GameObject(objectName);
        orb.transform.position = position;
        orb.transform.localScale = Vector3.one * scale;

        CreateOrbLayer(orb.transform, "OuterGlow", orbSprite,
            new Color(0.55f, 0.12f, 1f, 0.45f), Vector3.zero, 1.15f, 8);
        CreateOrbLayer(orb.transform, "BlueFlare", orbSprite,
            new Color(0.05f, 0.55f, 1f, 0.95f), Vector3.left * 0.11f, 0.72f, 10);
        CreateOrbLayer(orb.transform, "RedFlare", orbSprite,
            new Color(1f, 0.08f, 0.22f, 0.95f), Vector3.right * 0.11f, 0.72f, 10);
        CreateOrbLayer(orb.transform, "Core", orbSprite,
            new Color(1f, 0.88f, 1f, 0.95f), Vector3.zero, 0.27f, 11);

        CreateOrbRing(orb.transform, "BlueRing", new Color(0.12f, 0.75f, 1f, 0.95f), 0.62f, 0f, 12);
        CreateOrbRing(orb.transform, "RedRing", new Color(1f, 0.12f, 0.34f, 0.9f), 0.49f, 35f, 12);
        return orb;
    }

    private static void CreateOrbLayer(
        Transform parent,
        string objectName,
        Sprite sprite,
        Color color,
        Vector3 localPosition,
        float localScale,
        int sortingOrder)
    {
        GameObject layer = new GameObject(objectName);
        layer.transform.SetParent(parent, false);
        layer.transform.localPosition = localPosition;
        layer.transform.localScale = Vector3.one * localScale;
        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private void CreateOrbRing(
        Transform parent,
        string objectName,
        Color color,
        float radius,
        float rotation,
        int sortingOrder)
    {
        GameObject ringObject = new GameObject(objectName);
        ringObject.transform.SetParent(parent, false);
        ringObject.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        LineRenderer ring = ringObject.AddComponent<LineRenderer>();
        ConfigureLine(ring, color, 0.045f);
        ring.useWorldSpace = false;
        ring.positionCount = 49;
        ring.sortingOrder = sortingOrder;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / (ring.positionCount - 1);
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private static Sprite CreateEnergyOrbSprite(out Texture2D texture)
    {
        const int resolution = 64;
        texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.name = "GeneratedEnergyOrb";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = (x + 0.5f - resolution * 0.5f) / (resolution * 0.5f);
                float dy = (y + 0.5f - resolution * 0.5f) / (resolution * 0.5f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 1.7f);
                pixels[y * resolution + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    private void DrawCircle(LineRenderer line, Vector3 center, float radius, int segments, float rotation)
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments + rotation;
            line.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
        }
    }

    private LineRenderer CreateLine(string objectName, Color color, float width)
    {
        if (effectsMaterial == null)
        {
            return null;
        }

        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        ConfigureLine(line, color, width);
        line.useWorldSpace = true;
        line.enabled = false;
        return line;
    }

    private void ConfigureLine(LineRenderer line, Color color, float width)
    {
        line.material = effectsMaterial;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = 10;
        line.numCapVertices = 5;
        line.numCornerVertices = 5;
        line.textureMode = LineTextureMode.Stretch;
        line.alignment = LineAlignment.View;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(color, 0.45f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(color.a, 0.12f),
                new GradientAlphaKey(color.a, 0.8f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        line.colorGradient = gradient;
        line.widthCurve = new AnimationCurve(
            new Keyframe(0f, 0.15f),
            new Keyframe(0.2f, 0.85f),
            new Keyframe(0.5f, 1f),
            new Keyframe(0.8f, 0.65f),
            new Keyframe(1f, 0.08f)
        );
    }

    private ParticleSystem CreateParticleSystem(string objectName, Color color)
    {
        GameObject particleObject = new GameObject(objectName);
        particleObject.transform.SetParent(transform, false);
        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.55f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.095f);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 128;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient particleGradient = new Gradient();
        particleGradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(color, 0.35f),
                new GradientColorKey(color, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.85f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = particleGradient;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(
                new Keyframe(0f, 0.35f),
                new Keyframe(0.18f, 1f),
                new Keyframe(1f, 0f)
            )
        );

        ParticleSystemRenderer particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material = effectsMaterial;
        particleRenderer.sortingOrder = 11;
        return particles;
    }

    private void EmitSparks(Vector3 position, int count, float speed, Color color)
    {
        if (combatSparks == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = direction * Random.Range(speed * 0.35f, speed),
                startColor = color,
                startSize = Random.Range(0.035f, 0.09f),
                startLifetime = Random.Range(0.2f, 0.45f)
            };
            combatSparks.Emit(emit, 1);
        }
    }

    private void OnDestroy()
    {
        if (effectsMaterial != null)
        {
            Destroy(effectsMaterial);
        }

        if (generatedOrbSprite != null)
        {
            Destroy(generatedOrbSprite);
        }

        if (generatedOrbTexture != null)
        {
            Destroy(generatedOrbTexture);
        }
    }
}
