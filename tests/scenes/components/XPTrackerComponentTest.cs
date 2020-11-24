using System.Text.Json;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.entities;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class XPTrackerComponentTest {

    private readonly ITestOutputHelper _output;

    public XPTrackerComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = XPTrackerComponent.Create(0, 0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(XPTrackerComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      // I attempted to mock this, but both NSubstitute and Moq apparently have serious issues with mocking the GetCOmponent<T>
      // call, and would crash. So...TODO: look into mocking!
      var entity = Entity.Create("", "");
      var defenderComponent = DefenderComponent.Create(10, 10);
      entity.AddComponent(defenderComponent);

      var component = XPTrackerComponent.Create(45, 97);

      component.AddXP(39500);
      component.RegisterLevelUpChoice(entity, LevelUpBonus.MAX_HP);
      component.RegisterLevelUpChoice(entity, LevelUpBonus.REPAIR);

      string saved = component.Save();

      var newComponent = XPTrackerComponent.Create(saved);

      Assert.Equal(component.LevelUpBase, newComponent.LevelUpBase);
      Assert.Equal(component.LevelUpFactor, newComponent.LevelUpFactor);
      Assert.Equal(component.XP, newComponent.XP);
      Assert.Equal(component.Level, newComponent.Level);
      Assert.Equal(component.UnusedLevelUps.Count, newComponent.UnusedLevelUps.Count);
      Assert.Equal(component.UnusedLevelUps[0], newComponent.UnusedLevelUps[0]);
      Assert.Equal(component.UnusedLevelUps[1], newComponent.UnusedLevelUps[1]);
      Assert.Equal(component.NextLevelAtXP, newComponent.NextLevelAtXP);
      Assert.Equal(component.XPToNextLevel, newComponent.XPToNextLevel);
      Assert.Equal(component.ChosenLevelUps[2], newComponent.ChosenLevelUps[2]);
      Assert.Equal(component.ChosenLevelUps[3], newComponent.ChosenLevelUps[3]);
    }
  }
}