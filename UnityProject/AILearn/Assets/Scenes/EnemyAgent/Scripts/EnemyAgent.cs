using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;

public class EnemyAgent : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private GameObject _ground;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;
    //[SerializeField] private float _minRandomGoalLoc = -10f;
    //[SerializeField] private float _maxRandomGoalLoc = 9.00f;

    private Renderer _renderer;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    private Vector3 _orgPos = Vector3.zero; // The original pos of this gameobj

    public override void Initialize()
    {
        Debug.Log("Initialize()");
        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
        _orgPos = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");
        _currentEpisode++;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects();
    }

    private void SpawnObjects()
    {
        //transform.position = _orgPos;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.62f, 0f);

        Bounds bounds = _ground.GetComponent<Collider>().bounds;
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        //float randomDistance = UnityEngine.Random.Range(_minRandomGoalLoc, _maxRandomGoalLoc);
        float randomDistance = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
        
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

        //_goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }
    // Return the minimum normalized distance of the closest wall.
    private float minWallProximity()
    {
        Collider[] proximity = new Collider[4];
        int radius = 5;
        float minNormDistance = 1;
        Physics.OverlapSphereNonAlloc(transform.position, radius, proximity);
        //Collider[] proximity = Physics.OverlapSphere(transform.position, radius);

        for (int i = 0; i < proximity.Length; i++)
        {
            if (proximity[i] == null || !proximity[i].gameObject.CompareTag("wall"))
            {
                continue;
            }

            //Debug.LogFormat("tranform.position: {0}",transform.position);
            Vector3 closestPoint = proximity[i].ClosestPoint(transform.position);
            //Debug.LogFormat("closestPoint: {0}", proximity[i].transform.position);
            float normDistance = Vector3.Distance(transform.position, proximity[i].transform.position) / radius;
            //Debug.LogFormat("normDistance: {0}",normDistance);
            minNormDistance = Math.Min(minNormDistance, normDistance);
        }
        //Debug.LogFormat("minNormDistance: {0}",minNormDistance);
        if (minNormDistance < 1)
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
        return minNormDistance;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Debug.Log("CollectObservationos()");
        Bounds bounds = _ground.GetComponent<Collider>().bounds;
        float maxX = bounds.extents.x;
        float maxZ = bounds.extents.z;
        float goalPosX_normalized = _goal.localPosition.x / maxX;
        float goalPosZ_normalized = _goal.localPosition.z / maxZ;

        float enemyPosX_normalized = transform.localPosition.x / maxX;
        float enemyPosZ_normalized = transform.localPosition.z / maxZ;
        //Debug.LogFormat("goalX={0}, maxX={1}, goalPosX_normalized={2}", _goal.localPosition.x, maxX, goalPosX_normalized);
        float enemyRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        //float wallProximity_normalized = minWallProximity();
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(enemyPosX_normalized);
        sensor.AddObservation(enemyPosZ_normalized);
        sensor.AddObservation(enemyRotation_normalized);
        //sensor.AddObservation(wallProximity_normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        AddReward(-2f / MaxStep);

        _cumulativeReward = GetCumulativeReward();

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var action = act[0];
        switch (action)
        {
            case 1: //Forward
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: //Rotate left
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: //Rotate right
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent.CompareTag("target"))
        {
            Debug.LogError("Goal Reached");
            GoalReached();
        }
    }
    private void GoalReached()
    {
        AddReward(1.0f);
        _cumulativeReward = GetCumulativeReward();
        EndEpisode();
        // On Player Hit
        var component = _goal.gameObject.GetComponent<PlayerController>();
        if (component != null)
        {
            component.OnHitByEnemy(this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            AddReward(-0.05f);
            if (_renderer != null)
            {
                _renderer.material.color = Color.red;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if ( collision.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            if (_renderer != null)
            {
                _renderer.material.color = Color.blue;
            }
        }
    }
}

