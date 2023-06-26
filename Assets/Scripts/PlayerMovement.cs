using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class PlayerMovement : Agent
{
    public float moveSpeed = 5f;
    public int maxSteps = 200;
    int steps = 0;

    public Transform movePoint;
    public Transform goal;
    public LayerMask whatStopsMovement;
    public LayerMask portalLayer;
    public TMPro.TextMeshProUGUI UIText;


    // Update is called once per frame
    void Update()
    {
        if (steps >= maxSteps)
        {
            //Debug.Log("End Espisode: Reached max steps.");
            UIText.text = "Failed :(";
            EndEpisode();
        }

        // Define when to ask for an action
        if ((Vector3.Distance(transform.position, movePoint.position) <= 0.05f))
        {
            RequestDecision();
        }

        //// Move inmediatly
        //else
        //{
        //    transform.position = movePoint.position;
        //}

        // Constantly move player towards moving point
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
    }

    public override void OnEpisodeBegin()
    {
        movePoint.parent = null;
        steps = 0;
        transform.position = new Vector3(0f, -0.1f, 0f);
        movePoint.position = new Vector3(0f, 0f, 0f);
        goal.position = new Vector3(9f, 8f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(movePoint.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        int movement = actions.DiscreteActions[0];
        
        // Penalize at actions
        AddReward(-0.1f);
        steps+=1;
        
        // Check if player position is close to move point
        if ((Vector3.Distance(transform.position, movePoint.position) <= .05f))
        {
            if (movement==0) //UP
            {
                if (!Physics2D.OverlapCircle(movePoint.position +  new Vector3(0f, 1f, 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(0f, 1f, 0f);
                }
            }

            else if (movement==1) //RIGHT
            {
                if (!Physics2D.OverlapCircle(movePoint.position +  new Vector3(1f, 0f, 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(1f, 0f, 0f);
                }
            }

            else if (movement==2) //DOWN
            {
                if (!Physics2D.OverlapCircle(movePoint.position +  new Vector3(0f, -1f, 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(0f, -1f, 0f);
                }
            }

            else if (movement==3) //LEFT
            {
                if (!Physics2D.OverlapCircle(movePoint.position +  new Vector3(-1f, 0f, 0f), .2f, whatStopsMovement))
                {
                    movePoint.position += new Vector3(-1f, 0f, 0f);
                }
            }
        }

        // Move if Portal
        if (Physics2D.OverlapCircle(movePoint.position , .2f, portalLayer))
        {
            transform.position = new Vector3(11f, 6f, 0f);
            movePoint.position = new Vector3(11f, 6f, 0f);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetAxisRaw("Vertical")==1) //UP
        {
            discreteActions[0] = 0;
        }

        else if (Input.GetAxisRaw("Horizontal")==1) //RIGHT
        {
            discreteActions[0] = 1;
        }

        else if (Input.GetAxisRaw("Vertical")==-1) //DOWN
        {
            discreteActions[0] = 2;
        }

        else if (Input.GetAxisRaw("Horizontal")==-1) //LEFT
        {
            discreteActions[0] = 3;
        }

        else
        {
            discreteActions[0] = 4; // Do nothing. Only for heuristic.
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            AddReward(+2.3f);
            UIText.text = "Nice!";
            EndEpisode();
        }

        if (other.TryGetComponent<Portal>(out Portal portal))
        {
            transform.position = new Vector3(11f, 6f, 0f);
            movePoint.position = new Vector3(11f, 6f, 0f);
        }
    }
}
