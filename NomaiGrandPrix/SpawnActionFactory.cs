using System;
using System.Collections.Generic;
using UnityEngine;

namespace NomaiGrandPrix
{
    public class SpawnActionFactory
    {
        private static SpawnActionFactory Instance = new SpawnActionFactory();
        private const int SAND_CRUSH_THRESHOLD_SECS = 415;
        private static Vector3 CAMPFIRE_SPAWN_POSITION = new Vector3(18f, 0.18f, -21f);
        private static Vector3 CAMPFIRE_ROTATION = new Vector3(95f, 0f, 38f);

        private static Dictionary<string, Action[]> actionsMap = new Dictionary<string, Action[]>
        {
            { "Spawn_Module_Sunken", new Action[] { OpenSunkenModuleAirlock } },
            { "Spawn_Module_Intact", new Action[] { OpenProbeCannonAirlocks } },
            { "Spawn_TH_ZeroGCave", new Action[] { LowerZeroGElevator } },
            { "Spawn_TimeLoopDevice", new Action[] { CreateAshTwinCampfire, ActivateAshTwinWarpReceiver } },
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

        private static void CreateAshTwinCampfire()
        {
            var timeLoopRing = GameObject.Find("TimeLoopRing_Body/Interactibles_TimeLoopRing_Hidden");
            var campfirePrefab = Locator._hourglassTwinA._rootSector.transform.Find(
                "Sector_NorthHemisphere/Sector_NorthSurface/Sector_Lakebed/Interactables_Lakebed/Lakebed_VisibleFrom_Far/Prefab_HEA_Campfire"
            );
            var campfire = GameObject.Instantiate(campfirePrefab.gameObject, timeLoopRing.transform);

            campfire.transform.localPosition = CAMPFIRE_SPAWN_POSITION;
            campfire.transform.Rotate(CAMPFIRE_ROTATION, Space.Self);

            var interactReceiver = campfire.GetComponentInChildren<InteractReceiver>();
            interactReceiver.Start();
            interactReceiver.EnableInteraction();

            var attachPoint = campfire.GetComponentInChildren<PlayerAttachPoint>();
            attachPoint.Start();
            attachPoint.enabled = true;
        }

        private static void ActivateAshTwinWarpReceiver()
        {
            var returnPlatformGO = Locator._hourglassTwinB._rootSector.transform.Find(
                "Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)"
            );
            var returnPlatform = returnPlatformGO.GetComponent<NomaiWarpTransmitter>();
            returnPlatform.OnReceiveWarpedBody += KillPlayerIfEnteringSand;

            var warpReceiver = GameObject
                .Find("TimeLoopRing_Body/Interactibles_TimeLoopRing_Hidden/Prefab_NOM_WarpReceiver")
                .GetComponent<NomaiWarpReceiver>();
            warpReceiver._exitPlatformTime = Time.time;
            warpReceiver._waitToActivateReturnWarp = true;
            warpReceiver._returnPlatform = returnPlatform;
        }

        private static void KillPlayerIfEnteringSand(
            OWRigidbody warpedBody,
            NomaiWarpPlatform startPlaform,
            NomaiWarpPlatform targetPlatform
        )
        {
            if (Time.timeSinceLevelLoad <= SAND_CRUSH_THRESHOLD_SECS)
            {
                Locator._deathManager.KillPlayer(DeathType.Crushed);
            }
        }
    }
}
