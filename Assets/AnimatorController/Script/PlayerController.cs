using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{[System.Serializable]
    public class Status
    {
        // 体力.
        public int Hp = 10;
        // 攻撃力.
        public int Power = 1;
    }
    public UnityEvent GameOverEvent = new UnityEvent();
    //! HPバーのスライダー.
    [SerializeField] Slider hpBar = null;
    // 開始時位置.
    Vector3 startPosition = new Vector3();
    // 開始時角度.
    Quaternion startRotation = new Quaternion();
    // 自身のコライダー.
    [SerializeField] Collider myCollider = null;
    // 攻撃を食らったときのパーティクルプレハブ.
    [SerializeField] GameObject hitParticlePrefab = null;
    // パーティクルオブジェクト保管用リスト.
    List<GameObject> particleObjectList = new List<GameObject>();
 
    // 攻撃HitオブジェクトのColliderCall.
    [SerializeField] ColliderCallReceiver attackHitCall = null;
    // 基本ステータス.
    [SerializeField] Status DefaultStatus = new Status();
    // 現在のステータス.
    public Status CurrentStatus = new Status();
     [SerializeField] PlayerCameraController cameraController = null;
     // 設置判定用ColliderCall.
    [SerializeField] ColliderCallReceiver footColliderCall = null;
    [SerializeField] float jumpPower=20f;
    // タッチマーカー.
    [SerializeField] GameObject touchMarker = null;
    Rigidbody rigid=null;
   [SerializeField] GameObject attackHit = null; 
     // アニメーター. 
     Animator animator = null; 
    //! 攻撃アニメーション中フラグ. 
    bool isAttack = false;
    // 接地フラグ.
    bool isGround = false;
     // PCキー横方向入力.
    float horizontalKeyInput = 0;
　　　　  // PCキー縦方向入力.
    float verticalKeyInput = 0;
    bool isTouch=false;
    Vector2 leftStartTouch = new Vector2();
// 左半分タッチ入力.
Vector2 leftTouchInput = new Vector2();
    
    // Start is called before the first frame update
    void Start()
    {
        // 開始時の位置回転を保管.
　　　  startPosition = this.transform.position;
        startRotation = this.transform.rotation;
        // スライダーを初期化.
        hpBar.maxValue = DefaultStatus.Hp;
        hpBar.value = CurrentStatus.Hp;
        animator=GetComponent<Animator>();
         attackHit.SetActive( false );
         rigid=GetComponent<Rigidbody>();
         // FootSphereのイベント登録.
        footColliderCall.TriggerStayEvent.AddListener( OnFootTriggerStay );
        footColliderCall.TriggerExitEvent.AddListener( OnFootTriggerExit );
        // 攻撃判定用コライダーイベント登録.
        attackHitCall.TriggerEnterEvent.AddListener( OnAttackHitTriggerEnter );
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
    }
    void OnAttackHitTriggerEnter( Collider col )
    {
        if( col.gameObject.tag == "Enemy" )
        {
            var enemy = col.gameObject.GetComponent<EnemyBase>();
            enemy?.OnAttackHit( CurrentStatus.Power, this.transform.position );
            attackHit.SetActive( false );
        }
    }

    // Update is called once per frame
    void Update()
    {
        cameraController.UpdateCameraLook( this.transform );
        if( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
    {
        if( Input.touchCount > 0 )
        {
            isTouch=true;
            // タッチ情報をすべて取得.
            Touch[] touches = Input.touches;
            // 全部のタッチを繰り返して判定.
            foreach( var touch in touches )
            {
               bool isLeftTouch = false;
                bool isRightTouch = false;
                // タッチ位置のX軸方向がスクリーンの左側.
                if( touch.position.x > 0 && touch.position.x < Screen.width / 2 )
                {
                    isLeftTouch = true;
                }
                 // タッチ位置のX軸方向がスクリーンの右側.
                else if( touch.position.x > Screen.width / 2 && touch.position.x < Screen.width )
                {
                    isRightTouch = true;;
                }
 
                // 左タッチ.
                if( isLeftTouch == true )
                {
                     // 左半分をタッチした際の処理.
                     if(touch.phase==TouchPhase.Began)
                     {
                        Debug.Log("タッチ開始");
                        leftStartTouch = touch.position;
                        touchMarker.SetActive( true );
                        Vector3 touchPosition = touch.position;
                        touchPosition.z = 1f;
                        Vector3 markerPosition = Camera.main.ScreenToWorldPoint( touchPosition );
                        touchMarker.transform.position = markerPosition;
                     }
                     else if(touch.phase==TouchPhase.Moved||touch.phase==TouchPhase.Stationary)
                     {
                        Debug.Log( "タッチ中" );
                         // 現在の位置を随時保管.
                        Vector2 position = touch.position;
                         // 移動用の方向を保管.
                        leftTouchInput = position - leftStartTouch;
                     }
                     else if(touch.phase==TouchPhase.Ended)
                     {
                        Debug.Log( "タッチ終了" );
                        Debug.Log( "タッチ終了" );
                        leftTouchInput = Vector2.zero;
                        // マーカーを非表示.
                        touchMarker.gameObject.SetActive( false );
                     }
                }
 
                // 右タッチ.
                if( isRightTouch == true )
                {
                     // 右半分をタッチした際の処理.
                     cameraController.UpdateRightTouch( touch );
                }
            }
        }
        else
        {
            isTouch = false;
        }
    }
    else
    {
        // PCキー入力取得.
        horizontalKeyInput = Input.GetAxis( "Horizontal" );
        verticalKeyInput = Input.GetAxis( "Vertical" );
    }
        // プレイヤーの向きを調整.
    bool isKeyInput = ( horizontalKeyInput != 0 || verticalKeyInput != 0 || leftTouchInput != Vector2.zero );
    if( isKeyInput == true&&isAttack==false)
        {
           bool currentIsRun = animator.GetBool( "isRun" );
           if( currentIsRun == false ) animator.SetBool( "isRun", true );
           Vector3 dir = rigid.velocity.normalized;
           dir.y = 0;
           this.transform.forward = dir;
        }
    else
        {
            bool currentIsRun = animator.GetBool( "isRun" );
            if( currentIsRun == true ) animator.SetBool( "isRun", false );
        }
    }
    void FixedUpdate()
{
    // カメラの位置をプレイヤーに合わせる.
        cameraController.FixedUpdateCameraPosition( this.transform );
    if( isAttack == false )
        {
           Vector3 input = new Vector3();
           Vector3 move = new Vector3();
           if( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
           {
            input = new Vector3( leftTouchInput.x, 0, leftTouchInput.y );
            move = input.normalized * 2f;
           }
           else
           {
            input = new Vector3( horizontalKeyInput, 0, verticalKeyInput );
            move = input.normalized * 2f;
           }
 
        Vector3 cameraMove = Camera.main.gameObject.transform.rotation * move;
        cameraMove.y = 0;
        Vector3 currentRigidVelocity = rigid.velocity;
        currentRigidVelocity.y = 0;
 
        rigid.AddForce( cameraMove - currentRigidVelocity, ForceMode.VelocityChange );
        }
}
    public void OnAttackButtonClicked()
    {
       if( isAttack == false )
        {
            // AnimationのisAttackトリガーを起動.
            animator.SetTrigger( "isAttack" );
            // 攻撃開始.
            isAttack = true;
        }
    }
    void Anim_AttackHit()
    {
        Debug.Log( "Hit" );
        // 攻撃判定用オブジェクトを表示.
        attackHit.SetActive( true );
    }
    void Anim_AttackEnd()
    {
        Debug.Log( "End" );
        // 攻撃判定用オブジェクトを非表示に.
        attackHit.SetActive( false );
        // 攻撃終了.
        isAttack = false;
    }
    public void OnJumpButtonClicked()
    {
        if(isGround==true)
        {
                rigid.AddForce(Vector3.up*jumpPower,ForceMode.Impulse);
        }
    }
    void OnFootTriggerStay( Collider col )
    {
        if( col.gameObject.tag == "Ground" )
        {
            if( isGround == false ) isGround = true;
            if( animator.GetBool( "isGround" ) == false ) animator.SetBool( "isGround", true );
        }
    }
 
    // ---------------------------------------------------------------------
    /// <summary>
    /// FootSphereトリガーイグジットコール.
    /// </summary>
    /// <param name="col"> 侵入したコライダー. </param>
    // ---------------------------------------------------------------------
    void OnFootTriggerExit( Collider col )
    {
        if( col.gameObject.tag == "Ground" )
        {
            isGround = false;
            animator.SetBool( "isGround", false );
        }
    }
    public void OnEnemyAttackHit( int damage ,Vector3 attackPosition)
    {
        CurrentStatus.Hp -= damage;
        var pos = myCollider.ClosestPoint( attackPosition );
        var obj = Instantiate( hitParticlePrefab, pos, Quaternion.identity );
        var par = obj.GetComponent<ParticleSystem>();
        StartCoroutine( WaitDestroy( par ) );
        particleObjectList.Add( obj );
 
        if( CurrentStatus.Hp <= 0 )
        {
            OnDie();
        }
        else
        {
            Debug.Log( damage + "のダメージを食らった!!残りHP" + CurrentStatus.Hp );
        }
    }
    IEnumerator WaitDestroy( ParticleSystem particle )
    {
        yield return new WaitUntil( () => particle.isPlaying == false );
        if( particleObjectList.Contains( particle.gameObject ) == true ) particleObjectList.Remove( particle.gameObject );
        Destroy( particle.gameObject );
    }
    void OnDie()
    {
        GameOverEvent?.Invoke();
        Debug.Log( "死亡しました。" );
        StopAllCoroutines();
        if( particleObjectList.Count > 0 )
        {
            foreach( var obj in particleObjectList ) Destroy( obj );
            particleObjectList.Clear();
        }
    }
    public void Retry()
    {
        // 現在のステータスの初期化.
        CurrentStatus.Hp = DefaultStatus.Hp;
        CurrentStatus.Power = DefaultStatus.Power;
        // 位置回転を初期位置に戻す.
        this.transform.position = startPosition;
        this.transform.rotation = startRotation;
        hpBar.value = CurrentStatus.Hp;
        //攻撃処理の途中でやられた時用
        isAttack = false;
    }
    
}
