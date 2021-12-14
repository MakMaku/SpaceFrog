using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Sputnik : MonoBehaviour
{
    public float t = 0;
    public Vector2 cachedCenter;
    public float Mass = 500000f;

    private float radius, speed;    
    private Transform parent;
    private Planet planet;
    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent;
        radius = Mathf.Sqrt(Mathf.Pow(transform.position.x - parent.position.x, 2) + Mathf.Pow(transform.position.y - parent.position.y, 2));
        planet = GameObject.Find(gameObject.transform.parent.gameObject.name).GetComponent<Planet>();
        float  M = planet.Mass+Mass;
        speed = Mathf.Sqrt(M) / Mathf.Sqrt(Mathf.Pow(radius, 3));
    }

    // Update is called once per frame
    void Update()
    {
        cachedCenter = new Vector2(parent.position.x, parent.position.y) + new Vector2(radius, 0);
        t += Time.deltaTime;
        var x = Mathf.Cos(t * speed) * radius;
        var y = Mathf.Sin(t * speed) * radius;
        transform.position = new Vector2(x, y) + cachedCenter - new Vector2(radius, 0);
    }
}
