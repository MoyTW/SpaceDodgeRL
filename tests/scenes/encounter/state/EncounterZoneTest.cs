using System.Text.Json;
using SpaceDodgeRL.library.encounter;
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
      zone.AddFeatureToReadout(Entity.Create("e1 id", "e1 name"));
      zone.AddItemToReadout(Entity.Create("e2 id", "e2 name"));
      zone.AddFeatureToReadout(Entity.Create("e3 id", "e3 name"));

      var saved = JsonSerializer.Serialize(zone);
      var loaded = JsonSerializer.Deserialize<EncounterZone>(saved);

      Assert.Equal(zone.ZoneId, loaded.ZoneId);
      Assert.Equal(zone.Position, loaded.Position);
      Assert.Equal(zone.Width, loaded.Width);
      Assert.Equal(zone.Height, loaded.Height);
      Assert.Equal(zone.ZoneName, loaded.ZoneName);
      Assert.Equal(zone.ReadoutEncounterName, loaded.ReadoutEncounterName);
      Assert.Equal(zone._ReadoutFeatures[0].EntityName, loaded._ReadoutFeatures[0].EntityName);
      Assert.Equal(zone._ReadoutFeatures[1].EntityName, loaded._ReadoutFeatures[1].EntityName);
      Assert.Equal(zone._ReadoutItems[0].EntityName, loaded._ReadoutItems[0].EntityName);
      Assert.Equal(zone.Center, loaded.Center);
    }
  }
}