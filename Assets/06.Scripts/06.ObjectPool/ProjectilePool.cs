using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using ProjectileCurveVisualizerSystem;

/// <summary>
/// 투사체(화살) 전용 풀 매니저
/// 씬에 붙여둔 뒤, GetProjectile(...) 으로 화살을 가져올 수 있음.
/// 화살 타입별로 서로 다른 풀을 관리합니다.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
   #region 필드변수
   // 싱글톤 패턴으로 전역 접근 가능한 인스턴스
   public static ProjectilePool Instance { get; private set; }

   // 인스펙터에서 화살 타입과 프리팹을 매핑하기 위한 클래스
   [System.Serializable]
   public class ProjectilePoolSetting
   {
       public ProjectileData.ProjectileType arrowType; // 화살 타입 (Normal, Explosive, Poison)
       public Projectile projectilePrefab;            // 해당 타입의 프리팹
   }

   [Header("Projectile Prefabs & Pool Settings")]
   // 인스펙터에서 설정할 화살 타입과 프리팹 목록
   [SerializeField] private List<ProjectilePoolSetting> projectilePrefabs = new List<ProjectilePoolSetting>();
   [SerializeField] private int defaultCapacity = 10;   // 풀 초기 생성 갯수 - 각 타입별로 이 수만큼 미리 생성
   [SerializeField] private int maxSize = 50;          // 풀 최대 갯수 - 넘어가면 Destroy

   // 현재 선택된 화살 타입 (플레이어가 선택한 화살) - 기본값은 일반 화살
   private ProjectileData.ProjectileType currentArrowType = ProjectileData.ProjectileType.Normal;

   // 플레이어용 화살 풀 - 화살 타입별로 별도 관리
   private Dictionary<ProjectileData.ProjectileType, IObjectPool<Projectile>> projectilePools = new Dictionary<ProjectileData.ProjectileType, IObjectPool<Projectile>>();
   
   // 화살 타입별 프리팹을 빠르게 찾기 위한 딕셔너리 - O(1) 시간 복잡도로 접근 가능
   private Dictionary<ProjectileData.ProjectileType, Projectile> prefabDictionary = new Dictionary<ProjectileData.ProjectileType, Projectile>();
   
   #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        // 싱글톤 패턴 구현 - 중복 인스턴스 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 각 화살 타입별 프리팹 딕셔너리 초기화
        // 인스펙터에서 설정한 리스트를 실행 중 빠르게 찾을 수 있는 딕셔너리로 변환
        foreach (var setting in projectilePrefabs)
        {
            prefabDictionary[setting.arrowType] = setting.projectilePrefab;
        }

        // 플레이어 화살 풀 초기화 - 각 타입별 풀 생성
        InitializePlayerPools();
    }

   #endregion

   #region 오브젝트풀 관련

   // 플레이어 화살 풀 초기화 - 각 화살 타입별로 별도의 풀 생성
   private void InitializePlayerPools()
   {
       foreach (var setting in projectilePrefabs)
       {
           // Unity의 Object Pool 사용하여 각 화살 타입별 풀 생성
           IObjectPool<Projectile> pool = new ObjectPool<Projectile>(
               createFunc: () => CreatePooledItem(setting.arrowType), // false = 플레이어용 화살
               actionOnGet: OnTakeFromPool,        // 풀에서 꺼낼 때 실행할 작업
               actionOnRelease: OnReturnedToPool,  // 풀로 반환할 때 실행할 작업
               actionOnDestroy: OnDestroyPoolObject, // 파괴될 때 실행할 작업
               collectionCheck: false,     // 중복 반환 체크 (꺼내온 화살 재반환시 오류 체크)
               defaultCapacity: defaultCapacity, // 초기 생성 수량 (50개)
               maxSize: maxSize            // 최대 수량 (150개)
           );
           
           // 타입별 풀 저장
           projectilePools[setting.arrowType] = pool;
       }
   }

   // 풀에 화살 생성 - 플레이어용과 타워용 구분
   private Projectile CreatePooledItem(ProjectileData.ProjectileType arrowType)
   {
       // 해당 타입의 프리팹이 없으면 기본 타입(Normal)으로 대체
       if (!prefabDictionary.ContainsKey(arrowType))
       {
           arrowType = ProjectileData.ProjectileType.Normal;
       }

       // 프리팹을 실제로 생성
       Projectile newProjectile = Instantiate(prefabDictionary[arrowType]);
       
        // 플레이어용 풀 참조 설정
        newProjectile.SetPoolReference(projectilePools[arrowType]);
        // 플레이어용 화살임을 이름으로 표시 (디버깅용)
        newProjectile.gameObject.name = $"Player_{arrowType}Arrow";

       return newProjectile;
   }

   // 풀에서 화살을 꺼낼 때 호출되는 메서드
   private void OnTakeFromPool(Projectile projectile)
   {
       // 화살 상태 초기화 (물리, 위치, 회전 등)
       projectile.ResetState();
       // 화살 활성화
       projectile.gameObject.SetActive(true);
   }

   // 풀로 화살을 반환할 때 호출되는 메서드
   private void OnReturnedToPool(Projectile projectile)
   {
       // 화살 비활성화
       projectile.gameObject.SetActive(false);
       // 화살 상태 초기화 (물리값 등)
       projectile.ResetState();
   }

   // 풀에서 화살이 영구 제거될 때 호출되는 메서드
   private void OnDestroyPoolObject(Projectile projectile)
   {
       // maxSize(150개) 초과 시 호출되어 실제 오브젝트 파괴
       Destroy(projectile.gameObject);
   }

   #endregion

   #region 화살 관련 함수

   /// <summary>
   /// 현재 화살 타입을 설정합니다. (플레이어가 화살 타입 변경 시 호출)
   /// </summary>
   /// <param name="arrowType">화살 타입</param>
   public void SetCurrentArrowType(ProjectileData.ProjectileType arrowType)
   {
       // 등록된 타입인지 확인 - 없는 타입이면 Normal로 대체
       if (!projectilePools.ContainsKey(arrowType))
       {
           currentArrowType = ProjectileData.ProjectileType.Normal;
       }
       else
       {
           // 현재 활성 화살 타입 변경
           currentArrowType = arrowType;
       }
   }

   /// <summary>
   /// 현재 설정된 화살 타입에 맞는 화살을 플레이어 풀에서 꺼내옵니다.
   /// ArrowShooter에서 화살 발사 시 호출됨
   /// </summary>
   public Projectile GetProjectile()
   {
       // 현재 타입의 풀이 없으면 기본 화살(Normal) 풀 사용
       if (!projectilePools.ContainsKey(currentArrowType))
       {
           currentArrowType = ProjectileData.ProjectileType.Normal;
       }

       // 풀에서 화살 가져오기
       return projectilePools[currentArrowType].Get();
   }

   #endregion
}