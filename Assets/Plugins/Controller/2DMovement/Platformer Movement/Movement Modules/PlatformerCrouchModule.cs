using NetControllerSystem.Platformer2D;

public class PlatformerCrouchModule : PlatformerMotorModule
{
    private bool _crouching;
    public bool Crouching => motor.Grounded && _crouching;
    
    public override void HandleLocalMovement()
    {
        _crouching = Controller.Input.move.GetValue().y < -0.8f;
    }
}
