﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdSpawner : MonoBehaviour
{
    public GameObject entityObject;

    public static int Id = 0;

    public int sizeX = 5;
    public int sizeZ = 5;

    public float distance = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Id++;
        for (int i = 0; i<sizeX; i++)
        {
            for (int j= 0; j<sizeZ; j++)
            {
                var position = new Vector3((i - sizeX/2) * distance, 0, (j - sizeZ / 2) * distance);
                var obj = Instantiate(entityObject, transform);
                obj.transform.localPosition = position;
                var people = obj.GetComponent<PeopleAuth>();
                people.crowdId = Id;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}