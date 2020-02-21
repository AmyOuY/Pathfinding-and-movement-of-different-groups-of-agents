using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;


public class WanderAgentBehavior : MonoBehaviour {

    internal class WanderAgent
    {
        public Vector2 destination = new Vector2();
        public float maxSpeed;
        public float maxAvoidForce = 20.0f;
        public float socialDistance = 5.0f;
    }

    private WanderAgent wAgent = new WanderAgent();
    public static List<GameObject> wanderAgents = new List<GameObject>();
    private Rigidbody wRb;
    private float nextActionTime = 0.0f;
    public float interval = 3.0f;
    //wAgent's current position ans velocity
    private Vector2 position;
    private Vector2 velocity;
    //ahead vector decides how far the wAgent can see, the vector is in the direction of wAgent's velocity
    //ahead2 is half length of the ahead vector
    private Vector2 ahead;
    private Vector2 ahead2;




    // Use this for initialization
    void Start () {
        wRb = GetComponent<Rigidbody>();
        wAgent.maxSpeed = Random.Range(15.0f, 20.0f);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Interfere();

        if (Time.time > nextActionTime)
        {
            //randomly select a destionation for wAgent
            nextActionTime += interval;
            float x = Random.Range(-70.0f, 70.0f);
            float y = Random.Range(-40.0f, 40.0f);
            wAgent.destination = new Vector2(x, y);
        }



        position = new Vector2(transform.position.x, transform.position.y);
        velocity = wAgent.destination - position;
        velocity.Normalize();
        //the faster the agent moves, the lower magnitude of ahead and ahead2 needed to take a quicker avoidance
        float magnitude = velocity.SqrMagnitude() / wAgent.maxSpeed;
        ahead = position + velocity * magnitude;
        ahead2 = position + velocity * magnitude * 0.5f;
        //update velocity by applying collision avoidance force
        velocity = Vector2.ClampMagnitude(velocity * wAgent.maxSpeed + CollisionAvoidance(), wAgent.maxSpeed);
        wRb.velocity = new Vector3(velocity.x, velocity.y, 0);
    }


    //check whether distance of obstacle center to the end of ahead or ahead2 vector is smaller than wAgent radius plus obstacle radius
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle(Vector2 ah, Vector2 ah2, ObstacleClass obstacle)
    {
        float avoidDist = obstacle.radius + transform.localScale.x + 2.0f;
        float d1 = Vector2.Distance(obstacle.center, ah);
        float d2 = Vector2.Distance(obstacle.center, ah2);
        return (d1 <= avoidDist) || (d2 <= avoidDist);
    }



