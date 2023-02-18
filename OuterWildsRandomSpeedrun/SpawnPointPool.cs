using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OuterWildsRandomSpeedrun
{
    public struct SpawnPointConfig
    {
        public string internalId;
        public string displayName;
    }

    public class SpawnPointPool
    {
        public List<SpawnPointConfig> SpawnPointConfigs
        {
            get => _spawnPointConfigs;
        }

        private List<SpawnPointConfig> _spawnPointConfigs;

        private SpawnPointPool(List<SpawnPointConfig> configs)
        {
            this._spawnPointConfigs = configs;
        }

        public static SpawnPointPool FromTsv(string pathToTsv)
        {
            var configs = ParseTsv(pathToTsv).Select(line => BuildSpawnPointConfig(line)).ToList();
            return new SpawnPointPool(configs);
        }

        private static SpawnPointConfig BuildSpawnPointConfig(string[] line) =>
            new SpawnPointConfig { internalId = line[0], displayName = line[1] };

        private readonly static char[] LINE_SEPARATORS = new char[] { '\r', '\n' };
        private readonly static char FIELD_SEPARATOR = '\t';
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

        public SpawnPointConfig RandomSpawnPointConfig(Random random)
        {
            var randomIndex = random.Next(_spawnPointConfigs.Count);
            return _spawnPointConfigs[randomIndex];
        }
    }
}
