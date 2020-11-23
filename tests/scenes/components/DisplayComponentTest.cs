using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class DisplayComponentTest {

    private readonly ITestOutputHelper _output;

    public DisplayComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = DisplayComponent.Create("", true);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(DisplayComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = DisplayComponent.Create("path", true);
      string saved = component.Save();

      var newComponent = DisplayComponent.Create(saved);

      Assert.Equal(component.TexturePath, newComponent.TexturePath);
      Assert.Equal(component.VisibleInFoW, newComponent.VisibleInFoW);
    }
  }
}