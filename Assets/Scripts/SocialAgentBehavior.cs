using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;


public class SocialAgentBehavior : MonoBehaviour {

    public class SocialAgent
    {
        public Vector2 destination = new Vector2();
        public float timer = 0f;
        public float timer2 = 0f;
        public float extendedTime = 1.0f;
        public float maxSpeed;
        public float maxAvoidForce = 20.0f;
        public float socialDistance = 5.0f;
        //bool readyToJoin = false;
    }


    private SocialAgent sAgent = new SocialAgent();
    public static List<GameObject> socialAgents = new List<GameObject>();
    //public static List<SocialAgentBehavior> socialAgents = new List<SocialAgentBehavior>();
    private Rigidbody sRb;
    private float nextActionTime = 0.0f;
    public float interval = 7.0f;
    //sAgent's current position ans velocity
    private Vector2 position;
    private Vector2 velocity;
    //ahead vector decides how far the sAgent can see, the vector is in the direction of sAgent's velocity
    //ahead2 is half length of the ahead vector
    private Vector2 ahead;
    private Vector2 ahead2;
    public bool join = false;
    public List<GameObject> socialGroup = new List<GameObject>();
    //public List<SocialAgentBehavior> socialGroup = new List<SocialAgentBehavior>();
    
    public float cooldown = 10.0f;



    // Use this for initialization
    void Start () {
        sRb = GetComponent<Rigidbody>();
        sAgent.maxSpeed = Random.Range(15.0f, 20.0f);
        
    }
	


	// Update is called once per frame
	void FixedUpdate () {
       
        sAgent.timer += Time.deltaTime;

        Conversation();
        
        if (sAgent.timer >= sAgent.extendedTime && join == true)
        {
           
            LeaveSocialGroup();
            sAgent.timer2 += Time.deltaTime;
            sAgent.timer = 0;
            
        }
        if (sAgent.timer2 > cooldown) {
            //Debug.Log("                timer gone                     ");
            sAgent.timer2 = 0;
        }

        //Debug.Log(socialGroup.Count);
        //Conversation();
        
        if (Time.time > nextActionTime)
        {

            //randomly select a destionation for wagent
            nextActionTime += interval;
            float x = Random.Range(-70.0f, 70.0f);
            float y = Random.Range(-40.0f, 40.0f);
            sAgent.destination = new Vector2(x, y);
        }
        if (join == false)
        {
            position = new Vector2(transform.position.x, transform.position.y);
            velocity = sAgent.destination - position;
            velocity.Normalize();
            //the faster the agent moves, the lower magnitude of ahead and ahead2 needed to take a quicker avoidance
            float magnitude = velocity.SqrMagnitude() / sAgent.maxSpeed;
            ahead = position + velocity * magnitude;
            ahead2 = position + velocity * magnitude * 0.5f;
            //update velocity by applying collision avoidance force
            velocity = Vector2.ClampMagnitude(velocity * sAgent.maxSpeed + CollisionAvoidance(), sAgent.maxSpeed);
            sRb.velocity = new Vector3(velocity.x, velocity.y, 0);
        }
        else //not move if in a social conversation
        {
            sRb.velocity = new Vector3(0, 0, 0);
            sRb.angularVelocity = new Vector3(0, 0, 0);
        }
    }



    


    //check whether distance of obstacle center to the end of ahead or ahead2 vector is smaller than sAgent radius plus obstacle radius
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle(Vector2 ah, Vector2 ah2, ObstacleClass obstacle)
    {
        float avoidDist = obstacle.radius + transform.localScale.x + 2.0f;
        float d1 = Vector2.Distance(obstacle.center, ah);
        float d2 = Vector2.Distance(obstacle.center, ah2);
        return (d1 <= avoidDist) || (d2 <= avoidDist);
    }