    //check whether distance of other agent's center to the end of ahead or ahead2 vector is smaller than the radius of other agent plus wAgent's
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle1(Vector2 ah, Vector2 ah2, GameObject agent)
    {
        //avoidance distance is the two agents' radius plus tAgent's social distance to allow some distance between them
        float avoidDist = (agent.transform.localScale.x / 2.0f) + (transform.localScale.x / 2.0f) + wAgent.socialDistance;
        float d1 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah);
        float d2 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah2);
        return (d1 <= avoidDist) || (d2 <= avoidDist);
    }




    //find the closest obstacle in front of the wander agent
    ObstacleClass ClosestObstacle()
    {
        ObstacleClass closestOB = null;

        //loop through each obstacle to check for collision avoidance with wAgent
        foreach (ObstacleClass ob in Polygon.obstacles.ToArray())
        {
            bool collision = LineIntersectsCircle(ahead, ahead2, ob);
            //look for closest obstacle to the wAgent
            if (collision && (closestOB == null || Vector2.Distance(position, ob.center) < Vector2.Distance(position, closestOB.center)))
            {
                closestOB = ob;
            }
        }

        return closestOB;
    }



    //find the closest wander agent in front of the wAgent
    GameObject ClosestWanderAgent()
    {
        GameObject closestAG = null;

        foreach (GameObject agent in wanderAgents.ToArray())
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            //position is current wAgent's current position
            //look for closest wander agent
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



    //find the closest travel agent in front of the wAgent
    GameObject ClosestTravelAgent()
    {
        GameObject closestAG = null;

        foreach (GameObject agent in TravelAgentBehavior.travelAgents.ToArray())
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            //position is current wAgent's current position
            //look for closest travel agent
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



    //find the closest social agent in front of the wAgent
    GameObject ClosestSocialAgent()
    
    {
        GameObject closestAG = null;
        //SocialAgentBehavior closestAG = null;

        foreach (GameObject agent in SocialAgentBehavior.socialAgents.ToArray())
        //foreach (SocialAgentBehavior agent in SocialAgentBehavior.socialAgents.ToArray())
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            
            //position is current wAgent's current position
            //look for closest wander agent
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
        GameObject wAgent2 = ClosestWanderAgent();
        GameObject sAgent = ClosestSocialAgent();
        //SocialAgentBehavior sAgent = ClosestSocialAgent();
        Vector2 avoidance = new Vector2(0, 0);

        //privilege is to avoid the obstacle if both obstacle and agent are needed to avoid
        if (ob != null)
        {
            //avoidance force is in the direction of ahead minus obstacle center
            avoidance = new Vector2(ahead.x - ob.center.x, ahead.y - ob.center.y);
            avoidance.Normalize();

            //avoidance force is scaled by maxAvoidForce
            avoidance *= wAgent.maxAvoidForce;

            //combine avoidance force from the wander agent if it exists
            if (wAgent2 != null)
            {
                //avoidance force is in the direction of ahead minus agent center
                Vector2 avoidance2 = new Vector2(ahead.x - wAgent2.transform.position.x, ahead.y - wAgent2.transform.position.y);
                avoidance2.Normalize();
                //avoidance force is scaled by maxAvoidForce/5
                avoidance2 *= wAgent.maxAvoidForce / 5.0f;

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
                avoidance3 *= wAgent.maxAvoidForce / 5.0f;

                //accumulate all the avoidance force from the obstacle
                avoidance += avoidance3;

            }

        }//if no obstacle is needed to avoid
        else
        {
            //if there is wander agent to avoid
            if (wAgent2 != null)
            {
                Vector2 avoidance1 = new Vector2(ahead.x - wAgent2.transform.position.x, ahead.y - wAgent2.transform.position.y);
                avoidance1.Normalize();
                avoidance1 *= wAgent.maxAvoidForce;
                avoidance += avoidance1;

            }
            //if there is social agent to avoid
            if (sAgent != null)
            {
                Vector2 avoidance2 = new Vector2(ahead.x - sAgent.transform.position.x, ahead.y - sAgent.transform.position.y);
                avoidance2.Normalize();
                avoidance2 *= wAgent.maxAvoidForce;
                avoidance += avoidance2;
            }

        }

        return avoidance;
    }



    //if closest agent is found, wAgent will attemp to interpose itself between the travel agent and its intended exit
    void Interfere()
    {
        Vector2 destination = wAgent.destination;
        GameObject agent = ClosestTravelAgent();
        
        if (agent != null)
        {
            //if closest travel agent is within 8f distance, wAgent will iterpose in between  
            if (Vector3.Distance(transform.position, agent.transform.position) <= 8.0f)
            {
                //find the travel agent's intended direction 
                Vector2 velocity = new Vector2(agent.GetComponent<Rigidbody>().velocity.x, agent.GetComponent<Rigidbody>().velocity.y);
                velocity.Normalize();
                //calculate travel agent's intended exit
                destination = new Vector2(agent.transform.position.x + velocity.x * 5.0f, agent.transform.position.y + velocity.y * 5.0f);
            }
        }
        //wAgent interposes in between travel agent's intended exit
        wAgent.destination = destination;
    }

}
