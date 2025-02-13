﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10WiggleSeedling : MonoBehaviour
{
    private Vector3 wiggleDirection;
    private float currentWiggleAmount = 0;
    private float goalWiggleAmount = 0f;
    private float wiggleUpSlew, wiggleDownSlew;
    public float maxWiggleAmount = 0.1f;
    private Vector3 originalLocalPosition;
    private float wiggleRate;

    private static float wiggleMultiplier = 1;

    public static void SetWiggleMultiplier( float m )
    {
        wiggleMultiplier = m;
    }

    // Use this for initialization
    void Start()
    {
        wiggleDirection = new Vector3( Random.Range( -1f, 1f ), 0, Random.Range( -1f, 1f ) ).normalized;
        wiggleUpSlew = 0.1f;
        wiggleDownSlew = 0.05f;
        wiggleRate = Random.Range( 5f, 13f );

        originalLocalPosition = transform.localPosition;

        //InvokeRepeating( "AnimateWiggle", 0, 5f );
    }

    // Update is called once per frame
    void Update()
    {
        if( goalWiggleAmount > currentWiggleAmount )
        {
            currentWiggleAmount += wiggleUpSlew * ( goalWiggleAmount - currentWiggleAmount );
        }
        else
        {
            currentWiggleAmount += wiggleDownSlew * ( goalWiggleAmount - currentWiggleAmount );
        }

        transform.localPosition = originalLocalPosition +
            maxWiggleAmount * currentWiggleAmount * Mathf.Sin( 2 * Mathf.PI * Time.time * wiggleRate ) * wiggleMultiplier *
            wiggleDirection;
    }

    public void AnimateWiggle()
    {
        StartCoroutine( DoWiggle() );
    }

    public float GetWiggleAmount()
    {
        return currentWiggleAmount;
    }

    IEnumerator DoWiggle()
    {
        goalWiggleAmount = 1f;
        yield return new WaitForSeconds( 0.2f );
        goalWiggleAmount = 0f;
    }
}
