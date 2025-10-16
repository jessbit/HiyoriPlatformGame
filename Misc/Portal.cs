using UnityEngine;

[RequireComponent(typeof(Collider), typeof(AudioSource))]
public class Portal :MonoBehaviour
{
    public bool useFlash = true;
    public Portal exit;
    //出门偏移量
    public float exitOffset = 1f;
    public AudioClip teleportClip;

    protected Collider m_collider;
    protected AudioSource m_audio;

    protected PlayerCamera m_camera;

    //当前传送门朝向，位置
    public Vector3 position => transform.position;
    public Vector3 forward => transform.forward;

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_audio = GetComponent<AudioSource>();
        m_camera = FindObjectOfType<PlayerCamera>();
        m_collider.isTrigger = true;
    }
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (exit && other.TryGetComponent(out Player player))
        {
            //记录高度的offsetoffset
            var yOffset = player.unsizedPosition.y - transform.position.y;
            //还原出角色出去后应在的高度
            player.transform.position = exit.position + Vector3.up * yOffset;
            player.FaceDirection(exit.forward);
            m_camera.Reset();

            var inputDirection = player.inputs.GetMovementCameraDirection();
            //摆正输入方向
            if (Vector3.Dot(inputDirection, exit.forward) < 0)
            {
                player.FaceDirection(-exit.forward);
            }
            
            player.transform.position += player.transform.forward * exit.exitOffset;
            //使用当前速度
            player.lateralVelocity = player.transform.forward * player.lateralVelocity.magnitude;

            if (useFlash)
            {
                Flash.Instance?.Trigger();
            }

            m_audio.PlayOneShot(teleportClip);
        }
    }
}