﻿using System.Collections.Generic;
using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class ProjectileCurveVisualizer : MonoBehaviour
    {
        // 라인렌더러 컴포넌트를 통해 궤적을 시각화합니다.
        private LineRenderer lineRenderer;

        // 착탄(투사체가 맞을) 지점을 나타내기 위한 Transform (예: 평면)
        public Transform projectileTargetPlaneTransform;
        // projectileTargetPlaneTransform의 MeshRenderer로 착탄 지점 표시 여부를 제어합니다.
        private MeshRenderer projectileTargetPlaneMeshRenderer;

        // 무시할 레이어 (예: 플레이어, 특정 트리거 등)
        public LayerMask ignoredLayers;

        // 라인렌더러가 표시할 곡선의 점 개수 (더 많으면 부드럽게 표현됨)
        public int curveSubdivision = 32;
        // 투사체의 최대 비행 시간 (이 시간 이후에는 궤적 계산 중단)
        public float maximumInAirTime = 6.0f;
        // 투사체 경로를 샘플링하는 간격 (시간 간격)
        public float detectionInterval = 0.5f;

        // 착탄 지점의 오브젝트 Transform을 가져올지 여부
        public bool getHitObjectTransform = false;
        // 착탄 시 속도를 계산할지 여부
        public bool calculateProjectileVelocityWhenHit = false;

        // 포물선 궤적 계산에 사용되는 변수들
        private Vector3 horizontalVector;
        private float horizontalDisplacement;
        private float horizontalTime;

        // 투사체 궤적 탐지를 위한 변수들
        private Vector3 previousDetectionPosition = Vector3.zero;
        private Vector3 nextDetectionPosition = Vector3.zero;
        private float t; // 시간 변수로, 경로를 샘플링할 때 사용
        // 샘플링된 경로 점들을 저장하는 리스트
        public List<Vector3> detectionPositionList = new List<Vector3>();
        // OverlapSphere 검사용 Collider 배열 (크기가 1로 설정)
        private Collider[] hitColliderArray = new Collider[1];
        // 투사체가 아무것도 맞추지 않았는지 여부
        private bool notHit = true;
        // 기본 RaycastHit 값
        private RaycastHit defaultRaycastHit;
        // 레이캐스트 방향 및 길이
        private Vector3 rayDirection;
        private float rayLength;
        // 실제 충돌한 위치 및 표면의 법선 벡터
        public Vector3 hitPosition;
        private Vector3 hitNormal;

        // 목표 예측을 위한 변수들 (타겟 움직임 반영)
        private float predictedProjectileTravelTime;
        private Vector3 horizontalDirection;
        private float horizontalDistance;
        private float deltaHeight;
        private float launchSpeedSquare;
        private float component;
        private float launchAngle;

        // Bézier 곡선 계산에 필요한 변수들
        private Vector3 startPoint = Vector3.zero;
        private Vector3 endPoint = Vector3.zero;
        private Vector3 controlPoint = Vector3.zero;

        // 충돌한 오브젝트의 Transform (선택적)
        public Transform hitObjectTransform;
        // 착탄 시 계산된 투사체의 속도 (선택적)
        public Vector3 projectileVelocityWhenHit;

        void Awake()
        {
            // 라인렌더러 컴포넌트를 가져오고, 곡선 점의 개수를 설정합니다.
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = curveSubdivision;

            // projectileTargetPlaneTransform에 붙은 MeshRenderer를 가져옵니다.
            projectileTargetPlaneMeshRenderer = projectileTargetPlaneTransform.GetComponent<MeshRenderer>();

            // 기본 RaycastHit를 초기화합니다.
            defaultRaycastHit = new RaycastHit();
        }

        // 에디터에서 선택 시, 충돌 지점을 시각적으로 확인할 수 있게 합니다.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hitPosition, 0.25f);
        }

        // 기본 투사체 궤적 시각화 메서드
        // projectileStartPosition: 투사체 시작 위치
        // projectileStartPositionForwardOffset: 시작 위치에서 전방 오프셋 (조준 시 위치 보정)
        // launchVelocity: 발사 속도 및 방향
        // projectileRadius: 투사체의 반지름 (충돌 검사용)
        // distanceOffsetAboveHitPosition: 착탄 지점 위에 오프셋 (시각화용)
        // debugMode: 디버그 시각화 활성화 여부
        // updatedProjectileStartPosition: 계산 후 업데이트된 시작 위치 (출력)
        // hit: 충돌 정보 (출력)
        public void VisualizeProjectileCurve(Vector3 projectileStartPosition, float projectileStartPositionForwardOffset, Vector3 launchVelocity, float projectileRadius, float distanceOffsetAboveHitPosition, bool debugMode, out Vector3 updatedProjectileStartPosition, out RaycastHit hit)
        {
            // 기본 RaycastHit를 할당
            hit = defaultRaycastHit;
            // 충돌한 오브젝트 Transform 초기화
            hitObjectTransform = null;

            // 라인렌더러가 비활성화 상태면 활성화 시킴
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;

            // 시간 초기화
            t = 0.0f;

            // 시작 위치 업데이트: 투사체 시작 위치에 전방 오프셋 적용
            updatedProjectileStartPosition = projectileStartPosition + new Vector3(launchVelocity.x, 0.0f, launchVelocity.z).normalized * projectileStartPositionForwardOffset;
            // 이전 검출 위치는 업데이트된 시작 위치로 설정
            previousDetectionPosition = updatedProjectileStartPosition;
            // 경로 점 리스트 초기화
            detectionPositionList = new List<Vector3>();
            // 아직 충돌하지 않았다고 가정
            notHit = true;

            // 최대 비행 시간 동안 궤적을 샘플링
            while (t < maximumInAirTime)
            {
                // 포물선 공식: 초기위치 + (속도 * 시간) - (중력가속도 * 시간^2)
                // 여기서 4.9는 중력가속도의 절반(9.8/2)
                // 포물선 공식에 따라 다음 점 계산 (중력 고려)
                nextDetectionPosition = updatedProjectileStartPosition + new Vector3(launchVelocity.x * t, launchVelocity.y * t - 4.9f * t * t, launchVelocity.z * t);
                // 시간 간격만큼 증가
                t += detectionInterval;
                // 샘플링된 위치를 리스트에 추가
                detectionPositionList.Add(nextDetectionPosition);

                // 이전 위치와 다음 위치 사이의 방향과 거리를 계산 (Raycast용)
                rayDirection = (nextDetectionPosition - previousDetectionPosition).normalized;
                rayLength = Vector3.Distance(previousDetectionPosition, nextDetectionPosition);

                if (debugMode)
                    // 디버그 모드에서 경로 선을 초록색으로 그림
                    Debug.DrawLine(previousDetectionPosition, previousDetectionPosition + rayDirection * rayLength, Color.green);

                // 이전 위치에서 다음 위치로 Raycast를 날려 장애물이 있는지 확인
                if (Physics.Raycast(previousDetectionPosition, rayDirection, out hit, rayLength, ~ignoredLayers, QueryTriggerInteraction.Ignore))
                {
                    // 장애물을 발견하면 충돌 처리
                    notHit = false;
                    hitPosition = hit.point;
                    hitNormal = hit.normal;

                    // 옵션에 따라 충돌한 오브젝트의 Transform을 저장
                    if (getHitObjectTransform)
                        hitObjectTransform = hit.transform;

                    // 옵션에 따라 충돌 시 투사체 속도를 계산
                    if (calculateProjectileVelocityWhenHit)
                    {
                        horizontalVector = new Vector3(launchVelocity.x, 0.0f, launchVelocity.z);
                        horizontalDisplacement = Vector3.Distance(new Vector3(previousDetectionPosition.x, 0.0f, previousDetectionPosition.z), new Vector3(hitPosition.x, 0.0f, hitPosition.z));
                        horizontalTime = horizontalDisplacement / horizontalVector.magnitude;
                        projectileVelocityWhenHit = (hitPosition - previousDetectionPosition) / horizontalTime;
                    }

                    if (debugMode)
                        // 충돌 지점과 법선 벡터를 빨간색 선으로 표시
                        Debug.DrawLine(hitPosition, hitPosition + hitNormal, Color.red);

                    // 충돌이 발생하면 루프 종료
                    break;
                }
                else
                {
                    // Raycast로 충돌하지 않으면, OverlapSphere를 사용하여 주변 충돌체가 있는지 검사
                    // Raycast는 충돌했을때 반환되는데 만약에 안된다면 주변을 검사해서 충돌체의 여부를 판별하는건가?
                    if (Physics.OverlapSphereNonAlloc(nextDetectionPosition, projectileRadius, hitColliderArray, ~ignoredLayers, QueryTriggerInteraction.Ignore) > 0)
                    {
                        notHit = false;
                        // 가장 가까운 충돌 지점을 구함
                        hitPosition = hitColliderArray[0].ClosestPoint(nextDetectionPosition);
                        // 충돌면의 법선을 구함 (투사체가 해당 지점으로부터 멀어지는 방향)
                        hitNormal = Vector3.Normalize(nextDetectionPosition - hitPosition);
                        break;
                    }
                }
                // 이전 위치를 현재 위치로 업데이트
                previousDetectionPosition = nextDetectionPosition;
            }

            // 시작점 설정
            startPoint = updatedProjectileStartPosition;

            // 충돌이 없었으면, 마지막 샘플링된 점을 끝점으로 사용
            if (notHit)
            {
                endPoint = detectionPositionList[detectionPositionList.Count - 1];
                // 착탄 지점 표시용 오브젝트를 비활성화
                if (projectileTargetPlaneMeshRenderer.enabled)
                    projectileTargetPlaneMeshRenderer.enabled = false;
            }
            else
            {
                // 충돌이 발생했으면, 충돌 지점을 끝점으로 사용
                endPoint = hitPosition;
                // 착탄 지점 표시용 오브젝트의 위치 및 회전을 설정
                projectileTargetPlaneTransform.position = hitPosition;
                projectileTargetPlaneTransform.LookAt(projectileTargetPlaneTransform.position + hitNormal);
                projectileTargetPlaneTransform.rotation = Quaternion.Euler(projectileTargetPlaneTransform.rotation.eulerAngles.x + 90.0f, projectileTargetPlaneTransform.rotation.eulerAngles.y, projectileTargetPlaneTransform.rotation.eulerAngles.z);
                // 약간의 오프셋 적용하여 표시
                projectileTargetPlaneTransform.position += hitNormal * distanceOffsetAboveHitPosition;
                // 착탄 지점 표시를 활성화
                if (!projectileTargetPlaneMeshRenderer.enabled)
                    projectileTargetPlaneMeshRenderer.enabled = true;
            }

            // 중간점(컨트롤 포인트) 계산
            if (detectionPositionList.Count > 2)
                controlPoint = 2.0f * detectionPositionList[detectionPositionList.Count / 2] - startPoint * 0.5f - endPoint * 0.5f;
            else if (detectionPositionList.Count == 2)
                controlPoint = (startPoint + endPoint) / 2.0f;

            // Bézier 곡선을 이용해 부드러운 궤적을 렌더링합니다.
            // 시작점과 끝점을 라인렌더러에 설정
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPoint);

            // t는 0부터 1까지 변화하는 Bézier 곡선의 파라미터 (시간이 아니라 곡선상의 위치)
            t = 0.0f;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                // 2차 Bézier 공식: (1-t)^2 * 시작점 + 2*(1-t)*t * 컨트롤 포인트 + t^2 * 끝점
                lineRenderer.SetPosition(i, (1 - t) * (1 - t) * startPoint + 2 * (1 - t) * t * controlPoint + t * t * endPoint);
                t += (1 / (float)lineRenderer.positionCount);
            }
        }

        // 목표 위치를 고려하여 궤적을 시각화하는 메서드
        // (타겟의 속도를 반영하여 예측 위치와 발사 속도를 계산)
        public bool VisualizeProjectileCurveWithTargetPosition(Vector3 projectileStartPosition, float projectileStartPositionForwardOffset, Vector3 projectileEndPosition, float launchSpeed, Vector3 throwerVelocity, Vector3 targetObjectVelocity, float projectileRadius, float distanceOffsetAboveHitPosition, bool debugMode, out Vector3 updatedProjectileStartPosition, out Vector3 projectileLaunchVelocity, out Vector3 predictedTargetPosition, out RaycastHit hit)
        {
            hit = defaultRaycastHit;

            // 타겟의 속도를 이용하여 예측되는 목표 위치를 계산
            predictedProjectileTravelTime = (Vector3.Distance(projectileStartPosition, projectileEndPosition) - projectileStartPositionForwardOffset) / launchSpeed;
            predictedTargetPosition = projectileEndPosition + (targetObjectVelocity - throwerVelocity) * predictedProjectileTravelTime;

            // 투사체의 수평 방향 계산 (y 성분 제외)
            horizontalDirection = predictedTargetPosition - projectileStartPosition;
            horizontalDirection.y = 0.0f;
            horizontalDirection = horizontalDirection.normalized;

            // 업데이트된 시작 위치 계산 (전방 오프셋 적용)
            updatedProjectileStartPosition = projectileStartPosition + horizontalDirection * projectileStartPositionForwardOffset;
            projectileLaunchVelocity = Vector3.zero;

            // 수평 거리와 높이 차이를 계산하여 발사 각도 산출에 사용
            horizontalDistance = Vector3.Distance(new Vector3(updatedProjectileStartPosition.x, 0.0f, updatedProjectileStartPosition.z), new Vector3(predictedTargetPosition.x, 0.0f, predictedTargetPosition.z));
            deltaHeight = predictedTargetPosition.y - updatedProjectileStartPosition.y;

            // 발사 속도의 제곱 값
            launchSpeedSquare = launchSpeed * launchSpeed;
            // 발사 각도를 계산하기 위한 중간 변수 (포물선 공식에 사용)
            component = launchSpeedSquare * launchSpeedSquare - 9.8f * (9.8f * horizontalDistance * horizontalDistance + 2.0f * deltaHeight * launchSpeedSquare);

            // 만약 음수가 나오면 (목표에 도달할 수 없는 경우) false 반환
            if (component < 0.0f)
                return false;

            component = Mathf.Sqrt(component);

            // 낮은 각도로 계산 (더 현실적인 궤적)
            launchAngle = Mathf.Atan2(launchSpeedSquare - component, 9.8f * horizontalDistance);

            // 계산된 발사 각도를 기반으로 투사체의 발사 속도 벡터를 구함
            projectileLaunchVelocity = horizontalDirection * Mathf.Cos(launchAngle) * launchSpeed + Vector3.up * Mathf.Sin(launchAngle) * launchSpeed;

            // 계산된 발사 속도를 바탕으로 궤적 시각화를 진행
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;

            t = 0.0f;
            previousDetectionPosition = updatedProjectileStartPosition;
            detectionPositionList = new List<Vector3>();

            while (t < maximumInAirTime)
            {
                // 발사 속도와 중력을 반영한 포물선 공식으로 현재 위치 계산
                nextDetectionPosition = updatedProjectileStartPosition + new Vector3(projectileLaunchVelocity.x * t, projectileLaunchVelocity.y * t - 4.9f * t * t, projectileLaunchVelocity.z * t);
                t += detectionInterval;
                detectionPositionList.Add(nextDetectionPosition);

                rayDirection = (nextDetectionPosition - previousDetectionPosition).normalized;
                rayLength = Vector3.Distance(previousDetectionPosition, nextDetectionPosition);

                if (debugMode)
                    Debug.DrawLine(previousDetectionPosition, previousDetectionPosition + rayDirection * rayLength, Color.green);

                if (Physics.Raycast(previousDetectionPosition, rayDirection, out hit, rayLength, ~ignoredLayers, QueryTriggerInteraction.Ignore))
                {
                    notHit = false;
                    hitPosition = hit.point;
                    hitNormal = hit.normal;

                    if (debugMode)
                        Debug.DrawLine(hitPosition, hitPosition + hitNormal, Color.red);

                    break;
                }
                else
                {
                    if (Physics.OverlapSphereNonAlloc(nextDetectionPosition, projectileRadius, hitColliderArray, ~ignoredLayers, QueryTriggerInteraction.Ignore) > 0)
                    {
                        notHit = false;
                        hitPosition = hitColliderArray[0].ClosestPoint(nextDetectionPosition);
                        hitNormal = Vector3.Normalize(nextDetectionPosition - hitPosition);
                        break;
                    }
                }
                previousDetectionPosition = nextDetectionPosition;
            }

            // 시작점과 끝점을 각각 업데이트된 시작 위치와 충돌 위치(또는 마지막 샘플링 위치)로 설정
            startPoint = updatedProjectileStartPosition;
            endPoint = hitPosition;

            // 착탄 지점 표시용 오브젝트의 위치 및 회전을 갱신
            projectileTargetPlaneTransform.position = hitPosition;
            projectileTargetPlaneTransform.LookAt(projectileTargetPlaneTransform.position + hitNormal);
            projectileTargetPlaneTransform.rotation = Quaternion.Euler(projectileTargetPlaneTransform.rotation.eulerAngles.x + 90.0f, projectileTargetPlaneTransform.rotation.eulerAngles.y, projectileTargetPlaneTransform.rotation.eulerAngles.z);
            projectileTargetPlaneTransform.position += hitNormal * distanceOffsetAboveHitPosition;

            if (!projectileTargetPlaneMeshRenderer.enabled)
                projectileTargetPlaneMeshRenderer.enabled = true;

            if (detectionPositionList.Count > 2)
                controlPoint = 2.0f * detectionPositionList[detectionPositionList.Count / 2] - startPoint * 0.5f - endPoint * 0.5f;
            else if (detectionPositionList.Count == 2)
                controlPoint = (startPoint + endPoint) / 2.0f;

            // Bézier 곡선을 사용하여 부드러운 궤적을 렌더링
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, endPoint);

            t = 0.0f;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, (1 - t) * (1 - t) * startPoint + 2 * (1 - t) * t * controlPoint + t * t * endPoint);
                t += (1 / (float)lineRenderer.positionCount);
            }

            return true;
        }

        // 라인렌더러와 착탄 지점 표시용 MeshRenderer를 비활성화합니다.
        public void HideProjectileCurve()
        {
            if (lineRenderer.enabled)
            {
                lineRenderer.enabled = false;
                projectileTargetPlaneMeshRenderer.enabled = false;
            }
        }
    }
}
