using System.Text.Json;
using SpaceDodgeRL.scenes.components.use;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.use {

  public class UseEffectHealComponentTest {

    private readonly ITestOutputHelper _output;

    public UseEffectHealComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = UseEffectHealComponent.Create(0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(UseEffectHealComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = UseEffectHealComponent.Create(55);
      string saved = component.Save();

      var newComponent = UseEffectHealComponent.Create(saved);

      Assert.Equal(component.Healpower, newComponent.Healpower);
    }
  }
}