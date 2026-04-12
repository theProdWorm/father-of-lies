using UnityEngine;
using Entities;

namespace Abilities.Attacks
{
    public class AttackFlipper : MonoBehaviour
    {
        private void Start()
        {
            if (!PlayerEntity.INSTANCE.FlipFenrirAttack) 
                return;
            
            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z);

            transform.position += transform.right * 0.9f;
            
            // transform.rotation = Quaternion.Euler(
            //     transform.rotation.x,
            //     transform.rotation.y - 5f,
            //     transform.rotation.z);
        }
    }
}