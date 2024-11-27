using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using Random = UnityEngine.Random;
using Photon.Pun.UtilityScripts;

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

    public int TowerNumber = 0;

    private void Start()
    {
        //��Ʈ��ũ �׽�Ʈ��
        if (photonView.IsMine)
        {
            TowerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            GameObject TowerTest = GameObject.Find($"Tower{TowerNumber}");
            blockMaxHeightManager = TowerTest.GetComponent<BlockMaxHeightManager>();
            GetComponent<PlayerMovement>().SetMaxHeightManager(blockMaxHeightManager);

            transform.position = new Vector2(TowerTest.transform.position.x - 5f, 
                TowerTest.transform.position.y + 5f);
        }



        if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
        {
            Debug.LogError("��������, ���������� �߸� ���� �Ǿ����ϴ�.");
            return;
        }

        //Start�� �ƴ� �濡 ���� ���� �� �����ϴ� ������ ���� ����
        SpawnBlock();
    }

    /*private void OnDestroy()
    {
        if (photonView.IsMine)
        {
            //Block.OnBlockEntered -= BlockEntered;
        }
    }*/

    private void Update()
    {
        if (photonView.IsMine && currentBlock != null )
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

    public void SpawnBlock()
    {
        if (!PhotonNetwork.IsConnected) 
        {
            Debug.LogError("Photon Network�� ����Ǿ� ���� �ʽ��ϴ�. ���� ������ �� �����ϴ�.");
            return;
        }

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        //SpawnPoint�� �÷��̾� ��ġ or ���� ���� y�� �ִ�ġ or Ÿ���� ���� ���ġ�� ���� ������ ����
        //GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);

        // �� �ʱ� ���̾� ���� (��: "Default")
        SetLayerAll(newBlock, LayerMask.NameToLayer("Default"));

        currentBlock = newBlock.GetComponent<Blocks>();
        /*currentBlock.OnBlockEntered += BlockEnter;
        currentBlock.OnBlockExited += BlockExit;
        currentBlock.OnBlockFallen += BlockFallen;*/
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

    public void BlockEnter(Blocks block)
    {
        // ���� ���� ���� ����
        if (currentBlock == block)
        {
            //���� ���̾� ����
            SetLayerAll(currentBlock.gameObject, LayerMask.NameToLayer("Blocks"));
            currentBlock = null;
            // ���ο� �� ����
            SpawnBlock();
        }

        BlockCount++;
        OnChangeBlockCount?.Invoke(BlockCount);
        blockMaxHeightManager.UpdateHighestPoint();
    }

    public void BlockExit(Blocks block)
    {
        BlockCount--;
        OnChangeBlockCount?.Invoke(BlockCount);
        blockMaxHeightManager.UpdateHighestPoint();
    }

    public void BlockFallen(Blocks block)
    {
        Debug.Log("���� ī�޶� �ٱ����� ������");
        //�÷��̾� ü��ó��

        // ���� ���� ���� ����
        if (currentBlock != null)
        {
            currentBlock = null;
        }
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
        Debug.Log($"SetLayer{obj.name}");
        foreach (Transform child in obj.transform)
        {
            SetLayerAll(child.gameObject, newLayer);
        }
    }
}
