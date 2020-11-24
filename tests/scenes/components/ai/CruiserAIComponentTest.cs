using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class CruiserAIComponentTest {

    private readonly ITestOutputHelper _output;

    public CruiserAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new CruiserAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(CruiserAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new CruiserAIComponent("groupid");
      string saved = component.Save();

      var newComponent = CruiserAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
      Assert.Equal(component.RailgunCooldown, newComponent.RailgunCooldown);
      Assert.Equal(component.CurrentRailgunCooldown, newComponent.CurrentRailgunCooldown);
      Assert.Equal(component.FlakCooldown, newComponent.FlakCooldown);
      Assert.Equal(component.CurrentFlakCooldown, newComponent.CurrentFlakCooldown);
    }
  }
}