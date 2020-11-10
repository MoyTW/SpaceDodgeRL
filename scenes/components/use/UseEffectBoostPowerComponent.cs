namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectBoostPowerComponent: Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_BOOST_POWER_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int Power { get; private set; }
    public int Duration { get; private set; }

    public static UseEffectBoostPowerComponent Create(int power, int duration) {
      var component = new UseEffectBoostPowerComponent();

      component.Power = power;
      component.Duration = duration;

      return component;
    }
  }
}