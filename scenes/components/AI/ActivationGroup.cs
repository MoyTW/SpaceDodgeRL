namespace SpaceDodgeRL.scenes.components.AI {

  public class ActivationGroup {

    public bool IsActive { get; private set; }

    public ActivationGroup(bool isActive = false) {
      this.IsActive = isActive;
    }

    public void Activate() {
      this.IsActive = true;
    }
  }
}