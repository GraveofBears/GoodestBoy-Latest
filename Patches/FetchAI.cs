﻿using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GoodestBoy.Patches;

public class FetchAI : MonoBehaviour
{
    public enum AIStates
    {
        BaseAI,
        Idle,
        GettingBall,
        ReturningBall,
        Wait,
        DropBall
    }

    public static readonly List<FetchAI> _fetchAiList = new();
    public AIStates m_AiState;
    public Animator m_animator;
    public MonsterAI m_monsterAI;
    public float m_updateTimer;
    public float m_distanceFromPlayer = 2.25f; //distance of pet before it drops the item
    public float m_distanceFromItem = 1f; // dont change this
    public bool m_pausing;
    [FormerlySerializedAs("m_targetBall")] public Rigidbody m_targetItem;
    public float m_stateTime;
    public bool m_hasABall;
    private readonly Vector3 m_boneOffset = new(-0.001f, -0.147f, -0.023f); // also you dont have to tweak this since the item is already invisible
    private readonly Vector3 m_mouthOffset = new(1f, 0.56f, 0f); // also this you dont have to tweak item is already invisible.

    private bool ValidBallToGet => m_targetItem != null && m_targetItem.useGravity;

    private bool ValidBall => m_targetItem != null && !m_targetItem.useGravity;

    public void Awake()
    {
        m_monsterAI = GetComponent<MonsterAI>();
        m_monsterAI.enabled = false;
        _fetchAiList.Add(this);
        m_animator = GetComponentInChildren<Animator>();
    }

    public void Start()
    {
        m_monsterAI.Start();
    }

    private void OnDestroy()
    {
        _fetchAiList.Remove(this);
    }

    private Rigidbody GetBall(float range)
    {
        m_targetItem = FetchSystem.FindBall(transform.position, range); // the FindBall method is in the plugin.cs only change this if you change its location.
        return !m_targetItem ? null : m_targetItem;
    }

    public void GetBall(ItemDrop ball)
    {
        m_targetItem = ball.GetComponent<Rigidbody>();
        if (!m_targetItem)
        {
            return;
        }

        m_AiState = AIStates.GettingBall;
        m_stateTime = 10f;
    }

    public void FixedUpdate()
    {
        try
        {
            if (!m_monsterAI.m_nview.IsValid()) return;
            m_updateTimer += Time.fixedDeltaTime;
            if (m_updateTimer < 0.05f) return;
            UpdateAI(0.05f);
            m_updateTimer -= 0.05f;
        }
        catch
        {
        }
    }

    public void UpdateAI(float dt)
    {
        m_stateTime -= dt;
        if (m_monsterAI.IsAlerted())
        {
            m_stateTime = 3f;
            m_AiState = AIStates.BaseAI;
            DropBall();
        }

        if (m_stateTime <= 0f && m_pausing)
        {
            m_pausing = false;
        }
        else if (m_stateTime <= 0f && m_AiState != 0)
        {
            NewAction();
        }
        else if (m_pausing)
        {
            return;
        }

        switch (m_AiState)
        {
            case AIStates.BaseAI:
            {
                m_monsterAI.UpdateAI(dt);
                if (m_stateTime <= 0f)
                {
                    if (m_monsterAI.m_randomMoveUpdateTimer <= 0f)
                    //if (m_monsterAI.m_aiStatus.StartsWith("Random"))
                    {
                        NewAction();
                    }
                    else
                    {
                        m_stateTime = 3f;
                    }
                }
                break;
            }
            case AIStates.GettingBall:
            {
                if (!ValidBallToGet)
                {
                    m_stateTime = 0f;
                    break;
                }

                var position = m_targetItem.transform.position;
                if (m_monsterAI.MoveTo(dt, position, m_distanceFromItem, run: true) &&
                    global::Utils.DistanceXZ(transform.position, position) <= m_distanceFromItem)
                {
                    m_monsterAI.LookAt(position);
                    if (m_monsterAI.IsLookingAt(position, 20f) && ValidBallToGet)
                    {
                        m_animator.Play("consume");
                        StartCoroutine(PickupBall());
                        m_AiState = AIStates.Wait;
                        m_stateTime = 5f;
                    }
                }
                break;
            }
            case AIStates.ReturningBall:
            {
                if (!ValidBall)
                {
                    m_hasABall = false;
                    m_stateTime = 0f;
                    break;
                }

                var playerTransform = Player.m_localPlayer.transform;
                if (m_monsterAI.MoveTo(dt, playerTransform.position + playerTransform.forward * m_distanceFromPlayer,
                        0f, run: true))
                {
                    m_monsterAI.LookAt(playerTransform.position);
                    if (m_monsterAI.IsLookingAt(playerTransform.position, 15f))
                    {
                        m_pausing = true;
                        m_AiState = AIStates.DropBall;
                        m_stateTime = Random.Range(0.4f, 1.3f);
                    }
                }
                break;
            }
            case AIStates.DropBall:
            {
                DropBall();
                m_AiState = AIStates.Wait;
                m_stateTime = Random.Range(2f, 4f);
                break;
            }
            case AIStates.Idle:
            case AIStates.Wait:
                break;
        }
    }

