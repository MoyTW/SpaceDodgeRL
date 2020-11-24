using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class CarrierAIComponentTest {

    private readonly ITestOutputHelper _output;

    public CarrierAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new CarrierAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(CarrierAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new CarrierAIComponent("groupid");
      string saved = component.Save();

      var newComponent = CarrierAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
      Assert.Equal(component.FlakCooldown, newComponent.FlakCooldown);
      Assert.Equal(component.CurrentFlakCooldown, newComponent.CurrentFlakCooldown);
      Assert.Equal(component.LaunchTable, newComponent.LaunchTable);
    }
  }
}