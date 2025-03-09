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
    private float distance = 0;
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
        transform.position = new Vector3(Random.Range(-10.5f, 16.5f),0.6f,Random.Range(-7.5f, 19.2f));
        target.position = new Vector3(Random.Range(-10.5f, 16.5f),0.6f,Random.Range(-7.5f, 19.2f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.position);
        //sensor.AddObservation((Vector2)target.position);
        // Add a distance punishement.
        float targetDistance = Vector2.Distance((Vector2)target.position, (Vector2)transform.position);
        sensor.AddObservation(targetDistance);
        sensor.AddObservation(distanceTraveled);

        distance = 0;
        //Debug.Log("hi");
        if (Physics.Raycast (transform.position, transform.TransformDirection (Vector3.forward), out RaycastHit hitinfo, 20f))
        {
            Debug.Log(hitinfo.collider.gameObject.tag);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitinfo.distance, Color.red);
            distance = (float)hitinfo.distance;
            if (hitinfo.collider.gameObject.tag.Equals("wall"))
            {
                    AddReward(distance);
                    sensor.AddObservation(0);
                    sensor.AddObservation(distance);

            }
            //else
            //{
            //        AddReward(-distance);
            //        sensor.AddObservation(distance);
            //        sensor.AddObservation(0);

            //}
        }
        else
        {
            sensor.AddObservation(0);
            sensor.AddObservation(0);
        }
        //sensor.AddObservation(episode_steps);
        //float targetDistanceReward = -1 / (targetDistance);
        float targetDistanceReward = -targetDistance;
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
        Vector3 move = new Vector3(moveX, 0, moveY) * Time.deltaTime * movementSpeed;
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

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("hi");

        if (collision.gameObject.tag == "target")
        {
            AddReward(1000f);
            //backgroundSpriteRender.color = Color.green;

            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.green);
            //.color = Color.green;
            EpisodeStats();
            EndEpisode();
        }
        else if (collision.gameObject.tag == "wall")
        {
            AddReward(-200f);
            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.red);
            wallContact += 1;
            EndEpisode();
        }
    }
}

