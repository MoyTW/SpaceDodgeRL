using System.Collections.Generic;

namespace SpaceDodgeRL.resources.gamedata {

  public static class EntityDefId {
    public static string SCOUT = "ENTITY_DEF_ID_SCOUT";
    public static string FIGHTER = "ENTITY_DEF_ID_FIGHTER";
    public static string GUNSHIP = "ENTITY_DEF_ID_GUNSHIP";
    public static string FRIGATE = "ENTITY_DEF_ID_FRIGATE";
    public static string DESTROYER = "ENTITY_DEF_ID_DESTROYER";
    public static string CRUISER = "ENTITY_DEF_ID_CRUISER";
    public static string CARRIER = "ENTITY_DEF_ID_CARRIER";
  }

  public static class EncounterDefId {
    public static string EMPTY_ENCOUNTER = "EMPTY_ENCOUNTER_ID";
    public static string SCOUT_ENCOUNTER = "SCOUT_ENCOUNTER_ID";
    public static string SCOUT_PAIR_ENCOUNTER = "SCOUT_PAIR_ENCOUNTER_ID";
    public static string SCOUT_TRIO_ENCOUNTER = "SCOUT_TRIO_ENCOUNTER_ID";
    public static string FIGHTER_ENCOUNTER = "FIGHTER_ENCOUNTER_ID";
    public static string FIGHTER_RECON_ENCOUNTER = "FIGHTER_RECON_ENCOUNTER_ID";
    public static string FIGHTER_PAIR_ENCOUNTER = "FIGHTER_PAIR_ENCOUNTER_ID";
    public static string FIGHTER_FLIGHT_ENCOUNTER = "FIGHTER_FLIGHT_ENCOUNTER_ID";
    public static string GUNSHIP_ENCOUNTER = "GUNSHIP_ENCOUNTER_ID";
    public static string GUNSHIP_RECON_ENCOUNTER = "GUNSHIP_RECON_ENCOUNTER_ID";
    // There was a GUNSHIP_PAIR in the original but it was never used...I assume that's just cruft?
    public static string FRIGATE_ENCOUNTER = "FRIGATE_ENCOUNTER_ID";
    public static string DESTROYER_ENCOUNTER = "DESTROYER_ENCOUNTER_ID";
    public static string CRUISER_ENCOUNTER = "CRUISER_ENCOUNTER_ID";
    public static string CARRIER_ENCOUNTER = "CARRIER_ENCOUNTER_ID";
    public static string FRIGATE_PAIR_ENCOUNTER = "FRIGATE_PAIR_ENCOUNTER_ID";
    public static string FRIGATE_GUNSHIP_ENCOUNTER = "FRIGATE_GUNSHIP_ENCOUNTER_ID";
    public static string DESTROYER_FIGHTER_ENCOUNTER = "DESTROYER_FIGHTER_ENCOUNTER_ID";
    public static string DESTROYER_FRIGATE_ENCOUNTER = "DESTROYER_FRIGATE_ENCOUNTER_ID";
    public static string CRUISER_PAIR_ENCOUNTER = "CRUISER_PAIR_ENCOUNTER_ID";
    public static string CRUISER_FIGHTER_ENCOUNTER = "CRUISER_FIGHTER_ENCOUNTER_ID";
    public static string CARRIER_SCREEN_ENCOUNTER = "CARRIER_SCREEN_ENCOUNTER_ID";
    public static string CARRIER_DESTROYER_ENCOUNTER = "CARRIER_DESTROYER_ENCOUNTER_ID";
    public static string CARRIER_TASK_FORCE_ENCOUNTER = "CARRIER_TASK_FORCE_ENCOUNTER_ID";
    public static string FAST_RESPONSE_FLEET_ENCOUNTER = "FAST_RESPONSE_FLEET_ENCOUNTER_ID";
    public static string HEAVY_STRIKE_FORCE_ENCOUNTER = "HEAVY_STRIKE_FORCE_ENCOUNTER_ID";
    public static string EVER_VICTORIOUS_FLEET_ENCOUNTER = "EVER_VICTORIOUS_FLEET_ENCOUNTER_ID";
  }

