using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class XPValueComponentTest {

    private readonly ITestOutputHelper _output;

    public XPValueComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = XPValueComponent.Create(0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(XPValueComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = XPValueComponent.Create(794);
      string saved = component.Save();

      var newComponent = XPValueComponent.Create(saved);

      Assert.Equal(component.XPValue, newComponent.XPValue);
    }
  }
}