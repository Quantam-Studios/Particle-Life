using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleSimulator : MonoBehaviour
{
    [Header("General")]
    public List<ParticleObject> particleObjects = new List<ParticleObject>();
    public GameObject[] particlePrefabs;

    [Header("Settings")]
    [Header("General")]
    public int initialParticleAmount;
    public float friction;
    public float minEffectRadius;
    public float maxEffectRadius;
    public float minPull;
    public float maxPull;

    [Header("Boundaries")]
    public float minX;
    public float minY;
    public float maxX;
    public float maxY;

    [Header("Specific Interactions")]
    //public List<float[]> interactionMatrix = new List<float[]>();
    public InteractionLayout interactionMatrix;

    // Start is called before the first frame update
    void Start()
    {
        int column = 0;
        foreach (var currentParticle in particlePrefabs)
        {
            Particle properties = currentParticle.GetComponent<ParticleObject>().properties;
            properties.column = column;
            properties.effectRadius = Random.Range(minEffectRadius, maxEffectRadius);

            properties.row = new int[particlePrefabs.Length];
                
            for (int row = 0; row < properties.row.Length; row++)
            {
                currentParticle.GetComponent<ParticleObject>().properties.row[row] = row;
            }

            column++;
        }

        interactionMatrix = GenerateInteractionMatrix();
        
        CreateInitialParticles(initialParticleAmount);
    }

    // Update is called once per frame
    void Update()
    {
        // Loop through all particles
        foreach (var currentParticle in particleObjects)
        {
            float x = 0f;
            float y = 0f;

            // Determine what forces are applied to this 
            foreach (var particle in particleObjects)
            {
                // If the object is itself or if the particles are out of range of eachother continue
                if (particle == gameObject)
                {
                    continue;
                }
                else if (Vector2.Distance(particle.transform.position, currentParticle.transform.position) > particle.properties.effectRadius)
                {
                    continue;
                }

                // Get forces acting on the current particle 
                float[] components = GetPullComponents(particle, currentParticle);
                x += components[0];
                y += components[1];
            }

            // Periodic boundary conditions to ensure particles stay in the desired area.
            if (currentParticle.transform.position.y < minY)
                currentParticle.transform.position = new Vector2(currentParticle.transform.position.x, maxY);

            if (currentParticle.transform.position.y > maxY)
                currentParticle.transform.position = new Vector2(currentParticle.transform.position.x, minY);

            if (currentParticle.transform.position.x < minX)
                currentParticle.transform.position = new Vector2(maxX, currentParticle.transform.position.y);

            if (currentParticle.transform.position.x > maxX)
                currentParticle.transform.position = new Vector2(minX, currentParticle.transform.position.y);

            // If there are effectively no changes than continue
            if (x == 0 && y == 0)
            {
                continue;
            }

            // Apply forces
            currentParticle.GetComponent<Rigidbody2D>().SetRotation(Mathf.Atan2(y, x) * Mathf.Rad2Deg);
            if (GetFinalPull(x, y) > friction)
                currentParticle.GetComponent<Rigidbody2D>().velocity = currentParticle.transform.up * GetFinalVeclocity(x, y, Mathf.Atan2(y, x) * Mathf.Rad2Deg);
            else
                currentParticle.GetComponent<Rigidbody2D>().velocity = currentParticle.transform.up * 0;
        }
    }

    void CreateInitialParticles(int amount)
    {
        for (int particles = 0; particles < amount; particles++)
        {
            Vector3 spawnPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
            GameObject particleType = particlePrefabs[Random.Range(0, particlePrefabs.Length)];
            
            GameObject particle = Instantiate(particleType, spawnPos, Quaternion.identity);

            particleObjects.Add(particle.GetComponent<ParticleObject>());
        }
    }

    float[] GetPullComponents(ParticleObject otherParticle, ParticleObject particle)
    {
        float distanceA = interactionMatrix.rows[otherParticle.properties.column].row[otherParticle.properties.row[particle.properties.column]] / Vector2.Distance(particle.transform.position, otherParticle.transform.position);
        if (float.IsNaN(distanceA) || float.IsInfinity(distanceA))
            distanceA = otherParticle.properties.pull;

        float angleB = Mathf.Atan2(particle.transform.position.y - otherParticle.transform.position.y, particle.transform.position.x - otherParticle.transform.position.x) * Mathf.Rad2Deg;

        // y component
        // distanceA / sin(90) = distanceB / sin(angleB)
        float distanceB = distanceA * Mathf.Sin(angleB * Mathf.Deg2Rad);

        // x component
        // distanceA / sin(90) = distanceC / (180 - 90 - angleB)
        float distanceC = distanceA * Mathf.Sin((90f - angleB) * Mathf.Deg2Rad);

        float[] components = { distanceC, distanceB };

        return components;
    }

    float GetFinalVeclocity(float x, float y, float angle)
    {
        // friction y component
        float frictionY = friction * Mathf.Sin(angle * Mathf.Deg2Rad);

        // friction x component
        float frictionX = friction * Mathf.Sin((90f - angle) * Mathf.Deg2Rad);

        return Mathf.Sqrt(Mathf.Pow(x - frictionX, 2) + Mathf.Pow(y - frictionY, 2));
    }

    float GetFinalPull(float x, float y)
    {
        return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
    }

/*    List<float[]> GenerateInteractionMatrix()
    {
        List<float[]> matrix = new List<float[]>();
        foreach (var particleType in particlePrefabs)
        {
            float[] interactions = new float[particlePrefabs.Length];

            for (int i = 0; i < interactions.Length; i++)
            {
                interactions[i] = Random.Range(minPull, maxPull);
            }

            matrix.Add(interactions);
        }
        Debug.Log($"Columns {matrix.Count}, Rows: {matrix[0].Length}");
        return matrix;
    }*/

    InteractionLayout GenerateInteractionMatrix()
    {
        InteractionLayout matrix = new InteractionLayout();

        int index = 0;
        foreach (var particleType in particlePrefabs)
        {
            float[] interactions = new float[particlePrefabs.Length];

            for (int i = 0; i < interactions.Length; i++)
            {
                interactions[i] = Random.Range(minPull, maxPull);
            }

            matrix.rows[index].row = (interactions);
            index++;
        }

        return matrix;
    }
}