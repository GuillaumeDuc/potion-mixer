using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [Header("Color Settings")]
    [Range(-1f, 1)]
    public float alpha;
    [Range(-100f, 100)]
    public float glowingPower;
    public Color color;

    [Header("Wave Settings")]
    public bool enableWave;
    public bool disableWave;
    public float amplitude;
    public float speed;
    public float period;
    public Vector3 origin;

    [Header("Ingredient Settings")]
    public float disappearSpeed = .1f;
    bool disappear;
    float startTime;
    SpriteRenderer spriteRenderer;

    [Header("Smoke Settings")]
    public bool enableSmoke = false;
    public bool disableSmoke = false;
    [ColorUsage(true, true)]
    public Color smokeColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDrag()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gameObject.transform.position = new Vector3(mousePos.x, mousePos.y, gameObject.transform.position.z);
    }
    void OnMouseUp()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Mix"))
        {
            ApplyToMix(collision.gameObject.GetComponent<DrawCauldron>());
            disappear = true;
            startTime = Time.deltaTime;
        }
    }

    void ApplyToMix(DrawCauldron cauldron)
    {
        cauldron.alpha = Mathf.Max(cauldron.alpha + alpha, 0);
        cauldron.glowingPower += glowingPower;
        cauldron.color += color / 2;

        cauldron.wave = enableWave || cauldron.wave && !disableWave && cauldron.wave;
        cauldron.amplitude += amplitude;
        cauldron.speed += speed;
        cauldron.period += period;
        cauldron.origin += origin;

        cauldron.smoke = enableSmoke || cauldron.smoke && !disableSmoke && cauldron.smoke;
        cauldron.smokeColor = enableSmoke ? cauldron.smokeColor + smokeColor / 2 : cauldron.smokeColor;

        cauldron.ApplyMaterial();
        cauldron.ApplySmoke(); 
    }

    void Disappear()
    {
        Color spriteColor = spriteRenderer.color;
        spriteColor.a += (Time.deltaTime - startTime) * disappearSpeed;
        spriteRenderer.color = spriteColor;
        if (spriteColor.a <= 0.01)
        {
            Destroy(gameObject);
        }
    }


    private void Update()
    {
        if (disappear)
        {
            Disappear();
        }
        
    }
}
