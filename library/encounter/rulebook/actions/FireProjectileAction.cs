using System;
using System.Collections.Generic;

namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // Move this to its own file!
  public enum ProjectileType {
    CUTTING_LASER,
    SMALL_CANNON,
    SMALL_GATLING,
    SMALL_SHOTGUN,
    RAILGUN,
    REVERSER
  }

  public class FireProjectileAction : EncounterAction {

    public ProjectileType ProjectileType { get; private set; }
    public int Power { get; private set; }
    // A function that takes the source position
    public Func<EncounterPosition, EncounterPath> PathFunction { get; private set; }
    public int Speed { get; private set; }

    protected FireProjectileAction(
      string actorId,
      ProjectileType projectileType,
      int power,
      Func<EncounterPosition, EncounterPath> pathFunction,
      int speed
    ) : base(actorId, ActionType.FIRE_PROJECTILE) {
      this.ProjectileType = projectileType;
      this.Power = power;
      this.PathFunction = pathFunction;
      this.Speed = speed;
    }

    public static FireProjectileAction CreateCuttingLaserAction(string playerId, int playerPower, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        playerId,
        ProjectileType.CUTTING_LASER,
        power: playerPower,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, endsAtTarget: true),
        speed: 0
      );
    }

    public static FireProjectileAction CreateReverserAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.REVERSER,
        power: 2,
        (sourcePos) => EncounterPathBuilder.BuildReverseLinePath(sourcePos, targetPosition, overshoot: 4),
        speed: 33
      );
    }

    private static FireProjectileAction CreateSmallShotgunAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.SMALL_SHOTGUN,
        power: 1,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, maxSteps: 25),
        speed: 25
      );
    }

    /**
     * Fires a spread of shotgun pellets in a box formation around the centerpoint. Spread is half the width/height of the box.
     */
    public static List<FireProjectileAction> CreateSmallShotgunAction(string actorId, EncounterPosition targetPosition, int numPellets, int spread, Random seededRand) {
      if (spread == 0) { throw new NotImplementedException("shotguns can't have 0 spread what is this"); }

      List<FireProjectileAction> pellets = new List<FireProjectileAction>();

      for (int i = 0; i < numPellets; i++) {
        var dx = seededRand.Next(spread * 2 + 1) - spread;
        var dy = seededRand.Next(spread * 2 + 1) - spread;
        pellets.Add(CreateSmallShotgunAction(actorId, new EncounterPosition(targetPosition.X + dx, targetPosition.Y + dy)));
      }

      return pellets;
    }

    public static FireProjectileAction CreateSmallGatlingAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.SMALL_GATLING,
        power: 2,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, maxSteps: 25),
        speed: 50
      );
    }

    public static FireProjectileAction CreateSmallCannonAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.SMALL_CANNON,
        power: 5,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, maxSteps: 25),
        speed: 50
      );
    }

    public static FireProjectileAction CreateRailgunAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.RAILGUN,
        power: 15,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, maxSteps: 40),
        speed: 20
      );
    }
  }
}