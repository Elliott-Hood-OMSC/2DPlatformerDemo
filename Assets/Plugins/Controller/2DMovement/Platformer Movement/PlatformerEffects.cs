using UnityEngine;

namespace NetControllerSystem.Platformer2D {
    public class PlatformerEffects : MonoBehaviour
    {
        /*[SerializeField] protected PlatformerMotor _platformerMotor;
        private SpriteRenderer _sr;

        protected virtual void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            
            _platformerMotor.OnLand += OnLand;
            _jumpModule.OnJump += OnJump;
            _jumpModule.OnDoubleJump += OnDoubleJump;
            _wallModule.OnWallJump += WallModuleOnOnWallJump;
        }

        protected virtual void OnDestroy()
        {
            if (_platformerMotor != null)
            {
                _platformerMotor.OnLand -= OnLand;   
            }
            if (_jumpModule != null)
            {
                _jumpModule.OnJump -= OnJump;
                _jumpModule.OnDoubleJump -= OnDoubleJump;
            }
        }

        private void InitializePooledAnimation(PooledAnimation animation)
        {
            animation.SetSorting(_sr.sortingLayerName, _sr.sortingOrder + 1);
        }
        
        #region Land
        
        [SerializeField] private EffectGroup _landEffects;
        [SerializeField] private AnimationClip _landDustAnimationClip;
        private void OnLand(object sender, PositionEventArgs e)
        {
            _jumpEffects.Play(e.Position, Vector2.down);
            PooledAnimation dustAnimation = _landDustAnimationClip.Play(e.Position);
        }
        
        #endregion

        #region Jump
        
        [SerializeField] private PlatformerJumpModule _jumpModule;
        [SerializeField] private EffectGroup _jumpEffects;
        [SerializeField] private AnimationClip _jumpAnimationClip;
        [Tooltip("The horizontal movement needs to be at least this % of the jump speed to play the angled jump clip")]
        [SerializeField] private float _jumpHorizontalSpeedThreshold = 0.3f;
        [SerializeField] private AnimationClip _angledJumpAnimationClip;
        
        private void OnJump(object sender, PositionEventArgs e)
        {
            _jumpEffects.Play(e.Position, Vector2.up);

            Vector2 velocity = _platformerMotor.Rb.velocity;
            bool playAngledClip = Mathf.Abs(velocity.x) > Mathf.Max(_jumpHorizontalSpeedThreshold * _jumpModule.JumpSettings.JumpHeight, 0);

            if (playAngledClip)
            {
                PooledAnimation jumpAnimation = _angledJumpAnimationClip.Play(e.Position);
                
                jumpAnimation.SetLocalScale(new Vector3(Mathf.Sign(_platformerMotor.Rb.velocity.x), 1, 1));
                InitializePooledAnimation(jumpAnimation);
            }
            else
            {
                PooledAnimation jumpAnimation = _jumpAnimationClip.Play(e.Position);
                InitializePooledAnimation(jumpAnimation);
            }
        }
        
        #endregion
        
        #region Wall Module

        [SerializeField] private AnimationClip _wallSlideAnimationClip;
        [SerializeField] private AnimationClip _wallJumpAnimationClip;
        [SerializeField] private float _wallJumpEffectDistanceFromWall = 6 / 16f;
        [SerializeField] private PlatformerWallModule _wallModule;
        
        private void WallModuleOnOnWallJump(int directionFacing)
        {
            Vector3 effectPosition = _platformerMotor.transform.position + new Vector3(_wallJumpEffectDistanceFromWall * -directionFacing, 0, 0); 
            PooledAnimation wallJumpAnimation = _wallJumpAnimationClip.Play(effectPosition);
                
            wallJumpAnimation.SetLocalScale(new Vector3(directionFacing, 1, 1));
            InitializePooledAnimation(wallJumpAnimation);
        }
        
        /// <summary>
        /// Called by animation events
        /// </summary>
        public void OnWallSlide()
        {
            PooledAnimation pooledAnimation = _wallSlideAnimationClip.Play(_platformerMotor.Controller.BodyCenter);
            pooledAnimation.SetLocalScale(new Vector3(_wallModule.GetWallClingDirection(), 1, 1));
            
            InitializePooledAnimation(pooledAnimation);
        }
        
        /// <summary>
        /// Called by animation events
        /// </summary>
        public void OnStepWallClimb()
        {
            PooledAnimation pooledAnimation = _wallSlideAnimationClip.Play(_platformerMotor.Controller.BodyCenter);
            pooledAnimation.SetLocalScale(new Vector3(_wallModule.GetWallClingDirection(), -1, 1));
            
            InitializePooledAnimation(pooledAnimation);
        }
        
        #endregion
        
        #region Double Jump

        [SerializeField] private EffectGroup _doubleJumpEffect;
        [SerializeField] private AnimationClip _doubleJumpAnimationClip;
        private void OnDoubleJump(object sender, PositionEventArgs e)
        {
            _doubleJumpEffect.Play(e.Position, Vector2.up);
            _jumpEffects.Play(e.Position, Vector2.up);
        }
        
        #endregion
        
        #region Animation Events
        
        [SerializeField] private AnimationClip _runDustAnimationClip;

        /// <summary>
        /// Called by animation events
        /// </summary>
        public void OnStepRun()
        {
            PooledAnimation pooledAnimation = _runDustAnimationClip.Play(_platformerMotor.GroundContactPoint);
            pooledAnimation.SetLocalScale(new Vector3(_platformerMotor.Controller.FacingLeft ? -1 : 1, 1, 1));
            
            InitializePooledAnimation(pooledAnimation);
        }
        
        #endregion*/
    }
}