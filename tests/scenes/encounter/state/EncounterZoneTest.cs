using System.Text.Json;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.encounter.state {

  public class EncounterZoneTest {

    private readonly ITestOutputHelper _output;

    public EncounterZoneTest(ITestOutputHelper output) {
      _output = output;
    }

    [Fact]
    public void EncounterZoneSerializesAndDeserializes() {
      EncounterZone zone = new EncounterZone("some id", new EncounterPosition(10, 20), width: 20, height: 48, "some name");
      zone.ReadoutEncounterName = "some encounter name";

      var e1 = Entity.Create("e1 id", "e1 name");
      e1.AddComponent(DisplayComponent.Create("e1 path", "", false));
      zone.AddFeatureToReadout(e1);

      var e2 = Entity.Create("e2 id", "e2 name");
      e2.AddComponent(DisplayComponent.Create("e2 path", "", false));
      zone.AddItemToReadout(e2);

      var e3 = Entity.Create("e3 id", "e3 name");
      e3.AddComponent(DisplayComponent.Create("e3 path", "", false));
      zone.AddFeatureToReadout(e3);

      var saved = JsonSerializer.Serialize(zone);
      var loaded = JsonSerializer.Deserialize<EncounterZone>(saved);

      Assert.Equal(zone.ZoneId, loaded.ZoneId);
      Assert.Equal(zone.Position, loaded.Position);
      Assert.Equal(zone.Width, loaded.Width);
      Assert.Equal(zone.Height, loaded.Height);
      Assert.Equal(zone.ZoneName, loaded.ZoneName);
      Assert.Equal(zone.ReadoutEncounterName, loaded.ReadoutEncounterName);
      Assert.Equal(zone._ReadoutFeatures[0].EntityId, loaded._ReadoutFeatures[0].EntityId);
      Assert.Equal(zone._ReadoutFeatures[0].EntityName, loaded._ReadoutFeatures[0].EntityName);
      Assert.Equal(zone._ReadoutFeatures[0].TexturePath, loaded._ReadoutFeatures[0].TexturePath);
      Assert.Equal(zone._ReadoutFeatures[1].EntityId, loaded._ReadoutFeatures[1].EntityId);
      Assert.Equal(zone._ReadoutFeatures[1].EntityName, loaded._ReadoutFeatures[1].EntityName);
      Assert.Equal(zone._ReadoutFeatures[1].TexturePath, loaded._ReadoutFeatures[1].TexturePath);
      Assert.Equal(zone._ReadoutItems[0].EntityId, loaded._ReadoutItems[0].EntityId);
      Assert.Equal(zone._ReadoutItems[0].EntityName, loaded._ReadoutItems[0].EntityName);
      Assert.Equal(zone._ReadoutItems[0].TexturePath, loaded._ReadoutItems[0].TexturePath);
      Assert.Equal(zone.Center, loaded.Center);
    }
  }
}