using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPun
{

    [Header("Block")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Block currentBlock;


    [Header("PlayerStat")]
    [SerializeField] private int curHp;
    [SerializeField] private int maxHp;
    public event Action OnChangeHp;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        if (photonView.IsMine)
        {

            if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("��������, ���������� �߸� ���� �Ǿ����ϴ�.");
                return;
            }

            //Start�� �ƴ� �濡 ���� ���� �� �����ϴ� ������ ���� ����
            SpawnBlock();

            //Block.OnBlockEntered += BlockEntered;
        }
    }

    private void OnDestroy()
    {
        if (photonView.IsMine)
        {
            //Block.OnBlockEntered -= BlockEntered;
        }
    }

    private void Update()
    {
        if (photonView.IsMine && currentBlock != null)
        {
            PlayerInput();
        }
    }

    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //currentBlock.Move();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            //currentBlock.Move();
        }


        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
        {
            //currentBlock.Rotate(90);
        }


        if (Input.GetKey(KeyCode.DownArrow))
        {
            //currentBlock.Down();
        }
            
    }
    private void BlockEntered()
    {
        // ���� ���� ���� ����
        if (currentBlock != null)
        {
            currentBlock = null;
        }

        // ���ο� �� ����
        SpawnBlock();
    }

    public void SpawnBlock()
    {
        if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("Photon Network�� ����Ǿ� ���� �ʽ��ϴ�. ���� ������ �� �����ϴ�.");
            return;
        }

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        //SpawnPoint�� �÷��̾� ��ġ or ���� ���� y�� �ִ�ġ or Ÿ���� ���� ���ġ�� ���� ������ ����
        GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock.GetComponent<Block>();
    }

    public void TakeDamage(int damage)
    {
        curHp -= damage;
        Debug.Log($"���� ü�� : {curHp}");

        // �÷��̾��� ü���� 0 ���ϰ� �Ǹ� �� ������ ��Ȳ�� ó��
        if (curHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //�÷��̾� ���� �ִϸ��̼� �߰�
        //�Ѹ��� �÷��̾ ������ �� ���� ���
        OnChangeHp?.Invoke();
    }


}
