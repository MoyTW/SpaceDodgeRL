using Godot;
using System;
using System.Collections.Generic;

public class EncounterState : Node {

  public InputHandler inputHandlerRef = null;
  public EntityBuilder entityBuilderRef = null;

  // TODO: Save/Load/Proper New Game
  private bool hasCreated = false;

  public Entity Player {
    get => this.GetTree().GetNodesInGroup(PlayerComponent.ENTITY_GROUP)[0] as Entity;
  }

  // TODO: cache, maybe & also tag entities with groups per component
  public Godot.Collections.Array ActionEntities() {
    return this.GetTree().GetNodesInGroup(ActionTimeComponent.ENTITY_GROUP);
  }

  public Entity NextEntity() {
    int lowestTTL = int.MaxValue;
    Entity next = null;

    // TODO: We're doing a full scan every time right now; we should store these in a sorted list!
    // TODO: Also this code is really awful!
    var children = this.GetChildren();
    foreach(Node node in children) {
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

  // TODO: Proper actions
  // TODO: Invert the Y-axis?
  private void TemporaryPlayerMoveFn(int dx, int dy) {
    var positionComponent = this.Player.GetNode<PositionComponent>("PositionComponent");
    var gamePosition = positionComponent.GamePosition;
    positionComponent.GamePosition = new GamePosition(gamePosition.X + dx, gamePosition.Y + dy);

    // TODO: Action!
    var actionTimeComponent = this.Player.GetNode<ActionTimeComponent>("ActionTimeComponent");
    actionTimeComponent.EndTurn(this.Player.GetNode<SpeedComponent>("SpeedComponent"));
  }

  // TODO: Move into map gen & save/load
  private void InitState() {
    this.AddChild(entityBuilderRef.CreatePlayerEntity(new GamePosition(3, 5)));
    this.AddChild(entityBuilderRef.CreateScoutEntity(new GamePosition(5, 5)));

    // TODO: Attaching camera to the player like this is extremely jank! ALSO, it's causing a weird jumping behaviour where the
    // camera moves milliseconds after the player teleports to the next position!
    var camera = this.GetNode<Camera2D>("EncounterCamera");
    this.RemoveChild(camera);
    this.Player.GetNode<PositionComponent>("PositionComponent").AddChild(camera);

    this.hasCreated = true;
  }

  // TODO: Move into EncounterRunner
  public void PassTime(int time) {
    var actionEntities = this.ActionEntities();
    foreach (Entity entity in actionEntities) {
      var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
      actionTimeComponent.PassTime(time);
    }
  }

  // TODO: Move into EncounterRunner
  public void RunTurn() {
    var entity = this.NextEntity();
    var actionTimeComponent = entity.GetNode<ActionTimeComponent>("ActionTimeComponent");
    if (actionTimeComponent.TicksUntilTurn > 0) {
      PassTime(actionTimeComponent.TicksUntilTurn);
    }

    // TODO: "player"
    if (entity.IsInGroup("player")) {
      var action = this.inputHandlerRef.PopQueue();
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
      aIComponent.DecideNextAction(this);
      actionTimeComponent.EndTurn(entity.GetNode<SpeedComponent>("SpeedComponent"));
    }
  }

  public override void _Process(float delta) {
    if (!this.hasCreated) {
      InitState();
    }

    RunTurn();    
  }
}
