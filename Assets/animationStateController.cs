using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{
    Animator animator; 
    System.Random rand;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>(); 
        rand = new System.Random();
        
    }
    
    // Update is called once per frame
    void Update()
    {  
        bool isHavingHeartAttack = animator.GetBool("havingHeartAttack");
        if (!isHavingHeartAttack && rand.NextDouble() >= 0.995) {
            animator.SetBool("havingHeartAttack", true);
            animator.SetFloat("dyingSpeed", (float)(rand.NextDouble()));
        }
        
    }
}
