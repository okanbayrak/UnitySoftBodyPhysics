using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    [Range(1f, 50f)]
    public float springForce = 20f;
    [Range(1f, 20f)]
    public float damping = 5f;
    [Range(0.2f, 10f)]
    public float stiffness = 1;
    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices, vertexVelocities;
    float uniformScale = 1f;
    private void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
    }
    private void Update()
    {
        uniformScale = transform.localScale.x;
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    private void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
    }

    private void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
        float attenuatedForce;
        attenuatedForce = force / (1 + pointToVertex.sqrMagnitude) / stiffness;
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        AddDeformingForce(collision.contacts[0].point, rb.mass * rb.maxAngularVelocity * 10);
    }
}
