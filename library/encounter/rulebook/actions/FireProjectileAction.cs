using System;

namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // Move this to its own file!
  public enum ProjectileType {
    CUTTING_LASER,
    SMALL_GATLING,
    SMALL_SHOTGUN
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
        // TODO: Cutting laser range
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, 25),
        speed: 1 // TODO: If the player fires on the same tick that the enemy moves, the enemy will move before the laser gets a chance, causing a miss!
      );
    }

    // TODO: These should have spread
    public static FireProjectileAction CreateSmallShotgunAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.SMALL_SHOTGUN,
        power: 1,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, 25),
        speed: 25
      );
    }

    public static FireProjectileAction CreateSmallGatlingAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        ProjectileType.SMALL_GATLING,
        power: 2,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, 25),
        speed: 50
      );
    }
  }
}