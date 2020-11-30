using Godot;
using SpaceDodgeRL.scenes.encounter.state;
using System;

public class AutopilotZoneReadout : HBoxContainer {

  public Button AutopilotButton { get => this.GetNode<Button>("AutopilotButton"); }

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

  public void SetReadout(EncounterZone zone) {
    this.GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneNameLabel").Text = zone.ZoneName;
    this.GetNode<Label>("ReadoutContainer/NameEncounterBar/ZoneEncounterLabel").Text = zone.ReadoutEncounterName;

    var itemsBar = this.GetNode<HBoxContainer>("ReadoutContainer/ItemsFeaturesBar/ItemsBar");
    var itemsTextureSize = itemsBar.RectSize.y;
    foreach (var itemReadout in zone.ReadoutItems) {
      itemsBar.AddChild(BuildTextureRect(itemReadout.TexturePath, itemsTextureSize));
    }

    var featuresBar = this.GetNode<HBoxContainer>("ReadoutContainer/ItemsFeaturesBar/FeaturesBar");
    var featuresTextureSize = featuresBar.RectSize.y;
    foreach (var featureReadout in zone.ReadoutFeatures) {
      featuresBar.AddChild(BuildTextureRect(featureReadout.TexturePath, featuresTextureSize));
    }
  }
}
