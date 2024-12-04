using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Blocks : MonoBehaviourPun, IPunObservable
{
    #region Serialize Field
    [SerializeField] private float basicFallSpeed;          // �ϰ� �ӵ�
    [SerializeField] private float fastFallSpeed;           // ���� �ϰ� �ӵ�
    [SerializeField] private GameObject spotlightObject;    // spotlight ������Ʈ
    [SerializeField] private float moveAmount;              // �Ϲ� �̵���
    [SerializeField] private float pushAmount;              // ��ġ�� �̵���
    [SerializeField] private float rotateAmount;            // ȸ����
    [SerializeField] private float moveDelay;               // �̵� �� �ο��� ������
    [SerializeField] private GameObject[] tiles;            // �� Ÿ�ϵ�
    [SerializeField] private LayerMask castLayer;           // ����ĳ��Ʈ�� layermask
    [SerializeField] private float rotateSpeed;             // ȸ�� �ӵ�
    [SerializeField] private Vector2 blockSize;             // �� ������ (Ÿ�� �ϳ��� 0.5�� ���)  
    [SerializeField] private float panaltyDrag;             // Ÿ�� ��� �� ������ Drag
    #endregion

    #region Private Field
    private Rigidbody2D rigid;              // Rigidbody2D ������Ʈ ����                                          
    private Vector2 currentVelocity;        // ���� �ϰ� �ӵ�
    private Vector2 currentDirection;       // ���� �̵� ����
    private float currentAmount;            // ���� �̵���
    private float targetAngle;              // ��ǥ ȸ����              

    private bool isControllable;            // ���� ���� ����
    private bool isFastDown;                // ���� �ϰ� ����
    private bool isPushing;                 // ��ġ�� ����
    private bool isRotate;                  // ȸ�� ����
    private bool isEntered;                 // �� ���� ����
    private bool isVertical = false;        // ȸ�� ���¸� Ȯ���ϱ� ���� flag

    private int collisionCount = 0;         // ���� ���� �浹���ִ� �浹ü�� �� (exit ������ ���)
    private float normalDrag;               // �Ϲ� ������ Drag ��ġ (���� ������ Drag) 

    private Coroutine moveRoutine;          // �̵� �� ����� �ڷ�ƾ
    private WaitForSeconds wsMoveDelay;     // �̵� �ڷ�ƾ���� Ȱ���� WaitForSeconds ��ü

    private PlayerController owner;         // �ڽ��� ������ Player

    private Coroutine freezeRotateRoutine;  // ȸ�� ó���� ���� �ڷ�ƾ
    #endregion

    #region Public Field
    public UnityAction<Blocks> OnBlockFallen;       // ���� �ʹ����� ������ �� Invoke (ü�°���, ��� ó���� Ȱ��)
    public UnityAction<Blocks> OnBlockEntered;      // ���� �������� �� Invoke (�� ī���ÿ� Ȱ��)
    public UnityAction<Blocks> OnBlockExited;       // ���� �����ߴٰ� ��� �� (�� ī���ÿ� Ȱ��)
  
    public bool IsControllable { get { return isControllable; } } // �ܺο��� ���� ���ɿ��θ� Ȯ���ϱ� ���� ������Ƽ
    public bool IsEntered { get { return isEntered; } }     // �ܺο��� �������θ� Ȯ���ϱ� ���� ������Ƽ
    public PlayerController Owner { get { return owner; } } // �ܺο��� �� Owner�� Ȯ���ϱ� ���� ������Ƽ
    #endregion

    private void Awake()
    {
        // ������Ʈ ����
        rigid = GetComponent<Rigidbody2D>();

        // ���� Drag ����
        normalDrag = rigid.drag;
    }

    private void Start()
    {
        isControllable = true;
        wsMoveDelay = new WaitForSeconds(moveDelay);

        // ��Ʈ��ũ or local ó��
        if (PhotonNetwork.IsConnected)
        {
            // �ڽ��� ���� �ƴϸ� spotlight�� ������ �ʵ��� �Ѵ�.
            if (photonView.IsMine)
                SetSpotlight();
            else
                spotlightObject.SetActive(false);
        }
        else
        {
            SetSpotlight();
        }

        // �⺻ �ϰ� �ӵ� ����
        currentVelocity = new Vector2(rigid.velocity.x, basicFallSpeed);
    }

    private void Update()
    {
        // Ÿ���� ���� �ε��� ���ĺ��ʹ� �ش� ���� ���� ���� �Ұ���
        if (!isControllable)
        {
            return;
        }

        // �̵�, ��ġ�� ���� Ȯ���Ͽ� amount ����
        currentAmount = isPushing ? pushAmount : moveAmount;
        // �̵� or ��ġ�� ����
        ExecuteMove();

        // ���� �ϰ� ���� Ȯ���Ͽ� velocity ����
        currentVelocity.y = isFastDown ? fastFallSpeed : basicFallSpeed;
        // �ϰ�
        rigid.velocity = currentVelocity;

        // flag �ʱ�ȭ
        // �÷��̾ ���������� key�� ������ �ִ´ٸ� �ٽ� true�� �� ���̰�
        // �÷��̾ key�� ���̻� ������ �ʴ´ٸ� false�� ���·� ���ӵ� ���̴�.
        isPushing = false;
        isFastDown = false;
    }

    private void FixedUpdate()
    {
        if (!isControllable)
            return;

        if (isRotate)
        {
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (rigid.rotation >= targetAngle)
                rigid.rotation = targetAngle;
            else
                rigid.rotation += rotateSpeed;

            if (freezeRotateRoutine == null)
                freezeRotateRoutine = StartCoroutine(FreezeRotateRoutine());
        }
    }

    private IEnumerator FreezeRotateRoutine()
    {
        var delay = new WaitForSeconds(0.2f);
        yield return delay;
        // spotlight�� ũ�� ����
        SetSpotlight();

        rigid.constraints = RigidbodyConstraints2D.None;

        spotlightObject.SetActive(isControllable);

        // �ٽ� ȸ���� �����ϵ��� ȸ�� �Ϸ� ó��
        isRotate = false;
        freezeRotateRoutine = null;
    }

    public void ChangeDrag(bool bPanalty)
    {
        // Ÿ�� ��� ���� ��
        if (bPanalty)
        {
            // �������ִ� ���� ���
            if (isEntered)
            {
                rigid.drag = panaltyDrag;
                rigid.velocity = Vector2.zero;
            }
        }
        // Ÿ�� ��� ���� ��
        else
        {
            rigid.drag = normalDrag;
            rigid.velocity = Vector2.zero;
        }
    }

    public void SetOwner(PlayerController player)
    {
        // owner ����
        owner = player;
        // �̺�Ʈ ����
        owner.OnPlayerDone += Freeze;
        owner.OnProcessPanalty += ChangeDrag;
    }

    // Player�� ���� �ϰ� ������ ���� Interface
    public void Down()
    {
        isFastDown = true;
    }

    // Player�� �̵� ������ ���� Interface
    public void Move(Vector2 dir)
    {
        currentDirection = dir;
    }

    // Player�� ��ġ�� ������ ���� Interface
    public void Push(Vector2 dir)
    {
        currentDirection = dir;
        isPushing = true;
    }

    public void Rotate()
    {
        if (!isControllable)
            return;

        // ȸ�� ���̸� return
        if (isRotate)
            return;

        // ���� ȸ���� ����
        targetAngle += 90;

        // ȸ���� spotlight ��Ȱ��ȭ
        spotlightObject.SetActive(false);

        // flag set
        isRotate = true;
    }

    private void Freeze()
    {
        // ���� �Ұ� ���·� ����
        isControllable = false;

        if (rigid != null)
        {
            // �� �̻� ����ó�� ���� �ʵ��� ����
            rigid.simulated = false;

            // velocity �ʱ�ȭ
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0;
        }

        SpriteRenderer sr;
        foreach (GameObject tile in tiles)
        {
            sr = tile.GetComponent<SpriteRenderer>();
            // sprite color�� �����Ͽ� freeze ���¸� Ȯ���� �� �ֵ��� ��
            sr.color = Color.gray;
        }
    }

    private void SetSpotlight()
    {
        if (isVertical)
        {
            spotlightObject.transform.localScale = new Vector3(blockSize.y, spotlightObject.transform.localScale.y, spotlightObject.transform.localScale.z);

            // ���� ȸ���� �� ���� ȸ���Ǿ� ������ Ʋ�����Ƿ� ������ ���� ��ȸ�� ���ش�.
            spotlightObject.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            spotlightObject.transform.localScale = new Vector3(blockSize.x, spotlightObject.transform.localScale.y, spotlightObject.transform.localScale.z);

            // ���� ȸ���� �� ���� ȸ���Ǿ� ������ Ʋ�����Ƿ� ������ ���� ��ȸ�� ���ش�.
            spotlightObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        // ȸ�� ��Ȳ ������Ʈ
        isVertical = !isVertical;
    }

    // ���� �̵�, ��ġ�⸦ ����
    private void ExecuteMove()
    {
        // �̵��� ���� ���
        if (currentDirection == Vector2.zero)
        {
            // �̵� �ڷ�ƾ ����
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
            return;
        }

        // ���� �̵� �ڷ�ƾ�� ������� �ʾҴٸ� ����
        if (moveRoutine == null)
        {
            moveRoutine = StartCoroutine(MoveRoutine());
        }
    }

    // �̵� �ڷ�ƾ�� ���� �̵� Ÿ�̹��� ����
    // Update���� �ٷ� �̵��ع����� �̵��ӵ� ��� �ȉ´�
    // �߰��� �¿� �̵� �� �浹 ���� �� �浹�� ó���Ѵ�.
    private IEnumerator MoveRoutine()
    {
        Vector2 lastDir; // ������ ����
        float lastAmount; // ������ ��
        Vector2 moveDist; // ������ �Ÿ�

        RaycastHit2D hit1; // ���� üũ ���
        RaycastHit2D hit2; // �Ʒ��� üũ ���
        Vector2 startPos1; // ���� ���� ��ġ
        Vector2 startPos2; // �Ʒ��� ���� ��ġ

        RaycastHit2D resultHit; // ���������� ������ �浹 ��� (���ʰ� �Ʒ����� �Ǻ��ؼ� ���������� ����)
        Vector2 resultStartPos; // ���������� ������ ���� ��ġ (����)

        Vector2 toHit; // ���� ���� ��ġ -> ������ ��ġ�� ���ϴ� ����

        bool hitWall = false;

        // ��� ������ ���� ����
        while (isControllable)
        {
            moveDist = currentDirection * currentAmount;
            lastDir = currentDirection;
            lastAmount = currentAmount;

            // �浹 ���� ����
            // position �̵� �ÿ� ������ġ�� �����Ǵ� ��ü�� ������ ���� �̵����� �ʰ� ���� ó��
            for (int i = 0; i < tiles.Length; i++)
            {
                // �� Ÿ���� ��ġ�κ��� �̵��� �������� raycast
                startPos1 = (Vector2)tiles[i].transform.position + (lastDir * 0.25f) + (Vector2.up * 0.22f);
                startPos2 = (Vector2)tiles[i].transform.position + (lastDir * 0.25f) + (Vector2.down * 0.22f);
                hit1 = Physics2D.Raycast(startPos1, lastDir, lastAmount - 0.05f, castLayer);
                hit2 = Physics2D.Raycast(startPos2, lastDir, lastAmount - 0.05f, castLayer);

                // �浹������ �ϳ��� �ƴ��� Ȯ��
                if (hit1.collider == null && hit2.collider == null)
                    continue;

                // �Ѵ� ���� �� ���
                if (hit1.collider != null && hit2.collider != null)
                {
                    resultHit = hit1.distance <= hit2.distance ? hit1 : hit2;
                    resultStartPos = hit1.distance <= hit2.distance ? startPos1 : startPos2;
                }
                // hit1�� ������ ���
                else if (hit1.collider != null)
                {
                    resultHit = hit1;
                    resultStartPos = startPos1;
                }
                // hit2�� ������ ���
                else
                {
                    resultHit = hit2;
                    resultStartPos = startPos2;
                }
                
                // �θ� ������Ʈ�� hit�ȴٸ� �ش� Ÿ���� �߰��� ���� Ÿ���̹Ƿ� �Ѿ��.
                if (resultHit.transform == transform)
                    continue;

                if (resultHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    //rigid.rotation = targetAngle;
                    //print($"�̵� �߿� ȸ��ó��{rigid.rotation}");

                    Debug.Log("Hit Wall (MoveRoutine)");
                    hitWall = true;
                    break;
                }

                // ������� ����
                isControllable = false;

                // ���� �浹 �� �浹 ȿ���� ���
                SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_FIRSTCOLLISION);

                // spotlight ��Ȱ��ȭ
                spotlightObject.SetActive(false);

                // �浹 �˻縦 ������ ��ġ�� ���� �浹�� ���������� �Ÿ��� Ȯ��
                toHit = resultHit.point - resultStartPos;
                // �浹�� ���������� ��ġ�� �� �پ����� �ʴ� ���
                // (��ġ���� ��� ���� 2ĭ���� (1f) �����̱� ������ �ʿ��� ����)
                if (toHit.magnitude > 0.01f)
                {
                    Debug.Log("pos calibration");
                    // ����� ��ġ�� �������ش�.
                    transform.position += new Vector3(toHit.x, 0, 0);
                }

                // �浹 �� �浹�� ��ġ�� �������� �ڽſ��� �ݴ�������� ���� ���Ѵ�.
                rigid.AddForceAtPosition(-lastDir * lastAmount, resultHit.point, ForceMode2D.Impulse);

                // �浹�� ��� ���� ���ؼ��� ���� �����ش�.                
                // ���� �浹�� ��� ������ٵ�� �θ� �ֱ� ������ GetComponentInParent�� ���
                Rigidbody2D otherRigid = resultHit.collider.GetComponentInParent<Rigidbody2D>();

                // ��������� ��� ������Ʈ���� rigidbody�� ã������ ���� �����ش�.
                if (otherRigid != null)
                    otherRigid.AddForceAtPosition(lastDir * lastAmount, resultHit.point, ForceMode2D.Impulse);

                // for debug
                Debug.Log($"Horizontal Collision : {tiles[i].name} > {resultHit.collider.name}");
                Debug.Log($"origin : {(Vector2)tiles[i].transform.position}, direction : {lastDir}");

                yield break;
            }

            if (!hitWall)
            {
                // ���������� �̵��ؾ� �ϹǷ� position���� �����Ѵ�.
                rigid.position += moveDist;
            }

            // delay��ŭ ���
            yield return wsMoveDelay;
        }

        // �ڷ�ƾ ���� �ʱ�ȭ
        moveRoutine = null;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            //rigid.rotation = targetAngle;
            //print($"�� �浹 �� ȸ��ó��{rigid.rotation}");

            Debug.Log("Hit Wall (OnCollision)");
            return;
        }

        spotlightObject.SetActive(false);

        if (isControllable)
        {
            rigid.simulated = false;
            rigid.velocity = Vector2.zero;
            rigid.angularVelocity = 0;
            rigid.simulated = true;

            // �浹 �� flag ���� 
            isControllable = false;

            // ���� �浹 �� �浹 ȿ���� ���
            SoundManager.Instance.Play(Enums.ESoundType.SFX, SoundManager.SFX_FIRSTCOLLISION);
        }

        // �浹ü ī��Ʈ ����
        collisionCount++;

        // �̹� enter�� ���¸� return
        if (isEntered)
            return;

        // enter flag set
        isEntered = true;

        // �̺�Ʈ �߻�
        OnBlockEntered?.Invoke(this);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!photonView.IsMine)
            return;

        // ������ ���¸� exit ó�� ����
        if (owner.IsGoal)
            return;

        // �浹ü ī��Ʈ ����
        collisionCount--;

        // ���� enter�� ���°� �ƴϸ� return
        if (!isEntered)
            return;

        // �浹���� ��ü�� �ִٸ� exit�� �ƴѰ����� ����
        if (collisionCount > 0)
            return;

        // enter flag set
        isEntered = false;

        // �̺�Ʈ �߻�
        OnBlockExited?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �� �߶� ���� Ȯ��
        if (other.CompareTag("FallTrigger"))
        {
            // �̺�Ʈ �߻�
            OnBlockFallen?.Invoke(this);
            // �ٷ� ����
            Destroy(gameObject);
        }
    }

    // �̺�Ʈ ����
    private void OnDestroy()
    {
        if (owner != null)
        {
            owner.OnPlayerDone -= Freeze;
            owner.OnProcessPanalty -= ChangeDrag;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (tiles.Length <= 0)
            return;

        for (int i = 0; i < tiles.Length; i++)
        {
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.up * 0.22f), Vector3.left * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.left*0.25f) + (Vector3.down * 0.22f), Vector3.left * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.up * 0.22f), Vector3.right * 0.2f);
            Gizmos.DrawRay((tiles[i].transform.position+Vector3.right*0.25f) + (Vector3.down * 0.22f), Vector3.right * 0.2f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rigid.simulated);
            stream.SendNext(rigid.rotation);
            stream.SendNext(rigid.position);
            stream.SendNext(rigid.drag);
        }
        else
        {
            rigid.simulated = (bool)stream.ReceiveNext();
            rigid.rotation = (float)stream.ReceiveNext();
            rigid.position = (Vector2)(stream.ReceiveNext());
            rigid.drag = (float)stream.ReceiveNext();
        }
    }
}
