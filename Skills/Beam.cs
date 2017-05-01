using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public const float MINIMUM_BEAM_WIDTH    = 0.5f;
    public const float MAXIMUM_BEAM_WIDTH    = 5.0f;

    [SerializeField] private GameObject beamStart;
    [SerializeField] private GameObject beamMiddle;
    [SerializeField] private GameObject beamEnd;
    [SerializeField] private Color beamColor;
    [SerializeField] private float beamWidth;

    private SpriteRenderer myBeamStart, myBeamMiddle, myBeamEnd;
    private float startSpriteWidth, endSpriteWidth;
    private BattleNPC target;
    private Vector2 laserDirection;
    private Action<BattleNPC> hitCallback;
    
    //Properties for inspector elements
#if UNITY_EDITOR
    public float BeamWidth
    {
        get
        {
            return beamWidth;
        }
        set
        {
            beamWidth = GameGlobals.SnapToMinOrMax(GameGlobals.StepByPointOne(value), MINIMUM_BEAM_WIDTH, MAXIMUM_BEAM_WIDTH);
        }
    }
#endif
       
    void Awake()
    {        
        myBeamStart          = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamStart, this.transform));
        myBeamStart.color    = beamColor;
        myBeamStart.transform.localScale = myBeamStart.transform.localScale * beamWidth;
        startSpriteWidth     = myBeamStart.sprite.bounds.size.x;
        myBeamStart.enabled  = false;

        myBeamMiddle         = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamMiddle, this.transform));
        myBeamMiddle.color   = beamColor;
        myBeamMiddle.transform.localScale = myBeamMiddle.transform.localScale * beamWidth;
        myBeamMiddle.enabled = false;

        myBeamEnd            = GameGlobals.AttachCheckComponent<SpriteRenderer>(Instantiate(beamEnd, this.transform));
        myBeamEnd.color      = beamColor;
        myBeamEnd.transform.localScale = myBeamEnd.transform.localScale * beamWidth;
        myBeamEnd.enabled    = false;
        endSpriteWidth       = myBeamEnd.sprite.bounds.size.x;

        this.enabled         = false;
    }
       
    public void PointBeam(Vector3 startPos, BattleNPC aimTarget, Action<BattleNPC> callback)
    {
        target         = aimTarget;                
        hitCallback    = callback;
        laserDirection = Vector2.right;
        beamStart.tag  = BattleGlobals.TAG_FOR_ENEMY_PROJ;

        if (startPos.x > target.transform.position.x)
        {
            myBeamStart.flipX = true;
            myBeamEnd.flipX   = true;
            laserDirection    = Vector2.left;
            beamStart.tag     = BattleGlobals.TAG_FOR_HERO_PROJ;   
        }

        myBeamStart.transform.position = startPos;

        myBeamStart.enabled  = true;
        myBeamMiddle.enabled = true;
        myBeamEnd.enabled    = true;
        this.enabled         = true;        
    }

    public void StopBeam()
    {
        myBeamStart.enabled  = false;
        myBeamMiddle.enabled = false;
        myBeamEnd.enabled    = false;
        target               = null;
        this.enabled         = false;
    }

    void Update()
    {
        float distance      = Vector2.Distance(myBeamStart.transform.position, target.transform.position + target.MidSpritePosition);
        RaycastHit2D hit    = Physics2D.Linecast(myBeamStart.transform.position, target.transform.position, BattleGlobals.GetBattleNPCLayerMask());
        Vector3 endPosition = target.transform.position + target.MidSpritePosition;
        
        if(hit.collider != null)
        {
            distance    = Vector2.Distance(hit.transform.position, myBeamStart.transform.position);            
            endPosition = hit.collider.transform.position;
            BattleNPC b = GameGlobals.GetBattleNPC(hit.collider.gameObject); 
            if (b != null)
            {
                hitCallback(b);
            }                
        }

        myBeamMiddle.transform.localScale = new Vector3(distance - (startSpriteWidth + endSpriteWidth),
                                                myBeamMiddle.transform.localScale.y, 
                                                myBeamMiddle.transform.localScale.z);

        float multiplier                  = (laserDirection == Vector2.left) ? -1 : 1;
        myBeamMiddle.transform.position   = myBeamStart.transform.position + multiplier * new Vector3((distance - (startSpriteWidth + endSpriteWidth))/2.0f, 
                                            (transform.position.y - endPosition.y)/2.0f, 0f);
        myBeamEnd.transform.position      = endPosition - multiplier * new Vector3(endSpriteWidth, 0f, 0f);

        myBeamMiddle.transform.rotation   = BattleGlobals.LookAt(myBeamMiddle.gameObject, myBeamEnd.transform.position, this.tag);
        myBeamStart.transform.rotation    = BattleGlobals.LookAt(myBeamStart.gameObject, myBeamMiddle.transform.position, this.tag);
    }
}