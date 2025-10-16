using UnityEngine;

public class WayPointEnemyState :EnemyState
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

        var destination = entity.waypoints.current.position;
        destination = new Vector3(destination.x,entity.position.y, destination.z);
        
        var head = destination -entity.position;
        var distance=head.magnitude;
        var direction = head / distance;

        if (distance <= entity.stats.current.waypointMinDistance)
        {
            entity.Decelerate();
            entity.waypoints.Next();
        }
        else
        {
            entity.Accelerate(
                direction,
                entity.stats.current.waypointAcceleration,
                entity.stats.current.waypointTopSpeed);

            if (entity.stats.current.faceWaypoint)
            {
                entity.FaceDirectionSmooth(direction);
            }
        }
    }

    public override void OnContact(Enemy entity, Collider other)
    {
    }
}