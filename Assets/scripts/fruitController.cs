using System.Collections;
using UnityEditor.VersionControl;
using UnityEngine;

public class FruitController : MonoBehaviour
{
    public float fallSpeed = 60f; // Falling speed
    public float horizontalSpeed = 500f;// Horizontal movement speed
    private Vector3 targetScale = Vector3.one*1.2f;
    private float scaleDuration = 0.5f; // time to scale up
    public GameObject target;
    private RectTransform canvasRect;
    public Vector3 prevPosition;

    void Start()
    {

        canvasRect = GameController.instance.mainCanvas.GetComponent<RectTransform>();
        StartCoroutine(ScaleUp());
    }
    IEnumerator ScaleUp()
    {
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale; // ensure final scale
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right

        Vector3 pos = transform.localPosition;
        pos.x += horizontal * horizontalSpeed * Time.deltaTime;
        float halfWidth = (canvasRect.rect.width / 2f) - 50f; // 50 = margin (adjust if needed)
        pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
        pos.y -= fallSpeed * Time.deltaTime;
        transform.localPosition = pos;

        if (pos.y < -(canvasRect.rect.height / 2f) + 50)
        {
            FruitSpawner.instance.setPrePosition(transform.localPosition);
            FailFruit();
            FruitSpawner.instance.clearController();
            FruitSpawner.instance.clearCurrentFruit();
        }
        
         

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == target.name)
        {
            FruitSpawner.instance.setPrePosition(transform.localPosition);
            CatchFruit();
            FruitSpawner.instance.clearController();
            FruitSpawner.instance.clearCurrentFruit();
            return;
        }
        FailFruit();
    }
    void CatchFruit()
    {
        GameController.instance.setSuccess();
        transform.SetParent(target.transform); // Re-parent fruit to basket
        transform.localPosition = transform.localPosition;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }

        transform.rotation = Quaternion.identity; // Reset rotation
        // Optionally resize
        transform.localScale = Vector3.one * 0.8f;
    }
    void FailFruit()
    {
        GameController.instance.setFailure();
        gardener.instance.AddFallenFruit(gameObject);
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        transform.localScale = Vector3.one * 0.5f;

    }
}
