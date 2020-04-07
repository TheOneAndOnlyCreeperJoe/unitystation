using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GUI_PDA : NetTab
{
	// GUI starting screen
	[SerializeField]
	private NetPageSwitcher mainSwitcher = null;
	[SerializeField]
	private NetPage menuPage = null;
	[SerializeField]
	private NetLabel loadedCartridge = null;
	[SerializeField]
	private NetLabel notifications = null;
	[SerializeField]
	private NetLabel idLabel = null;

	private PDA PDA;

	private void Start()
	{
		if (IsServer)
		{
			PDA = Provider.GetComponent<PDA>();
			string test = PDA.registeredName;
			OpenMenu();
		}


	}

	public void LoadComponent(int index)
	{
		//Provider.GetComponent<PDA>().LoadComponent(index);

	}

	public void ToggleFlashlight ()
	{
		Provider.GetComponent<PDA>().ToggleFlashlight();
	}

	public void OpenMenu()
	{
		mainSwitcher.SetActivePage(menuPage);
		UpdateIdStatus();
	}

	public void UpdateIdStatus()
	{
		var IdCard = PDA.IdCard;

		if (IdCard)
		{
			idLabel.SetValue = $"{IdCard.RegisteredName}, {IdCard.JobType.ToString()}";
			
		}
		else
		{
			idLabel.SetValue = "<No ID inserted>";
		}
		//Provider.GetComponent<PDA>().ServerAddAccess;


		/*foreach (Access access in AccessSyncID)
		{
			if (!PDA.HasAccess(access))
			{
				PDA.ServerAddAccess(access);
			}
			else if (PDA.HasAccess(access))
			{
				PDA.ServerRemoveAccess(access);
			}
		} */
		
	}

	public void RemoveId()
	{
		if (PDA.IdCard)
		{
			PDA.RemoveIDCard();
		}
		UpdateIdStatus();
	}

	public void CloseTab()
	{
		ControlTabs.CloseTab(Type, Provider);
	}
}