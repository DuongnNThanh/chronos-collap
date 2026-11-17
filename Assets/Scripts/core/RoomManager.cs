using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Danh sách 4 Container Phòng")]
    public GameObject roomCenter;
    public GameObject roomRight;
    public GameObject roomLeft;
    public GameObject roomDeskZoom;

    // Index góc nhìn: 0 = Left, 1 = Center, 2 = Right, 3 = DeskZoom
    private int currentRoomIndex = 1;

    void Start()
    {
        UpdateRoomVisibility();
    }

    public void MoveLeft()
    {
        if (currentRoomIndex == 3) return; // Đang zoom bàn làm việc thì không xoay phòng
        currentRoomIndex--;
        if (currentRoomIndex < 0) currentRoomIndex = 2; // Xoay vòng về màn Right
        UpdateRoomVisibility();
    }

    public void MoveRight()
    {
        if (currentRoomIndex == 3) return;
        currentRoomIndex++;
        if (currentRoomIndex > 2) currentRoomIndex = 0; // Xoay vòng về màn Left
        UpdateRoomVisibility();
    }

    public void ZoomToDesk()
    {
        currentRoomIndex = 3;
        UpdateRoomVisibility();
    }

    public void ExitDeskZoom()
    {
        currentRoomIndex = 1; // Quay về màn Center
        UpdateRoomVisibility();
    }

    private void UpdateRoomVisibility()
    {
        if (roomLeft != null) roomLeft.SetActive(currentRoomIndex == 0);
        if (roomCenter != null) roomCenter.SetActive(currentRoomIndex == 1);
        if (roomRight != null) roomRight.SetActive(currentRoomIndex == 2);
        if (roomDeskZoom != null) roomDeskZoom.SetActive(currentRoomIndex == 3);
    }
}