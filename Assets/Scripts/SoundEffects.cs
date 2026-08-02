using UnityEngine;
using System.Collections;

public class SoundEffects : MonoBehaviour
{
    [Header("UI Sounds")]
    [SerializeField] private AudioClip turnPage01;
    [SerializeField] private AudioClip turnPage02;
    [SerializeField] private AudioClip turnPage03;
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip hudbuttonSound;
    [SerializeField] private AudioClip nameTagSound;

    [Header("Game Sounds")]
    [SerializeField] private AudioClip rouletteSound;
    [SerializeField] private AudioClip drawCard;
    [SerializeField] private AudioClip rightSlotSound;
    [SerializeField] private AudioClip wrongSlotSound;
    [SerializeField] private AudioClip clickSlotSound;

    [Header("Component Sounds")]
    [SerializeField] private AudioClip componentExplosionSound;
    [SerializeField] private AudioClip componentFinalExplosionSound;
    [SerializeField] private AudioClip componentRepairSound;

    [Header("Steam Sounds")]
    [SerializeField] private AudioClip steamValveLow;
    [SerializeField] private AudioClip steamValveMedium;
    [SerializeField] private AudioClip steamValveHigh;
    [SerializeField] private AudioSource steamLoopSourceA;
    [SerializeField] private AudioSource steamLoopSourceB;
    [SerializeField] [Range(0f, 1f)] private float steamVolume = 1f;
    [SerializeField] private float steamCrossfadeDuration = 1.5f;

    private AudioSource audioSource;
    private AudioSource _activeSteam;
    private AudioSource _inactiveSteam;
    private Coroutine _steamCrossfadeCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (steamLoopSourceA != null && steamLoopSourceB != null)
        {
            steamLoopSourceA.loop        = true;
            steamLoopSourceA.playOnAwake = false;
            steamLoopSourceA.volume      = 0f;

            steamLoopSourceB.loop        = true;
            steamLoopSourceB.playOnAwake = false;
            steamLoopSourceB.volume      = 0f;
        }

        _activeSteam   = steamLoopSourceA;
        _inactiveSteam = steamLoopSourceB;
    }

    // ── One-shot sounds ──────────────────────────────────────────────────────

    public void TurnPageSound(int number)
    {
        if (number == 1) audioSource.PlayOneShot(turnPage01);
        else if (number == 2) audioSource.PlayOneShot(turnPage03);
    }

    public void PressButtonSound()                 { audioSource.PlayOneShot(buttonSound); }
    public void PressHudButtonSound()              { audioSource.PlayOneShot(hudbuttonSound); }
    public void TagSound()                         { audioSource.PlayOneShot(nameTagSound); }
    public void PlayRouletteSound()                { audioSource.PlayOneShot(rouletteSound); }
    public void PlayDrawCardSound()                { audioSource.PlayOneShot(drawCard); }
    public void PlayRightSlotSound()               { audioSource.PlayOneShot(rightSlotSound); }
    public void PlayWrongSlotSound()               { audioSource.PlayOneShot(wrongSlotSound); }
    public void PlayClickSlotSound()               { audioSource.PlayOneShot(clickSlotSound); }
    public void PlayComponentExplosionSound()      { audioSource.PlayOneShot(componentExplosionSound); }
    public void PlayFinalComponentExplosionSound() { audioSource.PlayOneShot(componentFinalExplosionSound); }
    public void PlayComponentRepairSound()         { audioSource.PlayOneShot(componentRepairSound); }

    // ── Steam valve loop com crossfade ───────────────────────────────────────

    public void PlaySteamValveLow()    => CrossfadeToClip(steamValveLow);
    public void PlaySteamValveMedium() => CrossfadeToClip(steamValveMedium);
    public void PlaySteamValveHigh()   => CrossfadeToClip(steamValveHigh);

    public void StopSteamValve()
    {
        if (_activeSteam == null) return;

        if (_steamCrossfadeCoroutine != null)
        {
            StopCoroutine(_steamCrossfadeCoroutine);
            _steamCrossfadeCoroutine = null;
        }

        steamLoopSourceA.Stop();
        steamLoopSourceA.volume = 0f;
        steamLoopSourceB.Stop();
        steamLoopSourceB.volume = 0f;

        _activeSteam   = steamLoopSourceA;
        _inactiveSteam = steamLoopSourceB;
    }

    private void CrossfadeToClip(AudioClip newClip)
    {
        if (newClip == null || _activeSteam == null) return;

        // Já tocando esse clip em volume cheio — nada a fazer
        if (_activeSteam.isPlaying &&
            _activeSteam.clip == newClip &&
            Mathf.Approximately(_activeSteam.volume, steamVolume) &&
            _steamCrossfadeCoroutine == null)
            return;

        // Abortar crossfade anterior: para o inactive que estava entrando
        // e restaura o active para volume cheio antes de iniciar o novo crossfade
        if (_steamCrossfadeCoroutine != null)
        {
            StopCoroutine(_steamCrossfadeCoroutine);
            _steamCrossfadeCoroutine = null;
            _inactiveSteam.Stop();
            _inactiveSteam.volume = 0f;
            _activeSteam.volume   = steamVolume;
        }

        if (!_activeSteam.isPlaying)
            _steamCrossfadeCoroutine = StartCoroutine(FadeInCoroutine(newClip));
        else
            _steamCrossfadeCoroutine = StartCoroutine(CrossfadeCoroutine(newClip));
    }

    // Primeira ativação: faz fade in do novo clip sem clip anterior tocando
    private IEnumerator FadeInCoroutine(AudioClip clip)
    {
        _activeSteam.clip   = clip;
        _activeSteam.volume = 0f;
        _activeSteam.Play();

        float elapsed = 0f;
        while (elapsed < steamCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / steamCrossfadeDuration);
            t = t * t * (3f - 2f * t); // smoothstep
            _activeSteam.volume = Mathf.Lerp(0f, steamVolume, t);
            yield return null;
        }
        _activeSteam.volume      = steamVolume;
        _steamCrossfadeCoroutine = null;
    }

    // Crossfade: faz fade out do active e fade in do inactive simultaneamente,
    // depois troca os papéis das duas sources
    private IEnumerator CrossfadeCoroutine(AudioClip newClip)
    {
        _inactiveSteam.clip   = newClip;
        _inactiveSteam.volume = 0f;
        _inactiveSteam.Play();

        float startVolume = _activeSteam.volume;
        float elapsed     = 0f;

        while (elapsed < steamCrossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / steamCrossfadeDuration);
            t = t * t * (3f - 2f * t); // smoothstep

            _activeSteam.volume   = Mathf.Lerp(startVolume, 0f, t);
            _inactiveSteam.volume = Mathf.Lerp(0f, steamVolume, t);
            yield return null;
        }

        _activeSteam.volume = 0f;
        _activeSteam.Stop();
        _inactiveSteam.volume = steamVolume;

        // Swap: quem estava entrando vira o active para o próximo crossfade
        (_activeSteam, _inactiveSteam) = (_inactiveSteam, _activeSteam);

        _steamCrossfadeCoroutine = null;
    }
}
