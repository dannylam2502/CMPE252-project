using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class MoveToTargetAgent :  Agent
{
    [SerializeField] private Transform target;
    private float distanceTraveled = 0;
    private int wallContact = 0;
    //[SerializeField] private gameObject backgroundSpriteRender;
    private void EpisodeStats()
    {
        Debug.LogFormat("distanceTraveled :{0}\nwallContact :{1}\n", distanceTraveled, wallContact);
    }
    public override void OnEpisodeBegin()
    {
        distanceTraveled = 0;
        wallContact = 0;
        Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
        planeRenderer.material.SetColor("_Color", Color.gray);
        //Vector3(x,0,z), y=0 so it player on target stay on the ground.
        transform.position = new Vector3(Random.Range(-3.5f, -1.5f),0.6f,Random.Range(-3.5f, 3.5f));
        target.position = new Vector3(Random.Range(1.5f, 3.5f),0.6f,Random.Range(-3.5f, 3.5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.position);
        sensor.AddObservation((Vector2)target.position);
        // Add a distance punishement.
        float targetDistance = Vector2.Distance((Vector2)target.position, (Vector2)transform.position);
        sensor.AddObservation(targetDistance);
        sensor.AddObservation(distanceTraveled);
        //int episode_steps = Academy.Instance.StepCount;
        //sensor.AddObservation(episode_steps);
        float targetDistanceReward = 1 / (targetDistance);
        AddReward(targetDistanceReward);

        float travelPunishment = 0;
        if (distanceTraveled == 0)
        {
            travelPunishment = 0;
        }
        else
        {
            travelPunishment = System.MathF.Log(distanceTraveled);
        }
        AddReward(-travelPunishment);

        //float punishment = System.MathF.Log(dist);
        //sensor.AddObservation(punishment);
        //AddReward(-punishment);

   
    }
    public override void OnActionReceived(ActionBuffers actions) {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        float movementSpeed = 20f;

        //Vector3(x,0,z), y=0 so it player's movement stays on the ground.
        Vector3 move = new Vector3(moveX,0, moveY) * Time.deltaTime * movementSpeed;
        distanceTraveled += Vector3.Distance(move,transform.localPosition);
        transform.localPosition += move;
        //transform.localPosition += new Vector3(moveX,0, moveY) * Time.deltaTime * movementSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
    //The collision isn't working, need to find a fix.
    private void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("hi");

        if (collider.gameObject.tag == "target") {
            AddReward(1000f);
            //backgroundSpriteRender.color = Color.green;

            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.green);
            //.color = Color.green;
            EpisodeStats();
            EndEpisode();
        }
        else if (collider.gameObject.tag == "wall")
        {
            AddReward(-200f);
            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.red);
            wallContact += 1;
            EndEpisode();
        }

    }
   
}

