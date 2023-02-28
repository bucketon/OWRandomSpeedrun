using UnityEngine;
using UnityEngine.UI;

namespace SpawnPointSelector
{
    public class SpawnPointListItem : MonoBehaviour
    {
        public GameObject LeftArrow
        {
            get => _leftArrow;
            set => _leftArrow = value;
        }

        public GameObject RightArrow
        {
            get => _rightArrow;
            set => _rightArrow = value;
        }

        public Image LeftArrowImage
        {
            get => _leftArrowImage;
            set => _leftArrowImage = value;
        }

        public Image RightArrowImage
        {
            get => _rightArrowImage;
            set => _rightArrowImage = value;
        }

        public LayoutElement LeftArrowLayout
        {
            get => _leftArrowLayout;
            set => _leftArrowLayout = value;
        }

        public LayoutElement RightArrowLayout
        {
            get => _rightArrowLayout;
            set => _rightArrowLayout = value;
        }

        public Text Text
        {
            get => _text;
            set => _text = value;
        }

        [SerializeField]
        private GameObject _leftArrow;

        [SerializeField]
        private GameObject _rightArrow;

        [SerializeField]
        private Image _leftArrowImage;

        [SerializeField]
        private Image _rightArrowImage;

        [SerializeField]
        private LayoutElement _leftArrowLayout;

        [SerializeField]
        private LayoutElement _rightArrowLayout;

        [SerializeField]
        private Text _text;

        private float AlphaStep
        {
            get
            {
                if (_alphaStep == 0)
                {
                    var listHeight = gameObject.transform.parent.GetComponent<RectTransform>().sizeDelta.y;
                    _alphaStep = 1.2f / (listHeight / 2f);
                }

                return _alphaStep;
            }
        }

        private float _alphaStep = 0f;

        public void Update()
        {
            var parentPos = gameObject.transform.parent.localPosition.y;
            var thisPos = gameObject.transform.localPosition.y;
            var distanceFromCenter = Mathf.Abs(parentPos + thisPos);
            var targetColor = Text.color;
            targetColor.a = Mathf.Clamp(1 - AlphaStep * distanceFromCenter, 0, 1);
            Text.color = targetColor;
        }
    }
}
