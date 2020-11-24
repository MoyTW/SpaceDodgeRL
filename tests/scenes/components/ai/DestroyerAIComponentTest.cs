using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class DestroyerAIComponentTest {

    private readonly ITestOutputHelper _output;

    public DestroyerAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new DestroyerAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(DestroyerAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new DestroyerAIComponent("groupid");
      string saved = component.Save();

      var newComponent = DestroyerAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
      Assert.Equal(component.VolleyCooldown, newComponent.VolleyCooldown);
      Assert.Equal(component.CurrentVolleyCooldown, newComponent.CurrentVolleyCooldown);
    }
  }
}