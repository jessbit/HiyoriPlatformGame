using UnityEngine;

public class FollowEnemyState :EnemyState
{
    protected override void OnEnter(Enemy player)
    {
        
    }

    protected override void OnExit(Enemy entity)
    {
    }

    protected override void OnStep(Enemy entity)
    {
        entity.Gravity();
        entity.SnapToGround();

        var head = entity.player.position - entity.position;
        var diraction = new Vector3(head.x, 0, head.z).normalized;
        
        entity.Accelerate(diraction,entity.stats.current.turningDrag,entity.stats.current.followAcceleration,entity.stats.current.followTopSpeed);
        entity.FaceDirectionSmooth(diraction);
    }

    public override void OnContact(Enemy entity, Collider other)
    {
    }
}