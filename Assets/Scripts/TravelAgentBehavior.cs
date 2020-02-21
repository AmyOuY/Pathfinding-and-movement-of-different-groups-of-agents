using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;


public class TravelAgentBehavior : MonoBehaviour {

    internal class TravelAgent
    {
        //travel agent's destination
        public Vector2 destination = new Vector2();
        public float timer = 0f;
        public float extendedTime = 5.0f;
        //travel agent's speed
        public float maxSpeed;
        public float maxAvoidForce = 15.0f;
        public float socialDistance = 5f;

    }


    private TravelAgent tAgent = new TravelAgent();
    public static List<GameObject> travelAgents = new List<GameObject>();
    public static int tAgentCounter;
    private Rigidbody tRb;
    private Vector2 position;
    private Vector2 velocity;
    //ahead vector decides how far the tAgent can see, the vector is in the direction of tAgent's velocity
    //ahead2 is half length of the ahead vector
    private Vector2 ahead;
    private Vector2 ahead2;
    public static int count =0;


    // Use this for initialization
    void Start () {
        tRb = GetComponent<Rigidbody>();
        //random speed for each travel agent
        tAgent.maxSpeed = Random.Range(15.0f, 20.0f);

        //randomly select the destination
        int r = Random.Range(1, 3);
        if (r == 1)
        {
            GameObject door = GameObject.Find("TopLeftDoor");
            tAgent.destination.x = door.transform.position.x;
            tAgent.destination.y = door.transform.position.y;
        }
        else
        {
            GameObject door = GameObject.Find("BottomLeftDoor");
            tAgent.destination.x = door.transform.position.x;
            tAgent.destination.y = door.transform.position.y;           
        }
        
    }
	


	// Update is called once per frame
	void FixedUpdate () {
        //travel agent can change destination if he can not reach intended exit doorway for an extended time
        tAgent.timer += Time.deltaTime;
        if (tAgent.timer >= tAgent.extendedTime)
        {
            if (tAgent.destination.y == -20.0f)
            {
                tAgent.destination.y = 20.0f;
            }
            else if (tAgent.destination.y == 20.0f)
            {
                tAgent.destination.y = -20.0f;
            }

            tAgent.timer = 0f;
        }

        position = new Vector2(transform.position.x, transform.position.y);
        velocity = tAgent.destination - position;
        velocity.Normalize();
        //the faster the agent moves, the lower magnitude of ahead and ahead2 needed to take a quicker avoidance
        float magnitude = velocity.SqrMagnitude() / tAgent.maxSpeed;
        ahead = position + velocity * magnitude;
        ahead2 = position + velocity * magnitude * 0.5f;
        //update velocity by applying collision avoidance force
        velocity = velocity * tAgent.maxSpeed + CollisionAvoidance();
        tRb.velocity = new Vector3(velocity.x, velocity.y, 0);
    }



    //check whether distance of obstacle center to the end of ahead or ahead2 vector is smaller than tAgent radius plus obstacle radius
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle(Vector2 ah, Vector2 ah2, ObstacleClass obstacle)
    {
        float avoidDist = obstacle.radius + transform.localScale.x + 2.0f;
        float d1 = Vector2.Distance(obstacle.center, ah);
        float d2 = Vector2.Distance(obstacle.center, ah2);
        return (d1 <= avoidDist) || (d2 <= avoidDist);
    }



