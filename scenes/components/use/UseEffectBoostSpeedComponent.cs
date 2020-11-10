namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectBoostSpeedComponent: Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_BOOST_SPEED_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int BoostPower { get; private set; }
    public int Duration { get; private set; }

    public static UseEffectBoostSpeedComponent Create(int boostPower, int duration) {
      var component = new UseEffectBoostSpeedComponent();

      component.BoostPower = boostPower;
      component.Duration = duration;

      return component;
    }
  }
}