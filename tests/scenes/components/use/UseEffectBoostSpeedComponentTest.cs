using System.Text.Json;
using SpaceDodgeRL.scenes.components.use;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.use {

  public class UseEffectBoostSpeedComponentTest {

    private readonly ITestOutputHelper _output;

    public UseEffectBoostSpeedComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = UseEffectBoostSpeedComponent.Create(0, 0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(UseEffectBoostSpeedComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = UseEffectBoostSpeedComponent.Create(99, 11);
      string saved = component.Save();

      var newComponent = UseEffectBoostSpeedComponent.Create(saved);

      Assert.Equal(component.BoostPower, newComponent.BoostPower);
      Assert.Equal(component.Duration, newComponent.Duration);
    }
  }
}