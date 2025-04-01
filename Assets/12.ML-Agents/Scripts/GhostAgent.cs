using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GhostAgent : Agent
{
    [Header("기본 설정")]
    public float forceMultiplier = 10f;
    public Vector3 boundaryMin;
    public Vector3 boundaryMax;
    [SerializeField] public GameObject ovrCameraRig; // OVR 카메라 참조
    
    private Rigidbody rb;
    private Transform currentTargetCamera; // 현재 활성화된 카메라

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        
        // OVR 카메라 찾기
        if (ovrCameraRig == null)
        {
            ovrCameraRig = GameObject.FindWithTag("MainCamera");
        }
        
        // 초기 카메라 설정
        UpdateTargetCamera();
    }
    
    // 현재 타겟 카메라 업데이트
    private void UpdateTargetCamera()
    {
        if (ovrCameraRig != null)
        {
            Transform centerEye = ovrCameraRig.transform.Find("CenterEyeAnchor");
            if (centerEye != null)
            {
                currentTargetCamera = centerEye;
            }
            else
            {
                currentTargetCamera = ovrCameraRig.transform;
            }
        }
    }

    private bool IsOutOfBounds()
    {
        return transform.position.x < boundaryMin.x || transform.position.x > boundaryMax.x ||
               transform.position.z < boundaryMin.z || transform.position.z > boundaryMax.z;
    }

    public void SetBoundaries(Vector3 min, Vector3 max)
    {
        boundaryMin = min;
        boundaryMax = max;
    }
    
    // Y값 강제 고정 (LateUpdate에서 처리)
    private void LateUpdate()
    {
        // 항상 Y=35로 고정
        if (transform.position.y != 35f)
        {
            transform.position = new Vector3(
                transform.position.x,
                35f,
                transform.position.z
            );
        }
    }

    public override void OnEpisodeBegin()
    {
        // 물리 속도 초기화
        this.transform.localRotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 현재 활성화된 카메라 업데이트
        UpdateTargetCamera();

        // 위치를 현재 위치 근처로 약간 이동
        Vector3 newPosition = transform.position + 
            new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        
        // 경계 내에 있는지 확인하고 조정
        newPosition.x = Mathf.Clamp(newPosition.x, boundaryMin.x, boundaryMax.x);
        newPosition.z = Mathf.Clamp(newPosition.z, boundaryMin.z, boundaryMax.z);
        newPosition.y = 35f;  // Y축은 항상 35로 고정
        
        transform.position = newPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (currentTargetCamera == null)
        {
            UpdateTargetCamera();
            if (currentTargetCamera == null) return;
        }

        // 타겟(카메라)의 위치
        sensor.AddObservation(currentTargetCamera.transform.localPosition);

        // 에이전트의 위치
        sensor.AddObservation(transform.localPosition);

        // 에이전트의 속도
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 연속 액션 값 가져오기 (x, z축 이동)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];

        rb.AddForce(controlSignal * forceMultiplier);
        
        // 이동 방향으로 회전
        if (controlSignal != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(controlSignal, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        if (currentTargetCamera != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTargetCamera.position);

            if(distanceToTarget < 5f)
            {
                // 카메라 근처에 도달했을 때
                Debug.Log("유령이 카메라 근처에 도달했습니다!");
                AddReward(1.0f);
                EndEpisode();
            }
        }
        
        if(IsOutOfBounds())
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }

    // 사용자 입력 처리: 학습이 아닌 경우, 사용자 입력으로 에이전트를 조작합니다.
    // 에이전트 제어 방식:
    // 키보드 입력을 통해 연속적인 행동 값으로 변환하여 에이전트를 이동시킵니다.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionOut = actionsOut.ContinuousActions;
        continuousActionOut[0] = Input.GetAxis("Horizontal");
        continuousActionOut[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CameraTrigger"))
        {
            AddReward(0.1f);
            Debug.Log("유령이 카메라 트리거에 진입했습니다");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentTargetCamera != null)
        {
            // 에이전트와 카메라 사이의 선
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTargetCamera.position);

            // 목표 거리 범위 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTargetCamera.position, 5f);
        }
    }
}
