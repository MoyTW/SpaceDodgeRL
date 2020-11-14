namespace SpaceDodgeRL.scenes.components {

  /**
   * Tracks levels and level-up bonuses. TODO: track levels, level-up bonuses
   */
  public class XPTrackerComponent : Component {
    public static readonly string ENTITY_GROUP = "XP_TRACKER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int CurrentXPTotal { get; private set; }

    public static XPTrackerComponent Create(int startingXPTotal = 0) {
      var component = new XPTrackerComponent();
      component.CurrentXPTotal = startingXPTotal;
      return component;
    }

    public void AddXP(int xp) {
      this.CurrentXPTotal += xp;
      // TODO: Display this somewhere
      Godot.GD.Print("XP gained, XP total now at " + this.CurrentXPTotal);
    }
  }
}