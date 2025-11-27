using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifeTime = 1.0f;

    void Start()
    {
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    void Update()
    {
        if (lifeTime <= 0)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f &&
                    !anim.IsInTransition(0))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}