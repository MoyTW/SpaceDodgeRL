namespace SpaceDodgeRL.scenes.components.use {

  public class UseEffectHealComponent: Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_HEAL_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int Healpower { get; private set; }

    public static UseEffectHealComponent Create(int healpower) {
      var component = new UseEffectHealComponent();

      component.Healpower = healpower;

      return component;
    }
  }
}