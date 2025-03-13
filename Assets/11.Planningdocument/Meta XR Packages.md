## Meta에서 제공하는 Unity용 XR 패키지

[링크](https://developers.meta.com/horizon/documentation/unity/unity-package-manager/)

Meta에서는 Meta Quest 기기용 가상현실(VR) 및 혼합현실(MR) 애플리케이션 개발을 위한 Unity 패키지를 제공합니다. 이 패키지들은 몰입감 있는 사용자 경험, 소셜 연결, 그리고 하드웨어 디스플레이 최적화를 돕는 다양한 기능을 포함하고 있습니다.

### Meta XR SDKs

각각의 Meta XR SDK는 VR이나 MR 애플리케이션에 필요한 특정 기능을 제공합니다. 예를 들어, 카메라, 컨트롤러, 손 추적 등의 기능들을 포함하고 있습니다.

### Meta XR 개발 도구

Meta XR 개발 도구는 XR 애플리케이션을 더 빠르게 개발, 빌드, 테스트할 수 있도록 도와주는 소프트웨어 패키지입니다.

### Meta XR 샘플

Meta XR 샘플은 애플리케이션 개발 시 기능과 도구의 사용법을 예제 형태로 보여줍니다.

---

## 준비 사항

Meta XR 패키지를 Unity 프로젝트에 가져오려면 다음 준비 사항을 충족해야 합니다:

- [Before You Begin] 문서에 나와 있는 하드웨어 및 소프트웨어 요구 사항을 모두 만족해야 합니다.
- XR 개발에 맞게 설정된 새로운 3D Unity 프로젝트가 필요합니다. Unity 프로젝트를 XR 개발용으로 설정하는 방법은 [Set Up Unity for XR Development] 문서를 참고하세요.

---

## 패키지 가져오기

Meta XR 패키지는 Unity의 패키지 관리 시스템인 Unity Package Manager(UPM)를 통해 관리됩니다.

### Unity Asset Store에서 다운로드 및 가져오기

가장 쉬운 방법은 Unity Asset Store에서 Meta XR 패키지를 다운로드하는 것입니다. 이 경우, 모든 패키지와 의존성이 자동으로 관리되어 XR 개발을 빠르게 시작할 수 있습니다.  
**방법:**

1. Unity Editor에서 프로젝트를 엽니다.
2. Unity Asset Store에 로그인합니다.
3. 설치하고자 하는 SDK 패키지로 이동하여 **Add to My Assets** 버튼을 클릭합니다.
4. **My Asset** 페이지에서 원하는 SDK를 선택한 후 **Open in Unity**를 클릭하면 Unity Editor의 Package Manager 창이 열립니다. (필요한 경우 Unity 로그인 정보를 입력합니다.)
5. Package Manager 창에서 최신 버전을 선택한 후, 오른쪽 상단의 **Install** 버튼을 클릭합니다.
6. OVRPlugin(메타 퀘스트의 VR 런타임과 Unity가 통신할 수 있도록 돕는 플러그인) 업데이트 요청이 뜨면, Unity Editor를 재시작합니다.

패키지 버전을 업데이트하거나 다른 버전으로 전환하려면 사용 중인 Unity Editor 버전에 맞는 Unity의 문서를 참고하세요. 단, 패키지는 **My Assets**에 추가되어 있어야 여러 버전을 살펴볼 수 있습니다.

### 고급 사용법: Meta XR 패키지 tarball 파일 가져오기

고급 사용자는 Meta의 NPM Registry에서 tarball 파일 형태로 패키지를 다운로드한 후, Unity Package Manager UI를 통해 가져올 수 있습니다. 이 경우 의존성은 직접 관리해야 합니다.

> **참고:** UPM을 통해 가져온 패키지는 기본적으로 읽기 전용입니다. 이렇게 하면 패키지 파일이 완벽한 상태로 설치되어 프로젝트에 손상을 줄 위험을 줄일 수 있습니다. 패키지 내용을 수정해야 한다면 “Make Local Changes to Packages” 문서를 참고하세요.

---

## Oculus Integration SDK에서 Meta XR SDK로 이전하기

Oculus Integration SDK는 버전 57부터 더 이상 업데이트되지 않으므로, 최신 기능과 개선 사항(예: Mixed Reality Utility Kit)을 사용하려면 Meta XR SDK로 이전하는 것이 좋습니다.

> **주의:** 이전 작업 시 기존 콘텐츠가 제거되므로, 이전 전에 현재 Unity 프로젝트를 백업하는 것을 권장합니다.

### 이전 방법

1. Unity Editor를 종료합니다.
2. 컴퓨터에서 프로젝트 폴더(예: `/username/sample-project/`)로 이동합니다.
3. **Assets** 폴더 안에서 Oculus 관련 폴더들을 삭제합니다.
4. **Library** 폴더 안의 `/Library/PackageCache/` 폴더도 삭제합니다.
5. 프로젝트를 다시 엽니다. 컴파일 오류가 발생할 경우 안전 모드로 열어야 할 수 있습니다.
6. 이후 Meta XR SDK를 UPM 패키지로 설치합니다. 처음에는 Meta XR All-in-One SDK부터 시작하는 것이 좋습니다. 이 SDK에는 이전 Oculus Integration SDK와 유사한 기능들이 포함되어 있습니다.
7. 프로젝트에 오류나 누락된 에셋이 있을 경우, 이는 일부 샘플 에셋(예: SampleFrameworks)이 GitHub의 Unity StarterSamples로 이동되었기 때문입니다. 이 경우 [여기](해당 문서 링크 참조)의 지침을 따라 에셋을 가져오세요.

### 이전 후 발생할 수 있는 일반적인 문제 해결 방법

- 만약 기존 Meta XR SDK 파일에 커스텀 수정이 있었다면, 새로운 UPM 배포 파일과 수동으로 병합해야 합니다. 자세한 내용은 “Import Meta XR Packages” 문서를 참고하세요.
- Assets/Oculus 폴더 외부에 Oculus 관련 파일이 참조되고 있을 수 있습니다. “Oculus”나 “OVR”로 시작하는 파일들을 검색하여 삭제하세요.
- Android 앱 빌드 시에는 AndroidManifest, vrapi, vrlib, vrplatlib와 관련된 파일도 검색 후 삭제해야 합니다.

---

## UPM 패키지 내용 수정 방법

UPM 패키지는 기본적으로 읽기 전용입니다. 만약 패키지 내용을 수정하고 싶다면 두 가지 방법이 있습니다:

1. **패키지 복사하기:** PackageCache에서 읽기 전용 패키지를 프로젝트 내 **Packages** 폴더로 복사해 임베디드 패키지로 만듭니다. (자세한 내용은 Unity 문서를 참조)
2. **로컬 패키지로 가져오기:** Meta의 NPM Registry에서 패키지 tarball 파일을 다운로드한 후 압축을 풀어, Unity Package Manager 창을 통해 “Local Package on Disk”로 가져옵니다. (자세한 내용은 Unity 문서를 참조)

---

## Meta XR 패키지 개요

아래는 Meta XR SDK, 도구, 샘플 중 일부를 소개합니다.

### Meta XR All-in-One SDK

Meta XR All-in-One SDK는 여러 Meta XR SDK(Core, Audio, Interaction, Platform, Voice 등)를 한 번에 설치할 수 있어, XR 개발을 빠르게 시작할 수 있는 가장 쉬운 방법입니다.

### 개별 SDK

All-in-One SDK에 포함된 기능이 너무 많다면, 필요한 기능만 선택하여 설치할 수 있습니다. Unity Asset Store에서 설치하면 의존성은 자동으로 관리됩니다.

**예시로 제공되는 개별 SDK:**

- **Meta XR Core SDK:**  
    Meta XR 헤드셋의 기본 기능(카메라 리그, 컨트롤러, 손 추적, 컴포지터 레이어, 패스스루, 공간 앵커, 씬 관리 등)을 제공합니다.
    
- **Meta XR Interaction SDK:**  
    컨트롤러 및 손을 위한 레이, 터치, 이동, 잡기 등의 상호작용 기능을 추가합니다. 손 전용 상호작용, 포즈 및 제스처 감지, 디버그 시각화 기능도 포함되어 있습니다.  
    자세한 내용은 Meta XR Interaction SDK 문서를 참고하세요.
    
- **Meta XR Audio SDK:**  
    공간 오디오 기능(객체 및 앰비소닉 공간화, HRTF 기반 처리, 실내 음향 시뮬레이션)을 제공합니다.  
    자세한 내용은 Meta XR Audio SDK 문서를 참고하세요.
    
- **Meta XR Voice SDK:**  
    음성 상호작용 기능을 통해 자연스러운 방식으로 애플리케이션과 상호작용할 수 있도록 도와줍니다.  
    자세한 내용은 Meta XR Voice SDK 문서를 참고하세요.
    
- **Meta XR Platform SDK:**  
    소셜 VR 애플리케이션 개발을 지원하며, 매치메이킹, DLC, 인앱 구매, 클라우드 저장, 음성 채팅, 커스텀 아이템, 업적 등의 기능을 제공합니다.  
    자세한 내용은 Platform Solutions 문서를 참고하세요.
    

전체 Meta XR UPM 패키지 목록은 Developer Center, Unity Asset Store, Meta NPM Registry에서 확인할 수 있습니다.

---

## Meta XR 개발 도구

Meta XR 개발 도구는 Unity Asset Store와 Meta NPM Registry에서 제공되며, 주요 도구는 다음과 같습니다:

- **Meta XR Simulator:**  
    Meta Quest 헤드셋 및 기능을 API 수준에서 시뮬레이션할 수 있는 경량 OpenXR 런타임입니다. 이를 통해 헤드셋을 착용하지 않고도 앱 테스트 및 디버깅을 할 수 있으며, 자동화 테스트 환경 구축을 쉽게 합니다.  
    자세한 내용은 Meta XR Simulator 문서를 참고하세요.
    
- **Meta XR Mixed Reality Utility Kit:**  
    공간 인식 앱을 개발할 때 자주 사용되는 작업들을 도와주는 다양한 유틸리티와 도구를 제공합니다.  
    자세한 내용은 Mixed Reality Utility Kit 문서를 참고하세요.
    

전체 Meta XR 도구 목록은 Developer Center, Unity Asset Store, Meta NPM Registry에서 확인할 수 있습니다.

---

## Meta XR 샘플

Meta에서는 Meta XR 패키지 사용법을 보여주는 다양한 샘플을 제공합니다.

### SDK에 포함된 샘플

일부 SDK 패키지에는 작은 샘플들이 포함되어 있어, Unity Package Manager 창에서 쉽게 가져올 수 있습니다.

**방법:**

1. Unity Editor에서 **Window > Package Manager** 메뉴를 엽니다.
2. 창 좌측 상단의 **Packages:** 드롭다운 메뉴에서 **Packages: In Project**를 선택합니다.
3. 설치된 패키지(녹색 체크 표시가 있는)를 선택하면 패키지 상세 정보가 나타납니다.
4. 만약 해당 패키지에 샘플이 있다면, 샘플 섹션(또는 탭)이 나타납니다. 여기서 **Samples** 탭을 선택하고 **Import** 버튼을 클릭하여 샘플을 가져올 수 있습니다.

### GitHub에서 제공하는 대형 샘플

더 크고 완성도 높은 샘플들은 GitHub의 oculus-samples 조직에서 찾을 수 있습니다. 여기에는 다음과 같은 샘플이 포함됩니다:

- **Unity Starter Samples:** 기본 VR 기능들을 보여주는 간단한 씬 모음
- **Unity Showcases:** 다양한 기능을 갖춘 완전한 경험의 샘플
- **Discover Showcase:** Meta Quest Mixed Reality API의 주요 기능들을 하이라이트하는 샘플

---

## 추가 학습 자료

Meta XR 패키지에 대해 더 알고 싶다면 다음 리소스를 참고하세요:

- [Meta Quest Developer Center 다운로드](https://developers.facebook.com/quest/)
- Meta on Unity Asset Store
- Meta NPM Registry
- Unity Package Manager 문서