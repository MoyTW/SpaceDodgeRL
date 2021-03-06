using Godot;
using SpaceDodgeRL.scenes.encounter.state;

namespace SpaceDodgeRL.scenes.encounter {
  public class AutopilotZoneEntry : EntryButton {

    private TextureRect BuildTextureRect(string path, float squareWidth) {
      var textureRect = new TextureRect();

      var texture = GD.Load<Texture>(path);
      textureRect.Texture = texture;
      // This forces the size to be in line with the itemsBar - I don't know of a more elegant way to do this.
      textureRect.Expand = true;
      textureRect.RectMinSize = new Vector2(squareWidth, squareWidth);
      textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspect;

      return textureRect;
    }

    public void SetReadout(EncounterState state, EncounterZone zone, bool hasIntel) {
      GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneNameLabel").Text = zone.ZoneName;
      if (hasIntel) {
        GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneEncounterLabel").Text = zone.ReadoutEncounterName;
      } else {
        GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneEncounterLabel").Text = "NO INTEL";
      }

      var itemsBar = GetNode<HBoxContainer>("ReadoutContainer/ItemsFeaturesBar");
      if (hasIntel) {
        var itemsTextureSize = itemsBar.RectSize.y;
        foreach (var itemReadout in zone.ReadoutItems) {
          if (state.GetEntityById(itemReadout.EntityId) != null) {
            itemsBar.AddChild(BuildTextureRect(itemReadout.TexturePath, itemsTextureSize));
          } else {
            itemsBar.AddChild(BuildTextureRect("res://resources/checkmark_18x18.png", itemsTextureSize));
          }
        }
      } else {
        var noIntelLabel = new Label();
        noIntelLabel.Text = "-?-";
        itemsBar.AddChild(noIntelLabel);
      }

      var featuresBar = GetNode<HBoxContainer>("ReadoutContainer/ItemsFeaturesBar");
      if (hasIntel) {
        var featuresTextureSize = featuresBar.RectSize.y;
        foreach (var featureReadout in zone.ReadoutFeatures) {
          if (state.GetEntityById(featureReadout.EntityId) != null) {
            featuresBar.AddChild(BuildTextureRect(featureReadout.TexturePath, featuresTextureSize));
          } else {
            featuresBar.AddChild(BuildTextureRect("res://resources/checkmark_18x18.png", featuresTextureSize));
          }
        }
      } else {
        var noIntelLabel = new Label();
        noIntelLabel.Text = "-?-";
        featuresBar.AddChild(noIntelLabel);
      }
    }
  }
}