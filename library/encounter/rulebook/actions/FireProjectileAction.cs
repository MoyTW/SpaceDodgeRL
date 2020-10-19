using System;

namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // Move this to its own file!
  public enum ProjectileType {
      SMALL_SHOTGUN
  }

  public class FireProjectileAction : EncounterAction {

    public int Power { get; private set; }
    // A function that takes the source position
    public Func<EncounterPosition, EncounterPath> PathFunction { get; private set; }
    public int Speed { get; private set; }
    public ProjectileType ProjectileType { get; private set; }

    public FireProjectileAction(
      string actorId,
      int power,
      Func<EncounterPosition, EncounterPath> pathFunction,
      int speed,
      ProjectileType projectileType
    ) : base(actorId, ActionType.FIRE_PROJECTILE) {
      this.Power = power;
      this.PathFunction = pathFunction;
      this.Speed = speed;
      this.ProjectileType = projectileType;
    }
  }
}