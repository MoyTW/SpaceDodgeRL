using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class CollisionComponentTest {

    private readonly ITestOutputHelper _output;

    public CollisionComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = CollisionComponent.Create(false, false);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(CollisionComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = CollisionComponent.Create(true, false, true, true);
      string saved = component.Save();

      var newComponent = CollisionComponent.Create(saved);

      Assert.Equal(component.BlocksMovement, newComponent.BlocksMovement);
      Assert.Equal(component.BlocksVision, newComponent.BlocksVision);
      Assert.Equal(component.OnCollisionAttack, newComponent.OnCollisionAttack);
      Assert.Equal(component.OnCollisionSelfDestruct, newComponent.OnCollisionSelfDestruct);
    }
  }
}