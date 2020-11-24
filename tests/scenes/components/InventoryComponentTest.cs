using System.Collections.Generic;
using System.Text.Json;
using SpaceDodgeRL.resources.gamedata;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.use;
using SpaceDodgeRL.scenes.entities;
using Xunit;
using Xunit.Abstractions;

namespace SpaceDodgeRL.tests.scenes.components {

  public class InventoryComponentTest {

    private readonly ITestOutputHelper _output;

    public InventoryComponentTest(ITestOutputHelper output) {
      this._output = output;
    }

    [Fact]
    public void IncludesEntityGroup() {
      var component = InventoryComponent.Create(0);
      JsonElement deserialized = JsonSerializer.Deserialize<JsonElement>(component.Save());
      Assert.Equal(InventoryComponent.ENTITY_GROUP, deserialized.GetProperty("EntityGroup").GetString());
    }

    [Fact]
    public void SerializesAndDeserializesCorrectly() {
      var component = InventoryComponent.Create(91);

      // TODO: Once you have Entity working, clean this up.
      var battery = EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_EXTRA_BATTERY);
      component.AddEntity(battery);
      var ductTape = EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_DUCT_TAPE);
      component.AddEntity(ductTape);
      var battery2 = EntityBuilder.CreateItemByEntityDefId(EntityDefId.ITEM_EXTRA_BATTERY);
      component.AddEntity(battery2);

      string saved = component.Save();

      var newComponent = InventoryComponent.Create(saved);

      Assert.Equal(component.InventorySize, newComponent.InventorySize);
      Assert.Equal(component.InventoryUsed, newComponent.InventoryUsed);
      var componentItems = new List<Entity>(component.StoredItems);
      var newItems = new List<Entity>(newComponent.StoredItems);
      
      Assert.Equal(componentItems[0].EntityId, newItems[0].EntityId);
      Assert.True(newItems[0].GetComponent<UseEffectBoostPowerComponent>() != null);
      Assert.Equal(componentItems[0].GetComponent<UseEffectBoostPowerComponent>().Duration,
        newItems[0].GetComponent<UseEffectBoostPowerComponent>().Duration);

      Assert.Equal(componentItems[1].EntityId, newItems[1].EntityId);
      Assert.True(newItems[1].GetComponent<UseEffectHealComponent>() != null);
      Assert.Equal(componentItems[0].GetComponent<UseEffectHealComponent>().Healpower,
        newItems[0].GetComponent<UseEffectHealComponent>().Healpower);

      Assert.Equal(componentItems[2].EntityId, newItems[2].EntityId);
      Assert.True(newItems[2].GetComponent<UseEffectBoostPowerComponent>() != null);
      Assert.Equal(componentItems[0].GetComponent<UseEffectBoostPowerComponent>().Duration,
        newItems[0].GetComponent<UseEffectBoostPowerComponent>().Duration);
    }
  }
}