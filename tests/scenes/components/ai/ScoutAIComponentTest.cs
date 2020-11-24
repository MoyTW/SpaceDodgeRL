using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class ScoutAIComponentTest {

    private readonly ITestOutputHelper _output;

    public ScoutAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new ScoutAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(ScoutAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new ScoutAIComponent("groupid");
      string saved = component.Save();

      var newComponent = ScoutAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
    }
  }
}