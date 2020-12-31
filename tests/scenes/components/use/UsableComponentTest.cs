using System.Text.Json;
using SpaceDodgeRL.scenes.components.use;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.use {

  public class UsableComponentTest {

    private readonly ITestOutputHelper _output;

    public UsableComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = UsableComponent.Create(false);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(UsableComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = UsableComponent.Create(true);
      string saved = component.Save();

      var newComponent = UsableComponent.Create(saved);

      Assert.Equal(component.UseOnGet, newComponent.UseOnGet);
    }
  }
}