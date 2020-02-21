using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets;
public class Polygon : MonoBehaviour
{

    public int numObstacles;
    public static List<ObstacleClass> obstacles = new List<ObstacleClass>();

    // Use this for initialization
    void Start()
    {
        DistributeObstacles();
        
    }


    // Update is called once per frame
    void Update()
    {
        //loop through obstacle list and connect vertices inside each obstacle
        int index = 0;
        foreach (ObstacleClass ob in obstacles.ToArray())
        {
            ConnectVertices(index);
            index++;
        }
    }


    void DistributeObstacles()
    {
        for (int i = 0; i < numObstacles; i++)
        {
            ObstacleClass obstacle = new ObstacleClass();
            float tempr = Random.Range(0.125f, 0.5f) * 70f;
            float x;
            float y;
            //variable left, right, top and bottom is the distance of polygon center to the sourrounding region boundary
            float left;
            float right;
            float top;
            float bottom;

            float offset = 140 / Mathf.Ceil((float)numObstacles / 2.0f);

            if (i % 2 == 0)
            {
                //when obstacle index is even number, randomly select the polygon center with constraint to bottom half of the floor
                x = Random.Range(-70 + (i / 2) * offset + 15f, -70 + (i / 2 + 1) * offset - 15f);
                y = Random.Range(-25f, -15f);


                top = Vector2.Distance(new Vector2(y, 0), new Vector2(0, 0));
                bottom = Vector2.Distance(new Vector2(y, 0), new Vector2(-40f, 0));

                //create more randomness to the position of second to last obstacle when number of obstacles is even number
                if (numObstacles % 2 == 1 && i == (numObstacles - 1))
                {
                    y = Random.Range(-25f, 25f);
                    top = Vector2.Distance(new Vector2(y, 0), new Vector2(40, 0));
                    bottom = Vector2.Distance(new Vector2(y, 0), new Vector2(-40f, 0));
                }


                //Debug.Log(x + "                     " + y);
                left = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i / 2) * offset, 0));
                right = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i / 2 + 1) * offset, 0));

                //make sure that left-most obstacle is at least 10f from the floor left boundary
                if (i == 0)
                {
                    left = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i / 2) * offset + 10.0f, 0));
                }

                //make sure that right-most obstacle is at least 10f from the floor right boundary
                if (i == (numObstacles - 1))
                {
                    right = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i / 2 + 1) * offset - 10.0f, 0));
                }
            }
            else
            {   //when obstacle index is odd number, randomly select the polygon center with constraint to top half of the floor
                x = Random.Range(-70 + (i - 1) / 2 * offset + 15f, -70 + (((i - 1) / 2) + 1) * offset - 15f);
                y = Random.Range(15f, 25f);

                //calculate distance of polygon center to the sourrounding region boundary 
                top = Vector2.Distance(new Vector2(y, 0), new Vector2(40f, 0));
                bottom = Vector2.Distance(new Vector2(y, 0), new Vector2(0f, 0));
                left = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i - 1) / 2 * offset, 0));
                right = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (((i - 1) / 2) + 1) * offset, 0));

                //make sure that left-most obstacle is at least 10f from the floor left boundary
                if (i == 1)
                {
                    left = Vector2.Distance(new Vector2(x, 0), new Vector2(-70 + (i - 1) / 2 * offset + 10.0f, 0));
                }
            }

            float[] value = new float[5];
            value[0] = tempr;
            value[1] = left;
            value[2] = right;
            value[3] = top;
            value[4] = bottom;
            obstacle.radius = Mathf.Min(value);
            if (obstacle.radius < 10.0f)
            {
                obstacle.radius = 10.0f;
            }

            obstacle.center = new Vector2(x, y);
            obstacles.Add(obstacle);

            GenerateObstacle(obstacle.center, obstacle.radius, i);

        }
    }


    //based on selected center and radius generate obstacle polygon with random number of vertices
    void GenerateObstacle(Vector2 center, float radius, int index)
    {
        //number of vertices for each polygon obstacle is between 4 and 16
        int numVertices = Random.Range(4, 17);

        Vector2[] vertices = new Vector2[numVertices + 1];
        float averageX = 0;
        float averageY = 0;

        for (int i = 0; i < numVertices; i++)
        {
            //generate random angle and radius for each vertex
            float a = Random.Range(0.0f, 2.0f * Mathf.PI);
            float r = radius * Mathf.Sqrt(Random.Range(0.125f, 1.0f));

            //calculate Cartesian position of each vertex
            float x = center.x + r * Mathf.Cos(a);
            float y = center.y + r * Mathf.Sin(a);

            //sum up all x-cordinate values and y-cordinate values
            averageX += x;
            averageY += y;

            //store position of each vertex
            vertices[i] = new Vector2(x, y);
        }

        //calculate average x-position and y-position of all the vertices and used as center of the polygon
        averageX /= numVertices;
        averageY /= numVertices;
        obstacles[index].center.x = averageX;
        obstacles[index].center.y = averageY;

        //calculate distance of each vertex to the polygon center, use the maximun distance as radius of polygon 
        float maxRadius = -1f;
        for (int i = 0; i < numVertices; i++)
        {
            //calculate the angle of each vertex with respect to polygon center
            Vector3 point = new Vector3(vertices[i].x, vertices[i].y, Mathf.Atan2(vertices[i].y - averageY, vertices[i].x - averageX));
            obstacles[index].points.Add(point);

            //calculate distance of each vertex to polygon center and look for max distance
            float distance = Vector2.Distance(obstacles[index].center, new Vector2(vertices[i].x, vertices[i].y));
            if (distance > maxRadius)
            {
                maxRadius = distance;
            }

        }

        //store polygon radius
        obstacles[index].radius = maxRadius;

        //sort the vertices according to their angles to polygon center
        obstacles[index].points.Sort((a, b) => a.z.CompareTo(b.z));

        //loop to close the polygon
        obstacles[index].points.Add(obstacles[index].points[0]);
    }


    void ConnectVertices(int index)
    {
        GameObject line = new GameObject();
        line.AddComponent<LineRenderer>();
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = (Color.black);
        lr.endColor = (Color.black);
        lr.startWidth = 0.7f;
        lr.endWidth = 0.7f;
        lr.positionCount = obstacles[index].points.Count;

        //iterate through the ordered vertices to connect them by line
        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 point = new Vector3(obstacles[index].points[i].x, obstacles[index].points[i].y, 0);
            lr.SetPosition(i, point);
        }
    }

}
