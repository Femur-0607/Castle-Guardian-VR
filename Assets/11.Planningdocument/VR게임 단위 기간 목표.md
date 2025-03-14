# 1. 미리 준비해야 할 리소스

## 1.1. 3D 모델 & 애니메이션

1. **캐릭터/적 모델**
    - 일반 병사, 궁수형 적, 정찰병, 탱커(오우거), 유령형 적 등
    - 중세 판타지 컨셉(로우폴리, 토니/신티 등)
    - 애니메이션(이동, 공격, 피격, 사망) 포함된 패키지면 좋음
2. **보스 모델**
    - 최종 보스 (거대 몬스터, 오우거 타입 등)
    - “부위 파괴”를 시각적으로 표현할 수 있는 파츠 구조라면 베스트
3. **NPC 동료(궁수) 모델**
    - 성벽 위 동료가 활쏘기 모션을 취하기 때문에, 활쏘기 애니메이션 포함 추천
4. **방어 타워/트랩 모델**
    - ArcherTower(활), CannonTower(대포/투석기), Trap(지뢰나 스파이크) 등
    - 애니메이션(발사, 폭발 등)을 별도 파티클 또는 이펙트로 표현

## 1.2. 맵/환경 에셋

1. **지형 및 성벽**
    - 중세 성문, 성벽 모델, 적 스폰 지점(깃발 or 포탈)
    - 언덕/바위 같은 지형물(LOD 지원)
2. **배경 오브젝트**
    - 판타지 풍 건물, 나무, 풀 등
    - 로우폴리 또는 카툰풍이면 성능 최적화에 유리

## 1.3. 사운드 리소스

1. **배경음악(BGM)**
    - 중세 판타지 느낌의 긴장감 있는 BGM (전투 시/대기 시)
    - 웨이브 시작/보스 등장/게임 오버에 맞춰 바뀔 수 있는 짧은 테마곡
2. **효과음(SFX)**
    - 활 장전/발사, 화살 충돌, 폭발(대포, 폭발 화살), 탱커의 공격, 유령 등장 사운드 등
    - 성문 피격 소리, UI 클릭/구매 소리
    - 승리/패배 효과음
3. **보이스(Voice) or 음성 안내**
    - 다이얼로그 시스템용 NPC 대사(신들의 목소리 등)
    - 보스 웨이브 경고 멘트
    - 필요 시 TTS(Text To Speech) 또는 Voice Actor 녹음

## 1.4. UI / 2D 아트 & 폰트

4. **UI 프레임/아이콘**
    - 상점 UI, 버튼 아이콘(화살 종류, 방어 타워, 트랩, NPC 동료 등)
    - HP 바, Wave/Score 표시, 골드 표시
    - 판타지 테마 GUI (돌맹이 UI, 스크롤 UI 등)
5. **폰트**
    - 판타지 분위기의 타이틀 폰트(저작권 확인)
    - 한글 표시 시 가독성 좋은 서체

## 1.5. 텍스트 / 스토리 자료

6. **NPC 대사 스크립트**
    - 튜토리얼 멘트, 웨이브/보스 도입 멘트
    - 중간 보스(5웨이브) 암시, 최종 보스(10웨이브) 대사
7. **UI 메시지/안내 문구**
    - 상점 설명, 웨이브 클리어 안내, 게임 오버, 옵션 메뉴 등
    - Localization(다국어 지원) 계획이 있으면 csv 등으로 정리

## 1.6. 파티클/이펙트

8. **폭발/파편 이펙트** (CannonTower, 폭발 화살)
9. **슬로우/스턴 이펙트** (Trap, 둔화 화살)
10. **보스 등장 연출** (포탈 소환, 번개, 검은 안개 등)
11. **웨이브 클리어** (축포, 반짝이는 효과 등)

## 1.7. 개발 지원 & 편의성 에셋

12. **Oculus Integration or Meta XR SDK**
    - VR 컨트롤러 인풋, HMD 추적, Spatializer 등
13. **DOTween**
    - UI 애니메이션, 활 궤적 가이드 등 트윈 작업 편리
14. **XR Interaction Toolkit**
    - 레이 인터렉션, 텔레포트 이동, 그립·트리거 이벤트 처리
15. **ML-Agents**
    - 유령형 적(네비메시 무시) 등 특수 AI 학습
16. **Photon/Netcode**(선택)
    - 멀티플레이 고려 시

