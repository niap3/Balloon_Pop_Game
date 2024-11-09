using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Screens")]
    public GameObject startScreen;
    public GameObject pauseScreen;
    public GameObject gameOverScreen;

    [Header("Shutter")]
    public Animator gateAnimator;

    [Header("Pause/Play")]
    public Button pauseButton;
    public Sprite pauseIcon;
    public Sprite resumeIcon;

    [Header("Play/Restart")]
    public Button playButton;
    public Button restartButton;

    [Header("Slider")]
    public Slider spawnRateSlider;

    [Header("Sound Buttons")]
    public List<Button> soundOnButtons;
    public List<Button> soundOffButtons;

    [Header("Night/Light")]
    public Button lightModeButton;
    public Button darkModeButton;
    public Animator backgroundTransitionAnimator;

    [Header("Info Screen")]
    public Button infoButton;
    public Button infoCloseButton;
    public GameObject infoPopup;

    private bool isSoundOn = true;
    public bool isPaused = false;
    private Image pauseButtonImage;
    private bool isTransitioning = false;
    private Coroutine pauseCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        pauseButtonImage = pauseButton.GetComponent<Image>();

        ShowStartScreen();

        // adding listeners
        // TODO: make function just for adding listeners
        playButton.onClick.AddListener(OnPlayButtonClicked);
        pauseButton.onClick.AddListener(OnPauseButtonClicked);
        restartButton.onClick.AddListener(RestartGame);
        lightModeButton.onClick.AddListener(LightModeButtonClicked);
        darkModeButton.onClick.AddListener(DarkModeButtonClicked);
        spawnRateSlider.onValueChanged.AddListener(delegate { BalloonManager.Instance.AdjustSpawnRate(); });
        infoButton.onClick.AddListener(InfoPopupOpen);
        infoCloseButton.onClick.AddListener(InfoPopupClose);

        foreach (Button btn in soundOnButtons)
            btn.onClick.AddListener(() => SetSoundState(true));

        foreach (Button btn in soundOffButtons)
            btn.onClick.AddListener(() => SetSoundState(false));

        isSoundOn = PlayerPrefs.GetInt("SoundState", 1) == 1;
        UpdateSoundState();
    }

    private void InfoPopupOpen()
    {
        infoPopup.SetActive(true);
    }

    private void InfoPopupClose()
    {
        infoPopup.SetActive(false);
    }

    private void LightModeButtonClicked()
    {
        backgroundTransitionAnimator.Play("DarkToLight");
        OnPauseButtonClicked();
    }

    private void DarkModeButtonClicked()
    {
        backgroundTransitionAnimator.Play("LightToDark");
        OnPauseButtonClicked();
    }

    public void SetSoundState(bool soundOn)
    {
        isSoundOn = soundOn;
        UpdateSoundState();
        
        //! IMP
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateSoundState()
    {
        AudioManager.Instance.SetSoundState(isSoundOn);
    }

    public void ShowStartScreen()
    {
        startScreen.SetActive(true);
        pauseScreen.SetActive(false);
        gameOverScreen.SetActive(false);

        Time.timeScale = 1;
    }

    public void OnPlayButtonClicked()
    {
        gateAnimator.Play("GateOpen");

        playButton.interactable = false;

        float animationDuration = GetAnimationClipLength("GateOpen");
        Invoke(nameof(StartGame), animationDuration);
    }

    private void StartGame()
    {
        pauseButton.gameObject.SetActive(true);
        
        startScreen.SetActive(false);

        GameManager.Instance.StartGame();

        playButton.interactable = true;
    }

    public void OnPauseButtonClicked()
    {
        if (isTransitioning)
            return;

        if (!isPaused)
        {
            isPaused = true;
            pauseButtonImage.sprite = resumeIcon;

            gateAnimator.Play("GatePause");

            pauseScreen.SetActive(true);

            isTransitioning = true;

            if (pauseCoroutine != null)
                StopCoroutine(pauseCoroutine);

            pauseCoroutine = StartCoroutine(PauseGameAfterAnimation());
        }
        else
        {
            isPaused = false;
            pauseButtonImage.sprite = pauseIcon;

            // TODO: shouldnt be here but works for now
            gateAnimator.Play("GateResume");

            pauseScreen.SetActive(false);

            isTransitioning = true;

            if (pauseCoroutine != null)
                StopCoroutine(pauseCoroutine);

            pauseCoroutine = StartCoroutine(ResumeGameAfterAnimation());
        }
    }

    private IEnumerator PauseGameAfterAnimation()
    {
        float animationDuration = GetAnimationClipLength("GatePause");
        yield return new WaitForSeconds(animationDuration);

        Time.timeScale = 0;

        isTransitioning = false;
    }

    private IEnumerator ResumeGameAfterAnimation()
    {
        Time.timeScale = 1;

        float animationDuration = GetAnimationClipLength("GateResume");
        yield return new WaitForSeconds(animationDuration);

        isTransitioning = false;
    }

    public void OnGameOver()
    {
        gateAnimator.Play("GateClose");

        float animationDuration = GetAnimationClipLength("GateClose");
        Invoke(nameof(ShowGameOverScreen), animationDuration);
    }

    private void ShowGameOverScreen()
    {
        gameOverScreen.SetActive(true);

        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;

        gameOverScreen.SetActive(false);

        gateAnimator.Play("GateOpen");

        float animationDuration = GetAnimationClipLength("GateOpen");
        Invoke(nameof(StartGame), animationDuration);
    }

    private float GetAnimationClipLength(string clipName)
    {
        AnimationClip[] clips = gateAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
            if (clip.name == clipName)
                return clip.length;
        return 0f;
    }
}
