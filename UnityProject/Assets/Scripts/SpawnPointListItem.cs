using UnityEngine;
using UnityEngine.UI;

namespace SpawnPointSelector
{
  public class SpawnPointListItem : MonoBehaviour
  {
    public GameObject LeftArrow {
      get => _leftArrow;
      set => _leftArrow = value;
    }

    public GameObject RightArrow {
      get => _rightArrow;
      set => _rightArrow = value;
    }

    public Text Text {
      get => _text;
      set => _text = value;
    }
    
    [SerializeField]
    private GameObject _leftArrow;

    [SerializeField]
    private GameObject _rightArrow;

    [SerializeField]
    private Text _text;
  }
}