    //check whether distance of other agent's center to the end of ahead or ahead2 vector is smaller than the radius of other agent plus sAgent's
    //if smaller then apply collision avoidance
    bool LineIntersectsCircle1(Vector2 ah, Vector2 ah2, GameObject agent)
    {
        //avoidance distance is the two agents' radius plus tAgent's social distance to allow some distance between them
        float avoidDist = (agent.transform.localScale.x / 2.0f) + (transform.localScale.x / 2.0f) + sAgent.socialDistance;
        float d1 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah);
        float d2 = Vector2.Distance(new Vector2(agent.transform.position.x, agent.transform.position.y), ah2);
        return (d1 <= avoidDist) || (d2 <= avoidDist);
    }



    //find the closest obstacle in front of the social agent
    ObstacleClass ClosestObstacle()
    {
        ObstacleClass closestOB = null;

        //loop through each obstacle to check for collision avoidance with sAgent
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



    //find the closest wander agent in front of the sAgent
    GameObject ClosestWanderAgent()
    {
        GameObject closestAG = null;

        foreach (GameObject agent in WanderAgentBehavior.wanderAgents.ToArray())
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



    //find the closest travel agent in front of the sAgent
    GameObject ClosestTravelAgent()
    {
        GameObject closestAG = null;

        foreach (GameObject agent in TravelAgentBehavior.travelAgents.ToArray())
        {
            bool collision = LineIntersectsCircle1(ahead, ahead2, agent);
            //position is current sAgent's current position
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




    //calculate steer force to avoid the closest obstacle and agent
    Vector2 CollisionAvoidance()
    {
        ObstacleClass ob = ClosestObstacle();
        GameObject wAgent = ClosestWanderAgent();
        GameObject tAgent = ClosestTravelAgent();
        Vector2 avoidance = new Vector2(0, 0);

        //privilege is to avoid the obstacle if both obstacle and agent are needed to avoid
        if (ob != null)
        {
            //avoidance force is in the direction of ahead minus obstacle center
            avoidance = new Vector2(ahead.x - ob.center.x, ahead.y - ob.center.y);
            avoidance.Normalize();

            //avoidance force is scaled by maxAvoidForce
            avoidance *= sAgent.maxAvoidForce;

            //combine avoidance force from the wander agent if it exists
            if (wAgent != null)
            {
                //avoidance force is in the direction of ahead minus agent center
                Vector2 avoidance2 = new Vector2(ahead.x - wAgent.transform.position.x, ahead.y - wAgent.transform.position.y);
                avoidance2.Normalize();
                //avoidance force is scaled by maxAvoidForce/5
                avoidance2 *= sAgent.maxAvoidForce / 5.0f;

                //accumulate all the avoidance force from the obstacle
                avoidance += avoidance2;

            }


            //combine avoidance force from the travel agent if it exists
            if (tAgent != null)
            {
                //avoidance force is in the direction of ahead minus agent center
                Vector2 avoidance3 = new Vector2(ahead.x - tAgent.transform.position.x, ahead.y - tAgent.transform.position.y);
                avoidance3.Normalize();
                //avoidance force is scaled by maxAvoidForce/5
                avoidance3 *= sAgent.maxAvoidForce / 5.0f;

                //accumulate all the avoidance force from the obstacle
                avoidance += avoidance3;

            }

        }//if no obstacle is needed to avoid
        else
        {
            //if there is wander agent to avoid
            if (wAgent != null)
            {
                Vector2 avoidance1 = new Vector2(ahead.x - wAgent.transform.position.x, ahead.y - wAgent.transform.position.y);
                avoidance1.Normalize();
                avoidance1 *= sAgent.maxAvoidForce;
                avoidance += avoidance1;
            }
            //if there is travel agent to avoid
            if (tAgent != null)
            {
                Vector2 avoidance2 = new Vector2(ahead.x - tAgent.transform.position.x, ahead.y - tAgent.transform.position.y);
                avoidance2.Normalize();
                avoidance2 *= sAgent.maxAvoidForce;
                avoidance += avoidance2;
            }

        }

        return avoidance;
    }






    //find social neighbors of social agent 
    List<GameObject> Neighbors()
    { 
        List<GameObject> neighbors = new List<GameObject>();
       
        foreach (GameObject agent in socialAgents.ToArray())
        {
           
            if (Vector2.Distance(position, new Vector2(agent.transform.position.x, agent.transform.position.y))
                < sAgent.socialDistance)
            {
                neighbors.Add(agent);
            }
        }

     

        foreach (GameObject agent in neighbors.ToArray())
        {
            if (agent.GetComponent<SocialAgentBehavior>().join == true)//in social conversation now
            {                
                continue;
            }
            else
            {
                socialGroup.Add(agent);               
            }

        }
        return socialGroup;
                       
    }




    //if closest agent is found, wAgent will attemp to interpose itself between the travel agent and its intended exit
    public void Conversation()
    {
        Vector2 velocity = new Vector2(0, 0);
        Vector2 destination = sAgent.destination;
        List<GameObject> socialGroup = Neighbors();
        
        int neighborCount = 0;
        if (sAgent.timer2 == 0 && join == false) {//not in cool down time and not in social conversation
            foreach (GameObject agent in socialGroup.ToArray())
            {
                //Debug.Log(agent);
                velocity.x += agent.transform.position.x;
                velocity.y += agent.transform.position.y;
                neighborCount++;
            }

            velocity.x /= neighborCount;
            velocity.y /= neighborCount;
            //velocity = new Vector2(velocity.x - transform.position.x, velocity.y - transform.position.y);
            //velocity.Normalize();

            //destination = new Vector2(velocity.x * 10.0f, velocity.y * 10.0f);
            destination = new Vector2(velocity.x, velocity.y);
            sAgent.destination = destination;
            join = true;
            sAgent.timer = 0;
            sAgent.extendedTime = Random.Range(0.5f, 2.0f);
        }
    }


    void LeaveSocialGroup()
    {
        Vector2 velocity = new Vector2(GetComponent<Rigidbody>().velocity.x, GetComponent<Rigidbody>().velocity.y);
        velocity *= -1;
        //Vector2 destination = sAgent.destination;

        sAgent.destination = new Vector2(velocity.x * 10.0f, velocity.y * 10.0f);
        //sAgent.destination = destination;
        join = false;
    }



   

}
