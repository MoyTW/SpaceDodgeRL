using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class StatusEffectTrackerComponentTest {

    private readonly ITestOutputHelper _output;

    public StatusEffectTrackerComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = StatusEffectTrackerComponent.Create();
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(StatusEffectTrackerComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = StatusEffectTrackerComponent.Create();
      component.AddEffect(new StatusEffectTimedSpeedBoost(50, 23, 48));
      component.AddEffect(new StatusEffectTimedSpeedBoost(4, 51, 99));
      component.AddEffect(new StatusEffectTimedPowerBoost(64, 13, 22));
      string saved = component.Save();

      var newComponent = StatusEffectTrackerComponent.Create(saved);

      Assert.Equal((component._StatusEffects[0] as StatusEffectTimed).StartTick, (newComponent._StatusEffects[0] as StatusEffectTimed).StartTick);
      Assert.Equal((component._StatusEffects[0] as StatusEffectTimed).EndTick, (newComponent._StatusEffects[0] as StatusEffectTimed).EndTick);
      Assert.Equal((component._StatusEffects[2] as StatusEffectTimed).StartTick, (newComponent._StatusEffects[2] as StatusEffectTimed).StartTick);
      Assert.Equal((component._StatusEffects[2] as StatusEffectTimed).EndTick, (newComponent._StatusEffects[2] as StatusEffectTimed).EndTick);
      Assert.Equal(component.GetTotalBoost(StatusEffectType.BOOST_SPEED), newComponent.GetTotalBoost(StatusEffectType.BOOST_SPEED));
      Assert.Equal(component.GetTotalBoost(StatusEffectType.BOOST_POWER), newComponent.GetTotalBoost(StatusEffectType.BOOST_POWER));
    }
  }
}