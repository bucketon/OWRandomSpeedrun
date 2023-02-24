using UnityEngine;
using UnityEngine.UI;

namespace SpawnPointSelector
{
    public class SpawnPointSelector : MonoBehaviour
    {
        public SpawnPointList FromSpawnPointList
        {
            get => _fromSpawnPointList;
        }

        public SpawnPointList ToSpawnPointList
        {
            get => _toSpawnPointList;
        }

        public SpawnPointTooltip Tooltip
        {
            get => _tooltip;
        }

        public Text CourseSelectText
        {
            get => _courseSelectText;
        }

        [SerializeField]
        private SpawnPointList _fromSpawnPointList;

        [SerializeField]
        private SpawnPointList _toSpawnPointList;

        [SerializeField]
        private Text _courseSelectText;

        [SerializeField]
        private SpawnPointTooltip _tooltip;
    }
}
