using System;
using System.Collections.Generic;

namespace NomaiGrandPrix
{
    public class SpawnActionFactory
    {
        private static SpawnActionFactory Instance = new SpawnActionFactory();
        private static Dictionary<string, Action[]> actionsMap = new Dictionary<string, Action[]>
        {
            { "Spawn_Module_Sunken", new Action[] { OpenSunkenModuleAirlock } }
        };

        private SpawnActionFactory()
        {
            // Private constructor
        }

        public static Action[] GetActionsForSpawn(string spawnId)
        {
            if (spawnId == null)
            {
                throw new InvalidOperationException("Spawn ID for getting actions cannot be null");
            }

            return actionsMap.GetValueOrDefault(spawnId, new Action[0]);
        }

        private static void OpenSunkenModuleAirlock()
        {
            var giantsDeep = Locator._giantsDeep;
            var airlock = giantsDeep.GetComponentInChildren<NomaiAirlock>();
            var position = airlock._closeSwitches[0].transform.position;
            airlock._listInterfaceOrb[0].SetOrbPosition(position);
        }
    }
}
