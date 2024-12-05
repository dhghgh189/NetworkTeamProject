using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;
using System;
using Random = UnityEngine.Random;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPun, IPunObservable
{

    [Header("Block")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Blocks currentBlock;
    public int BlockCount = 0;
    [SerializeField] private BlockMaxHeightManager blockMaxHeightManager;
    public int InvisibleTime;
    private WaitForSeconds wsInvisibleTime;
    private Coroutine invisibleRoutine;

    [Header("PlayerStat")]
    [SerializeField] private int curHp;
    [SerializeField] private int maxHp;
    public event Action<int> OnChangeHp;

    [Header("PlayerCheck")]
    public bool IsGoal = false;
    [SerializeField] private bool canDie;

    public event System.Action<int> OnChangeBlockCount;
    public event Action OnFallenOffTheCamera;
    public event Action OnPlayerDone;

    // test
    public event Action<bool> OnProcessPanalty;

    public int TowerNumber = 0;

    [Header("PlayerUI")]
    public SpriteRenderer[] HeartImages;     // ��Ʈ �̹��� �迭 (UI���� �Ҵ�)
    public Sprite FullHeartSprite;  // ���� ��Ʈ �̹���
    public Sprite EmptyHeartSprite; // ���� ��Ʈ �̹���
    public Animator Animator;

    [SerializeField] private GameObject Heart;

    private void Start()
    {
        curHp = maxHp;

        //��Ʈ��ũ �׽�Ʈ��
        if (photonView.IsMine)
        {
            wsInvisibleTime = new WaitForSeconds(InvisibleTime);
            TowerNumber = PhotonNetwork.LocalPlayer.GetPlayerNumber();
            //print($"PlayerStart Tower{TowerNumber}");
            GameObject TowerTest = GameObject.Find($"Tower{TowerNumber}");
            blockMaxHeightManager = TowerTest.GetComponent<BlockMaxHeightManager>();
            GetComponent<PlayerMovement>().SetMaxHeightManager(blockMaxHeightManager);

            print($"Ÿ�� x ������ {TowerTest.transform.position.x}");
            transform.position = new Vector2(TowerTest.transform.position.x - 2.5f, 
                TowerTest.transform.position.y + 5f);

            UpdateHealthUI(); // ���� ���� �� ��Ʈ UI �ʱ�ȭ

            if (spawnPoint == null || blockPrefabs == null || blockPrefabs.Length == 0)
            {
                Debug.LogError("��������, ���������� �߸� ���� �Ǿ����ϴ�.");
                return;
            }

            //Start�� �ƴ� �濡 ���� ���� �� �����ϴ� ������ ���� ����
            SpawnBlock();
        }
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

    // test
    public void ProcessPanalty(bool bProcessing)
    {
        OnProcessPanalty?.Invoke(bProcessing);
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

        if (photonView.IsMine == false)
            return;

        if (IsGoal == true)
            return;

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        //SpawnPoint�� �÷��̾� ��ġ or ���� ���� y�� �ִ�ġ or Ÿ���� ���� ���ġ�� ���� ������ ����
        //GameObject newBlock = Instantiate(blockPrefabs[randomIndex], spawnPoint.position, Quaternion.identity);
        GameObject newBlock = PhotonNetwork.Instantiate(blockPrefabs[randomIndex].name, spawnPoint.position, Quaternion.identity);

        // �� �ʱ� ���̾� ���� (��: "Default")
        SetLayerAll(newBlock, LayerMask.NameToLayer("Default"));

        currentBlock = newBlock.GetComponent<Blocks>();
        currentBlock.SetOwner(this);
        currentBlock.OnBlockEntered += BlockEnter;
        currentBlock.OnBlockExited += BlockExit;
        currentBlock.OnBlockFallen += BlockFallen;
        //�̺�Ʈ ���� ���?
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        //if (photonView.IsMine == false) return;

        curHp -= damage;
        Debug.Log($"���� ü�� : {curHp}");


        OnChangeHp?.Invoke(curHp);

        //photonView.RPC("UpdateHealthUI", RpcTarget.All);
        UpdateHealthUI();

        // �÷��̾��� ü���� 0 ���ϰ� �Ǹ� �� ������ ��Ȳ�� ó��
        if (curHp <= 0
            && canDie == true)
        {
            //photonView.RPC("Die", RpcTarget.All);
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < HeartImages.Length; i++)
        {
            if (i >= curHp)
            {
                // ü���� ������ ���� ��Ʈ �̹��� ����
                HeartImages[i].sprite = EmptyHeartSprite;
            }
            else
            {
                //ü���� ������ ���� ��Ʈ �̹��� ����
                HeartImages[i].sprite = FullHeartSprite;
            }
        }
    }

    private void Die()
    {
        Animator.CrossFade("Die", 0.1f);
        //Destroy(gameObject);
        Heart.SetActive(false);
    }

    public void ReachGoal()
    {
        IsGoal = true;
        OnPlayerDone?.Invoke();
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
        // ���� ���� ���� ����
        if (currentBlock == block)
        {
            Debug.Log("���� ī�޶� �ٱ����� ������");

            // OnBlockFallen �̺�Ʈ ȣ��
            OnFallenOffTheCamera?.Invoke();

            currentBlock = null;
            // ���ο� �� ����
            SpawnBlock();
        }

        //�÷��̾� ü��ó��
        if (invisibleRoutine == null)
        {
            photonView.RPC("TakeDamage", RpcTarget.All, 1);
            SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_DAMAGED);
            invisibleRoutine = StartCoroutine(InvisibleRoutine());
        }
        blockMaxHeightManager.UpdateHighestPoint();
    }
    private IEnumerator InvisibleRoutine()
    {
        Debug.Log("<color=yellow>Invisible Start</color>");
        yield return wsInvisibleTime;
        invisibleRoutine = null;
        Debug.Log("<color=yellow>Invisible End</color>");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(BlockCount);
            stream.SendNext(IsGoal);
            //stream.SendNext(curHp);
        }
        else
        {
            BlockCount = (int)stream.ReceiveNext();
            IsGoal = (bool)stream.ReceiveNext();
            //curHp = (int)stream.ReceiveNext();
        }
    }

    public void SetLayerAll(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        //Debug.Log($"SetLayer{obj.name}");
        foreach (Transform child in obj.transform)
        {
            SetLayerAll(child.gameObject, newLayer);
        }
    }
}
