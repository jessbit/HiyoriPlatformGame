using UnityEngine;

public class IdleEnemyState : EnemyState 
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
        entity.Friction();
    }

    public override void OnContact(Enemy entity, Collider other)
    {
    }
}