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

        public SpawnPointConfig(string internalId, string displayName)
        {
            this.internalId = internalId;
            this.displayName = displayName;
        }
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

        private static SpawnPointConfig BuildSpawnPointConfig(string[] line)
        {
            var internalId = line[0];
            var displayName = line[1];
            return new SpawnPointConfig(internalId, displayName);
        }

        const char FIELD_SEPARATOR = '\t';
        private static string[][] ParseTsv(string pathToTsv)
        {
            using (var reader = new StreamReader(pathToTsv))
            {
                // TODO: Discard header row
                var contents = reader.ReadToEnd();
                var lines = contents.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                return lines.Select(line => line.Split(FIELD_SEPARATOR)).ToArray();
            }
        }
    }
}