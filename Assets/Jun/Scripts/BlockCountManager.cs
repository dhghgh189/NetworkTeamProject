using Photon.Pun.Demo.Procedural;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCountManager : MonoBehaviour
{
    private Blocks blocks;
    public int BlockCount { get; private set; } = 0;  // �� ����

    public event System.Action<int> OnChangeBlockCount;

    private void OnEnable()
    {
        // �̺�Ʈ ����
        //blocks.OnBlockEntered += BlockPlaced;
        //blocks.OnBlockExited += BlockRemoved;
    }

    private void OnDisable()
    {
        // �̺�Ʈ ���� ����
        //blocks.OnBlockEntered -= BlockPlaced;
        //blocks.OnBlockExited -= BlockRemoved;
    }

    private void BlockPlaced(Block block)
    {
        BlockCount++;
        Debug.Log($"�� ����: {BlockCount}");

        OnChangeBlockCount?.Invoke(BlockCount);
    }

    private void BlockRemoved(Block block)
    {
        BlockCount--;
        Debug.Log($"�� ����: {BlockCount}");

        OnChangeBlockCount?.Invoke(BlockCount);
    }
}
