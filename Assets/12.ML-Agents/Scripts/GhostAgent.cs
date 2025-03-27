// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;
// using Unity.MLAgents.Actuators;

// public class GhostAgent : Agent
// {
//     [Header("기본 설정")]
//     public float forceMultiplier = 10f;
//     public Vector3 boundaryMin;
//     public Vector3 boundaryMax;

//     private Rigidbody rb;
//     private Camera currentTargetCamera; // 현재 활성화된 카메라
//     private GhostTrainingManager trainingManager;
//     private SimpleCameraController cameraController;

//     public override void Initialize()
//     {
//         rb = GetComponent<Rigidbody>();
//         trainingManager = FindFirstObjectByType<GhostTrainingManager>();
//         cameraController = FindFirstObjectByType<SimpleCameraController>();

//         // 초기 카메라 설정
//         if (cameraController != null)
//         {
//             currentTargetCamera = cameraController.GetCurrentCamera();
//         }
//     }

//         private bool IsOutOfBounds()
//     {
//         return transform.position.x < boundaryMin.x || transform.position.x > boundaryMax.x ||
//                transform.position.z < boundaryMin.z || transform.position.z > boundaryMax.z;
//     }

//         public void SetBoundaries(Vector3 min, Vector3 max)
//     {
//         boundaryMin = min;
//         boundaryMax = max;
//     }

//     public override void OnEpisodeBegin()
//     {
//         // 물리 속도 초기화
//         this.transform.localRotation = Quaternion.identity;
//         rb.linearVelocity = Vector3.zero;
//         rb.angularVelocity = Vector3.zero;

//         // 현재 활성화된 카메라 업데이트
//         currentTargetCamera = cameraController.GetCurrentCamera();

//         // 에이전트 재 스폰
//         Vector3 newPosition = trainingManager.GetRandomSpawnPosition();
//         transform.position = newPosition;
//     }

//     public override void CollectObservations(VectorSensor sensor)
//     {
//         if (currentTargetCamera == null) return;

//         // 타겟(카메라)의 위치
//         sensor.AddObservation(currentTargetCamera.transform.localPosition);

//         // 에이전트의 위치
//         sensor.AddObservation(transform.localPosition);

//         // 에이전트의 속도
//         sensor.AddObservation(rb.linearVelocity.x);
//         sensor.AddObservation(rb.linearVelocity.z);
//     }

//     public override void OnActionReceived(ActionBuffers actionBuffers)
//     {
//         // 연속 액션 값 가져오기 (x, z축 이동)
//         Vector3 controlSignal = Vector3.zero;
//         controlSignal.x = actionBuffers.ContinuousActions[0];
//         controlSignal.z = actionBuffers.ContinuousActions[1];

//         rb.AddForce(controlSignal * forceMultiplier);
        
//         // 이동 방향으로 회전
//         if (controlSignal != Vector3.zero)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(controlSignal, Vector3.up);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//         }

//         float distanceToTarget = Vector3.Distance(this.transform.position, currentTargetCamera.transform.position);

//         if(distanceToTarget < 5f)
//         {
//             Debug.Log("성공");
//             SetReward(1.0f);
//             trainingManager.OnGhostTriggeredCamera();
//         }
//         else if(IsOutOfBounds())
//         {
//             EndEpisode();
//         }
//     }

//     // 사용자 입력 처리: 학습이 아닌 경우, 사용자 입력으로 에이전트를 조작합니다.
//     // 에이전트 제어 방식:
//     // 키보드 입력을 통해 연속적인 행동 값으로 변환하여 에이전트를 이동시킵니다.
//     public override void Heuristic(in ActionBuffers actionsOut)
//     {
//         var continuousActionOut = actionsOut.ContinuousActions;
//         continuousActionOut[0] = Input.GetAxis("Horizontal");
//         continuousActionOut[1] = Input.GetAxis("Vertical");
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("CameraTrigger"))
//         {
//             // 학습 이후 구현
//             SetReward(0.01f);
//             Debug.Log("카메라 근처는옴");
//         }
//     }

//     private void OnDrawGizmos()
//     {
//         if (currentTargetCamera != null)
//         {
//             // 에이전트와 카메라 사이의 선
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawLine(transform.position, currentTargetCamera.transform.position);

//             // 거리 표시를 위한 텍스트
//             float distance = Vector3.Distance(transform.position, currentTargetCamera.transform.position);
//             Vector3 textPosition = Vector3.Lerp(transform.position, currentTargetCamera.transform.position, 0.5f);
//             #if UNITY_EDITOR
//                 UnityEditor.Handles.Label(textPosition, distance.ToString("F2") + "m");
//             #endif

//             // 목표 거리 범위 표시 (5f)
//             Gizmos.color = Color.green;
//             Gizmos.DrawWireSphere(currentTargetCamera.transform.position, 5f);
//         }
//     }
// }
