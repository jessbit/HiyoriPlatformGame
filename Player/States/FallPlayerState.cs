using UnityEngine;

public class FallPlayerState :PlayerState
{
    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();//贴地
        player.FallWithWall();
        player.FaceDirectionSmooth(player.lateralVelocity);//平滑转向
        player.AccelerateToInputDirection();//根据玩家输入方向加速
        player.Jump();
        player.Spin();
        player.PickAndThrow();
        player.Dash();
        player.AirDive();
        player.StompAttack();
        player.Glide();
        player.LedgeGrab();
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }

    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
        player.WallDrag(other);
        player.GrabPole(other);
    }        
}
