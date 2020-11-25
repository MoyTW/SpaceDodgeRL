using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.resources.gamedata {

  public static class EntityDefId {
    // Enemies
    public static string SCOUT = "ENTITY_DEF_ID_SCOUT";
    public static string FIGHTER = "ENTITY_DEF_ID_FIGHTER";
    public static string GUNSHIP = "ENTITY_DEF_ID_GUNSHIP";
    public static string FRIGATE = "ENTITY_DEF_ID_FRIGATE";
    public static string DESTROYER = "ENTITY_DEF_ID_DESTROYER";
    public static string CRUISER = "ENTITY_DEF_ID_CRUISER";
    public static string CARRIER = "ENTITY_DEF_ID_CARRIER";
    public static string DIPLOMAT = "ENTITY_DEF_ID_DIPLOMAT";

    // Items
    public static string ITEM_DUCT_TAPE = "ENTITY_DEF_ID_ITEM_DUCT_TAPE";
    public static string ITEM_EXTRA_BATTERY = "ENTITY_DEF_ID_ITEM_EXTRA_BATTERY";
    public static string ITEM_RED_PAINT = "ENTITY_DEF_ID_ITEM_RED_PAINT";
    public static string ITEM_EMP = "ENTITY_DEF_ID_ITEM_EMP";
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
    public static string DIPLOMAT_ENCOUNTER = "DIPLOMAT_ENCOUNTER_ID";
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

  public class WeightedOption<T> {
    public T Option { get; }
    public int Weight { get; }

    public WeightedOption(T option, int weight) {
      this.Option = option;
      this.Weight = weight;
    }
  }

  public static class ChallengeRating {
    public static int ZERO = 0;
    public static int ONE = 1;
    public static int TWO = 2;
    public static int THREE = 3;
    public static int FOUR = 4;
    public static int FIVE = 5;
    public static int SIX = 6;
    public static int SEVEN = 7;
    public static int END = int.MaxValue;
  }

  public static class DungeonLevel {
    public static int ONE = 1;
    public static int TWO = 2;
    public static int THREE = 3;
    public static int FOUR = 4;
    public static int FIVE = 5;
    public static int SIX = 6;
    public static int SEVEN = 7;
    public static int EIGHT = 8;
    public static int NINE = 9;
    public static int TEN = 10;
  }

  public static class LevelData {
    // TODO: easy to turn this into json and not literally just have it splatted here in a giant code file lol
    private static Dictionary<string, EncounterDef> EncounterDefs = new Dictionary<string, EncounterDef>() {
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
      { EncounterDefId.DIPLOMAT_ENCOUNTER,
        new EncounterDef(
          EncounterDefId.DIPLOMAT_ENCOUNTER,
          "The Diplomat",
          new List<string> { EntityDefId.DIPLOMAT }
        ) },
    };

    // Mapping from ChallengeRating -> EncounterDefId
    private static Dictionary<int, WeightedOption<string>[]> ChallengeRatingToEncounterDefIdOptions = new Dictionary<int, WeightedOption<string>[]>() {
      { ChallengeRating.ZERO,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.SCOUT_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.SCOUT_PAIR_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.SCOUT_TRIO_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.FIGHTER_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.ONE,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.FIGHTER_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.FIGHTER_RECON_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.FIGHTER_PAIR_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.GUNSHIP_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.TWO,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.GUNSHIP_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.FIGHTER_FLIGHT_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.GUNSHIP_RECON_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.FRIGATE_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.THREE,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.FRIGATE_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.FRIGATE_PAIR_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.FRIGATE_GUNSHIP_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.DESTROYER_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.FOUR,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.DESTROYER_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.DESTROYER_FIGHTER_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.DESTROYER_FRIGATE_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.CRUISER_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.FIVE,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.CRUISER_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.CRUISER_PAIR_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.CRUISER_FIGHTER_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.CARRIER_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.SIX,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.CARRIER_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.CARRIER_SCREEN_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.CARRIER_DESTROYER_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.CARRIER_TASK_FORCE_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.SEVEN,
        new WeightedOption<string>[4] {
          new WeightedOption<string>(EncounterDefId.CARRIER_TASK_FORCE_ENCOUNTER, 50),
          new WeightedOption<string>(EncounterDefId.FAST_RESPONSE_FLEET_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.HEAVY_STRIKE_FORCE_ENCOUNTER, 100),
          new WeightedOption<string>(EncounterDefId.EVER_VICTORIOUS_FLEET_ENCOUNTER, 50)
        }
      },
      { ChallengeRating.END,
        new WeightedOption<string>[1] {
          new WeightedOption<string>(EncounterDefId.DIPLOMAT_ENCOUNTER, 1)
        }
      }
    };

    // Mapping from DungeonLevel -> ChallengeRating - this is really hard to read, with all the cruft!
    private static Dictionary<int, WeightedOption<int>[]> DungeonLevelToChallengeRatingOptions = new Dictionary<int, WeightedOption<int>[]>() {
      {1, new WeightedOption<int>[2] {
        new WeightedOption<int>(ChallengeRating.ZERO, 20),
        new WeightedOption<int>(ChallengeRating.ONE, 10)
      }},
      {2, new WeightedOption<int>[3] {
        new WeightedOption<int>(ChallengeRating.ZERO, 10),
        new WeightedOption<int>(ChallengeRating.ONE, 20),
        new WeightedOption<int>(ChallengeRating.TWO, 10)
      }},
      {3, new WeightedOption<int>[3] {
        new WeightedOption<int>(ChallengeRating.ONE, 10),
        new WeightedOption<int>(ChallengeRating.TWO, 20),
        new WeightedOption<int>(ChallengeRating.THREE, 10)
      }},
      {4, new WeightedOption<int>[3] {
        new WeightedOption<int>(ChallengeRating.TWO, 10),
        new WeightedOption<int>(ChallengeRating.THREE, 20),
        new WeightedOption<int>(ChallengeRating.FOUR, 10)
      }},
      {5, new WeightedOption<int>[3] {
        new WeightedOption<int>(ChallengeRating.THREE, 10),
        new WeightedOption<int>(ChallengeRating.FOUR, 20),
        new WeightedOption<int>(ChallengeRating.FIVE, 10)
      }},
      {6, new WeightedOption<int>[3] {
        new WeightedOption<int>(ChallengeRating.FOUR, 10),
        new WeightedOption<int>(ChallengeRating.FIVE, 20),
        new WeightedOption<int>(ChallengeRating.SIX, 10)
      }},
      {7, new WeightedOption<int>[2] {
        new WeightedOption<int>(ChallengeRating.FIVE, 10),
        new WeightedOption<int>(ChallengeRating.SIX, 20)
      }},
      {8, new WeightedOption<int>[1] {
        new WeightedOption<int>(ChallengeRating.SIX, 1)
      }},
      {9, new WeightedOption<int>[1] {
        new WeightedOption<int>(ChallengeRating.SEVEN, 1)
      }},
      {10, new WeightedOption<int>[1] {
        new WeightedOption<int>(ChallengeRating.END, 1)
      }}
    };

    private static Dictionary<int, int> DungeonLevelToMaxItems = new Dictionary<int, int>() {
      { DungeonLevel.ONE, 3 },
      { DungeonLevel.TWO, 3 },
      { DungeonLevel.THREE, 3 },
      { DungeonLevel.FOUR, 2 },
      { DungeonLevel.FIVE, 2 },
      { DungeonLevel.SIX, 1 },
      { DungeonLevel.SEVEN, 1 },
      { DungeonLevel.EIGHT, 1 },
      { DungeonLevel.NINE, 1 },
      { DungeonLevel.TEN, 0 }
    };

    private static Dictionary<int, int> DungeonLevelToNumberOfSatellites = new Dictionary<int, int>() {
      { DungeonLevel.ONE, 20 },
      { DungeonLevel.TWO, 20 },
      { DungeonLevel.THREE, 15 },
      { DungeonLevel.FOUR, 15 },
      { DungeonLevel.FIVE, 10 },
      { DungeonLevel.SIX, 10 },
      { DungeonLevel.SEVEN, 10 },
      { DungeonLevel.EIGHT, 10 },
      { DungeonLevel.NINE, 30 },
      { DungeonLevel.TEN, 0 }
    };

    private static WeightedOption<string>[] ItemOptions = new WeightedOption<string>[4] {
      new WeightedOption<string>(EntityDefId.ITEM_DUCT_TAPE, 45),
      new WeightedOption<string>(EntityDefId.ITEM_EXTRA_BATTERY, 25),
      new WeightedOption<string>(EntityDefId.ITEM_RED_PAINT, 10),
      new WeightedOption<string>(EntityDefId.ITEM_EMP, 10)
    };

    // No thought put into perf surely that's fine right (well it only happens once per level soooo)
    private static T PickChoice<T>(WeightedOption<T>[] choices, Random seededRand) {
      int sum = 0;
      foreach(WeightedOption<T> option in choices) {
        sum += option.Weight;
      }

      int toNext = seededRand.Next(sum);
      foreach(WeightedOption<T> option in choices) {
        if (toNext < option.Weight) {
          return option.Option;
        } else {
          toNext -= option.Weight;
        }
      }
      // It will never actually reach this, since Next is exclusive of the input, but it throws up a warning, so we have it.
      return choices[choices.Length - 1].Option;
    }

    public static int GetNumberOfZones(int level) {
      if (level == 10) {
        return 2;
      } else {
        return 10;
      }
    }

    public static int GetNumberOfSatellites(int level) {
      return DungeonLevelToNumberOfSatellites[level];
    }

    public static EncounterDef GetEncounterDefById(string id) {
      return EncounterDefs[id];
    }

    public static EncounterDef ChooseEncounter(int level, Random seededRand) {
      int challengeRating = PickChoice(DungeonLevelToChallengeRatingOptions[level], seededRand);
      string encounterDefId = PickChoice(ChallengeRatingToEncounterDefIdOptions[challengeRating], seededRand);
      return EncounterDefs[encounterDefId];
    }

    public static List<string> ChooseItemDefs(int level, Random seededRand) {
      List<string> chosenItems = new List<string>();

      int numItems = seededRand.Next(DungeonLevelToMaxItems[level] + 1);
      for (int i = 0; i < numItems; i++) {
        chosenItems.Add(PickChoice(ItemOptions, seededRand));
      }

      return chosenItems;
    }
  }
}