using UnityEngine;

public class AirDivePlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero;//进入俯冲时清空垂直速度
        player.lateralVelocity = player.transform.forward
                                 * player.stats.current.airDiveForwardForce;
        player.FaceDirection(player.lateralVelocity);//平滑转向
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.Jump();
        player.FallWithWall();
        if (player.stats.current.applyDiveSlopeFactor)
            player.SlopeFactor(player.stats.current.slopeUpwardForce,
                player.stats.current.slopeDownwardForce);

        if (player.isGrounded)
        {
            //获取玩家相机方向的输入
            var inputDirection = player.inputs.GetMovementCameraDirection();
            //将输入转换到角色坐标系中
            var localInputDirection = player.transform.InverseTransformDirection(inputDirection);
            //根据输入和速度更新角度
            var rotation = localInputDirection.x
                           * player.stats.current.airDiveRotationSpeed
                           * Time.deltaTime;
            //将旋转应用到水平速度上
            player.lateralVelocity = Quaternion.Euler(0, rotation, 0) * player.lateralVelocity;
            player.Decelerate(player.stats.current.airDiveFriction);
            if (player.lateralVelocity.sqrMagnitude == 0)
            {
                player.verticalVelocity = Vector3.up * player.stats.current.airDiveGroundLeapHeight;
                player.states.Change<FallPlayerState>();
            }
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