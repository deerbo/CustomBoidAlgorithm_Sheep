using UnityEngine;

// Ella Goodbinder
// IMDM390, Assignment 2

// Credits: making heavy use of this psuedocode: https://vergenet.net/~conrad/boids/pseudocode.html
public class NewBehaviourScript : MonoBehaviour
{
    private int numberOfBoids = 30;
    private GameObject[] boids;
    BodyProperty[] bp;
    public GameObject boid_prefab;
    public GameObject dog_prefab;

    private Animator dogAnimator;

    private Vector3 dog_pos = new Vector3(10, 0, 0);

    private GameObject dog;
    struct BodyProperty
    {
        public Vector3 velocity;
        public Vector3 position;
    }
    void Start()
    {
        dog = Instantiate(dog_prefab, new Vector3(10, 0, 0), Quaternion.identity);
        dogAnimator = dog.GetComponent<Animator>();
        

        //initalize objects and positions positions
        bp = new BodyProperty[numberOfBoids];
        boids = new GameObject[numberOfBoids];
        for (int i = 0; i < numberOfBoids; i++)
        {
            float r = Random.Range(10, numberOfBoids);
            float angle = i * Mathf.PI * 2 / numberOfBoids;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * r, 0, Mathf.Sin(angle) * r);
            GameObject b = Instantiate(boid_prefab, pos, Quaternion.identity);
            boids[i] = b;
            bp[i].position = pos;
            bp[i].velocity = new Vector3(0, 0, 0);
        }
    }
    void Update()
    {
        MoveDog(dog);

        for (int i = 0; i < numberOfBoids; i++)
        {
            // the three rules of boids
            Vector3 v1 = RuleOne(i);
            Vector3 v2 = RuleTwo(i);
            Vector3 v3 = RuleThree(i);

            float dt = Time.deltaTime;
            // calculate new velocity
            Vector3 v = v1 + v2 + v3;
            bound(i);

            if (v != Vector3.zero)
            {
                v = v.normalized * 0.1f; // keep change small per frame
            }           

            bp[i].velocity += v;

            LimitVelocity(i);
            ScaryDog(i);
            
            // update position both physically and in body property
            boids[i].transform.position += bp[i].velocity * dt;
            bp[i].position += bp[i].velocity * dt;

            boids[i].transform.rotation = Quaternion.LookRotation(bp[i].velocity);
        }
    }

    private Vector3 RuleOne(int j)
    {
        // calculate center of mass of all the boids
        Vector3 center = new Vector3(0, 0, 0);

        for (int i = 0; i < numberOfBoids; i++)
        {
            if (i != j)
            {
                center = center + bp[i].position;
            }
        }

        center = center / (numberOfBoids - 1);

        // move 1% of the way to the center of mass of all the boids
        return (center - bp[j].position) / 70;
    }

    // if the boids get too close to each other, redirect them away
    private Vector3 RuleTwo(int j)
    {
        Vector3 c = new Vector3(0, 0, 0);

        for (int i = 0; i < numberOfBoids; i++)
        {
            if (i != j)
            {
                if (Vector3.Distance(bp[i].position, bp[j].position) < 1f)
                {
                    c = c - (bp[i].position - bp[j].position);
                }
            }
        }

        return c;
    }

    private Vector3 RuleThree(int j)
    {
        Vector3 newVc = new Vector3(0, 0, 0);

        for (int i = 0; i < numberOfBoids; i++)
        {
            if (i != j)
            {
                newVc = newVc + bp[i].velocity;
            }
        }

        newVc = newVc / (numberOfBoids - 1);

        return (newVc - bp[j].velocity) / 8f; // add about an eigth to current velocity
    }

    private void bound(int i)
    {
        float boundLimit = 20f;     // boundary size
        float pullStrength = 0.3f; // how strongly to steer back toward center

        Vector3 pos = bp[i].position;
        Vector3 center = Vector3.zero;
        Vector3 toCenter = center - pos;

        // compute distance from center on the XZ plane
        float distance = new Vector2(pos.x, pos.z).magnitude;

        // only apply correction if outside limit
        if (distance > boundLimit)
        {
            // steer back toward center
            Vector3 correction = toCenter.normalized * (distance - boundLimit) * pullStrength;

            bp[i].velocity += correction;
        }
    }
    public void MoveDog(GameObject dog)
    {
        float speed = 10;
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 move = new Vector3(moveX, 0, moveZ) * speed * Time.deltaTime;
        dog.transform.position += move;
        // dog_pos += move;
        dog_pos = dog.transform.position;

        if (move != Vector3.zero)
        {
            dog.transform.rotation = Quaternion.Slerp(
                dog.transform.rotation,
                Quaternion.LookRotation(move),
                10f * Time.deltaTime
            );
        }

        if (dogAnimator != null)
        {
            float currentSpeed = new Vector3(moveX, 0, moveZ).magnitude;
            dogAnimator.SetFloat("Speed", currentSpeed);
        }
    }

    private void ScaryDog(int i)
    {
        float safeDistance = 5f;
        float repulsionStrength = 5f;

        Vector3 awayFromDog = bp[i].position - dog_pos;
        float distance = awayFromDog.magnitude;

        if (distance < safeDistance)
        {
            Vector3 repulsion = awayFromDog.normalized * (safeDistance - distance) * repulsionStrength;
            bp[i].velocity += repulsion;
        }
    }

    private void LimitVelocity(int i)
    {
        float limit = 3f;
        Vector3 v = bp[i].velocity;

        // If the magnitude is greater than the limit, clamp it
        if (v.magnitude > limit)
        {
            bp[i].velocity = v.normalized * limit;
        }
    }
}
