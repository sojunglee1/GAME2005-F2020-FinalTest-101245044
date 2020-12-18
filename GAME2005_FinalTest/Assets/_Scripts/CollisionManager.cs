using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionManager : MonoBehaviour
{
    public CubeBehaviour[] cubes;
    public BulletBehaviour[] Cube_Bullets;
    private static Vector3[] faces;

    // Start is called before the first frame update
    void Start()
    {
        cubes = FindObjectsOfType<CubeBehaviour>();

        faces = new Vector3[]
        {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back , Vector3.forward
        };
    }

    // Update is called once per frame
    void Update()
    {
        Cube_Bullets = FindObjectsOfType<BulletBehaviour>();

        // check each AABB with every other AABB in the scene
        for (int i = 0; i < cubes.Length; i++)
        {
            for (int j = 0; j < cubes.Length; j++)
            {
                if (i != j)
                {
                    CheckCubeAABB(cubes[i], cubes[j]);
                }
            }
        }

         // Check each bullet against each AABB in the scene
        foreach (var bullets in Cube_Bullets)
        {
            foreach (var cube in cubes)
            {
                if (cube.name != "Player")
                {
                    //CheckSphereAABB(sphere, cube);
                
                    CheckBulletAABB(bullets, cube);
                }
                
            }
        }
    }

   public static void CheckBulletAABB(BulletBehaviour a, CubeBehaviour b)
    {
        // get box closest point to cube center by clamping
        var x = Mathf.Max(b.min.x, Mathf.Min(a.transform.position.x, b.max.x));
        var y = Mathf.Max(b.min.y, Mathf.Min(a.transform.position.y, b.max.y));
        var z = Mathf.Max(b.min.z, Mathf.Min(a.transform.position.z, b.max.z));

        var distance = Math.Sqrt((x - a.transform.position.x) * (x - a.transform.position.x) +
                                 (y - a.transform.position.y) * (y - a.transform.position.y) +
                                 (z - a.transform.position.z) * (z - a.transform.position.z));

        if ((distance < a.radius) && (!a.isColliding))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - a.transform.position.x),
                (a.transform.position.x - b.min.x),
                (b.max.y - a.transform.position.y),
                (a.transform.position.y - b.min.y),
                (b.max.z - a.transform.position.z),
                (a.transform.position.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }

            a.penetration = penetration;
            a.collisionNormal = face;
            
            //a.isColliding = true;

            Reflect(a);
        }

    }
    
    // This helper function reflects the bullet when it hits an AABB face
    private static void Reflect(BulletBehaviour a)
    {
        if ((a.collisionNormal == Vector3.forward) || (a.collisionNormal == Vector3.back))
        {
           a.direction = new Vector3(a.direction.x, a.direction.y, -a.direction.z);
        }
        else if ((a.collisionNormal == Vector3.right) || (a.collisionNormal == Vector3.left))
        {
            a.direction = new Vector3(-a.direction.x, a.direction.y, a.direction.z);
        }
        else if ((a.collisionNormal == Vector3.up) || (a.collisionNormal == Vector3.down))
        {
            a.direction = new Vector3(a.direction.x, -a.direction.y, a.direction.z);
        }
    }


    public static void CheckCubeAABB(CubeBehaviour a, CubeBehaviour b)
    {
        Contact contactB = new Contact(b);

        if ((a.min.x <= b.max.x && a.max.x >= b.min.x) &&
            (a.min.y <= b.max.y && a.max.y >= b.min.y) &&
            (a.min.z <= b.max.z && a.max.z >= b.min.z))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - a.min.x),
                (a.max.x - b.min.x),
                (b.max.y - a.min.y),
                (a.max.y - b.min.y),
                (b.max.z - a.min.z),
                (a.max.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }
            
            // set the contact properties
            contactB.face = face;
            contactB.penetration = penetration;

            b.penetration = penetration;
            b.collisionNormal = face;

            // check if contact does not exist
            if (!a.contacts.Contains(contactB))
            {
                // remove any contact that matches the name but not other parameters
                for (int i = a.contacts.Count - 1; i > -1; i--)
                {
                    if (a.contacts[i].cube.name.Equals(contactB.cube.name))
                    {
                        a.contacts.RemoveAt(i);
                    }
                }

                if (contactB.face == Vector3.down)
                {
                    a.gameObject.GetComponent<RigidBody3D>().Stop();
                    a.isGrounded = true;
                }

                Push(a, b);

                // add the new contact
                a.contacts.Add(contactB);
                a.isColliding = true; 
                
            }
        }
        else
        {

            if (a.contacts.Exists(x => x.cube.gameObject.name == b.gameObject.name))
            {
                a.contacts.Remove(a.contacts.Find(x => x.cube.gameObject.name.Equals(b.gameObject.name)));
                a.isColliding = false;

                if (a.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
                {
                    a.gameObject.GetComponent<RigidBody3D>().isFalling = true;
                    a.isGrounded = false;
                }
            }
        }
    }

    public static void Push(CubeBehaviour a, CubeBehaviour b)
    {
        if (a.name == "Player" && b.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
        {
            if (b.collisionNormal == Vector3.left)
            {
                b.transform.position += Vector3.left;
            }
            if (b.collisionNormal == Vector3.right)
            {
                b.transform.position += Vector3.right;
            }
            if(b.collisionNormal == Vector3.forward)
            {
                b.transform.position += Vector3.forward;
            } 
            if(b.collisionNormal == Vector3.back)
            {
                b.transform.position += Vector3.back;
            }       
      
        }
    }
}
