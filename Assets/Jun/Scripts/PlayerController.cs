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
    [SerializeField] private Blocks currentBlock;


    [Header("PlayerStat")]
    [SerializeField] private int curHp;
    [SerializeField] private int maxHp;
    public event Action<int> OnChangeHp;

    [Header("PlayerCheck")]
    public bool IsGoal = false;

    private void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
        if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogError("��������, ���������� �߸� ���� �Ǿ����ϴ�.");
            return;
        }

        //Start�� �ƴ� �濡 ���� ���� �� �����ϴ� ������ ���� ����
        SpawnBlock();
        /*if (photonView.IsMine)
        {

            if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("��������, ���������� �߸� ���� �Ǿ����ϴ�.");
                return;
            }

            //Start�� �ƴ� �濡 ���� ���� �� �����ϴ� ������ ���� ����
            SpawnBlock();

            currentBlock.OnBlockEntered += BlockEntered;
        }*/
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
        if (/*photonView.IsMine && */currentBlock != null )
        {
            PlayerInput();
        }
    }

    private void PlayerInput()
    {
        if (IsGoal)
            return;

        if (Input.GetKey(KeyCode.DownArrow))
            currentBlock.Down();

        if (Input.GetKey(KeyCode.U))
        {
            currentBlock.Push(Vector2.left);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            currentBlock.Push(Vector2.right);
        }
        else
        {
            currentBlock.Move(Vector2.right * Input.GetAxisRaw("Horizontal"));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentBlock.Rotate();
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
        /*if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("Photon Network�� ����Ǿ� ���� �ʽ��ϴ�. ���� ������ �� �����ϴ�.");
            return;
        }*/

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        //SpawnPoint�� �÷��̾� ��ġ or ���� ���� y�� �ִ�ġ or Ÿ���� ���� ���ġ�� ���� ������ ����
        GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        //GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);
        currentBlock = newBlock.GetComponent<Blocks>();
        //currentBlock.OnDisableControl += BlockEntered;
    }

    public void TakeDamage(int damage)
    {
        curHp -= damage;
        Debug.Log($"���� ü�� : {curHp}");

        OnChangeHp?.Invoke(curHp);

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
    }

    public void ReachGoal()
    {
        IsGoal = true;
        //���� ���� �� �߰����� ����
        //ex) ���� ����ó�� ū �������� ������ 1���� ������ ����� �� �ֵ���
    }
}
