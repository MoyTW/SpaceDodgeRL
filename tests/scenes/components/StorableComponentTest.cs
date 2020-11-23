using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class StorableComponentTest {

    private readonly ITestOutputHelper _output;

    public StorableComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = StorableComponent.Create(0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(StorableComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = StorableComponent.Create(4);
      string saved = component.Save();

      var newComponent = StorableComponent.Create(saved);

      Assert.Equal(component.Size, newComponent.Size);
    }
  }
}