using System.Collections.Generic;
using System.Text.Json;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components.AI;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components.AI {

  public class PathAIComponentTest {

    private readonly ITestOutputHelper _output;

    public PathAIComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = PathAIComponent.Create(new EncounterPath(new List<EncounterPosition>() { new EncounterPosition(0, 0) }));
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(PathAIComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var positions = new List<EncounterPosition>() { new EncounterPosition(0, 38), new EncounterPosition(123, 34), new EncounterPosition(3, 2) };
      var component = PathAIComponent.Create(new EncounterPath(positions));
      component.Path.Step();
      string saved = component.Save();

      var newComponent = PathAIComponent.Create(saved);

      Assert.Equal(component.Path.CurrentPosition, newComponent.Path.CurrentPosition);
      Assert.Equal(component.Path.CurrentStep, newComponent.Path.CurrentStep);
      Assert.Equal(component.Path.AtEnd, newComponent.Path.AtEnd);

      var componentPath = component.Path.FullPath.ToArray();
      var newPath = newComponent.Path.FullPath.ToArray();
      for (int i = 0; i < 3; i++) {
        Assert.Equal(componentPath[i], newPath[i]);
      }
    }
  }
}