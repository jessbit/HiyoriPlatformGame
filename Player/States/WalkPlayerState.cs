using UnityEngine;

public class WalkPlayerState : PlayerState
{    protected override void OnEnter(Player player)
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
        player.Dash();
        player.Spin();
        player.PickAndThrow();
        var inputDirection = player.inputs.GetMovementCameraDirection();

        if (inputDirection.sqrMagnitude > 0)
        {
            //输入方向与当前水平速度的点乘,用于判断刹车阈值
            var dot = Vector3.Dot(inputDirection, player.lateralVelocity);
            if (dot >= player.stats.current.brakeThreshold)
            {
                player.Accelerate(inputDirection);
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                //低于刹车阈值-》刹车
                player.states.Change<BrakePlayerState>();
            }
        }
        else
        {
            //摩擦力减速
            player.Friction();
            if (player.lateralVelocity.sqrMagnitude <= 0)
            {
                player.states.Change<IdlePlayerState>();
            }
        }

        if (player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
    }
        
}
