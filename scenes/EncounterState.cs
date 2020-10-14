using Godot;
using SpaceDodgeRL.library.encounter;
using SpaceDodgeRL.library.encounter.rulebook;
using SpaceDodgeRL.scenes.components;
using SpaceDodgeRL.scenes.components.AI;
using SpaceDodgeRL.scenes.entities;
using System;

namespace SpaceDodgeRL.scenes {

  public class EncounterState : Node {

    public InputHandler inputHandlerRef = null;
    public EntityBuilder entityBuilderRef = null;

    // TODO: Save/Load/Proper New Game
    private bool hasCreated = false;

    public Entity Player {
      get => GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
    }

    public Entity GetEntityById(string entityId) {
      var entities = GetTree().GetNodesInGroup(Entity.ENTITY_GROUP);
      // It kinda chafes that Godot arrays don't have all the fancy utility functions C# collections do.
      foreach (Entity entity in entities) {
        if (entity.EntityId == entityId) {
          return entity;
        }
      }
      return null;
    }

    // TODO: cache, maybe & also tag entities with groups per component
    public Godot.Collections.Array ActionEntities() {
      return GetTree().GetNodesInGroup(ActionTimeComponent.ENTITY_GROUP);
    }

    public Entity NextEntity() {
      int lowestTTL = int.MaxValue;
      Entity next = null;

      // TODO: We're doing a full scan every time right now; we should store these in a sorted list!
      // TODO: Also this code is really awful!
      var children = GetChildren();
      foreach (Node node in children) {
        if (node.IsInGroup(Entity.ENTITY_GROUP)) {
          var actionTimeComponent = (node as Entity).GetNode<ActionTimeComponent>("ActionTimeComponent");
          if (actionTimeComponent != null && actionTimeComponent.TicksUntilTurn < lowestTTL) {
            lowestTTL = actionTimeComponent.TicksUntilTurn;
            next = node as Entity;
          }
        }
      }

      if (lowestTTL == int.MaxValue) {
        throw new NotImplementedException();
      }

      return next;
    }

    public override void _Ready() { }

    // TODO: Create EncounterRunner & pull Rulebook & all logic outta the state, it should be just data read/write
    // TODO: Invert the Y-axis?
    private void TemporaryPlayerMoveFn(int dx, int dy) {
      var positionComponent = Player.GetNode<PositionComponent>("PositionComponent");
      var oldPos = positionComponent.GamePosition;
      Rulebook.ResolveAction(new MoveAction(Player.EntityId, new GamePosition(oldPos.X + dx, oldPos.Y + dy)), this);
    }

    // TODO: Move into map gen & save/load
    private void InitState() {
      AddChild(entityBuilderRef.CreatePlayerEntity(new GamePosition(3, 5)));
      AddChild(entityBuilderRef.CreateScoutEntity(new GamePosition(5, 5)));

      // TODO: Attaching camera to the player like this is extremely jank! ALSO, it's causing a weird jumping behaviour where the
      // camera moves milliseconds after the player teleports to the next position!
      var camera = GetNode<Camera2D>("EncounterCamera");
      RemoveChild(camera);
      Player.GetNode<PositionComponent>("PositionComponent").AddChild(camera);

      hasCreated = true;
    }

    // TODO: Move into EncounterRunner
    public void PassTime(int time) {
      var actionEntities = ActionEntities();
      foreach (Entity entity in actionEntities) {
        var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
        actionTimeComponent.PassTime(time);
      }
    }

    // TODO: Move into EncounterRunner
    public void RunTurn() {
      var entity = NextEntity();
      var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
      if (actionTimeComponent.TicksUntilTurn > 0) {
        PassTime(actionTimeComponent.TicksUntilTurn);
      }

      // TODO: "player"
      if (entity.IsInGroup("player")) {
        var action = inputHandlerRef.PopQueue();
        // Super not a fan of the awkwardness of checking this twice! Switch string -> enum, maybe?
        if (action == InputHandler.ActionMapping.MOVE_N) {
          TemporaryPlayerMoveFn(0, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_NE) {
          TemporaryPlayerMoveFn(1, -1);
        } else if (action == InputHandler.ActionMapping.MOVE_E) {
          TemporaryPlayerMoveFn(1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_SE) {
          TemporaryPlayerMoveFn(1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_S) {
          TemporaryPlayerMoveFn(0, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_SW) {
          TemporaryPlayerMoveFn(-1, 1);
        } else if (action == InputHandler.ActionMapping.MOVE_W) {
          TemporaryPlayerMoveFn(-1, 0);
        } else if (action == InputHandler.ActionMapping.MOVE_NW) {
          TemporaryPlayerMoveFn(-1, -1);
        }
      } else {
        // TODO: "get any AI component"
        // TODO: Take actions & do them!
        var children = entity.GetChildren();
        AIComponent aIComponent = entity.GetNode<TestAIComponent>("TestAIComponent");
        var aIActions = aIComponent.DecideNextAction(this);
        Rulebook.ResolveActions(aIActions, this);
      }
    }

    public override void _Process(float delta) {
      if (!hasCreated) {
        InitState();
      }

      RunTurn();
    }
  }
}