    //check whether distance of other agent's center to the end of ahead or ahead2 vector is smaller than the radius of other agent plus tAgent's
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle1(Vector2 ah, Vector2 ah2, GameObject agent)
    {
        //avoidance distance is the two agents' radius plus tAgent's social distance to allow some distance between them
        float avoidDist = (agent.transform.localScale.x / 2.0f) + (transform.localScale.x / 2.0f) + tAgent.socialDistance;
        float d1 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah);
        float d2 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah2);
        return (d1 <= avoidDist)|| (d2 <= avoidDist);
    }





    //find the closest obstacle in front of the tAgent
    ObstacleClass ClosestObstacle()
    {
        ObstacleClass closestOB = null;

        //loop through each obstacle to check for collision avoidance with tAgent
        foreach (ObstacleClass ob in Polygon.obstacles.ToArray())
        {
            bool collision = LineIntersectsCircle(ahead, ahead2, ob);
            if (collision && (closestOB == null || Vector2.Distance(position, ob.center) < Vector2.Distance(position, closestOB.center)))
            {
                closestOB = ob;
            }
        }
        
        return closestOB;
    }



    //find the closest travel agent in front of the tAgent
    GameObject ClosestTravelAgent()
    {
        GameObject closestAG = null;

        foreach (GameObject agent in travelAgents.ToArray())
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            if (collision && (closestAG == null ||
                Vector2.Distance(position, new Vector2(agent.transform.position.x, agent.transform.position.y))
                < Vector2.Distance(position, new Vector2(closestAG.transform.position.x, closestAG.transform.position.y))))
            {
                if (agent != gameObject)
                {
                    closestAG = agent;
                }

            }
        }
        return closestAG;
    }



    //find the closest social agent in front of the tAgent
    GameObject ClosestSocialAgent()
    {
        GameObject closestAG = null;
        //SocialAgentBehavior closestAG = null;

        foreach (GameObject agent in SocialAgentBehavior.socialAgents.ToArray())
        
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            
            if (collision && (closestAG == null ||
                Vector2.Distance(position, new Vector2(agent.transform.position.x, agent.transform.position.y))
                < Vector2.Distance(position, new Vector2(closestAG.transform.position.x, closestAG.transform.position.y))))
            {
                if (agent != gameObject)
                {
                    closestAG = agent;
                }

            }
        }
        return closestAG;
    }



    //calculate steer force to avoid the closest obstacle and agent
    Vector2 CollisionAvoidance()
    {
        ObstacleClass ob = ClosestObstacle();
        GameObject tAgent2 = ClosestTravelAgent();
        GameObject sAgent = ClosestSocialAgent();
        
        Vector2 avoidance = new Vector2(0, 0);
        
        //privilege is to avoid the obstacle if both obstacle and agent are needed to avoid
        if (ob != null)
        {
            //avoidance force is in the direction of ahead minus obstacle center
            avoidance = new Vector2(ahead.x - ob.center.x, ahead.y - ob.center.y);
            avoidance.Normalize();
            
            //avoidance force is scaled by maxAvoidForce
            avoidance *= tAgent.maxAvoidForce;

            //combine avoidance force from the agent if it exists
            if (tAgent2 != null)
            {
                //avoidance force is in the direction of ahead minus agent center
                Vector2 avoidance2 = new Vector2(ahead.x - tAgent2.transform.position.x, ahead.y - tAgent2.transform.position.y);
                avoidance2.Normalize();
                //avoidance force is scaled by maxAvoidForce/2
                avoidance2 *= tAgent.maxAvoidForce / 5.0f;

                //accumulate all the avoidance force from the obstacle
                avoidance += avoidance2;              
            }

            //combine avoidance force from the social agent if it exists
            if (sAgent != null)
            {
                //avoidance force is in the direction of ahead minus agent center
                Vector2 avoidance3 = new Vector2(ahead.x - sAgent.transform.position.x, ahead.y - sAgent.transform.position.y);
                avoidance3.Normalize();
                //avoidance force is scaled by maxAvoidForce/5
                avoidance3 *= tAgent.maxAvoidForce / 5.0f;

                //accumulate all the avoidance force from the obstacle
                avoidance += avoidance3;

            }

        }//if no obstacle is needed to avoid
        else
        {
            //if there is travel agent to avoid
            if (tAgent2 != null)
            {
                Vector2 avoidance1 = new Vector2(ahead.x - tAgent2.transform.position.x, ahead.y - tAgent2.transform.position.y);               
                avoidance1.Normalize();
                avoidance1 *= tAgent.maxAvoidForce;
                avoidance += avoidance1;
            }
            //if there is social agent to avoid
            if (sAgent != null)
            {
                Vector2 avoidance2 = new Vector2(ahead.x - sAgent.transform.position.x, ahead.y - sAgent.transform.position.y);
                avoidance2.Normalize();
                avoidance2 *= tAgent.maxAvoidForce;
                avoidance += avoidance2;
            }

        }

        return avoidance;
    }


    //destroy travel agent when he reaches the destination
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "door")
        {
            travelAgents.Remove(gameObject);
            Destroy(gameObject);
            tAgentCounter--;
            count++;
        }
    }


}
