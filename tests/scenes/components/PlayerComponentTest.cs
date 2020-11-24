using System.Collections.Generic;
using System.Text.Json;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class PlayerComponentTest {

    private readonly ITestOutputHelper _output;

    public PlayerComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = PlayerComponent.Create();
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(PlayerComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var path = new EncounterPath(new List<EncounterPosition>() { new EncounterPosition(15, 53) });

      var component = PlayerComponent.Create();
      component.LayInAutopilotPath(path);
      component.RegisterIntel(3);
      string saved = component.Save();

      var newComponent = PlayerComponent.Create(saved);

      _output.WriteLine(saved);

      Assert.Equal(component.KnowsIntel(0), newComponent.KnowsIntel(0));
      Assert.Equal(component.KnowsIntel(1), newComponent.KnowsIntel(1));
      Assert.Equal(component.KnowsIntel(2), newComponent.KnowsIntel(2));
      Assert.Equal(component.KnowsIntel(3), newComponent.KnowsIntel(3));
      Assert.Equal(component.CuttingLaserRange, newComponent.CuttingLaserRange);
      Assert.Equal(component.BaseCuttingLaserPower, newComponent.BaseCuttingLaserPower);
      Assert.Equal(component.IsAutopiloting, newComponent.IsAutopiloting);
      Assert.Equal(component.AutopilotPath.CurrentPosition, newComponent.AutopilotPath.CurrentPosition);
    }

    [Fact]
    public void PullsStatusEffectTrackerFromEntity() {
      var entity = Entity.Create("", "");
      
      var playerComponent = PlayerComponent.Create(baseCuttingLaserPower: 5);
      entity.AddComponent(playerComponent);
      
      var trackerComponent = new StatusEffectTrackerComponent();
      entity.AddComponent(trackerComponent);

      Assert.Equal(5, playerComponent.CuttingLaserPower);

      trackerComponent.AddEffect(new StatusEffectTimedPowerBoost(10, 5, 5));
      Assert.Equal(15, playerComponent.CuttingLaserPower);
    }
  }
}