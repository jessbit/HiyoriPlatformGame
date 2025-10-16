using System.Collections;
using UnityEngine;

public class LedgeClimbingPlayerState :PlayerState
{
    //协程引用，用于控制爬升动画 
    protected IEnumerator m_routine;
    protected override void OnEnter(Player player)
    {
        m_routine = SetPositionRoutine(player);
        player.StartCoroutine(m_routine);
    }

    protected override void OnExit(Player player)
    {
        player.ResetSkinParent();
        player.StopCoroutine(m_routine);
    }

    protected override void OnStep(Player player)
    {   
    }
    public override void OnContact(Player player, Collider other)
    {
    }            
    protected virtual IEnumerator SetPositionRoutine(Player player)
    {
        var elapsedTime = 0f;
        var totalDuration = player.stats.current.ledgeClimbingDuration;
        var halfDuration = totalDuration / 2;
        
        var initialPosition = player.transform.position;
        var targetVerticalPosition =
            player.transform.position + Vector3.up * (player.height + Physics.defaultContactOffset);
        var targetLateralPosition = targetVerticalPosition + player.transform.forward * player.radius * 2f;
        //有父对象，切换到父对象局部坐标进行位移
        if (player.transform.parent != null)
        {
            targetVerticalPosition = player.transform.parent.InverseTransformPoint(targetVerticalPosition);
            targetLateralPosition = player.transform.parent.InverseTransformPoint(targetLateralPosition);
        }

        player.SetSkinParent(player.transform.parent);
        player.skin.position += player.transform.rotation * player.stats.current.ledgeClimbSkinOffset;
        //垂直上升
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.localPosition = Vector3.Lerp(initialPosition,targetVerticalPosition,elapsedTime/halfDuration);
            yield return null;
        }

        elapsedTime = 0f;
        player.transform.localPosition = targetVerticalPosition;
        
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.localPosition = Vector3.Lerp(targetVerticalPosition,targetLateralPosition,elapsedTime/halfDuration);
            yield return null;
        }
        player.transform.localPosition = targetLateralPosition;
        player.states.Change<IdlePlayerState>();
    }
}
