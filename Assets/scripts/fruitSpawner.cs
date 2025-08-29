using UnityEngine;
using System.Collections;

public class FruitSpawner : MonoBehaviour
{
    public GameObject[] fruitPrefebs;
    public GameObject[] targetPrefebs;
    private GameObject fruit;
    private FruitController controller;
    public static FruitSpawner instance;
    private Vector3 prevPosition = Vector3.zero;
    private int lastFruitIndex;
    public void setPrePosition(Vector3 pos)
    {
        prevPosition = pos;
    }
    private void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
       
    }
    public void spawnFruit()
    {
       
        if(fruit != null)
        {
            return;
        }
        int index;
        do
        {
            index = Random.Range(0, fruitPrefebs.Length);
        }
        while (index == lastFruitIndex && fruitPrefebs.Length > 1);

        lastFruitIndex = index;

        RectTransform canvasRect = GameController.instance.mainCanvas.GetComponent<RectTransform>();
        //this should be change to Robot's x position instead of game object
        float x = (prevPosition.x != 0f)
            ? prevPosition.x
            : Random.Range(-(canvasRect.rect.width / 2f), (canvasRect.rect.width / 2f));

        float y = (canvasRect.rect.height / 2f) - 50; // Top of the screen
      
        fruit = Instantiate(fruitPrefebs[index], GameController.instance.mainCanvas.transform);
        fruit.transform.localPosition = new Vector3(x, y, 0f);
        fruit.transform.localScale = Vector3.one * 3f;
        controller = fruit.AddComponent<FruitController>();
        controller.target = targetPrefebs[index];
   

    }
    public void clearCurrentFruit()
    {
        fruit = null;  // Now spawnFruit() can create a new fruit
    }
    public void clearController()
    {
        if (fruit != null)
        {
            FruitController controller = fruit.GetComponent<FruitController>();
            if (controller != null)
            {
                Destroy(controller);  // Remove the FruitController script
            }
        }
    }


}
