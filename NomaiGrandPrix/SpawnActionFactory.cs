using System;

namespace NomaiGrandPrix
{
  public class SpawnActionFactory
  {
    private static SpawnActionFactory Instance = new SpawnActionFactory();


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
      
      switch (spawnId)
      {
        case "Spawn_Module_Sunken":
          return new Action[] { OpenSunkenModuleAirlock };
        default:
          return new Action[0];
      }

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