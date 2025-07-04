using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

// 게임 오버 상태를 표현하고, 게임 점수와 UI를 관리하는 게임 매니저
// 씬에는 단 하나의 게임 매니저만 존재할 수 있다.
public class GameManager : MonoBehaviour {
    public static GameManager instance; // 싱글톤을 할당할 전역 변수

    public bool isGameover = false; // 게임 오버 상태
    public TextMeshProUGUI scoreText; // 점수를 출력할 UI 텍스트
    public GameObject gameoverUI; // 게임 오버시 활성화 할 UI 게임 오브젝트
    public Button restartButton; // 재시작 버튼
    public CanvasGroup gameoverCanvasGroup; // 게임 오버 UI 페이드 효과용
    public AudioClip gameoverSoundClip; // 게임 오버 사운드
    public TextMeshProUGUI finalScoreText; // 최종 점수 표시 텍스트
    public MonoBehaviour gameOverUIController; // 게임 오버 UI 컨트롤러
    
    [Header("추가 효과 (옵션)")]
    public MonoBehaviour uiAnimationController; // UI 애니메이션 컨트롤러
    public MonoBehaviour gameOverEffectController; // 게임 오버 효과 컨트롤러

    private int score = 0; // 게임 점수
    private AudioSource audioSource; // 오디오 소스

    // 게임 시작과 동시에 싱글톤을 구성
    void Awake() {
        // 싱글톤 변수 instance가 비어있는가?
        if (instance == null)
        {
            // instance가 비어있다면(null) 그곳에 자기 자신을 할당
            instance = this;
        }
        else
        {
            // instance에 이미 다른 GameManager 오브젝트가 할당되어 있는 경우

            // 씬에 두개 이상의 GameManager 오브젝트가 존재한다는 의미.
            // 싱글톤 오브젝트는 하나만 존재해야 하므로 자신의 게임 오브젝트를 파괴
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }

    void Start() {
        // 오디오 소스 컴포넌트 가져오기 (없으면 추가)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 재시작 버튼 이벤트 등록
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // 게임 오버 UI 초기화
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(false);
            
            // CanvasGroup 초기화
            if (gameoverCanvasGroup != null)
            {
                gameoverCanvasGroup.alpha = 0f;
            }
        }
    }

    void Update() {
        if (isGameover && Input.GetMouseButtonDown(0))
        {
            // 게임 오버 상태에서 마우스 왼쪽 버튼을 클릭하면 현재 씬 재시작
            RestartGame();
        }
    }

    // 게임 재시작 메서드
    public void RestartGame() {
        // 버튼 클릭 사운드 효과
        if (audioSource != null && restartButton != null)
        {
            // 버튼 클릭 효과 (스케일 애니메이션)
            StartCoroutine(ButtonClickEffect());
        }
        
        // 약간의 지연 후 씬 재시작
        StartCoroutine(RestartGameCoroutine());
    }

    // 버튼 클릭 효과 코루틴
    private IEnumerator ButtonClickEffect() {
        if (restartButton != null)
        {
            Transform buttonTransform = restartButton.transform;
            Vector3 originalScale = buttonTransform.localScale;
            
            // 버튼 축소
            buttonTransform.localScale = originalScale * 0.9f;
            yield return new WaitForSeconds(0.1f);
            
            // 버튼 복원
            buttonTransform.localScale = originalScale;
        }
    }

    // 게임 재시작 코루틴 (딜레이 포함)
    private IEnumerator RestartGameCoroutine() {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 점수를 증가시키는 메서드
    public void AddScore(int newScore) {
        // 게임 오버가 아니라면
        if (!isGameover)
        {
            // 점수를 증가
            score += newScore;
            scoreText.text = "Score : " + score;
        }
    }

    // 플레이어 캐릭터가 사망시 게임 오버를 실행하는 메서드
    public void OnPlayerDead() {
        // 현재 상태를 게임 오버 상태로 변경
        isGameover = true;
        
        // 게임 오버 UI 컨트롤러 사용 (있다면)
        if (gameOverUIController != null)
        {
            gameOverUIController.SendMessage("ShowGameOverUI", score, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            // 기본 게임 오버 UI 처리
            ShowDefaultGameOverUI();
        }
    }
    
    // 기본 게임 오버 UI 표시 (컨트롤러가 없을 때)
    private void ShowDefaultGameOverUI()
    {
        // 최종 점수 표시
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
        }
        
        // 게임 오버 효과 재생 (있다면)
        if (gameOverEffectController != null)
        {
            gameOverEffectController.SendMessage("PlayGameOverEffect", SendMessageOptions.DontRequireReceiver);
        }
        
        // 게임 오버 사운드 재생
        if (audioSource != null && gameoverSoundClip != null)
        {
            audioSource.PlayOneShot(gameoverSoundClip);
        }
        
        // 게임 오버 UI 애니메이션으로 표시
        StartCoroutine(ShowGameOverUI());
    }

    // 게임 오버 UI 표시 애니메이션 코루틴
    private IEnumerator ShowGameOverUI() {
        // 약간의 지연 (효과가 먼저 재생되도록)
        yield return new WaitForSeconds(0.5f);
        if (gameoverUI != null)
        {
            gameoverUI.SetActive(true);
            // UI 애니메이션 컨트롤러가 있다면 사용
            if (uiAnimationController != null)
            {
                uiAnimationController.SendMessage("PlayUIAnimation", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // DOTween을 이용한 멋진 애니메이션
                Transform uiTransform = gameoverUI.transform;
                Vector3 originalScale = uiTransform.localScale;
                uiTransform.localScale = Vector3.zero;
                if (gameoverCanvasGroup != null)
                {
                    gameoverCanvasGroup.alpha = 0f;
                    // 페이드 인
                    gameoverCanvasGroup.DOFade(1f, 0.6f).SetEase(Ease.OutCubic);
                }
                // 스케일 업 + 팝 효과
                uiTransform.DOScale(originalScale * 1.1f, 0.4f).SetEase(Ease.OutBack)
                    .OnComplete(() => uiTransform.DOScale(originalScale, 0.15f).SetEase(Ease.InOutSine));
                // 애니메이션이 끝날 때까지 대기
                yield return new WaitForSeconds(0.6f);
            }
        }
    }
    
    // 기본 UI 애니메이션 (컨트롤러가 없을 때)
    private IEnumerator DefaultUIAnimation()
    {
        Transform uiTransform = gameoverUI.transform;
        Vector3 originalScale = uiTransform.localScale;
        uiTransform.localScale = Vector3.zero;
        
        // 페이드 인 효과
        float fadeInTime = 0.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeInTime;
            
            // 이징 효과 (EaseOutBack)
            float scale = EaseOutBack(progress);
            uiTransform.localScale = originalScale * scale;
            
            // 페이드 인
            if (gameoverCanvasGroup != null)
            {
                gameoverCanvasGroup.alpha = progress;
            }
            
            yield return null;
        }
        
        // 최종 값 설정
        uiTransform.localScale = originalScale;
        if (gameoverCanvasGroup != null)
        {
            gameoverCanvasGroup.alpha = 1f;
        }
    }

    // EaseOutBack 이징 함수
    private float EaseOutBack(float t) {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}