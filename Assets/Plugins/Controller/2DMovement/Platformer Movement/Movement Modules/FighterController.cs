using DamageSystem;
using ControllerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An EntityController with a RigidBody2D that can take Damage and Knockback
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FighterController : EntityController
{
    [SerializeField] private Motor _motor;
    public Rigidbody2D Rb { get; private set; }
    
    protected virtual void FixedUpdate()
    {
        HandleHitstop();

        if (InMovementState())
        {
            _motor.HandleLocalControllerMovement();
        }
    }

    public virtual bool InMovementState()
    {
        return CurrentState == States.Movement && !_inHitstop;
    }

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        ForceUpdateState(States.Movement);
        //CreateAbilities();
    }

    #region State Machine

    public enum States
    {
        Movement,
        Ability,
        Dead,
    }

    public class StateUpdateInfo
    {
        public States OldState;
        public States NewState;
    }

    public event Action<StateUpdateInfo> OnStateChanged;
    public States CurrentState { get; private set; }

    public override bool CanAnimateFlip => CurrentState == States.Movement;

    public void UpdateState(States newState)
    {
        if (CurrentState == newState)
            return;
        ForceUpdateState(newState);
    }

    private void ForceUpdateState(States newState)
    {
        States oldState = CurrentState;
        CurrentState = newState;

        OnStateChanged?.Invoke(new StateUpdateInfo
        {
            OldState = oldState,
            NewState = newState
        });

        UpdateMotorEnabledState(newState);
    }

    private void UpdateMotorEnabledState(States newState)
    {
        _motor.enabled = newState == States.Movement;
    }

    #endregion


    #region Hurtbox

    public Hurtbox Hurtbox;
    private Vector2 _storedHitstopVelocity;

    protected virtual void Start()
    {
        Hurtbox.OnHit.AddListener(Hurtbox_OnHit);
        Hurtbox.OnDeath.AddListener(Hurtbox_OnDeath);
    }

    private void Hurtbox_OnDeath()
    {
        UpdateState(States.Dead);
    }

    [Serializable]
    public class KnockbackBehaviourSettings
    {
        public float KnockbackMultiplier = 1;
        [Tooltip("The amount of previous momentum to keep")]
        [Range(0, 1)]
        public float MomentumRetention = 0.8f;
        [Tooltip("The amount of vertical knockback thats added to every hit. Affected by the knockback multiplier")]
        public float MinimumUpVelocityAddition = 1f;
        [Tooltip("The duration of time over which the knockback gets added")]
        public float KnockbackDuration = 0.2f;
        public AnimationCurve KnockbackFalloffCurve;
        [Tooltip("The angle range from straight down in which players cannot cancel their vertical knockback with a jump")]
        public float SpikeAngleRange = 15;
        public float KnockbackStiffness = 0.9f;
    }

    public KnockbackBehaviourSettings KnockbackSettings;
    private Coroutine _knockbackCoroutine;
    private bool _yKnockbackInterrupted = false;

    private void Hurtbox_OnHit(HitEventInfo e)
    {
        EnterHitstop(e.hitInfo.hitStop, () =>
        {
            if (_knockbackCoroutine != null)
                StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(ApplyKnockback(e.hitInfo.knockback));
        });
    }

    public void InterruptYKnockback()
    {
        _yKnockbackInterrupted = true;
    }

    private IEnumerator ApplyKnockback(Vector2 inputtedKnockback)
    {
        _yKnockbackInterrupted = false;

        // Cancel some momentum
        Rb.linearVelocity *= KnockbackSettings.MomentumRetention;

        // Apply some upward force and the knockback multiplier
        Vector2 calculatedKnockback = inputtedKnockback + new Vector2(0, KnockbackSettings.MinimumUpVelocityAddition);
        calculatedKnockback *= KnockbackSettings.KnockbackMultiplier;

        // Continuously apply knockback for a duration
        float timer = KnockbackSettings.KnockbackDuration;
        while (timer > 0)
        {
            yield return new WaitForFixedUpdate();
            // Calculate a multiplier based on how long it's been since impact (value of the animation curve)
            float normalizedKnockbackTime = 1 - (timer / KnockbackSettings.KnockbackDuration);
            float knockbackTimeMultiplier = KnockbackSettings.KnockbackFalloffCurve.Evaluate(normalizedKnockbackTime);

            //ApplyIncrementalKnockbackImplementation(knockbackTimeMultiplier);
            ApplyLerpKnockback(knockbackTimeMultiplier);

            // Apply the force (accounting for timescale)
            Rb.AddForce(calculatedKnockback * Time.fixedDeltaTime);

            timer -= Time.fixedDeltaTime;
        }
        yield break;

        // Knockback that works by lerping the player's velocity to a target velocity. 
        void ApplyLerpKnockback(float knockbackTimeMultiplier)
        {
            // Calculate desired velocity for this specific knockback
            Vector2 desiredVelocity = calculatedKnockback * knockbackTimeMultiplier;

            // Calculate how closely to have the player's momentum match the desired knockback
            float t = KnockbackSettings.KnockbackStiffness * knockbackTimeMultiplier * Time.fixedDeltaTime;

            // Weaker knockback will have a lower t value, and stronger knockback will have a higher t value
            t *= desiredVelocity.magnitude;

            // Optionally disable y knockback (Used so that jumps don't get eaten when hit horizontally)
            if (_yKnockbackInterrupted)
            {
                // If the player is being sent virtually straight down, don't allow them to jump out of it
                bool withinSpikeRange = Vector2.Angle(Vector2.down, inputtedKnockback) < KnockbackSettings.SpikeAngleRange;

                if (!withinSpikeRange)
                {
                    desiredVelocity.y = Rb.linearVelocity.y;
                }
            }

            // Lerp the players velocity towards the desired knockback velocity
            Rb.linearVelocity = Vector2.Lerp(Rb.linearVelocity, desiredVelocity, t);
        }
    }

    /// --- HITSTOP ---

    private bool _inHitstop;
    private readonly Timer _hitstopTimer = new Timer();
    private readonly List<Action> _onHitstopCompleted = new List<Action>();

    public void EnterHitstop(float duration, Action completed = null)
    {
        /*foreach (Ability ability in Abilities)
        {
            ability.Paused = true;
        }*/
        
        _inHitstop = true;
        _hitstopTimer.Start(duration);

        // Store velocity
        _storedHitstopVelocity = Rb.linearVelocity;

        // Store events for when hitstun ends
        if (completed != null)
            _onHitstopCompleted.Add(completed);
    }

    private void HandleHitstop()
    {
        if (!_inHitstop)
            return;

        if (_hitstopTimer.finished)
        {
            ExitHitstop();
        }
        else
        {
            float counteractGravityVelocity = -Physics2D.gravity.y * Rb.gravityScale * Time.fixedDeltaTime;
            Rb.linearVelocity = new Vector2(0, counteractGravityVelocity);
        }
    }

    private void ExitHitstop()
    {
        /*foreach (Ability ability in Abilities)
        {
            ability.Paused = false;
        }*/
        
        // Add velocity back
        Rb.linearVelocity = _storedHitstopVelocity;

        // Invoke delayed events
        foreach (Action action in _onHitstopCompleted)
        {
            action.Invoke();
        }
        _onHitstopCompleted.Clear();
        _inHitstop = false;
    }

    #endregion


    /*#region Abilities

    // Spawning abilities On Start
    [SerializeField] private EffectGroup onPrimaryCooldownRechargedEffect;
    [SerializeField] private EffectGroup onSecondaryCooldownRechargedEffect;
    [SerializeField] private AbilityConfig[] startAbilities;
    public event EventHandler OnAbilitiesChange;

    public void OnDestroy()
    {
        DestroyAbilities();
    }

    protected override void OnPossess(PlayerBrain playerBrain)
    {
        AttachAbilitiesToInput();
    }

    protected override void OnDepossess(PlayerBrain playerBrain)
    {
        UnattachAbilitiesFromInput();
    }

    [Serializable]
    private struct AbilityConfig
    {
        public AbilitySO abilitySO;
        public InputMapping inputMapping;
    }

    private void AttachAbilitiesToInput()
    {
        foreach (var ability in Abilities)
        {
            ability.TryLinkControllerInput();
        }
    }

    private void UnattachAbilitiesFromInput()
    {
        foreach (var ability in Abilities)
        {
            ability.UnlinkControllerInput();
        }
    }

    private void CreateAbilities()
    {
        foreach (AbilityConfig abilityConfig in startAbilities)
        {
            CreateAbility(abilityConfig.abilitySO, abilityConfig.inputMapping);
        }
    }

    // Used to despawn abilities when this controller is despawned
    public List<Ability> Abilities = new List<Ability>();

    internal void CreateAbility(AbilitySO ability, InputMapping inputMapping)
    {
        Ability newAbility = Instantiate(ability.prefab, null).GetComponent<Ability>();
        newAbility.NetworkObject.SpawnWithOwnership(OwnerClientId, true);
        newAbility.NetworkObject.TrySetParent(NetworkObject);
        newAbility.AttachToController(ability, this, inputMapping);
    }

    public void AddAbility(Ability ability)
    {
        Abilities.Add(ability);
        ability.OnCooldownFinished += Ability_OnCooldownFinished;

        // Remove null abilities
        for (int i = Abilities.Count - 1; i >= 0; i--)
        {
            if (Abilities[i] == null)
                Abilities.RemoveAt(i);
        }

        OnAbilitiesChange?.Invoke(this, EventArgs.Empty);
    }

    private void Ability_OnCooldownFinished(object sender, EventArgs e)
    {
        switch ((sender as Ability).InputMapping)
        {
            case InputMapping.PrimaryAbilityButton:
                break;
            case InputMapping.SecondaryAbilityButton:
                onPrimaryCooldownRechargedEffect.Play(BodyCenter, Vector2.down);
                break;
            case InputMapping.TertiaryAbilityButton:
                onSecondaryCooldownRechargedEffect.Play(BodyCenter, Vector2.down);
                break;
            default:
                break;
        }
    }

    public void DestroyAbilities()
    {
        // Don't despawn abilities when switching scenes. Switching scenes will do that for us
        if (NetworkSceneTransition.Instance.WaitingForClientToLoadScene)
            return;
        
        foreach (Ability ability in Abilities)
        {
            if (ability == null || !ability.NetworkObject.IsSpawned)
                continue;

            ability.NetworkObject.TryRemoveParent(true);
            ability.NetworkObject.Despawn();
            Destroy(ability.gameObject);
        }
    }

    #endregion*/
}
