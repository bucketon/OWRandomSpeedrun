using UnityEngine;
using UnityEngine.UI;

public class SpawnPointTooltip : MonoBehaviour
{
  public Image LeftImage { get => _leftImage; }
  public Image RightImage { get => _rightImage; }
  public Image ConfirmImage { get => _confirmImage; }
  public Image CancelImage { get => _cancelImage; }
  public Text SeparatorText { get => _separatorText; }
  public Text NextText { get => _nextText; }

  [SerializeField]
  private Image _leftImage;
  
  [SerializeField]
  private Image _rightImage;

  [SerializeField]
  private Image _confirmImage;
  
  [SerializeField]
  private Image _cancelImage;

  [SerializeField]
  private Text _separatorText;

  [SerializeField]
  private Text _nextText;
}
