namespace SpaceDodgeRL.library.encounter.rulebook.actions {

  // Move this to its own file!
  public enum ProjectileType {
      SMALL_SHOTGUN
  }

  public class FireProjectileAction : EncounterAction {

    public int Power { get; private set; }
    public EncounterPath Path { get; private set; }
    public int Speed { get; private set; }
    public ProjectileType ProjectileType { get; private set; }

    public FireProjectileAction(
      string actorId,
      int power,
      EncounterPath path,
      int speed,
      ProjectileType projectileType
    ) : base(actorId, ActionType.FIRE_PROJECTILE) {
      this.Power = power;
      this.Path = path;
      this.Speed = speed;
      this.ProjectileType = projectileType;
    }
  }
}