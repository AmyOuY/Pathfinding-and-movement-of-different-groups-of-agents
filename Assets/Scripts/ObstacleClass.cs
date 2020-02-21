using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class ObstacleClass : MonoBehaviour
namespace Assets
{
    public class ObstacleClass
    {
        public Vector2 center = new Vector2();
        public float radius;
        public List<Vector3> points = new List<Vector3>();
        public List<Vector2> vertices = new List<Vector2>();
    }
}