using System.Collections;
using UnityEngine;

public class WaveArrowController : MonoBehaviour
{
    [Header("Arrow References")]
    [SerializeField] private GameObject[] arrowObjects; // 화살표 오브젝트 배열

    [Header("Animation Settings")]
    [SerializeField] private float rotationSpeed = 60f; // 초당 회전 각도
    [SerializeField] private float showDuration = 5f; // 화살표 표시 지속 시간
    [SerializeField] private float fadeOutDuration = 0.5f; // 페이드 아웃 지속 시간

    [Header("Wave Settings")]
    private int firstArrowStartWave = 1;
    private int secondArrowStartWave = 5; // 두 번째 화살표 활성화 웨이브
    private int thirdArrowStartWave = 7; // 세 번째 화살표 활성화 웨이브
    private bool isFirstSpawnPointAddedCompleted = false;   // SpawnPointAdded가 이전에 실행되었는지 체크하는 불리언 변수

    private Coroutine[] rotationCoroutines;
    private Coroutine[] fadeOutCoroutines;

    private void Awake()
    {
        // 배열 초기화
        rotationCoroutines = new Coroutine[arrowObjects.Length];
        fadeOutCoroutines = new Coroutine[arrowObjects.Length];
        
        // 모든 화살표 비활성화
        foreach (var arrow in arrowObjects)
        {
            if (arrow != null)
                arrow.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveStart;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStart;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void HandleWaveStart(int waveNumber)
    {
        // 모든 코루틴 중지
        StopAllArrowCoroutines();
        
        // 웨이브 1,5,7은 다이얼로그 종료 후 처리하므로 제외
        if (waveNumber == firstArrowStartWave || waveNumber == secondArrowStartWave || waveNumber == thirdArrowStartWave)
        {
            return;
        }
        
        // 지연 후 화살표 표시
        StartCoroutine(DelayedShowArrows(waveNumber));
    }

    private void HandleDialogueEnded(EventManager.DialogueType dialogueType)
    {
        // 다이얼로그 종료 후 웨이브 화살표 표시 (1초 지연)
        if (dialogueType == EventManager.DialogueType.Tutorial)
        {
            StartCoroutine(DelayedShowArrows(firstArrowStartWave));
        }
        else if (dialogueType == EventManager.DialogueType.SpawnPointAdded)
        {
            StartCoroutine(DelayedShowArrows(isFirstSpawnPointAddedCompleted ? thirdArrowStartWave : secondArrowStartWave));

            isFirstSpawnPointAddedCompleted = true;
        }
    }
    
    private IEnumerator DelayedShowArrows(int waveNumber)
    {
        // 게임 매니저와 같은 1초 지연 적용
        yield return new WaitForSeconds(1f);
        
        // 지연 후 화살표 표시
        ShowArrowsByWave(waveNumber);
    }

    private void ShowArrowsByWave(int waveNumber)
    {
        if (waveNumber == 10)
        {
            return;
        }
        
        // 활성화할 화살표 개수 결정
        int arrowsToShow = 1; // 기본적으로 첫 번째 화살표는 항상 보임
        
        if (waveNumber >= secondArrowStartWave)
        {
            arrowsToShow = 2;
        }
        
        if (waveNumber >= thirdArrowStartWave)
        {
            arrowsToShow = 3;
        }
        
        // 모든 코루틴 중지
        StopAllArrowCoroutines();
        
        // 해당 웨이브에 맞는 화살표만 활성화 및 애니메이션 적용
        for (int i = 0; i < arrowObjects.Length; i++)
        {
            if (i < arrowsToShow)
            {
                StartArrowAnimation(i);
            }
            else
            {
                if (arrowObjects[i] != null)
                    arrowObjects[i].SetActive(false);
            }
        }
    }

    private void StartArrowAnimation(int arrowIndex)
    {
        if (arrowIndex >= 0 && arrowIndex < arrowObjects.Length && arrowObjects[arrowIndex] != null)
        {
            GameObject arrow = arrowObjects[arrowIndex];
            arrow.SetActive(true);
            
            // 화살표를 초기 상태로 리셋 - Z축은 -90도로 설정
            arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
            
            // 렌더러 컴포넌트 가져오기 (머티리얼 투명도를 위해)
            Renderer renderer = arrow.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                Color originalColor = renderer.material.color;
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
            }
            
            // 기존 코루틴 중지
            if (rotationCoroutines[arrowIndex] != null)
                StopCoroutine(rotationCoroutines[arrowIndex]);
            
            if (fadeOutCoroutines[arrowIndex] != null)
                StopCoroutine(fadeOutCoroutines[arrowIndex]);
            
            // 새 코루틴 시작
            rotationCoroutines[arrowIndex] = StartCoroutine(RotateArrow(arrow, arrowIndex));
            fadeOutCoroutines[arrowIndex] = StartCoroutine(FadeOutArrow(arrow, arrowIndex));
        }
    }
    
    private IEnumerator RotateArrow(GameObject arrow, int arrowIndex)
    {
        float elapsedTime = 0f;
        
        // showDuration 동안 회전
        while (elapsedTime < showDuration && arrow != null && arrow.activeInHierarchy)
        {
            // Y축으로만 회전 (Z축 -90도 유지)
            float currentYRotation = arrow.transform.rotation.eulerAngles.y;
            arrow.transform.rotation = Quaternion.Euler(0, currentYRotation + (rotationSpeed * Time.deltaTime), -90);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rotationCoroutines[arrowIndex] = null;
    }
    
    private IEnumerator FadeOutArrow(GameObject arrow, int arrowIndex)
    {
        // showDuration 시간만큼 대기한 후 페이드 아웃 시작
        yield return new WaitForSeconds(showDuration);
        
        // 화살표가 활성화 상태인지 확인
        if (arrow != null && arrow.activeInHierarchy)
        {
            Renderer renderer = arrow.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                Color originalColor = renderer.material.color;
                float elapsedTime = 0f;
                
                // fadeOutDuration 동안 알파값 감소
                while (elapsedTime < fadeOutDuration)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                    renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                // 완전히 투명하게 설정
                renderer.material.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            }
            
            // 애니메이션 완료 후 화살표 비활성화
            arrow.SetActive(false);
        }
        
        fadeOutCoroutines[arrowIndex] = null;
    }
    
    private void StopAllArrowCoroutines()
    {
        for (int i = 0; i < arrowObjects.Length; i++)
        {
            if (rotationCoroutines[i] != null)
            {
                StopCoroutine(rotationCoroutines[i]);
                rotationCoroutines[i] = null;
            }
            
            if (fadeOutCoroutines[i] != null)
            {
                StopCoroutine(fadeOutCoroutines[i]);
                fadeOutCoroutines[i] = null;
            }
        }
    }
}