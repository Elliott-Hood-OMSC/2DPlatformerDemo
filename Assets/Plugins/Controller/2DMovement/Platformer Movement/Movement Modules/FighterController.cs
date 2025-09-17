using DamageSystem;
using NetControllerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterController : RigidbodyController
{
    [SerializeField] private Motor _motor;

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
        return CurrentState == States.Movement && !_inHitstun;
    }

    protected override void Awake()
    {
        ForceUpdateState(States.Movement);
        ResetRigidbodySettings();
        //CreateAbilities();
    }

    #region State Machine

    public enum States
    {
        Movement,
        Ability,
        Dead,
    }

    public class StateEventArgs : EventArgs
    {
        public States oldState;
        public States newState;
    }

    public event EventHandler<StateEventArgs> OnStateChanged;
    public States CurrentState { get; private set; }

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

        OnStateChanged?.Invoke(this, new StateEventArgs
        {
            oldState = oldState,
            newState = newState
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
    private Vector2 _storedHitstunVelocity;

    protected override void Start()
    {
        base.Start();

        Hurtbox.OnHit.AddListener(Damageable_OnTakeDamage);
        Hurtbox.OnDeath.AddListener(Hurtbox_OnDeath);
    }

    private void Hurtbox_OnDeath()
    {
        UpdateState(States.Dead);
    }

    [Serializable]
    public class KnockbackBehaviourSettings
    {
        public float knockbackMultiplier = 1;
        [Tooltip("The amount of previous momentum to keep")]
        [Range(0, 1)]
        public float momentumRetention = 0.8f;
        [Tooltip("The amount of vertical knockback thats added to every hit. Affected by the knockback multiplier")]
        public float minimumUpVelocityAddition = 1f;
        [Tooltip("The duration of time over which the knockback gets added")]
        public float knockbackDuration = 0.2f;
        public AnimationCurve knockbackFalloffCurve;
        [Tooltip("The angle range from straight down in which players cannot cancel their vertical knockback with a jump")]
        public float spikeAngleRange = 15;
        public float knockbackStiffness;
    }

    public KnockbackBehaviourSettings KnockbackSettings;
    private Coroutine _knockbackCoroutine;
    private bool _yKnockbackInterrupted = false;

    private void Damageable_OnTakeDamage(HitEventArgs e)
    {
        if (Hurtbox.Invincible) return;
        
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
        Rb.linearVelocity *= KnockbackSettings.momentumRetention;

        // Apply some upward force and the knockback multiplier
        Vector2 calculatedKnockback = inputtedKnockback + new Vector2(0, KnockbackSettings.minimumUpVelocityAddition);
        calculatedKnockback *= KnockbackSettings.knockbackMultiplier;

        // Continuously apply knockback for a duration
        float timer = KnockbackSettings.knockbackDuration;
        while (timer > 0)
        {
            yield return new WaitForFixedUpdate();
            // Calculate a multiplier based on how long it's been since impact (value of the animation curve)
            float normalizedKnockbackTime = 1 - (timer / KnockbackSettings.knockbackDuration);
            float knockbackTimeMultiplier = KnockbackSettings.knockbackFalloffCurve.Evaluate(normalizedKnockbackTime);

            //ApplyIncrementalKnockbackImplementation(knockbackTimeMultiplier);
            ApplyLerpKnockback(knockbackTimeMultiplier);

            /*            // Apply the force (accounting for timescale)
                        Rb.AddForce(calculatedKnockback * Time.fixedDeltaTime);*/

            timer -= Time.fixedDeltaTime;
        }
        yield break;

        // Knockback that works by lerping the player's velocity to a target velocity. 
        // This gives a desired visual effect, but causes things like jumping after getting hit to get nullified?
        void ApplyLerpKnockback(float knockbackTimeMultiplier)
        {
            // Calculate desired velocity for this specific knockback
            Vector2 desiredVelocity = calculatedKnockback * knockbackTimeMultiplier;

            // Calculate how closely to have the player's momentum match the desired knockback
            float t = KnockbackSettings.knockbackStiffness * knockbackTimeMultiplier * Time.fixedDeltaTime;

            // Weaker knockback will have a lower t value, and stronger knockback will have a higher t value
            t *= desiredVelocity.magnitude;

            // Optionally disable y knockback (Used so that jumps don't get eaten when hit horizontally)
            if (_yKnockbackInterrupted)
            {
                // If the player is being sent virtually straight down, don't allow them to jump out of it
                bool withinSpikeRange = Vector2.Angle(Vector2.down, inputtedKnockback) < KnockbackSettings.spikeAngleRange;

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

    private bool _inHitstun = false;
    private readonly Timer _hitstunTimer = new Timer();
    private readonly List<Action> _onHitstopCompleted = new List<Action>();

    public void EnterHitstop(float duration, Action completed = null)
    {
        /*foreach (Ability ability in Abilities)
        {
            ability.Paused = true;
        }*/
        
        _inHitstun = true;
        _hitstunTimer.Start(duration);
        SetRigidbodySettings(RigidbodySettings.Weightless);

        // Store velocity
        _storedHitstunVelocity = Rb.linearVelocity;

        // Store events for when hitstun ends
        if (completed != null)
            _onHitstopCompleted.Add(completed);
    }

    private void HandleHitstop()
    {
        if (!_inHitstun)
            return;

        if (_hitstunTimer.finished)
        {
            ExitHitstop();
        }
        else
        {
            Rb.linearVelocity = Vector2.zero;
        }
    }

    private void ExitHitstop()
    {
        /*foreach (Ability ability in Abilities)
        {
            ability.Paused = false;
        }*/
        
        // Add velocity back
        Rb.linearVelocity = _storedHitstunVelocity;

        // Invoke delayed events
        foreach (var action in _onHitstopCompleted)
        {
            action.Invoke();
        }
        _onHitstopCompleted.Clear();
        _inHitstun = false;

        ResetRigidbodySettings();
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


    #region Rigidbody Settings

    public override void SetRigidbodySettings(RigidbodySettings settings)
    {
        base.SetRigidbodySettings(settings);
        _motor?.SetRigidbodySettings(settings);
    }

    public virtual void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    #endregion
}
