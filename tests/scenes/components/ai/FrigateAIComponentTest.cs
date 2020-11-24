using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class FrigateAIComponentTest {

    private readonly ITestOutputHelper _output;

    public FrigateAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new FrigateAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(FrigateAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new FrigateAIComponent("groupid");
      string saved = component.Save();

      var newComponent = FrigateAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
      Assert.Equal(component.ReverserCooldown, newComponent.ReverserCooldown);
      Assert.Equal(component.CurrentReverserCooldown, newComponent.CurrentReverserCooldown);
    }
  }
}