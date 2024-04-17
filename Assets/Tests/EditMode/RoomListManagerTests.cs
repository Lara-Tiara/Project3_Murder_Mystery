using NUnit.Framework;
using UnityEngine;
using Moq;
using System.Collections.Generic;
using Photon.Realtime;

public class RoomListManagerTests
{
    private RoomListManager roomListManager;
    private GameObject gameObject;

    [SetUp]
    public void Setup()
    {
        // Create a GameObject and add RoomListManager to it
        gameObject = new GameObject();
        roomListManager = gameObject.AddComponent<RoomListManager>();

        // Set up any initial states if necessary
    }

    [Test]
    public void UpdateCachedRoomList_RemovesRoom_WhenRemovedFromList()
    {
        // Arrange
        var mockRoomInfo1 = new Mock<RoomInfo>();
        mockRoomInfo1.Setup(x => x.Name).Returns("Room1");
        mockRoomInfo1.Setup(x => x.RemovedFromList).Returns(true);

        var roomList = new List<RoomInfo> { mockRoomInfo1.Object };

        // Act
        roomListManager.OnRoomListUpdate(roomList);

        // Assert
        Assert.IsFalse(roomListManager.IsRoomCached("Room1"));
    }

    [Test]
    public void UpdateCachedRoomList_AddsRoom_WhenNotRemovedFromList()
    {
        // Arrange
        var mockRoomInfo1 = new Mock<RoomInfo>();
        mockRoomInfo1.Setup(x => x.Name).Returns("Room1");
        mockRoomInfo1.Setup(x => x.RemovedFromList).Returns(false);
        mockRoomInfo1.Setup(x => x.PlayerCount).Returns(3);

        var roomList = new List<RoomInfo> { mockRoomInfo1.Object };

        // Act
        roomListManager.OnRoomListUpdate(roomList);

        // Assert
        Assert.IsTrue(roomListManager.IsRoomCached("Room1"));
        Assert.AreEqual(1, roomListManager.GetCachedRoomCount());
    }

    [Test]
    public void UpdateCachedRoomList_DoesNotAddRoom_WhenPlayerCountIsZero()
    {
        // Arrange
        var mockRoomInfo1 = new Mock<RoomInfo>();
        mockRoomInfo1.Setup(x => x.Name).Returns("Room1");
        mockRoomInfo1.Setup(x => x.RemovedFromList).Returns(false);
        mockRoomInfo1.Setup(x => x.PlayerCount).Returns(0);

        var roomList = new List<RoomInfo> { mockRoomInfo1.Object };

        // Act
        roomListManager.OnRoomListUpdate(roomList);

        // Assert
        Assert.IsFalse(roomListManager.IsRoomCached("Room1"));
    }

    [TearDown]
    public void Teardown()
    {
        // Destroy game object after test
        GameObject.DestroyImmediate(gameObject);
    }
}