﻿using UnityEngine;

public class DashPlayerState:PlayerState
{
    protected override void OnEnter(Player player)
    {
        //清空垂直速度
        player.verticalVelocity = Vector3.zero;
        player.lateralVelocity = player.transform.forward * player.stats.current.dashForce *player.dashSpeedMultiplier;
        player.playerEvents.OnDashStarted?.Invoke();
    }

    protected override void OnExit(Player player)
    {
        player.lateralVelocity = Vector3.ClampMagnitude(
            player.lateralVelocity, player.stats.current.topSpeed);
        player.playerEvents.OnDashEnded?.Invoke();
    }

    protected override void OnStep(Player player)
    {
        player.Jump();
        player.PickAndThrow();
        if (timeSinceEntered > player.stats.current.dashDuration)
        {
            if (player.isGrounded)
                player.states.Change<WalkPlayerState>();
            else 
                player.states.Change<FallPlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
        player.WallDrag(other);
        player.GrabPole(other);
    }
}