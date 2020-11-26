using System.Collections.Generic;
using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class OnDeathComponentTest {

    private readonly ITestOutputHelper _output;

    public OnDeathComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = OnDeathComponent.Create(new List<string>());
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(OnDeathComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = OnDeathComponent.Create(new List<string>() { OnDeathEffectType.PLAYER_DEFEAT, OnDeathEffectType.PLAYER_VICTORY });
      string saved = component.Save();

      var newComponent = OnDeathComponent.Create(saved);

      Assert.Equal(component.ActiveEffectTypes.Count, newComponent.ActiveEffectTypes.Count);
      Assert.Equal(component.ActiveEffectTypes[0], newComponent.ActiveEffectTypes[0]);
      Assert.Equal(component.ActiveEffectTypes[1], newComponent.ActiveEffectTypes[1]);
    }
  }
}