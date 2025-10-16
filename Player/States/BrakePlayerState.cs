using UnityEngine;

public class BrakePlayerState : PlayerState
{
    protected override void OnEnter(Player player)
    {
        
    }

    protected override void OnExit(Player player)
    {
    }

    protected override void OnStep(Player player)
    {
        var inputDirection = player.inputs.GetMovementCameraDirection();
        if (player.stats.current.canBackflip &&
            Vector3.Dot(inputDirection, player.transform.forward) < 0 &&
            player.inputs.GetJumpDown())
        {
            player.Backflip(player.stats.current.backflipBackwardForce);
        }
        else
        {
            player.SnapToGround();//贴地
            player.Jump();
            player.Fall();
            player.Decelerate();
            if (player.lateralVelocity.sqrMagnitude == 0)
            {
                player.states.Change<IdlePlayerState>();
            }  
        }
    }

    public override void OnContact(Player player, Collider other)
    {
    }
}
