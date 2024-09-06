using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public Potion potion;

    [Header("Ingredient Settings")]
    public float disappearSpeed = .1f;

    private bool contact;
    SpriteRenderer spriteRenderer;


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
            DrawCauldron cauldron = collision.gameObject.GetComponent<DrawCauldron>();
            cauldron.AddIngredient(this);
            contact = true;
        }
    }

    void Disappear()
    {
        Color spriteColor = spriteRenderer.color;
        spriteColor.a -= Time.deltaTime / disappearSpeed;
        spriteRenderer.color = spriteColor;
        if (spriteColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (contact)
        {
            Disappear();
        }
    }
}
