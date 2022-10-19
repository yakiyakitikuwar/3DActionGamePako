using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColliderCallReceiver : MonoBehaviour
{
    public class TriggerEvent : UnityEvent<Collider>{}
    // トリガーエンターイベント.
    public TriggerEvent TriggerEnterEvent = new TriggerEvent();
    // トリガーステイイベント.
    public TriggerEvent TriggerStayEvent = new TriggerEvent();
    // トリガーイグジットイベント.
    public TriggerEvent TriggerExitEvent = new TriggerEvent();
 
    void Start()
    {
        
    }
 
    // -------------------------------------------------------------------------
    /// <summary>
    /// トリガーエンターコールバック.
    /// </summary>
    /// <param name="other"> 接触したコライダー. </param>
    // -------------------------------------------------------------------------
    void OnTriggerEnter( Collider other )
    {
        TriggerEnterEvent?.Invoke( other );
    }
 
    // -------------------------------------------------------------------------
    /// <summary>
    /// トリガーステイコールバック.
    /// </summary>
    /// <param name="other"> 接触したコライダー. </param>
    // -------------------------------------------------------------------------
    void OnTriggerStay( Collider other )
    {
        TriggerStayEvent?.Invoke( other );
    }
 
    // -------------------------------------------------------------------------
    /// <summary>
    /// トリガーイグジットコールバック.
    /// </summary>
    /// <param name="other"> 接触したコライダー. </param>
    // -------------------------------------------------------------------------
    void OnTriggerExit( Collider other )
    {
        TriggerExitEvent?.Invoke( other );
    }
}
