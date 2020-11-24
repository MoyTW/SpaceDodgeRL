using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.entities {

  public class EntityTest {

    private readonly ITestOutputHelper _output;

    public EntityTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void SimpleEntity() {
      Entity entity = Entity.Create("some id", "some name");

      var saved = entity.Save();
      var loaded = Entity.Create(saved);

      Assert.Equal(entity.EntityId, loaded.EntityId);
      Assert.Equal(entity.EntityName, loaded.EntityName);
    }

    [Fact]
    public void EntityWithComponent() {
      Entity entity = Entity.Create("some id", "some name");
      entity.AddComponent(PlayerComponent.Create(baseCuttingLaserPower: 99));

      var saved = entity.Save();
      var loaded = Entity.Create(saved);

      Assert.Equal(entity._Components.Count, loaded._Components.Count);
      Assert.NotNull(loaded.GetComponent<PlayerComponent>());
      Assert.Equal(99, loaded.GetComponent<PlayerComponent>().BaseCuttingLaserPower);
    }
  }
}