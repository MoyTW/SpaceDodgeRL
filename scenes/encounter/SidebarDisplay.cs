using System.Text;
using Godot;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.encounter.state;
using SpaceDodgeRL.scenes.entities;

namespace SpaceDodgeRL.scenes.encounter {

  public class SidebarDisplay : TextureRect {

    private RichTextLabel _encounterLogLabel;

    public override void _Ready() {
      this._encounterLogLabel = this.GetNode<RichTextLabel>("SidebarVBox/EncounterLogLabel");
    }

    public void AddEncounterLogMessage(string bbCodeMessage, int encounterLogSize) {
      if (this._encounterLogLabel.GetLineCount() > encounterLogSize) {
        this._encounterLogLabel.RemoveLine(0);
      }
      this._encounterLogLabel.AppendBbcode(bbCodeMessage + "\n");
      this._encounterLogLabel.Update();
    }

    public void RefreshStats(EncounterState state) {
      var player = state.Player;

      // Left column
      var playerDefenderComponent = player.GetComponent<DefenderComponent>();
      var newHPText = string.Format("HP: {0}/{1}", playerDefenderComponent.CurrentHp, playerDefenderComponent.MaxHp);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/StatsBlock/HPLabel").Text = newHPText;

      var playerComponent = player.GetComponent<PlayerComponent>();
      var newAttackPowerText = string.Format("Laser Power: {0}", playerComponent.CuttingLaserPower);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/StatsBlock/AttackPowerLabel").Text = newAttackPowerText;

      var speedComponent = player.GetComponent<SpeedComponent>();
      var newSpeedText = string.Format("Speed: {0}", speedComponent.Speed);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/StatsBlock/SpeedLabel").Text = newSpeedText;

      var invComponent = player.GetComponent<InventoryComponent>();
      var newInvText = string.Format("Cargo Space: {0}/{1}", invComponent.InventoryUsed, invComponent.InventorySize);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/StatsBlock/InventoryLabel").Text = newInvText;

      // Right column
      var playerPos = player.GetComponent<PositionComponent>().EncounterPosition;

      var newTurnReadoutText = string.Format("Turn: {0:0.00}", state.CurrentTick / 100);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/PositionBlock/TurnReadoutLabel").Text = newTurnReadoutText;

      var newSectorZoneText = string.Format("Sector: {0}", state.DungeonLevel);
      var containingZone = state.ContainingZone(playerPos.X, playerPos.Y);
      if (containingZone != null) {
        newSectorZoneText += string.Format(" {0}", containingZone.ZoneName);
      }
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/PositionBlock/SectorZoneLabel").Text = newSectorZoneText;

      string newPositionZoneText = string.Format("Position: ({0}, {1})", playerPos.X, playerPos.Y);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/PositionBlock/PositionLabel").Text = newPositionZoneText;

      var xpComponent = player.GetComponent<XPTrackerComponent>();
      var newLevelText = string.Format("Level: {0}", xpComponent.Level);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/PositionBlock/LevelLabel").Text = newLevelText;
      var newXPText = string.Format("Experience: {0}/{1}", xpComponent.XP, xpComponent.NextLevelAtXP);
      GetNode<Label>("SidebarVBox/StatsAndPositionHBox/PositionBlock/ExperienceLabel").Text = newXPText;

      var posComponent = player.GetComponent<PositionComponent>();
    }

    public void DisplayScannedEntity(int x, int y, Entity entity) {
      if (entity != null) {
        var scanTextureRect = GetNode<TextureRect>("SidebarVBox/StatsHBox/StatsLeftColumn/ScanBlock/ReadoutTextureRect");
        scanTextureRect.Texture = entity.GetComponent<PositionComponent>().SpriteTexture;
        var descriptionLabel = GetNode<RichTextLabel>("SidebarVBox/StatsHBox/StatsLeftColumn/ScanBlock/DescriptionLabel");

        var descBuilder = new StringBuilder();

        var attackerComponent = entity.GetComponent<AttackerComponent>();
        if (attackerComponent != null) {
          descBuilder.AppendLine(string.Format("[b]Damage:[/b] {0}", attackerComponent.Power));
        }

        var defenderComponent = entity.GetComponent<DefenderComponent>();
        if (defenderComponent != null) {
          if (defenderComponent.IsInvincible) {
            descBuilder.AppendLine("[b]Invincible[/b]");
          } else {
            descBuilder.AppendLine(string.Format("[b]HP:[/b] {0}/{1}", defenderComponent.CurrentHp, defenderComponent.MaxHp));
            descBuilder.AppendLine(string.Format("[b]Armor:[/b] {0}", defenderComponent.BaseDefense));
          }
        }

        if (entity.GetComponent<SpeedComponent>() != null) {
          descBuilder.AppendLine(string.Format("[b]Speed:[/b] {0}", entity.GetComponent<SpeedComponent>().Speed));
        }

        var xpValueComponent = entity.GetComponent<XPValueComponent>();
        if (xpValueComponent != null) {
          descBuilder.AppendLine(string.Format("[b]XP Value:[/b] {0}", xpValueComponent.XPValue));
        }

        descBuilder.AppendLine(entity.GetComponent<DisplayComponent>().Description);

        descriptionLabel.BbcodeText = descBuilder.ToString();
      }
    }
  }
}