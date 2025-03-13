using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;

public class EnemyAgent : Agent
{
    // SerializeField are inputs accessible within Unity via the Inspector for the 
    // Script Component.
    
    // What the Agent will chase.
    [SerializeField] private Transform _goal;
    // Where the goal will spawn on.
    [SerializeField] private GameObject _ground;
    // How the fast the agent will move and turn.
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    // Can make the Agent change color.
    private Renderer _renderer;

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;

    // Set for variables to render, track episodes, and rewards.
    public override void Initialize()
    {
        Debug.Log("Initialize()");
        _renderer = GetComponent<Renderer>();
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }

    // Setup for each episode and place goal and agent.
    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");
        _currentEpisode++;
        _cumulativeReward = 0f;
        _renderer.material.color = Color.blue;

        SpawnObjects();
    }

    //  Randomly places the agent in a fixed place and the goal randomly on the ground.
    private void SpawnObjects()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.62f, 0f);

        Bounds bounds = _ground.GetComponent<Collider>().bounds;
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        float randomDistance = UnityEngine.Random.Range(1f, 9f);
        
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;

        _goal.localPosition = new Vector3(goalPosition.x, 0.62f, goalPosition.z);
        Debug.LogFormat("goalPos: {0}",_goal.localPosition);
    }

    // [REMOVED] Return the minimum normalized distance of the closest wall.
    private float minWallProximity()
    {
        Collider[] proximity = new Collider[4];
        int radius = 5;
        float minNormDistance = 1;
        Physics.OverlapSphereNonAlloc(transform.position, radius, proximity);

        for (int i = 0; i < proximity.Length; i++)
        {
            if (proximity[i] == null || !proximity[i].gameObject.CompareTag("wall"))
            {
                continue;
            }

            Vector3 closestPoint = proximity[i].ClosestPoint(transform.position);
            float normDistance = Vector3.Distance(transform.position, proximity[i].transform.position) / radius;
            minNormDistance = Math.Min(minNormDistance, normDistance);
        }
        if (minNormDistance < 1)
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
        return minNormDistance;
    }

    // Collect the positions of the goal, and the agent orientation.
    public override void CollectObservations(VectorSensor sensor)
    {
        Bounds bounds = _ground.GetComponent<Collider>().bounds;
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;

        float enemyPosX_normalized = transform.localPosition.x / 5f;
        float enemyPosZ_normalized = transform.localPosition.z / 5f;
        float enemyRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        //float wallProximity_normalized = minWallProximity();rvation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(enemyPosX_normalized);
        sensor.AddObservation(enemyPosZ_normalized);
        sensor.AddObservation(enemyRotation_normalized);
        //sensor.AddObservation(wallProximity_normalized);
    }

    // Move the agent and reward it for taking the action.
    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions);

        AddReward(-2f / MaxStep);

        _cumulativeReward = GetCumulativeReward();

    }

    // Move the agent based on its orientation and movement speed.
    public void MoveAgent(ActionSegment<int> act)
    {
        // From the sampled action from the ActionSegement will be numbers 1 to 3.
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

    // Set Goal Trigger when goal is touched.
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("target"))
        {
            Debug.Log("Goal Reached");
            GoalReached();
        }
    }
    
    // Reward the agent for reaching the goal and end the episode for the next on.
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
    
    // Penalize the agent and color it red for touching the walls.
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
    
    // Penalize the agent for sustained contact with walls.
    private void OnCollisionStay(Collision collision)
    {
        if ( collision.gameObject.CompareTag("wall"))
        {
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }
    
    // Return the agent to blue when it stops touching walls.
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

