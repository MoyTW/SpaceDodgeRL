using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class DefenderComponentTest {

    private readonly ITestOutputHelper _output;

    public DefenderComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = DefenderComponent.Create(0, 0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(DefenderComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = DefenderComponent.Create(95, 11, currentHp: 3, logDamage: false, isInvincible: true);
      string saved = component.Save();

      var newComponent = DefenderComponent.Create(saved);

      Assert.Equal(component.BaseDefense, newComponent.BaseDefense);
      Assert.Equal(component.Defense, newComponent.Defense);
      Assert.Equal(component.MaxHp, newComponent.MaxHp);
      Assert.Equal(component.CurrentHp, newComponent.CurrentHp);
      Assert.Equal(component.ShouldLogDamage, newComponent.ShouldLogDamage);
      Assert.Equal(component.IsInvincible, newComponent.IsInvincible);
    }
  }
}