  public class EncounterDef {

    public string Id { get; }
    public string Name { get; }
    public List<string> EntityDefIds { get; }

    public EncounterDef(string id, string name, List<string> entityDefIds) {
      this.Id = id;
      this.Name = name;
      this.EntityDefIds = entityDefIds;
    }
  }

  public class LevelData {
    // TODO: easy to turn this into json and not literally just have it splatted here in a giant code file lol
    public static Dictionary<string, EncounterDef> EncounterDefs = new Dictionary<string, EncounterDef>() {
      { EncounterDefId.EMPTY_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.EMPTY_ENCOUNTER,
          "none",
          new List<string>()
        ) },
      { EncounterDefId.SCOUT_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.SCOUT_ENCOUNTER,
          "single scout",
          new List<string>() { EntityDefId.SCOUT }
        ) },
      { EncounterDefId.SCOUT_PAIR_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.SCOUT_PAIR_ENCOUNTER,
          "scout pair",
          new List<string>() { EntityDefId.SCOUT, EntityDefId.SCOUT }
        ) },
      { EncounterDefId.SCOUT_TRIO_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.SCOUT_TRIO_ENCOUNTER,
          "scout element",
          new List<string>() { EntityDefId.SCOUT, EntityDefId.SCOUT, EntityDefId.SCOUT }
        ) },
      { EncounterDefId.FIGHTER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FIGHTER_ENCOUNTER,
          "single fighter",
          new List<string>() { EntityDefId.FIGHTER }
        ) },
      { EncounterDefId.FIGHTER_RECON_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FIGHTER_RECON_ENCOUNTER,
          "recon flight",
          new List<string>() { EntityDefId.FIGHTER, EntityDefId.SCOUT, EntityDefId.SCOUT }
        ) },
      { EncounterDefId.FIGHTER_PAIR_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FIGHTER_PAIR_ENCOUNTER,
          "fighter element",
          new List<string>() { EntityDefId.FIGHTER, EntityDefId.FIGHTER }
        ) },
      { EncounterDefId.FIGHTER_FLIGHT_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FIGHTER_FLIGHT_ENCOUNTER,
          "fighter flight",
          new List<string>() { EntityDefId.FIGHTER, EntityDefId.FIGHTER, EntityDefId.FIGHTER, EntityDefId.FIGHTER }
        ) },
      { EncounterDefId.GUNSHIP_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.GUNSHIP_ENCOUNTER,
          "single gunship",
          new List<string>() { EntityDefId.GUNSHIP }
        ) },
      { EncounterDefId.GUNSHIP_RECON_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.GUNSHIP_RECON_ENCOUNTER,
          "gunship and escorts",
          new List<string>() { EntityDefId.GUNSHIP, EntityDefId.SCOUT, EntityDefId.SCOUT }
        ) },
      { EncounterDefId.FRIGATE_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FRIGATE_ENCOUNTER,
          "single frigate",
          new List<string>() { EntityDefId.FRIGATE }
        ) },
      { EncounterDefId.DESTROYER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.DESTROYER_ENCOUNTER,
          "single destroyer",
          new List<string>() { EntityDefId.DESTROYER }
        )
      },
      { EncounterDefId.CRUISER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CRUISER_ENCOUNTER,
          "single cruiser",
          new List<string>() { EntityDefId.CRUISER }
        ) },
      { EncounterDefId.CARRIER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CARRIER_ENCOUNTER,
          "single carrier",
          new List<string>() { EntityDefId.CARRIER }
        ) },
      { EncounterDefId.FRIGATE_PAIR_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FRIGATE_PAIR_ENCOUNTER,
          "pair of frigates",
          new List<string>() { EntityDefId.FRIGATE, EntityDefId.FRIGATE }
        ) },
      { EncounterDefId.FRIGATE_GUNSHIP_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FRIGATE_GUNSHIP_ENCOUNTER,
          "frigate and gunship",
          new List<string>() { EntityDefId.FRIGATE, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP }
        ) },
      { EncounterDefId.DESTROYER_FIGHTER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.DESTROYER_FIGHTER_ENCOUNTER,
          "destroyer and escorts",
          new List<string>() { EntityDefId.DESTROYER, EntityDefId.FIGHTER, EntityDefId.FIGHTER, EntityDefId.FIGHTER,
                               EntityDefId.FIGHTER, EntityDefId.FIGHTER }
        ) },
      { EncounterDefId.DESTROYER_FRIGATE_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.DESTROYER_FRIGATE_ENCOUNTER,
          "destroyer and frigate",
          new List<string>() { EntityDefId.DESTROYER, EntityDefId.FRIGATE }
        ) },
      { EncounterDefId.CRUISER_PAIR_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CRUISER_PAIR_ENCOUNTER,
          "cruiser pair",
          new List<string>() { EntityDefId.CRUISER, EntityDefId.CRUISER }
        ) },
      { EncounterDefId.CRUISER_FIGHTER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CRUISER_FIGHTER_ENCOUNTER,
          "cruiser and escorts",
          new List<string>() { EntityDefId.CRUISER, EntityDefId.FIGHTER, EntityDefId.FIGHTER, EntityDefId.FIGHTER,
                               EntityDefId.FIGHTER, EntityDefId.FIGHTER, EntityDefId.FIGHTER }
        ) },
      { EncounterDefId.CARRIER_SCREEN_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CARRIER_SCREEN_ENCOUNTER,
          "carrier and screening force",
          new List<string>() { EntityDefId.CARRIER, EntityDefId.SCOUT, EntityDefId.SCOUT, EntityDefId.SCOUT, EntityDefId.GUNSHIP,
                               EntityDefId.GUNSHIP, EntityDefId.GUNSHIP }
        ) },
      { EncounterDefId.CARRIER_DESTROYER_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CARRIER_DESTROYER_ENCOUNTER,
          "carrier and destroyer",
          new List<string>() { EntityDefId.CARRIER, EntityDefId.DESTROYER, EntityDefId.GUNSHIP }
        ) },
      { EncounterDefId.CARRIER_TASK_FORCE_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.CARRIER_TASK_FORCE_ENCOUNTER,
          "carrier task force",
          new List<string>() { EntityDefId.CARRIER, EntityDefId.CRUISER, EntityDefId.DESTROYER, EntityDefId.DESTROYER,
                               EntityDefId.FRIGATE, EntityDefId.FRIGATE, EntityDefId.FRIGATE }
        ) },
      { EncounterDefId.FAST_RESPONSE_FLEET_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.FAST_RESPONSE_FLEET_ENCOUNTER,
          "fast response fleet",
          new List<string>() { EntityDefId.DESTROYER, EntityDefId.DESTROYER, EntityDefId.DESTROYER, EntityDefId.DESTROYER,
                               EntityDefId.DESTROYER, EntityDefId.DESTROYER, EntityDefId.FRIGATE, EntityDefId.FRIGATE,
                               EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP,
                               EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP,
                               EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP, EntityDefId.GUNSHIP,  }
        ) },
      { EncounterDefId.HEAVY_STRIKE_FORCE_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.HEAVY_STRIKE_FORCE_ENCOUNTER,
          "heavy strike force",
          new List<string>() { EntityDefId.CRUISER, EntityDefId.CRUISER, EntityDefId.CRUISER, EntityDefId.CRUISER,
                               EntityDefId.CARRIER, EntityDefId.CARRIER, EntityDefId.DESTROYER, EntityDefId.DESTROYER }
        ) },
      { EncounterDefId.EVER_VICTORIOUS_FLEET_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.EVER_VICTORIOUS_FLEET_ENCOUNTER,
          "Ever Victorious Fleet",
          new List<string>() { EntityDefId.CRUISER, EntityDefId.CRUISER, EntityDefId.CRUISER, EntityDefId.CRUISER,
                               EntityDefId.CRUISER, EntityDefId.CARRIER, EntityDefId.CARRIER, EntityDefId.CARRIER,
                               EntityDefId.DESTROYER, EntityDefId.DESTROYER, EntityDefId.FRIGATE, EntityDefId.FRIGATE,
                               EntityDefId.FRIGATE, EntityDefId.FRIGATE, EntityDefId.FRIGATE }
        ) },
    };
  }
}