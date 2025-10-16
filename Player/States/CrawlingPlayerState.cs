﻿using UnityEngine;

public class CrawlingPlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originalHeight);
    }

    protected override void OnStep(Player player)
    {
        player.RegularSlopeFactor();
        player.Gravity();
        player.SnapToGround();//贴地
        player.Jump();
        player.Fall();
        player.Decelerate(player.stats.current.crouchFriction);
        //获取输入移动方向(相对世界不考虑相机)
        var inputDirection = player.inputs.GetMovementCameraDirection();
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            if (inputDirection.sqrMagnitude > 0)
            {
                player.CrawlingAccelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                player.Decelerate(player.stats.current.crawlingFriction);
            }
        }
        else
        {
            player.states.Change<IdlePlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other)
    {
        if (!player.isGrounded)
        {
            player.WallDrag(other);
            player.GrabPole(other);
        }
    }      
}