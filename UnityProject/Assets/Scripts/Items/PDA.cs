using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Serialization;


// PDA's, the lowfi smartphones of the future. Acting like a flashlight, ID holder and providing a variety of useful programs depending on the inserted cartridge.
// The notes feature has also been moved to it, so its probably a good idea to look through your victims notes after "subdueing" them.
// It also allows you to interact with doors even if the ID is stored away, allowing you to conserve space. Note that the code for this is in AccessRestrictions.cs
// Bad guys can of course use this to their advantage with the deto cartridge which can blow up PDAs, including the card inside it.
// Also serves as the default syndicate uplink (TBA).

[RequireComponent(typeof(ItemStorage))]
public class PDA : NetworkBehaviour, ICheckedInteractable<HandApply>, IInteractable<HandActivate> //,IServerSpawn, IServerDespawn
{
	[FormerlySerializedAs("IdEvent")] public IDEvent OnServerIDCardChanged = new IDEvent();
	public enum Components
	{
		Menu = 0,
		Messenger = 1,
		Flashlight = 2,
		Ringtone = 3,

		//PDA components
	}

	//The actual list of access allowed set via the server and synced to all clients
	private SyncListInt accessSyncList = new SyncListInt();
	private IDCard idCard;

	// the name under who its registed
	public string registeredName;
	// the menu that's currently open, 0 for start.
	public Components activeMenu = Components.Menu;
	// We don't use ringtones anymore, alert messages are hip and trendy. Now with actual sounds!
	public string notifcationSound;
	// Is the flsahlight on?
	public bool flashlightOn = false;

	private ItemStorage itemStorage;
	private ItemSlot itemSlot;
	// The ID that's inserted, if any.
	public IDCard IdCard => itemSlot.Item != null ? itemSlot.Item.GetComponent<IDCard>() : null;
	// The cartridge that's inseted, if any.
	public GameObject insertedCartrdige;
	// The current paintjob it has.
	public Sprite paintJob;

	// New note system, put uplink sequence here. Better remove it once you're done!
	public List<Notes> storedNotes = new List<Notes>();
	// Shows the sender, receipent and the message.
	public List<Message> messageList = new List<Message>();
	// The sequence in which you have to set your notification to unlock the syndicate menu.  Nokia -> IPhone -> Discord Sound for example.
	public Array notifcationUplinkSequence;

	public class Notes
	{
		string message;
		string time;
	}
	public class Message
	{
		// the sender and the receipent
		string sender;
		string receipent;
		//actual physical message thats send
		string message;
		//time when it arrived at the user
		string time;
	}

	public void ServerPerformInteraction(HandActivate interaction)
	{
		//Open the PDA UI
			TabUpdateMessage.Send(interaction.Performer, gameObject, NetTabType.PDA, TabAction.Open);

	}
	
	public void LoadComponent(int index)
	{/*
		switch (activeMenu)
		{
			case Components.Menu:
			default:
				TabUpdateMessage.Send(interaction.Performer, gameObject, NetTabType.PDA, TabAction.Open);
			case Components.Messenger:
			case Components.Flashlight:
			case Components.Ringtone:
		}*/
		
	}

	public void ToggleFlashlight()
	{
		flashlightOn = !flashlightOn;
		/*if (flashlightOn)
		{
			MonoBehaviour flashlightlvl = gameObject.GetComponent<MonoBehaviour.Intensity>();
			gameObject.MonoBehaviour.Intesity = 0.9;
		}
		else
		{
			gameObject.MonoBehaviour.Intesity = 0;
		}*/
	}

	private void Awake()
	{
		//we can just store a single card.
		idCard = GetComponent<IDCard>();
		itemStorage = GetComponent<ItemStorage>();
		itemSlot = itemStorage.GetIndexedItemSlot(0);
		itemSlot.OnSlotContentsChangeServer.AddListener(OnServerSlotContentsChange);
	}



	private void OnServerSlotContentsChange()
	{
		//propagate the ID change to listeners
		OnServerIDCardChanged.Invoke(IdCard);
		//TabUpdateMessage.Send(null, gameObject, NetTabType.PDA, NetTabType.PDA.UpdateIdStatus());
	}

	public bool WillInteract(HandApply interaction, NetworkSide side)
	{
		if (!DefaultWillInteract.Default(interaction, side))
			return false;

		//interaction only works if using an ID card on console
		if (!Validations.HasComponent<IDCard>(interaction.HandObject))
			return false;

		if (!Validations.CanFit(itemSlot, interaction.HandObject, side, true))
			return false;

		return true;
	}

	public void ServerPerformInteraction(HandApply interaction)
	{
		//Eject existing id card if there is one and put new one in
		if (itemSlot.Item != null)
		{
			RemoveIDCard();
		}

		Inventory.ServerTransfer(interaction.HandSlot, itemSlot);
	}


	/// <summary>
	/// Spits out ID card from console and updates login details.
	/// </summary>
	public void RemoveIDCard()
	{
		Inventory.ServerDrop(itemSlot);
	}

	// Here is the part where we go ahead and pretend that we're an ID card
	public bool HasAccess(Access access)
	{
		return accessSyncList.Contains((int)access);
	}
	public SyncListInt AccessList()
	{
		return accessSyncList;
	}
	/// <summary>
	/// Removes the indicated access from this IDCard
	/// </summary>
	[Server]
	public void ServerRemoveAccess(Access access)
	{
		if (!HasAccess(access)) return;
		accessSyncList.Remove((int)access);
	}

	/// <summary>
	/// Adds the indicated access to this IDCard
	/// </summary>
	[Server]
	public void ServerAddAccess(Access access)
	{
		if (HasAccess(access)) return;
		accessSyncList.Add((int)access);
	}
}
