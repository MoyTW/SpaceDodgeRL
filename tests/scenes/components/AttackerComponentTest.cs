using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class AttackerComponentTest {

    private readonly ITestOutputHelper _output;

    public AttackerComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = AttackerComponent.Create("", 0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(AttackerComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = AttackerComponent.Create("arbitrary string", 10);
      string saved = component.Save();

      var newComponent = AttackerComponent.Create(saved);

      Assert.Equal(component.SourceEntityId, newComponent.SourceEntityId);
      Assert.Equal(component.Power, newComponent.Power);
    }
  }
}