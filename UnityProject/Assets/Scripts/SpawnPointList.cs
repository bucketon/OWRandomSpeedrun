using UnityEngine;
using UnityEngine.UI;

namespace SpawnPointSelector
{
  public class SpawnPointList : MonoBehaviour
  {
    public GameObject SpawnPointListItemPrefab { get; set; }

    public bool IsCollapsed { get; set; } = false;

    public RectTransform ContentTransform {
      get => _contentTransform;
      set => _contentTransform = value;
    }

    public RectTransform ViewportTransform {
      get => _viewportTransform;
      set => _viewportTransform = value;
    }

    public RectTransform ListTransform {
      get => _listTransform;
      set => _listTransform = value;
    }

    [SerializeField]
    private RectTransform _contentTransform;

    [SerializeField]
    private RectTransform _viewportTransform;

    [SerializeField]
    private RectTransform _listTransform;

    [SerializeField]
    private CanvasGroup _listCanvasGroup;

    private float _initialHeight;

    void Awake()
    {
      _initialHeight = this.GetComponent<RectTransform>().sizeDelta.y;
    }

    void Update()
    {

    }

    public void Initialize()
    {
      var listContentTransform = ContentTransform.GetComponent<RectTransform>();
      var firstChildTransform = ContentTransform.GetChild(0).GetComponent<RectTransform>();
      listContentTransform.anchoredPosition = new Vector2(listContentTransform.anchoredPosition.x, firstChildTransform.sizeDelta.y / 2);
    }

    public SpawnPointListItem AddItem(string text)
    {
      var listItemGO = Instantiate(SpawnPointListItemPrefab, this.ContentTransform);
      var listItem = listItemGO.GetComponent<SpawnPointListItem>();
      listItem.Text.text = text;
      return listItem;
    }

    public void SetContentPosition(GameObject selectedObj)
    {
      var listTransform = GetComponent<RectTransform>();
      var listSpacing = ContentTransform.GetComponent<VerticalLayoutGroup>().spacing;
      var selectedObjHeight = selectedObj.GetComponent<RectTransform>().sizeDelta.y;
      var selectedObjIndex = selectedObj.transform.GetSiblingIndex();
      var listContentTransform = ContentTransform.GetComponent<RectTransform>();
      var newPosition = selectedObjHeight / 2 + selectedObjIndex * (selectedObjHeight + listSpacing);
      listContentTransform.anchoredPosition = new Vector2(listContentTransform.anchoredPosition.x, newPosition);
    }

    public void SetCollapsed(bool collapsed)
    {
      if (collapsed == IsCollapsed) {
        return;
      }

      IsCollapsed = collapsed;
      var height = _initialHeight;

      if (collapsed) {
        var firstChildHeight = _contentTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
        height = firstChildHeight;
      }

      _listTransform.sizeDelta = new Vector2(_listTransform.sizeDelta.x, height);
      _listCanvasGroup.interactable = !collapsed;
    }
  }
}
