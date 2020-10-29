using System;

namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // Move this to its own file!
  public enum ProjectileType {
    CUTTING_LASER,
    SMALL_SHOTGUN
  }

  public class FireProjectileAction : EncounterAction {

    public string ProjectileName { get; private set; }
    public int Power { get; private set; }
    // A function that takes the source position
    public Func<EncounterPosition, EncounterPath> PathFunction { get; private set; }
    public int Speed { get; private set; }
    public ProjectileType ProjectileType { get; private set; }

    protected FireProjectileAction(
      string actorId,
      string projectileName,
      int power,
      Func<EncounterPosition, EncounterPath> pathFunction,
      int speed,
      ProjectileType projectileType
    ) : base(actorId, ActionType.FIRE_PROJECTILE) {
      this.ProjectileName = projectileName;
      this.Power = power;
      this.PathFunction = pathFunction;
      this.Speed = speed;
      this.ProjectileType = projectileType;
    }

    public static FireProjectileAction CreateCuttingLaserAction(string playerId, int playerPower, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        playerId,
        "cutting laser",
        playerPower,
        // TODO: Cutting laser range
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, 25),
        1, // TODO: If the player fires on the same tick that the enemy moves, the enemy will move before the laser gets a chance, causing a miss!
        ProjectileType.CUTTING_LASER
      );
    }

    // TODO: These should have spread
    public static FireProjectileAction CreateSmallShotgunAction(string actorId, EncounterPosition targetPosition) {
      return new FireProjectileAction(
        actorId,
        "small shotgun pellet",
        1,
        (sourcePos) => EncounterPathBuilder.BuildStraightLinePath(sourcePos, targetPosition, 25),
        25,
        ProjectileType.SMALL_SHOTGUN
      );
    }
  }
}