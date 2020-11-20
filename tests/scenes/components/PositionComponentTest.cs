using System.Text.Json;
using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class PositionComponentTest {

    private readonly ITestOutputHelper _output;

    public PositionComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    // TODO: You can't run these via 'dotnet test' - you'll get a Godot error. "ECall methods must be packaged into a system
    // module.". Which is a little inconvenient, as I think that means I can't really unit test any of the godot stuff until I
    // find a way to fix it?
    /*
    [Fact]
    public void IncludesEntityGroup() {
      var component = PositionComponent.Create(new EncounterPosition(0, 0), "res://resources/tex_test.tres");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(PositionComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = PositionComponent.Create(new EncounterPosition(23, -98), "res://resources/tex_test.tres");
      string saved = component.Save();

      var newComponent = PositionComponent.Create(saved);

      Assert.Equal(component.EncounterPosition, newComponent.EncounterPosition);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Position.x, newComponent.GetNode<Sprite>("Sprite").Position.x);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Position.y, newComponent.GetNode<Sprite>("Sprite").Position.y);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Texture.ResourcePath, newComponent.GetNode<Sprite>("Sprite").Texture.ResourcePath);
    }
    */
  }
}