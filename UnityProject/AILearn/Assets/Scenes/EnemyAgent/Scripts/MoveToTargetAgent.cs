using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class MoveToTargetAgent :  Agent
{
    [SerializeField] private Transform target;
    //[SerializeField] private gameObject backgroundSpriteRender;

    public override void OnEpisodeBegin()
    {
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
    }
    public override void OnActionReceived(ActionBuffers actions) {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        float movementSpeed = 5f;

        //Vector3(x,0,z), y=0 so it player's movement stays on the ground.
        transform.localPosition += new Vector3(moveX,0, moveY) * Time.deltaTime * movementSpeed;
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
        if (collision.gameObject.tag == "target") {
            AddReward(10f);
            //backgroundSpriteRender.color = Color.green;

            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.green);
                //.color = Color.green;
            EndEpisode();
        }
        else if (collision.gameObject.tag == "wall")
        {
            AddReward(-2f);
            Renderer planeRenderer = GameObject.Find("Plane").GetComponent<Renderer>();
            planeRenderer.material.SetColor("_Color", Color.red);
            EndEpisode();
        }

    }
   
}
