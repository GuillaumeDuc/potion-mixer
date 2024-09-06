using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreUIScript : MonoBehaviour
{
    public List<GameObject> listIngredient;
    public GameObject UIImage;

    void Start()
    {
        listIngredient.ForEach(ingredient =>
        {
            SetInScrollView(ingredient);
        });
    }

    public GameObject SetInScrollView(GameObject ingredient)
    {
        GameObject go = Instantiate(UIImage);

        SetOnClick(go, ingredient);

        SpriteRenderer ingredientSR = ingredient.GetComponent<SpriteRenderer>();
        Image goImage = go.GetComponent<Image>();
        goImage.sprite = ingredientSR.sprite;
        goImage.color = ingredientSR.color;

        go.transform.SetParent(gameObject.transform);
        go.transform.localScale = new Vector3(1, 1, 1);
        return go;
    }

    public void SetOnClick(GameObject button, GameObject ingredient)
    {
        button.GetComponent<Button>().onClick.AddListener(()=> {
            GameObject go = Instantiate(ingredient);
            go.transform.position += new Vector3(0, 2);
        });
    }
}
