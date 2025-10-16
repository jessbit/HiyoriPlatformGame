using UnityEngine;

public class IdlePlayerState :PlayerState
{
    protected override void OnEnter(Player player)
    {
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        player.RegularSlopeFactor();
        player.Gravity();
        player.SnapToGround();//贴地
        player.Jump();
        player.Fall();
        player.Spin();
        player.PickAndThrow();
        var inputDirection = player.inputs.GetMovementDirection();
        //walk,输入操作-》配置数据-》速度，方向-》改变状态
        if (inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
        {
            player.states.Change<WalkPlayerState>();
        }
        else if (player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }
        player.Dash();
    }

    public override void OnContact(Player entity, Collider other)
    {
    }
}
