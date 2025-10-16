using UnityEngine;

public class GlidingPlayerState :PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity =Vector3.zero;
        player.playerEvents.OnGlidingStart.Invoke();
    }

    protected override void OnExit(Player player)=>
        player.playerEvents.OnGlidingStop.Invoke();

    protected override void OnStep(Player player)
    {
        var inputDirection = player.inputs.GetMovementCameraDirection();
        HandleGlidingGravity(player);
        player.FaceDirectionSmooth(player.lateralVelocity);
        player.GlidingAcceleration(inputDirection);
        player.LedgeGrab();
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }else if (!player.inputs.GetGlide())
        {
            player.states.Change<FallPlayerState>();
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
    protected virtual void HandleGlidingGravity(Player player)
    {
        var yVelocity = player.verticalVelocity.y;
        yVelocity -=player.stats.current.glidingGravity*Time.deltaTime;
        //限制最大掉落速度
        yVelocity =Mathf.Max(yVelocity,-player.stats.current.glidingMaxFallSpeed);
        player.verticalVelocity = new Vector3(0,yVelocity,0);
    }

}