---

# 2. 8주 단위 기간 목표 (예시)

아래는 대표적인 로드맵 예시입니다. 실제 스케줄은 팀 규모·개발 숙련도에 따라 조정하세요.

## 📌 1주차: **프로젝트 셋업 & 기본 구조**

17. **프로젝트 설정**
    - Unity + Meta Quest SDK / XR Interaction Toolkit 버전 결정
    - Git 저장소 연결, 협업 환경 세팅
18. **기본 씬 구성**
    - 성벽, 성문, 적 스폰 지점 배치 (간단 placeholder)
19. **테스트용 리소스 임시 적용**
    - 간단한 3D 큐브나 기초 모델로 적・플레이어 배치
20. **VR 환경 확인**
    - Oculus/Meta XR 빌드 테스트 (PC VR 시뮬, 또는 Quest 직접 연결)

## 📌 2주차: **적 AI & 웨이브 시스템 구축**

21. **NavMesh 설정**
    - 적 이동 경로 생성, 성문 위치까지 이동하도록 구현
22. **기본 적(일반 병사) AI**
    - FSM(이동 → 공격)
23. **WaveManager**
    - 웨이브 생성, 난이도/적 수 증가 로직
24. **성문 체력 시스템**
    - 적이 성문 도달 시 체력 감소, 0이면 게임 오버

## 📌 3주차: **전투(활·화살) 시스템 & 특수 화살**

25. **기본 활 발사** (PC & VR)
    - 마우스 드래그 / 컨트롤러 트리거로 장력 표현
26. **특수 화살 구현**
    - 폭발, 둔화, 불 화살 등
27. **포물선 궤적(LineRenderer)**
    - 조준 보조(화살 도착점 시각화)
28. **적 피격 처리**
    - OnTriggerEnter / Raycast로 화살 충돌, 적 체력 감소

## 📌 4주차: **방어 타워 & 상점 시스템**

29. **타워 배치**
    - `Instantiate()` or Object Pool로 타워 소환
    - ArcherTower(단일 타겟), CannonTower(광역), Trap(슬로우)
30. **상점 UI**
    - 웨이브 간 상호작용, 재화(골드)로 구매
31. **타워 업그레이드**
    - 데미지/사거리/쿨다운/광역 범위 등

## 📌 5주차: **추가 적 유형 & 난이도 조정**

32. **탱커형 적, 궁수형 적, 정찰병**
    - 각각의 체력/공격/이동속도 차별화
33. **중간 보스(5웨이브) 등장**
    - 높은 체력, 강한 공격, 보스 연출 추가
34. **난이도 곡선 조정**
    - 각 웨이브마다 적 조합·수량·스폰간격 튜닝

## 📌 6주차: **최적화 & VR UX 개선**

35. **오브젝트 풀링** (화살, 적, 타워 등)
    - GC 최소화
36. **LOD·LightProbe 적용**
    - 성능 최적화, VR 프레임 유지
37. **VR UX**
    - 컨트롤러 진동, 시야 흔들림, 핸드 트래킹 실험
38. **오디오 매니저**
    - BGM, SFX, Spatial Audio 세팅

## 📌 7주차: **최종 기능 & 버그 수정**

39. **최종 보스(10웨이브) 구현**
    - 특수 연출(부위 파괴, 쫄몹 소환 등)
40. **AI 음성 인식(선택)**
    - 예: “화살 보충!” 명령어 → 재고 충전
41. **UI·사운드 폴리싱**
    - 상점 아이콘, 이펙트, 보이스 대사 추가
42. **버그 리스트**
    - 충돌/물리 문제, UI 에러, VR 빌드 문제 수정

## 📌 8주차: **QA·테스트 & 최종 빌드**

43. **사내/지인 대상 플레이테스트**
    - 밸런스 점검(난이도, 화살/타워 성능)
    - VR 모션 어지럼증 테스트
44. **폴리싱 & 최종 연출**
    - 엔딩 씬, 보스撃파 애니메이션, 승리/패배 효과
45. **최종 빌드**
    - Meta Quest 3 전용 빌드 (APK)
    - PC(스탠드얼론 VR) 빌드
46. **포트폴리오 자료 정리**
    - 시연 영상, 문서화, 스크린샷

---

## 관련자료
- [[VR 디펜스 게임 기획서 - Castle Guardian VR]]
- [[기획서 참고자료]]