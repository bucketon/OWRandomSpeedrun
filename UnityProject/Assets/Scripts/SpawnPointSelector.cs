using UnityEngine;

namespace SpawnPointSelector
{
  public class SpawnPointSelector : MonoBehaviour
  {
    public SpawnPointList FromSpawnPointList {
      get => _fromSpawnPointList;
    }

    public SpawnPointList ToSpawnPointList {
      get => _toSpawnPointList;
    }

    public SpawnPointTooltip Tooltip {
      get => _tooltip;
    }

    [SerializeField]
    private SpawnPointList _fromSpawnPointList;

    [SerializeField]
    private SpawnPointList _toSpawnPointList;

    [SerializeField]
    private SpawnPointTooltip _tooltip;
  }
}
