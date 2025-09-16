using UnityEngine;
using NetControllerSystem.Platformer2D;

[RequireComponent(typeof(TransformSquisher2D))]
public class JumpSquashStretch : MonoBehaviour
{ 
    [SerializeField] private PlatformerMotor _platformerMotor;/*
    [SerializeField] private TemporaryEffector _landEffector;
    [SerializeField] private TemporaryEffector _jumpEffector;
    [SerializeField] private ToggleEffector _fallingEffector;
    [SerializeField] private TemporaryEffector _crouchEffector;
    [SerializeField] private TemporaryEffector _uncrouchEffector;
    private TransformSquisher2D _transformSquisher;
    private bool _crouching;

    private void Awake()
    {
        _transformSquisher = GetComponent<TransformSquisher2D>();
        _platformerMotor.OnLand += PlatformerMotorOnLand;
        _platformerMotor.JumpModule.OnJump += PlatformMovement_OnJump;
        _platformerMotor.JumpModule.OnDoubleJump += PlatformMovement_OnJump;
        _transformSquisher.squishValue.AddToggleableEffector(_fallingEffector);
    }

    private void FixedUpdate()
    {
        if (_platformerMotor.Grounded)
        {
            _fallingEffector.Disable(false);

        }
        else
        {
            if (!_fallingEffector.Enabled && _platformerMotor.Rb.velocity.y < -0.2f)
                _fallingEffector.Enable();
        }

        if (_platformerMotor.CrouchModule.Crouching)
        {
            if (!_crouching)
            {
                _transformSquisher.Squish(_crouchEffector);
                _crouching = true;
            }
        }
        else
        {
            if (_crouching)
            {
                _transformSquisher.Squish(_uncrouchEffector);
                _crouching = false;
            }
        }
    }

    private void PlatformerMotorOnLand(object sender, System.EventArgs e)
    {
        _transformSquisher.Squish(_landEffector);
    }

    private void PlatformMovement_OnJump(object sender, System.EventArgs e)
    {
        _transformSquisher.Squish(_jumpEffector);
    }*/
}
