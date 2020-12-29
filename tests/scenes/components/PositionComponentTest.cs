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

    [Fact]
    public void IncludesEntityGroup() {
      var component = PositionComponent.Create(new EncounterPosition(0, 0), "res://resources/tex_test.tres", 0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(component));
      Assert.Equal(PositionComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = PositionComponent.Create(new EncounterPosition(23, -98), "res://resources/tex_test.tres", 3);
      string saved = JsonSerializer.Serialize(component);

      var newComponent = PositionComponent.Create(saved);

      Assert.Equal(component.EncounterPosition, newComponent.EncounterPosition);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Position.x, newComponent.GetNode<Sprite>("Sprite").Position.x);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Position.y, newComponent.GetNode<Sprite>("Sprite").Position.y);
      Assert.Equal(component.GetNode<Sprite>("Sprite").Texture.ResourcePath, newComponent.GetNode<Sprite>("Sprite").Texture.ResourcePath);
      Assert.Equal(component.GetNode<Sprite>("Sprite").ZIndex, 3);
    }
  }
}