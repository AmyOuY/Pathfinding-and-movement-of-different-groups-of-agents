using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Assets;
public class GameController : MonoBehaviour
{
    public int numTravelAgent = 10;
    public GameObject TAgentPrefab;
    public int numWanderAgent = 10;
    public GameObject WAgentPrefab;
    public int numSocialAgent = 10;
    public GameObject SAgentPrefab;

    private float delaySpawn = 0.2f;
    private float delaySpawn2 = 0.2f;

    private float time = 0;


    // Use this for initialization
    void Start()
    {
        
        StartCoroutine(SpawnTravelAgent());
        StartCoroutine(SpawnWanderAgent());
        StartCoroutine(SpawnSocialAgent());

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time > 30.0f) {
            Debug.Log(TravelAgentBehavior.count);
        }
        RespawnTravelAgent();

    }


    //spawn travel agent at delay interval 
    public IEnumerator SpawnTravelAgent()
    {
        WaitForSeconds delay = new WaitForSeconds(delaySpawn);
        TravelAgentBehavior.tAgentCounter = numTravelAgent;
        GameObject door = GameObject.Find("RightDoor");

        for (int i = 0; i < numTravelAgent; i++)
        {
            yield return delay;
            GameObject tAgent = Instantiate(TAgentPrefab, new Vector3(door.transform.position.x - 4.6f, door.transform.position.y, 0), Quaternion.identity);
            TravelAgentBehavior.travelAgents.Add(tAgent);
        }
    }


    //spawn wander agent at delay interval
    public IEnumerator SpawnWanderAgent()
    {
        float x;
        float y;
        WaitForSeconds delay = new WaitForSeconds(delaySpawn2);
        for (int i = 0; i < numWanderAgent; i++)
        {
            yield return delay;
            x = Random.Range(-20.0f, 20.0f);
            y = Random.Range(-2.0f, 2.0f);
            GameObject wAgent = Instantiate(WAgentPrefab, new Vector3(x, y, 0), Quaternion.identity);
            WanderAgentBehavior.wanderAgents.Add(wAgent);
        }
        
     }



    //spawn wander agent at delay interval
    public IEnumerator SpawnSocialAgent()
    {
        float x;
        float y;
        WaitForSeconds delay = new WaitForSeconds(delaySpawn2);
        for (int i = 0; i < numSocialAgent; i++)
        {
            yield return delay;
            x = Random.Range(-20.0f, 20.0f);
            y = Random.Range(-2.0f, 2.0f);
            GameObject sAgent = Instantiate(SAgentPrefab, new Vector3(x, y, 0), Quaternion.identity);
            SocialAgentBehavior.socialAgents.Add(sAgent);
        }

    }


    //respawn a travel agent if it reaches the destination to ensure a constant population
    void RespawnTravelAgent()
    {
        GameObject door = GameObject.Find("RightDoor");

        if (TravelAgentBehavior.tAgentCounter < numTravelAgent)
        {
            for (int i = 0; i < (numTravelAgent - TravelAgentBehavior.tAgentCounter); i++)
            {
                GameObject tagent = Instantiate(TAgentPrefab, new Vector3(door.transform.position.x - 4.6f, door.transform.position.y, 0), Quaternion.identity);

                TravelAgentBehavior.tAgentCounter++;
                TravelAgentBehavior.travelAgents.Add(tagent);
            }
        }
    }
}
