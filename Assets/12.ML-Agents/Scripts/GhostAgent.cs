using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GhostAgent : Agent
{
    #region 필드 변수

    [Header("기본 설정")]
    public float moveSpeed = 10f;

    [Header("보상 설정")]
    public float cameraTriggerReward = 1.0f;     // 카메라 영역 도달 보상
    public float cameraNearbyReward = 0.5f;      // 카메라 근처 보상
    public float boundaryPenalty = -1.0f;        // 경계 벗어남 페널티
    public float timeBonus = 1.0f;              // 빠른 도달 시 추가 보상 (최대값)
    public float progressRewardScale = 0.05f;    // 진행도 보상 계수
    public float efficiencyRewardScale = 0.02f;  // 효율적 경로 보상 계수
    public float idlePenalty = -0.001f;          // 제자리에 머무를 때 작은 페널티

    [Header("시간 설정")]
    public float maxEpisodeTime = 30f;           // 에피소드 최대 지속 시간 (초)
    
    [Header("경계 설정")]
    public Vector3 boundaryMin; // 초기값 제거
    public Vector3 boundaryMax; // 초기값 제거

    [Header("수동 결정 요청 설정")]
    public bool manualDecisionRequest = true;    // Decision Requester가 없을 때 수동으로 결정 요청

    private Rigidbody rb;
    private Camera currentTargetCamera;
    private float distanceToTarget;
    private float previousDistance; // 이전 프레임의 거리
    private EventManager eventManager;
    private bool targetFound = false;             // 타겟 찾음 여부
    private bool hasDecisionRequester = false;    // Decision Requester 컴포넌트 존재 여부
    
    // 새로운 변수들
    private float episodeStartTime;              // 에피소드 시작 시간
    private float initialDistance;               // 초기 거리
    private Vector3 lastPosition;                // 이전 위치 (제자리 감지용)
    private float totalDistanceTraveled;         // 이동한 총 거리
    private int stepsSinceStart;                 // 시작 후 스텝 수

    #endregion

    #region Agent 이벤트 함수

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        eventManager = EventManager.Instance;

        // Decision Requester 컴포넌트 확인
        hasDecisionRequester = GetComponent<DecisionRequester>() != null;
        if (!hasDecisionRequester)
        {
            manualDecisionRequest = true;
        }
        else
        {
            manualDecisionRequest = false;  // Decision Requester가 있으면 수동 요청 비활성화
        }

        if (eventManager != null)
        {
            eventManager.OnCameraChanged += HandleCameraChanged;
        }

        // 초기 카메라 설정
        var cameraController = FindFirstObjectByType<SimpleCameraController>();
        if (cameraController != null)
        {
            currentTargetCamera = cameraController.GetCurrentCamera();
        }
        
        // Rigidbody 설정 확인 및 조정
        if (rb != null)
        {
            // 키네마틱 설정 확인
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
            
            // 중력 끄기
            rb.useGravity = false;
            
            // 드래그 값 설정 (저항값 - 낮을수록 잘 움직임)
            rb.linearDamping = 0.1f;
            
            // 회전 제약 설정 (모든 회전 금지)
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    // 카메라 변경 이벤트 처리
    private void HandleCameraChanged(Camera newCamera, string position)
    {
        currentTargetCamera = newCamera;
    }

    protected override void OnDisable()
    {
        base.OnDisable(); // 부모 클래스의 OnDisable 호출 필수
        if (eventManager != null)
        {
            eventManager.OnCameraChanged -= HandleCameraChanged;
        }
    }

    // 충돌 감지 (카메라 트리거와의 충돌)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CameraTrigger"))
        {
            // 카메라 트리거 영역에 들어갔을 때 높은 보상
            AddReward(cameraTriggerReward);
            
            // 시간 기반 보너스 보상 (빠를수록 높은 보상)
            float elapsedTime = Time.time - episodeStartTime;
            if (elapsedTime < maxEpisodeTime)
            {
                float timeRatio = 1.0f - (elapsedTime / maxEpisodeTime); // 0~1 사이 값, 빠를수록 1에 가까움
                float timeBonus = this.timeBonus * timeRatio;
                AddReward(timeBonus);
                Debug.Log($"Time Bonus: {timeBonus:F2} (elapsed: {elapsedTime:F2}s)");
            }
            
            // 효율적 경로 보너스 (직선거리 대비 실제 이동 거리가 적을수록 높은 보상)
            if (initialDistance > 0 && totalDistanceTraveled > 0)
            {
                float pathEfficiency = initialDistance / totalDistanceTraveled; // 1에 가까울수록 효율적
                float efficiencyBonus = efficiencyRewardScale * pathEfficiency * 10; // 스케일링
                AddReward(efficiencyBonus);
                Debug.Log($"Path Efficiency: {pathEfficiency:F2}, Bonus: {efficiencyBonus:F2}");
            }
            
            targetFound = true;  // 타겟 찾음 표시
            EndEpisode();
        }
        else if (other.CompareTag("CameraNearby"))
        {
            // 카메라 주변 영역에 들어갔을 때 중간 보상
            AddReward(cameraNearbyReward);
        }
    }

    private void Update()
    {
        // 시간 기반 종료 조건 - 너무 오래 걸리면 에피소드 종료
        if (maxEpisodeTime > 0 && Time.time - episodeStartTime > maxEpisodeTime && !targetFound)
        {
            AddReward(-0.5f); // 시간 초과 페널티
            EndEpisode();
        }
    }

    #endregion

    #region ML-Agents 메서드

    public override void OnEpisodeBegin()
    {
        // 물리 속도만 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 상태 초기화
        targetFound = false;
        episodeStartTime = Time.time;
        totalDistanceTraveled = 0f;
        stepsSinceStart = 0;
        lastPosition = transform.position;

        // 카메라 거리 초기화
        if (currentTargetCamera != null)
        {
            previousDistance = Vector3.Distance(transform.position, currentTargetCamera.transform.position);
            initialDistance = previousDistance; // 초기 거리 저장
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (currentTargetCamera == null) return;

        // 현재 위치 관측 (Y축 제외)
        Vector3 position = transform.localPosition;
        sensor.AddObservation(position.x);
        sensor.AddObservation(position.z);

        // 카메라와의 상대 위치 (Y축 제외)
        Vector3 directionToCamera = currentTargetCamera.transform.position - transform.localPosition;
        directionToCamera.y = 0; // Y축은 무시
        Vector3 normalizedDirection = directionToCamera.normalized;
        sensor.AddObservation(normalizedDirection.x);
        sensor.AddObservation(normalizedDirection.z);

        // 카메라와의 XZ 평면 거리
        distanceToTarget = new Vector2(directionToCamera.x, directionToCamera.z).magnitude;
        sensor.AddObservation(distanceToTarget);
        
        // 경과 시간 정보 추가 (정규화된 값으로)
        float normalizedTime = Mathf.Clamp01((Time.time - episodeStartTime) / maxEpisodeTime);
        sensor.AddObservation(normalizedTime);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 스텝 카운트 증가
        stepsSinceStart++;
        
        // 연속 액션 값 가져오기 (x, z축 이동)
        float moveX = actionBuffers.ContinuousActions[0];
        float moveZ = actionBuffers.ContinuousActions[1];
        
        // 이동 벡터 계산
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;
        
        // 속도 설정: Rigidbody의 linearVelocity 직접 설정
        rb.linearVelocity = movement;
        
        // y축은 항상 34로 고정
        Vector3 fixedYPos = transform.position;
        fixedYPos.y = 34f;
        transform.position = fixedYPos;
        
        // 현재 위치 (이동 후)
        Vector3 currentPos = transform.position;
        
        // 이동 거리 추적
        float distanceMoved = Vector3.Distance(lastPosition, currentPos);
        totalDistanceTraveled += distanceMoved;
        lastPosition = currentPos;
        
        // 제자리에 있는지 확인 (약간의 움직임은 허용)
        if (distanceMoved < 0.01f)
        {
            AddReward(idlePenalty); // 제자리에 있으면 작은 페널티
        }
        
        // 경계 체크 - 경계값이 설정되었는지 확인
        bool outOfBounds = false;
        if (boundaryMin != Vector3.zero || boundaryMax != Vector3.zero)
        {
            if (currentPos.x < boundaryMin.x || currentPos.x > boundaryMax.x ||
                currentPos.z < boundaryMin.z || currentPos.z > boundaryMax.z)
            {
                outOfBounds = true;
                
                // 경계 내로 위치 제한
                Vector3 clampedPos = currentPos;
                clampedPos.x = Mathf.Clamp(clampedPos.x, boundaryMin.x, boundaryMax.x);
                clampedPos.z = Mathf.Clamp(clampedPos.z, boundaryMin.z, boundaryMax.z);
                transform.position = clampedPos;
            }
        }
        
        // 경계 벗어났을 때 페널티 적용
        if (outOfBounds)
        {
            AddReward(boundaryPenalty);
        }

        // 거리 기반 보상 - 카메라에 가까워졌을 때 작은 보상 제공
        if (currentTargetCamera != null)
        {
            float currentDistance = Vector3.Distance(transform.position, currentTargetCamera.transform.position);
            float distanceReward = (previousDistance - currentDistance) * 0.01f;

            // 가까워졌을 때 보상, 멀어졌을 때 작은 페널티
            if (distanceReward > 0)
            {
                AddReward(distanceReward);
            }
            else if (distanceReward < 0 && distanceToTarget > 10f) // 10미터 이상 떨어져 있을 때만 페널티
            {
                // 작은 페널티만 적용 (원래 값의 1/5)
                AddReward(distanceReward * 0.2f);
            }

            // 진행도 기반 추가 보상 - 초기 거리 대비 현재 거리의 비율에 따른 보상
            if (initialDistance > 0)
            {
                float progressRatio = 1.0f - (currentDistance / initialDistance); // 0~1 사이 값, 1에 가까울수록 목표에 가까움
                float progressReward = progressRewardScale * progressRatio * progressRatio; // 제곱하여 비선형 보상 증가
                AddReward(progressReward);
            }

            previousDistance = currentDistance;
        }

        // 추가 보상: 속도 벡터와 목표 방향 사이의 일치도 계산
        if (currentTargetCamera != null)
        {
            Vector3 normalizedVelocity = rb.linearVelocity.normalized;
            Vector3 directionToCamera = (currentTargetCamera.transform.position - transform.position).normalized;
            directionToCamera.y = 0; // Y축 무시
            
            float alignmentReward = Vector3.Dot(normalizedVelocity, directionToCamera) * 0.003f;
            if (alignmentReward > 0)
            {
                AddReward(alignmentReward);
            }
        }
    }

    #endregion

    #region 유틸리티 메서드

    // 경계값 설정을 위한 메서드 추가
    public void SetBoundaries(Vector3 min, Vector3 max)
    {
        boundaryMin = min;
        boundaryMax = max;
    }

    #endregion
}
