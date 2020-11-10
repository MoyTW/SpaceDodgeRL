namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectBoostPowerComponent: Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_BOOST_POWER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    // Rename "power" (as in, attack power) to something that's less generic
    public int BoostPower { get; private set; }
    public int Duration { get; private set; }

    public static UseEffectBoostPowerComponent Create(int boostPower, int duration) {
      var component = new UseEffectBoostPowerComponent();

      component.BoostPower = boostPower;
      component.Duration = duration;

      return component;
    }
  }
}