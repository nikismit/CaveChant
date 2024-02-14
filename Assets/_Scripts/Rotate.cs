using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float speed = 1;

    void Update()
    {
        transform.eulerAngles += new Vector3(0, speed * Time.deltaTime * 20, 0);
    }
}
