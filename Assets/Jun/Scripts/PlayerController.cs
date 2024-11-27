using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviourPun, IPunObservable
{

    [Header("Block")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Blocks currentBlock;
    public int BlockCount = 0;
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager;

    [Header("PlayerStat")]
    [SerializeField] private int curHp;
    [SerializeField] private int maxHp;
    public event Action<int> OnChangeHp;

    [Header("PlayerCheck")]
    public bool IsGoal = false;

    public event System.Action<int> OnChangeBlockCount;

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
    private void BlockDisabled()
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

        // �� �ʱ� ���̾� ���� (��: "Default")
        SetLayerAll(newBlock, LayerMask.NameToLayer("Default"));

        currentBlock = newBlock.GetComponent<Blocks>();
        currentBlock.OnDisableControl += BlockDisabled;
        currentBlock.OnBlockEntered += BlockEnter;
        currentBlock.OnBlockExited += BlockExit;
        currentBlock.OnBlockFallen += BlockFallen;
        //�̺�Ʈ ���� ���?
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

    public void BlockEnter()
    {
        BlockCount++;
        OnChangeBlockCount?.Invoke(BlockCount);
        blockMaxHeightManager.UpdateHighestPoint();

        // ���� ���̾ "Blocks"�� ����
        if (currentBlock != null)
        {
            SetLayerAll(currentBlock.gameObject, LayerMask.NameToLayer("Blocks"));
        }
    }

    public void BlockExit()
    {
        BlockCount--;
        OnChangeBlockCount?.Invoke(BlockCount);
        blockMaxHeightManager.UpdateHighestPoint();
    }

    public void BlockFallen()
    {
        Debug.Log("���� ī�޶� �ٱ����� ������");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(BlockCount);
        }
        else
        {
            BlockCount = (int)stream.ReceiveNext();
        }
    }

    public void SetLayerAll(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerAll(child.gameObject, newLayer);
        }
    }
}
