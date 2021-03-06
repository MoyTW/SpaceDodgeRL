using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceDodgeRL.scenes.encounter.state {

  public class DangerMap : TileMap {

    private DynamicFont _damageFont;
    private List<Label> _damageLabels;

    public override void _Ready() {
      var fontData = GD.Load<DynamicFontData>("res://resources/fonts/Fira_Code_v5.2/ttf/FiraCode-Bold.ttf");
      this._damageFont = new DynamicFont();
      this._damageFont.FontData = fontData;

      this._damageLabels = new List<Label>();
    }

    /**
     * Displays the danger map on the "DangerMap" TileMap. Sums the probable (get to that in a moment) damage the player would
     * take from being on the given square on their next turn.
     *
     * "Danger" is kind of tricky, because the possible danger can be influenced by enemy movement. For example, a railgun shot
     * can be intercepted by a scout, rendering all tiles in its path 'safe', but we have no idea if the scout will or won't move
     * onto that square. Our current rule is "You can never get hit on a safe square, but can sometimes avoid getting hit on a
     * dangerous square." So, we stop a projectile path if it would hit a satellite, but don't stop if it would hit a cruiser,
     * even if the cruiser wouldn't have time to move, because it might be destroyed by another projectile and then let the
     * projectile pass.
     *
     * While you could theoretically simulate to the maximum possible fidelity, such that you don't paint it if the cruiser has
     * HP than it could possibly take in a round and do if it doesn't, that doesn't seem worth it.
     */
    public void UpdateAllTiles(EncounterState state) {
      var pathEntities = GetTree().GetNodesInGroup(PathAIComponent.ENTITY_GROUP);
      var timeToNextPlayerMove = state.Player.GetComponent<SpeedComponent>().Speed;
      var positionsToPotentialDamage = new Dictionary<EncounterPosition, int>();

      // Fill the TileMap as appropriate & tally positionsToPotentialDamage
      this.Clear();
      // TODO: We don't actually need to update every entity, every time, since we only need to set the cell when the projectile itself moves
      foreach (Entity pathEntity in pathEntities) {
        int attackerPower = GetAttackerPower(pathEntity);
        var dangerPositions = CalcDangerPositions(pathEntity, timeToNextPlayerMove);

        RotateSpriteTowardsDestination(pathEntity, dangerPositions);
        foreach (EncounterPosition dangerPosition in dangerPositions.Where(p => state.FoVCache.IsVisible(p))) {
          // If we have a fully immobile, invincible entity at the position we stop the path - otherwise we still draw it.
          var blockingEntity = state.BlockingEntityAtPosition(dangerPosition.X, dangerPosition.Y);
          if (blockingEntity != null &&
              blockingEntity.GetComponent<ActionTimeComponent>() == null &&
              blockingEntity.GetComponent<DefenderComponent>() != null &&
              blockingEntity.GetComponent<DefenderComponent>().IsInvincible) {
            break;
          }

          if (positionsToPotentialDamage.ContainsKey(dangerPosition)) {
            positionsToPotentialDamage[dangerPosition] += attackerPower;
          } else {
            positionsToPotentialDamage[dangerPosition] = attackerPower;
          }
          this.SetCell(dangerPosition.X, dangerPosition.Y, 0);
        }
      }

      // Draw the damage numbers
      // If all this creation/deletion is a significant source of slowdown you can make an object pool, max size FoW area tiles
      foreach (var damageLabel in this._damageLabels) {
        this.RemoveChild(damageLabel);
        damageLabel.QueueFree();
      }
      _damageLabels.Clear();
      foreach (var pair in positionsToPotentialDamage) {
        var label = CreateLabel(pair.Value, pair.Key);
        this._damageLabels.Add(label);
        this.AddChild(label);
      }

      this.HighlightEnemies(state, timeToNextPlayerMove);
    }

    private Label CreateLabel(int power, EncounterPosition position) {
      var label = new Label();
      label.Text = power.ToString();
      var numCenterPos = PositionComponent.IndexToVector(position.X, position.Y);
      label.AddFontOverride("font", this._damageFont);
      label.AddColorOverride("font_color", new Color(1f, 0f, 0f));
      // The size isn't determined until after it's first placed, so we place, then reposition according to size to center it.
      label.SetPosition(numCenterPos);
      var size = label.RectSize;
      label.SetPosition(new Vector2(numCenterPos.x - size.x / 2, numCenterPos.y - size.y / 2));
      return label;
    }

    private int GetAttackerPower(Entity entity) {
      var attackerComponent = entity.GetComponent<AttackerComponent>();
      if (attackerComponent != null) {
        return attackerComponent.Power;
      } else {
        return 0;
      }
    }

    private List<EncounterPosition> CalcDangerPositions(Entity pathEntity, int timeToNextPlayerMove) {
      var pathEntitySpeed = pathEntity.GetComponent<SpeedComponent>().Speed;
      var path = pathEntity.GetComponent<PathAIComponent>().Path;

      int stepsToProject = pathEntitySpeed != 0 ? timeToNextPlayerMove / pathEntitySpeed : Int16.MaxValue;
      return path.Project(stepsToProject);
    }

    private void RotateSpriteTowardsDestination(Entity entity, List<EncounterPosition> dangerPositions) {
      if (dangerPositions.Count > 0) {
        var pathEntityPositionComponent = entity.GetComponent<PositionComponent>();
        var pathEntityPos = pathEntityPositionComponent.EncounterPosition;
        var lastDangerPosition = dangerPositions[dangerPositions.Count - 1];
        pathEntityPositionComponent.RotateSpriteTowards(lastDangerPosition.X - pathEntityPos.X, lastDangerPosition.Y - pathEntityPos.Y);
      }
    }

    private bool ShouldHighlightEntity(EncounterState state, Entity entity, EncounterPosition entityPos, int nextPlayerTick) {
      var aiComponent = entity.GetComponent<AIComponent>();

      return aiComponent != null &&
        !(aiComponent is PathAIComponent) &&
        state.FoVCache.IsVisible(entityPos) &&
        entity.GetComponent<ActionTimeComponent>().NextTurnAtTick < nextPlayerTick;
    }

    private void HighlightEnemies(EncounterState state, int timeToNextPlayerMove) {
      var nextPlayerTick = state.Player.GetComponent<ActionTimeComponent>().NextTurnAtTick + timeToNextPlayerMove;
      var actionEntities = state.ActionEntities();

      foreach (Entity actionEntity in actionEntities) {
        var actionEntityPos = actionEntity.GetComponent<PositionComponent>().EncounterPosition;

        if (ShouldHighlightEntity(state, actionEntity, actionEntityPos, nextPlayerTick)) {
          if (this.GetCell(actionEntityPos.X, actionEntityPos.Y) == 0) {
            this.SetCell(actionEntityPos.X, actionEntityPos.Y, 2);
          } else {
            this.SetCell(actionEntityPos.X, actionEntityPos.Y, 1);
          }
        }
      }
    }
  }
}