using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class SpeedComponentTest {

    private readonly ITestOutputHelper _output;

    public SpeedComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = SpeedComponent.Create(0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(SpeedComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = SpeedComponent.Create(64);
      string saved = component.Save();

      var newComponent = SpeedComponent.Create(saved);

      Assert.Equal(component.BaseSpeed, newComponent.BaseSpeed);
    }
  }
}