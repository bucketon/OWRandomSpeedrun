using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamOptimism
{
    public enum Area
    {
        None = 1,
        SunStation = 1 << 1,
        AshTwin = 1 << 2,
        EmberTwin = 1 << 3,
        TimberHearth = 1 << 4,
        BrittleHollow = 1 << 5,
        GiantsDeep = 1 << 6,
        DarkBramble = 1 << 7,
        Interloper = 1 << 8,
        WhiteHole = 1 << 9,
        QuantumMoon = 1 << 10,
        Stranger = 1 << 11,
        DreamZone = 1 << 12,
    }

    public struct SpawnPointConfig
    {
        public string internalId;
        public string displayName;
        public Area area;
        public bool isDreamZone;
        public bool shouldSpawn;
        public bool shouldGoal;
        public bool isThVillage;
    }

    public class SpawnPointPool
    {
        private readonly static char[] LINE_SEPARATORS = new char[] { '\r', '\n' };
        private readonly static char FIELD_SEPARATOR = '\t';

        public IList<SpawnPointConfig> SpawnPointConfigs
        {
            get => _spawnPointConfigs;
        }

        private List<SpawnPointConfig> _spawnPointConfigs;

        public SpawnPointConfig RandomSpawnPointConfig(Random random, Func<SpawnPointConfig, bool> filter = null)
        {
            var filtered = filter != null ? _spawnPointConfigs.Where(filter).ToList() : _spawnPointConfigs;
            var randomIndex = random.Next(filtered.Count);
            return filtered[randomIndex];
        }

        private SpawnPointPool(List<SpawnPointConfig> configs)
        {
            this._spawnPointConfigs = configs;
        }

        public static SpawnPointPool FromTsv(string pathToTsv, Func<SpawnPointConfig, bool> filter = null)
        {
            if (filter == null)
            {
                filter = spawnConfig => true;
            }
            var configs = ParseTsv(pathToTsv).Select(line => BuildSpawnPointConfig(line)).Where(filter).ToList();
            return new SpawnPointPool(configs);
        }

        private static SpawnPointConfig BuildSpawnPointConfig(string[] line) =>
            new SpawnPointConfig
            {
                internalId = line[0],
                displayName = line[1],
                area = line[2].Equals("") ? Area.None : (Area)Enum.Parse(typeof(Area), line[2]),
                isDreamZone = bool.Parse(line[3]),
                shouldSpawn = bool.Parse(line[4]),
                shouldGoal = bool.Parse(line[5]),
                isThVillage = bool.Parse(line[6]),
            };

        private static string[][] ParseTsv(string pathToTsv)
        {
            using (var reader = new StreamReader(pathToTsv))
            {
                var contents = reader.ReadToEnd();
                var lines = contents.Split(LINE_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
                return lines
                    .Skip(1) // Ignore the header row
                    .Select(line => line.Split(FIELD_SEPARATOR))
                    .ToArray();
            }
        }
    }
}
