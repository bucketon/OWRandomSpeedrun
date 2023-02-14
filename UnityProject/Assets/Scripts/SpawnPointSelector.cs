using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpawnPointSelector
{
  public class SpawnPointSelector : MonoBehaviour
  {
    public SpawnPointList FromSpawnPointList {
      get => _fromSpawnPointList;
      set => _fromSpawnPointList = value;
    }

    public SpawnPointList ToSpawnPointList {
      get => _toSpawnPointList;
      set => _toSpawnPointList = value;
    }

    [SerializeField]
    private SpawnPointList _fromSpawnPointList;

    [SerializeField]
    private SpawnPointList _toSpawnPointList;
  }
}