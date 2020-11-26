using System.Text.Json;
using System.Text.Json.Serialization;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.components.use {

  // In the original game, it disabled for [n turns], so if you disabled a scout it'd come online fast and if you disabled a
  // cruiser it'd come online slow. That seems a little bizarre, but it's replicated here for faithfulness; I can rip it out
  // later once I start improving/expanding.
  public class UseEffectEMPComponent : Component {
    public static readonly string ENTITY_GROUP = "USE_EFFECT_EMP_COMPONENT_GROUP";
    public string EntityGroup => ENTITY_GROUP;

    [JsonInclude] public int Radius { get; private set; }
    [JsonInclude] public int DisableTurns { get; private set; }

    public static UseEffectEMPComponent Create(int radius, int disableTurns) {
      var component = new UseEffectEMPComponent();

      component.Radius = radius;
      component.DisableTurns = disableTurns;

      return component;
    }

    public static UseEffectEMPComponent Create(string saveData) {
      return JsonSerializer.Deserialize<UseEffectEMPComponent>(saveData);
    }

    public string Save() {
      return JsonSerializer.Serialize(this);
    }

    public void NotifyAttached(Entity parent) { }

    public void NotifyDetached(Entity parent) { }

  }
}