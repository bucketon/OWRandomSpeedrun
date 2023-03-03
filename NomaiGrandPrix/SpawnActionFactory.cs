using System;
using System.Collections.Generic;

namespace NomaiGrandPrix
{
    public class SpawnActionFactory
    {
        private static SpawnActionFactory Instance = new SpawnActionFactory();
        private static Dictionary<string, Action[]> actionsMap = new Dictionary<string, Action[]>
        {
            { "Spawn_Module_Sunken", new Action[] { OpenSunkenModuleAirlock } },
            { "Spawn_Module_Intact", new Action[] { OpenProbeCannonAirlocks } },
            { "Spawn_TH_ZeroGCave", new Action[] { LowerZeroGElevator } }
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
            OpenAirlocks(Locator._giantsDeep);
        }

        private static void OpenProbeCannonAirlocks()
        {
            OpenAirlocks(Locator._orbitalProbeCannon);
        }

        private static void OpenAirlocks(AstroObject astroObject)
        {
            var airlocks = astroObject.GetComponentsInChildren<NomaiAirlock>();
            foreach (NomaiAirlock airlock in airlocks)
            {
                var position = airlock._closeSwitches[0].transform.position;
                airlock._listInterfaceOrb[0].SetOrbPosition(position);
            }
        }

        private static void LowerZeroGElevator()
        {
            var elevatorObj = Locator._timberHearth._rootSector.transform.Find("Interactables_TH/MineShaft/MineElevator");
            var elevator = elevatorObj.GetComponent<Elevator>();
            elevator._interactVolume.transform.Rotate(0f, 180f, 0f);
            elevator._goingToTheEnd = true;
            elevator._targetLocalPos = elevator._endLocalPos;
            elevator.StartLift();
        }
    }
}
