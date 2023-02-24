using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SpawnPointSelector
{
    public class SpawnPointList : MonoBehaviour
    {
        public GameObject SpawnPointListItemPrefab { get; set; }

        public bool IsCollapsed { get; set; } = false;

        public RectTransform ContentTransform
        {
            get => _contentTransform;
            set => _contentTransform = value;
        }

        public RectTransform ViewportTransform
        {
            get => _viewportTransform;
            set => _viewportTransform = value;
        }

        public RectTransform ListTransform
        {
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

        private float _effectDuration = 0.1f;

        void Awake()
        {
            _initialHeight = this.GetComponent<RectTransform>().sizeDelta.y;
        }

        public void Initialize()
        {
            var listContentTransform = ContentTransform.GetComponent<RectTransform>();
            var firstChildTransform = ContentTransform.GetChild(0).GetComponent<RectTransform>();
            listContentTransform.anchoredPosition = new Vector2(
                listContentTransform.anchoredPosition.x,
                firstChildTransform.sizeDelta.y / 2
            );
        }

        public SpawnPointListItem AddItem(string text)
        {
            var listItemGO = Instantiate(SpawnPointListItemPrefab, this.ContentTransform);
            var listItem = listItemGO.GetComponent<SpawnPointListItem>();
            listItem.Text.text = text;
            return listItem;
        }

        public IEnumerator MoveContentToPosition(GameObject selectedObj)
        {
            var listContentTransform = ContentTransform.GetComponent<RectTransform>();
            var targetPosition = new Vector2(listContentTransform.anchoredPosition.x, GetTargetContentPosition(selectedObj));
            var startPosition = new Vector2(listContentTransform.anchoredPosition.x, listContentTransform.anchoredPosition.y);
            var time = 0f;

            while (time < _effectDuration)
            {
                ContentTransform.anchoredPosition = Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    1 - Mathf.Pow(1 - time / _effectDuration, 3)
                );
                time += Time.deltaTime;
                yield return null;
            }

            ContentTransform.anchoredPosition = targetPosition;
        }

        public void SetContentPosition(GameObject selectedObj)
        {
            var listContentTransform = ContentTransform.GetComponent<RectTransform>();
            var newPosition = GetTargetContentPosition(selectedObj);
            listContentTransform.anchoredPosition = new Vector2(listContentTransform.anchoredPosition.x, newPosition);
        }

        private float GetTargetContentPosition(GameObject selectedObj)
        {
            var listTransform = GetComponent<RectTransform>();
            var listSpacing = ContentTransform.GetComponent<VerticalLayoutGroup>().spacing;
            var selectedObjHeight = selectedObj.GetComponent<RectTransform>().sizeDelta.y;
            var selectedObjIndex = selectedObj.transform.GetSiblingIndex();
            var listContentTransform = ContentTransform.GetComponent<RectTransform>();
            return selectedObjHeight / 2 + selectedObjIndex * (selectedObjHeight + listSpacing);
        }

        public void SetCollapsed(bool collapsed)
        {
            if (collapsed == IsCollapsed)
            {
                return;
            }

            IsCollapsed = collapsed;
            var height = _initialHeight;

            if (collapsed)
            {
                var firstChildHeight = _contentTransform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
                height = firstChildHeight;
            }

            _listTransform.sizeDelta = new Vector2(_listTransform.sizeDelta.x, height);
            _listCanvasGroup.interactable = !collapsed;
        }
    }
}
