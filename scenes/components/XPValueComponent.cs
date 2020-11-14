namespace SpaceDodgeRL.scenes.components {

  class XPValueComponent: Component {
    public static readonly string ENTITY_GROUP = "XP_VALUE_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    public int XPValue { get; private set; }

    public static XPValueComponent Create(int xpValue) {
      var component = new XPValueComponent();

      component.XPValue = xpValue;

      return component;
    }
  }
}