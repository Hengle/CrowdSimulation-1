﻿using Unity.Entities;
using UnityEngine;

public class RabbitAnimationObject : MonoBehaviour, IConvertGameObjectToEntity
{
   
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Animator()
        {
            animationIndex = 0,
            speed = 1f,
            currentTime = UnityEngine.Random.value,
            localPos = transform.localPosition,
            localRotation = transform.rotation,
        });
    }
}
