using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image image;
    private int cardIndex;
    private MemoryGame gameManager;
    private Color originalColor;
    private bool isInteractable = true;

    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void Init(int index, MemoryGame manager)
    {
        cardIndex = index;
        gameManager = manager;
    }

    public void SetColor(Color color)
    {
        originalColor = color;
        image.color = color;
    }

    public void Highlight()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Blink());
        }
    }

    private System.Collections.IEnumerator Blink()
    {
        isInteractable = false;
        image.color = Color.white;
        yield return new WaitForSeconds(0.5f);
        image.color = originalColor;
        yield return new WaitForSeconds(0.2f);
        isInteractable = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isInteractable)
        {
            gameManager?.OnCardSelected(cardIndex);
        }
    }
}