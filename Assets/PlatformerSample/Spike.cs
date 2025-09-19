using DamageSystem;
using DG.Tweening;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float _shakeIntensity = 0.3f;
    [SerializeField] private Hitbox _hitbox;
    [SerializeField] private ParticleSystem _hitParticles;

    private void Start()
    {
        _hitbox.Activate();
        _hitbox.OnHit.AddListener(PlayHitEffects);
    }

    private void OnDestroy()
    {
        _hitbox.OnHit.RemoveListener(PlayHitEffects);
    }

    private void PlayHitEffects(HitEventInfo hitInfo)
    {
        _hitParticles.transform.position = hitInfo.hitInfo.hitPosition;
        _hitParticles.Play();

        // This is a terrible way to do camera shake, don't do this lol
        Camera.main?.transform.DOShakePosition(0.25f, _shakeIntensity, vibrato: 30)
            .OnComplete(() =>
            {
                Camera.main?.transform.DOMove(new Vector3(0, 0, -10), 0.05f);
            });
    }
}
