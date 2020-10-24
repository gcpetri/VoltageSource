using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shakeCuties : MonoBehaviour
{
    public Sprite cutie;
    public Animator anim;
    // Update is called once per frame
    private void Shake()
    {
        anim.SetTrigger(anim.parameters[0].name);
    }
}
