using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter.state {

  public static class EncounterStateBuilder {

    public static int ZONE_MIN_SIZE = 20;
    public static int ZONE_MAX_SIZE = 40;

    private static void PopulateZone(EncounterZone zone, int dungeonLevel, Random seededRand, EncounterState state, bool safe=false) {
      // Add satellites
      int numSatellites = LevelData.GetNumberOfSatellites(dungeonLevel);
      for (int i = 0; i < numSatellites; i++) {
        var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
        var satellite = EntityBuilder.CreateSatelliteEntity();
        state.PlaceEntity(satellite, unblockedPosition);
      }

      EncounterDef encounterDef;
      if (safe) {
        encounterDef = LevelData.GetEncounterDefById(EncounterDefId.EMPTY_ENCOUNTER);
      } else {
        encounterDef = LevelData.ChooseEncounter(dungeonLevel, seededRand);
      }
      zone.ReadoutEncounterName = encounterDef.Name;

      if (encounterDef.EntityDefIds.Count > 0) {
        string activationGroupId = Guid.NewGuid().ToString();
        foreach (string entityDefId in encounterDef.EntityDefIds) {
          var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
          var newEntity = EntityBuilder.CreateEnemyByEntityDefId(entityDefId, activationGroupId, state.CurrentTick);
          state.PlaceEntity(newEntity, unblockedPosition);
        }
      }

      var chosenItemDefs = LevelData.ChooseItemDefs(dungeonLevel, seededRand);
      foreach(string chosenItemDefId in chosenItemDefs) {
        var unblockedPosition = zone.RandomUnblockedPosition(seededRand, state);
        var newEntity = EntityBuilder.CreateItemByEntityDefId(chosenItemDefId);
        state.PlaceEntity(newEntity, unblockedPosition);
        zone.AddItemToReadout(newEntity);
      }
    }

    private static void InitializeMapAndAddBorderWalls(EncounterState state, int width, int height) {
      // Initialize the map with empty tiles
      state.MapWidth = width;
      state.MapHeight = height;
      state._encounterTiles = new EncounterTile[width, height];
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          state._encounterTiles[x, y] = new EncounterTile();
        }
      }

      // Create border walls to prevent objects running off the map
      for (int x = 0; x < width; x++) {
        for (int y = 0; y < height; y++) {
          if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
            state.PlaceEntity(EntityBuilder.CreateMapWallEntity(), new EncounterPosition(x, y));
          }
        }
      }
    }

    public static void PopulateStateForLevel(Entity player, int dungeonLevel, EncounterState state, Random seededRand,
        int width = 300, int height = 300, int maxZoneGenAttempts = 100) {
      InitializeMapAndAddBorderWalls(state, width, height);

      // Place each empty zone onto the map
      int zoneGenAttemps = 0;
      List<EncounterZone> zones = new List<EncounterZone>();
      while (zoneGenAttemps < maxZoneGenAttempts && zones.Count < LevelData.GetNumberOfZones(dungeonLevel)) {
        int zoneWidth = seededRand.Next(ZONE_MIN_SIZE, ZONE_MAX_SIZE + 1);
        int zoneHeight = seededRand.Next(ZONE_MIN_SIZE, ZONE_MAX_SIZE + 1);
        int zoneX = seededRand.Next(1, state.MapWidth - zoneWidth);
        int zoneY = seededRand.Next(1, state.MapHeight - zoneHeight);

        var newZone = new EncounterZone(Guid.NewGuid().ToString(), new EncounterPosition(zoneX, zoneY), zoneWidth, zoneHeight,
          "Zone " + zones.Count.ToString());

        bool overlaps = zones.Any(existing => existing.Intersects(newZone));
        if (!overlaps) {
          zones.Add(newZone);
        }
      }
      state._zones = zones;

      // Add the player to the map
      var playerZoneIdx = seededRand.Next(0, zones.Count);
      state.PlaceEntity(player, zones[playerZoneIdx].Center);
      // TODO: delete the following test item
      var nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + 2, zones[playerZoneIdx].Center.Y + 1);
      state.PlaceEntity(EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_RED_PAINT), nextToPlayer);
      nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + 1, zones[playerZoneIdx].Center.Y + 1);
      state.PlaceEntity(EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_EMP), nextToPlayer);
      for (int i = 0; i < 26; i++) {
        nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + i, zones[playerZoneIdx].Center.Y + 3);
        state.PlaceEntity(EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_EXTRA_BATTERY), nextToPlayer);
      }
      
      /*
      nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + 5, zones[playerZoneIdx].Center.Y + 5);
      ActivationGroup activationGroup = new ActivationGroup();
      state.PlaceEntity(EntityBuilder.CreateEnemyByEntityDefId(EntityDefId.CARRIER, activationGroup, 0), nextToPlayer);
      nextToPlayer = new EncounterPosition(zones[playerZoneIdx].Center.X + 30, zones[playerZoneIdx].Center.Y + 30);
      state.PlaceEntity(EntityBuilder.CreateEnemyByEntityDefId(EntityDefId.SCOUT, activationGroup, 0), nextToPlayer);
      */

      // Add all the various zone features to the map
      // TODO: Draw this from LevelData instead of literally special-casing level 10 here
      if (dungeonLevel != 10) {
        // Generate the stairs (maybe we should refer interally as something more themetically appropriate?)
        // You can get stairs in your starting zone, but you probably shouldn't take them...
        var stairsZone = zones[playerZoneIdx]; // TODO: not this
        //var stairsZone = zones[seededRand.Next(0, zones.Count)];
        var stairsPosition = stairsZone.RandomUnblockedPosition(seededRand, state);
        var stairs = EntityBuilder.CreateStairsEntity();
        state.PlaceEntity(stairs, stairsPosition);
        stairsZone.AddFeatureToReadout(stairs);

        // Generate intel
        var intelZone = zones[playerZoneIdx]; // TODO: not this
        //var intelZone = zones[seededRand.Next(0, zones.Count)];
        var intelPosition = intelZone.RandomUnblockedPosition(seededRand, state);
        var intel = EntityBuilder.CreateIntelEntity(dungeonLevel + 1);
        state.PlaceEntity(intel, intelPosition);
        intelZone.AddFeatureToReadout(intel);
      }

      // Populate each zone with an encounter
      foreach (EncounterZone zone in zones) {
        if (zone == zones[playerZoneIdx]) {
          PopulateZone(zone, dungeonLevel, seededRand, state, safe: true);
        } else {
          PopulateZone(zone, dungeonLevel, seededRand, state);
        }
      }
    }
  }
}