using System.Data;
using UnityEngine;
using UnityEngine.Splines;

public class RailGrindPlayerState :PlayerState
{
    protected bool m_backwards;
    protected float m_speed;
    protected float m_lastDahTime;
    protected override void OnEnter(Player player)
    {
        Evaluate(player,out var point,out var forward,out var upward,out _);
        UpdatePosition(player, point, upward);
        m_backwards = Vector3.Dot(player.transform.forward, forward) < 0;
        m_speed = Mathf.Max(player.lateralVelocity.magnitude, player.stats.current.minGrindInitialSpeed);
        player.velocity =Vector3.zero;
        player.UseCustomCollision(true);
    }

    protected override void OnExit(Player player)
    {
        player.ExitRail();
        player.UseCustomCollision(false);
    }

    protected override void OnStep(Player player)
    {
        player.Jump();
        if (player.onRails)
        {
            Evaluate(player,out var point,out var forward,out var upward,out var t);
            var direction = m_backwards ? -forward : forward;
            var factor = Vector3.Dot(Vector3.up, direction);
            var multiplier = factor <= 0
                ? player.stats.current.slopeDownwardForce
                : player.stats.current.slopeUpwardForce;
            HandleDeceleration(player);
            HandleDash(player);
            if (player.stats.current.applyGrindingSlopeFactor)
            {
                m_speed -= factor * multiplier * Time.deltaTime;
            }
            m_speed = Mathf.Clamp(m_speed, player.stats.current.minGrindSpeed, player.stats.current.grindTopSpeed);
            RotateOnRail(player, direction, upward);
            player.velocity =direction * m_speed;
            if (player.rails.Spline.Closed || (t > 0 && t < 9f))
            {
                UpdatePosition(player,point,upward);
            }
        }
        else
        {
            player.states.Change<FallPlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other)
    {
        
    }

    protected virtual void Evaluate(Player player, out Vector3 point, out Vector3 forward,
        out Vector3 upward, out float t)
    {
        var origin = player.rails.transform.InverseTransformPoint(player.transform.position);
        //计算滑轨最近的点和最近点对应的归一化插值率
        SplineUtility.GetNearestPoint(player.rails.Spline, origin, out var nearest, out t);

        point = player.rails.transform.TransformPoint(nearest);
        //向前方向，计算空间中样条线上点t的切向量所得方向
        forward = Vector3.Normalize(player.rails.EvaluateTangent(t));
        //向上方向，点t的向上向量
        upward = Vector3.Normalize(player.rails.EvaluateUpVector(t));
    }

    protected virtual void UpdatePosition(Player player, Vector3 point, Vector3 upward)
    {
        //将玩家放入滑轨上并计算offset防止穿模
        player.transform.position = point + upward * GetDistanceToRail(player);
    }

    protected virtual float GetDistanceToRail(Player player)
    {
        return player.originalHeight * 0.5f + player.stats.current.grindRadiusOffset;
    }

    protected virtual void RotateOnRail(Player player, Vector3 forward, Vector3 upward)
    {
        if (forward != Vector3.zero)
        {
            //玩家面朝forward，以transform.up为轴
            player.transform.rotation = Quaternion.LookRotation(forward, player.transform.up);
        }
        //头顶朝向处理
        player.transform.rotation=Quaternion.FromToRotation(player.transform.up, upward) *player.transform.rotation;
    }

    protected virtual void HandleDeceleration(Player player)
    {
        if (player.stats.current.canGrindBrake && player.inputs.GetGrindBrake())
        {
            var decelerationDelata =player.stats.current.grindBrakeDeceleration*Time.deltaTime;
            m_speed = Mathf.MoveTowards(m_speed, 0, decelerationDelata);
        }
    }

    protected virtual void HandleDash(Player player)
    {
        if (player.stats.current.canGroundDash && player.inputs.GetDashDown() &&
            Time.time >= m_lastDahTime + player.stats.current.grindDashCoolDown)
        {
            m_lastDahTime = Time.time;
            m_speed = player.stats.current.grindDashForce;
            player.playerEvents.OnDashStarted?.Invoke();
        }
    }
}