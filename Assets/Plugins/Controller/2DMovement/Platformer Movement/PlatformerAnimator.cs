using UnityEngine;

namespace NetControllerSystem.Platformer2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlatformerAnimator : FighterAnimator
    {
        [SerializeField] private float _jumpAnimationLength = 0.25f;
        [SerializeField] private FighterController _fighterController;
        [SerializeField] private PlatformerJumpModule _platformerJumpModule;
        [SerializeField] private PlatformerWallModule _platformerWallModule;
        private PlatformerMotor PlatformerMotor => (PlatformerMotor)motor;

        protected override void Awake()
        {
            base.Awake();
            _platformerJumpModule.OnJump += PlatformerMovement_OnJump;
            _platformerJumpModule.OnFinalDoubleJump += PlatformerMotor_OnFinalDoubleJump;
        }

        protected virtual void OnDestroy()
        {
            _platformerJumpModule.OnJump -= PlatformerMovement_OnJump;
            _platformerJumpModule.OnFinalDoubleJump -= PlatformerMotor_OnFinalDoubleJump;
        }

        private void PlatformerMotor_OnFinalDoubleJump(object sender, PositionEventArgs e)
        {
            _lockInExhaustedJumpAnimationUntilTime = Time.time + _jumpAnimationLength;
        }

        protected virtual void PlatformerMovement_OnJump(object sender, PositionEventArgs e)
        {
            _lockInJumpAnimationUntilTime = Time.time + _jumpAnimationLength;
        }

        protected override void Animate()
        {
            base.Animate();
            HandleAnimation();
        }

        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Run = Animator.StringToHash("Run");
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Fall = Animator.StringToHash("Fall");
        public static readonly int Crouch = Animator.StringToHash("Crouch");
        public static readonly int WallClimb = Animator.StringToHash("WallClimb");
        public static readonly int WallCling = Animator.StringToHash("WallCling");
        public static readonly int WallSlide = Animator.StringToHash("WallSlide");
        public static readonly int WallHang = Animator.StringToHash("WallHang");
        public static readonly int Dead = Animator.StringToHash("Dead");

        private float _lockInExhaustedJumpAnimationUntilTime;
        private float _lockInJumpAnimationUntilTime;

        private void HandleAnimation()
        {
            int state = _fighterController.CurrentState == FighterController.States.Dead ? Dead : GetMovementState();

            SwitchAnimState(state);
        }

        private int GetMovementState()
        {
            int state;
            if (_platformerWallModule.State != PlatformerWallModule.WallState.None)
            {
                state = GetWallState();
            }
            else if (PlatformerMotor.Grounded)
            {
                state = GetGroundedState();
            }
            else
            {
                state = GetAirState();
            }

            return state;
        }

        protected virtual int GetWallState()
        {
            switch (_platformerWallModule.State)
            {
                case PlatformerWallModule.WallState.Cling:
                    return WallCling;
                case PlatformerWallModule.WallState.Slide:
                    return WallSlide;
                case PlatformerWallModule.WallState.Climb:
                    return WallClimb;
                case PlatformerWallModule.WallState.Hang:
                    return WallCling;
                default:
                    return -1;
            }
        }
        
        protected virtual int GetAirState()
        {
            int state;
            if (_platformerJumpModule.Rising || _lockInJumpAnimationUntilTime >= Time.time)
            {
                state = Jump;
            }
            else
            {
                state = Fall;
            }

            return state;
        }

        protected virtual int GetGroundedState()
        {
            int state;
            if (PlatformerMotor.CrouchModule.Crouching)
            {
                state = Crouch;
            }
            else if (motor.Controller.InputtingHorizontalMovement && Mathf.Abs(motor.Rb.linearVelocity.x) > 0.1f)
            {
                state = Run;
            }
            else
            {
                state = Idle;
            }

            return state;
        }
    }
}