    private void DropBall()
    {
        if (!m_hasABall) return;
        m_hasABall = false;
        if (!ValidBall) return;
        m_targetItem.gameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        m_targetItem.isKinematic = false;
        m_targetItem.collisionDetectionMode = CollisionDetectionMode.Continuous;
        Traverse.Create(m_targetItem.GetComponent<ZSyncTransform>()).Field("m_useGravity").SetValue(true);
        m_targetItem.transform.SetParent(null, worldPositionStays: true);
    }

    private IEnumerator PickupBall()
    {
        yield return new WaitForSeconds(0.48f);
        if (!ValidBallToGet || m_monsterAI.IsAlerted())
        {
            m_animator.CrossFadeInFixedTime("New State 0", 0.4f, 0);
            m_AiState = AIStates.Idle;
            m_stateTime = 0f;
            yield break;
        }
        
        Traverse.Create(m_targetItem.GetComponent<ZSyncTransform>()).Field("m_useGravity").SetValue(false);
        m_targetItem.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        m_targetItem.isKinematic = true;
        m_targetItem.GetComponent<ItemDrop>().m_autoPickup = false;
        //Transform petTransform = null;
        //if (transform.gameObject.name.ToLower().Contains("goodestboy"))
        //{
        //    petTransform = global::Utils.FindChild(transform, "Head");
        //}

        //else
        //{
        //    petTransform = global::Utils.FindChild(transform, "Head");
        //}
        var petTransform = Utils.FindChild(transform, "Head");
        var vector = m_targetItem.transform.rotation * m_boneOffset;
        m_targetItem.transform.SetParent(petTransform, worldPositionStays: true);
        m_targetItem.transform.position = petTransform.position + vector + petTransform.rotation * m_mouthOffset;
        m_hasABall = true;
        m_targetItem.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.52f);
        m_animator.CrossFadeInFixedTime("New State 0", 0.6f, 0);
        yield return new WaitForSeconds(Random.Range(0.2f, 1.5f));
        m_AiState = AIStates.ReturningBall;
        m_stateTime = 10f;
    }

    private void NewAction()
    {
        if (Random.Range(0, 3) == 0) //this randomizes the behaviour of the pet when its 0 it will fetch the ball if its 1 2 or 3 it will just use its base ai. so when you throw the item and the pet didnt fetch it it means its value is 1 2 or 3.
        {
            //!((transform.position - Player.m_localPlayer.transform.position).sqrMagnitude <= 36f) if the pet is more than 36meters away from the player it wont fetch the ball. 
            if (m_hasABall || !((transform.position - Player.m_localPlayer.transform.position).sqrMagnitude <= 36f) ||
                GetBall(50f) == null) return; // GetBall(30f) - 30f is the range should be the bone/item/fetch item to be in in order fo the pet to fetch it, if the item is more than 30f away it wont be pick 
            m_AiState = AIStates.GettingBall;
            m_stateTime = 10f;
        }
        else
        {
            m_AiState = AIStates.BaseAI;
            m_stateTime = Random.Range(2f, 4f);
        }
    }
}
