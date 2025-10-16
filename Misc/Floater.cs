using UnityEngine;

public class Floater : MonoBehaviour
{
   public float speed=2f;
   //浮动的幅度，上下移动的幅度
   public float amplitude=0.5f;

   protected virtual void LateUpdate()
   {
      //计算一个随时间变化的正弦值
      //Time.time*Speed控制频率快慢
      var wave = Mathf.Sin(Time.time * speed) * amplitude;
      
      transform.position +=transform.up*wave*Time.deltaTime;
   }
}