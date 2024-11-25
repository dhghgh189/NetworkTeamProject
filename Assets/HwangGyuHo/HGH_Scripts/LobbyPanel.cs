using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] RectTransform roomContent;
    [SerializeField] RoomEntry roomEntryPrefab;

    // ����� ������ �ʿ䰡 �ִ�.
    // ��ųʸ� : ���� ���� �� �߿� �̸��� �ش��ϴ� ���� ã�� �����ϱ�
    private Dictionary<string, RoomEntry> roomDictionary = new Dictionary<string, RoomEntry>();

    public void LeaveLobby()
    {
        Debug.Log("�κ� ���� ��û");
        PhotonNetwork.LeaveLobby();
    }

    // �� ��Ȳ�� ������Ʈ ���ִ� �Լ�
    public void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // ���� ����� ��� + ���� ������� ��� + ������ �Ұ����� ���� ���
            if (info.RemovedFromList == true || info.IsVisible == false || info.IsOpen == false)
            {
                // ���ܻ�Ȳ : �κ� ���ڸ��� ������� ���� ���
                // �κ� ���ڸ��� ������� �� �׷� ��� ����ó���� �ʿ��ϴ�
                // �� ��Ͽ� �߰��� �� ���µ�, ��� �� ������ ���ص� �ȴ�
                // �ٸ��浵 ó���ؾ� �ؼ� continue;
                if (roomDictionary.ContainsKey(info.Name) == false)
                    continue;

                // ��Ʈ�������� �������� �� �ʿ䰡 �־ Destroy�� �Ἥ ����
                Destroy(roomDictionary[info.Name].gameObject);
                // �� ���ǿ� �´� �͵��� ����Ʈ���� �����ǵ��� �Ѵ�
                roomDictionary.Remove(info.Name);
            }
            // ���� ���� ���� ���, �� ��Ͽ� ������ ���ϰŴ�
            else if (roomDictionary.ContainsKey(info.Name) == false)
            {
                // �濡 ���� ���ӿ�����Ʈ�� �������� �ʿ䰡 �ִ�
                // roomContent �� �ڽ����� ������ش�
                RoomEntry roomEntry = Instantiate(roomEntryPrefab, roomContent);
                // ���� ���ο� ���� ���� �Ʒ���  Add �� �߰�
                roomDictionary.Add(info.Name, roomEntry);
                // TODO : �� ���� ����
                roomEntry.SetRoomInfo(info);
            }
            // ���� ������ ����� ���
            else if (roomDictionary.ContainsKey((string)info.Name) == true)
            {
                RoomEntry roomEntry = roomDictionary[info.Name];
                //�� ���� ����
                roomEntry.SetRoomInfo(info);
            }
        }
    }

    public void ClearRoomEntries()
    {
        // �����ߴ� ����� ��ȸ�ϸ鼭 �����ִ� �۾�
        foreach (string name in roomDictionary.Keys)
        {
            Destroy(roomDictionary[name].gameObject);
        }
        // ��ųʸ� ��Ȳ�� �� �̻� �ʿ�� ���� ���� �� �����ϱ�
        // ����� �Լ��� ������ ����.
        roomDictionary.Clear();
    }
}
