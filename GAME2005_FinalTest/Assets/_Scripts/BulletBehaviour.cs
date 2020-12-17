using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BulletBehaviour : MonoBehaviour
{
    public float speed;
    public Vector3 direction;
    public float range;
    public float radius;
    public bool debug;
    public bool isColliding;
    public Vector3 collisionNormal;
    public float penetration;

    // Added Cube Properties to Bullet
    public Vector3 max;
    public Vector3 min;
    public Bounds bounds;
    public List<Contact> contacts;
    public Vector3 size;
    private MeshFilter meshFilter;


    public BulletManager bulletManager;

    // Start is called before the first frame update
    void Start()
    {
        isColliding = false;
        debug = false;
        radius = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 0.5f;
        bulletManager = FindObjectOfType<BulletManager>();

        meshFilter = GetComponent<MeshFilter>();
        bounds = meshFilter.mesh.bounds;
    }

    // Update is called once per frame
    void Update()
    {
        _Move();
        _CheckBounds();
    }

    private void _Move()
    {
        transform.position += direction * speed * Time.deltaTime;
        max = Vector3.Scale(bounds.max, transform.localScale) + transform.position;
        min = Vector3.Scale(bounds.min, transform.localScale) + transform.position;

    }

    private void _CheckBounds()
    {
        if (Vector3.Distance(transform.position, Vector3.zero) > range)
        {
            bulletManager.ReturnBullet(this.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (debug)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
