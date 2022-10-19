using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyBase : MonoBehaviour
{
    
    [System.Serializable]
    public class Status
    {
       // HP.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }
     public bool IsBattle = false;
     
  //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;
     // 敵の移動イベント定義クラス.
    public class EnemyMoveEvent : UnityEvent<EnemyBase>{}
    // 目的地設定イベント.
    public EnemyMoveEvent ArrivalEvent = new EnemyMoveEvent();

    // ナビメッシュ.
    NavMeshAgent navMeshAgent = null;
 
    // 現在設定されている目的地.
     Transform currentTarget = null;
      protected Transform currentAttackTarget = null;
　  // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();
    //! 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    //! 攻撃ヒット時エフェクトプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;
     // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
     // 周辺レーダーコライダーコール.
    [SerializeField] ColliderCallReceiver aroundColliderCall = null;
    [SerializeField] protected ColliderCallReceiver attackHitColliderCall = null;
    public Status CurrentStatus = new Status();
    Animator animator = null;
    [SerializeField] float attackInterval = 3f;
    // 攻撃状態フラグ.
    bool isBattle = false;
    // 攻撃時間計測用.
    float attackTimer = 0f;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        // 開始時の位置回転を保管.
        startPosition = this.transform.position;
        startRotation = this.transform.rotation;
        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
        // 攻撃コライダーイベント登録.
        attackHitColliderCall.TriggerEnterEvent.AddListener( OnAttackTriggerEnter );
        attackHitColliderCall.gameObject.SetActive( false );
 
        attackHitColliderCall.gameObject.SetActive( false );
        aroundColliderCall.TriggerEnterEvent.AddListener( OnAroundTriggerEnter );
        aroundColliderCall.TriggerStayEvent.AddListener( OnAroundTriggerStay );
        aroundColliderCall.TriggerExitEvent.AddListener( OnAroundTriggerExit );
        animator = GetComponent<Animator>();
        // 最初に現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        //aroundColliderCall.TriggerStayEvent.AddListener( OnAroundTriggerStay );
    }
    protected virtual void Update()
    {
        // ターゲットまでの距離を測定し、イベントを実行.
        if( currentTarget == null )
        {
            ArrivalEvent?.Invoke( this );
            Debug.Log( gameObject.name + "移動開始." );
        }
        else
        {
            var sqrDistance = ( currentTarget.position - this.transform.position ).sqrMagnitude;
            if( sqrDistance < 3f )
            {
                ArrivalEvent?.Invoke( this );
            }
        }
        // 攻撃できる状態の時.
        if( IsBattle == true )
        {
            attackTimer += Time.deltaTime;
            animator.SetBool( "isRun", false );
            if( attackTimer >= 3f )
            {
                animator.SetTrigger( "isAttack" );
                attackTimer = 0;
            }
        }
        else
        {
            attackTimer = 0;
            if( currentTarget == null )
            {
                animator.SetBool( "isRun", false );
 
                ArrivalEvent?.Invoke( this );
                Debug.Log( gameObject.name + "移動開始." );
            }
            else
            {
                animator.SetBool( "isRun", true );
 
                var sqrDistance = ( currentTarget.position - this.transform.position ).sqrMagnitude;
                if( sqrDistance < 3f )
                {
                    ArrivalEvent?.Invoke( this );
                }
            }
        }
    }
     void OnAroundTriggerEnter( Collider other )
    {
        if( other.gameObject.tag == "Player" )
        {
            IsBattle = true;
            navMeshAgent.SetDestination( this.transform.position );
          currentTarget = null;
        }
    }
    protected virtual void OnAroundTriggerExit( Collider other )
    {
        if( other.gameObject.tag == "Player" )
        {
            IsBattle = false;
            currentAttackTarget = null;
        }
    }
     public void OnRetry()
    {
        // 現在のステータスを基本ステータスとして設定.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // 開始時の位置回転を保管.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;
         hpBar.value = CurrentStatus.Hp;
 
        //敵を再度表示
        this.gameObject.SetActive(true);
    }
    protected virtual void Anim_AttackHit()
    {
        attackHitColliderCall.gameObject.SetActive( true );

    }
    
    // ----------------------------------------------------------
    /// <summary>
    /// 攻撃アニメーション終了時コール.
    /// </summary>
    // ----------------------------------------------------------
    protected virtual void Anim_AttackEnd()
    {
        attackHitColliderCall.gameObject.SetActive( false );
    }
    public void OnAttackHit( int damage  ,Vector3 attackPosition )
    {
        CurrentStatus.Hp -= damage;
        hpBar.value = CurrentStatus.Hp;
        Debug.Log( "Hit Damage " + damage + "/CurrentHp = " + CurrentStatus.Hp );
        var pos = myCollider.ClosestPoint( attackPosition );
        var obj = Instantiate( hitParticlePrefab, pos, Quaternion.identity );
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine( WaitDestroy( par ) );
 
        if( CurrentStatus.Hp <= 0 )
        {
            OnDie();
        }
        else
        {
            animator.SetTrigger( "isHit" );
        }
    }
    IEnumerator WaitDestroy( ParticleSystem particle )
    {
        yield return new WaitUntil( () => particle.isPlaying == false );
        Destroy( particle.gameObject );
    }
    void OnDie()
    {
        Debug.Log( "死亡" );
        animator.SetBool( "isDie", true );
    }
    void Anim_DieEnd()
    {
         this.gameObject.SetActive( false );
    }
    protected virtual void OnAroundTriggerStay( Collider other )
    {
        if( other.gameObject.tag == "Player" )
        {
            var _dir = ( other.gameObject.transform.position - this.transform.position ).normalized;
            _dir.y = 0;
            this.transform.forward = _dir;
            currentAttackTarget = other.gameObject.transform;
        }
    }
    void OnAttackTriggerEnter( Collider other )
    {
        if( other.gameObject.tag == "Player" )
        {
            var player = other.GetComponent<PlayerController>();
             player?.OnEnemyAttackHit( CurrentStatus.Power, this.transform.position );
            Debug.Log( "プレイヤーに敵の攻撃がヒット！" + CurrentStatus.Power + "の力で攻撃！" );
            attackHitColliderCall.gameObject.SetActive( false );
        }
    }
     public void SetNextTarget( Transform target )
    {
        if( target == null ) return;
        if( navMeshAgent == null ) navMeshAgent = GetComponent<NavMeshAgent>();
 
        navMeshAgent.SetDestination( target.position );
        Debug.Log( gameObject.name + "ターゲットへ移動." + target.gameObject.name );
        currentTarget = target;
    }
    

    // Update is called once per frame
    
}
