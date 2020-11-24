using System.Text.Json;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class GunshipAIComponentTest {

    private readonly ITestOutputHelper _output;

    public GunshipAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = new GunshipAIComponent("");
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(GunshipAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = new GunshipAIComponent("groupid");
      string saved = component.Save();

      var newComponent = GunshipAIComponent.Create(saved);

      Assert.Equal(component.ActivationGroupId, newComponent.ActivationGroupId);
      Assert.Equal(component.ShotgunCooldown, newComponent.ShotgunCooldown);
      Assert.Equal(component.CurrentShotgunCooldown, newComponent.CurrentShotgunCooldown);
    }
  }
}