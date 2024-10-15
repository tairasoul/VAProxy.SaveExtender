using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using Rewired;

namespace SaveExtender {
	[HarmonyPatch(typeof(SaveSlotSelect))]
	internal class SaveSlotSelectPatch {

		private static void ScrollToElement(GameObject element) {
			ScrollRect scrollRect = GameObject.Find("Canvas").Find("SlotSelect").Find("Scroll View").GetComponent<ScrollRect>();
			Vector3 targetPosition = scrollRect.viewport.InverseTransformPoint(element.GetComponent<RectTransform>().position);
			
			int offset = 40;

			float viewportTop = scrollRect.viewport.rect.yMax - offset;
			float viewportBottom = scrollRect.viewport.rect.yMin + offset;

			if (targetPosition.y > viewportTop) {
				float scrollAmount = (targetPosition.y - viewportTop) / scrollRect.content.rect.height;
				int maxIterations = 100;
				int iterations = 0;
				while (targetPosition.y > viewportTop && iterations < maxIterations) {
					scrollRect.verticalNormalizedPosition += scrollAmount;
					targetPosition = scrollRect.viewport.InverseTransformPoint(element.GetComponent<RectTransform>().position); 
					iterations++;
				}
			}
			else if (targetPosition.y < viewportBottom) {
				float scrollAmount = (viewportBottom - targetPosition.y) / scrollRect.content.rect.height;
				int maxIterations = 100;
				int iterations = 0;
				while (targetPosition.y < viewportBottom && iterations < maxIterations) {
					scrollRect.verticalNormalizedPosition -= scrollAmount;
					targetPosition = scrollRect.viewport.InverseTransformPoint(element.GetComponent<RectTransform>().position);
					iterations++;
				}
			}
		}

		private static void ClearSlotData(int ID)
		{
			GameObject canvas = GameObject.Find("Canvas");
			GameObject slotSelect = canvas.transform.Find("SlotSelect").gameObject;
			GameObject scrollView = slotSelect.transform.Find("Scroll View").gameObject;
			GameObject viewport = scrollView.transform.Find("Viewport").gameObject;
			GameObject content = viewport.transform.Find("Content").gameObject;

			SaveSlotSelect select = content.GetComponent<SaveSlotSelect>();
			string[] items = select.items;
			for (int i = 0; i < items.Length; i++)
			{
				PlayerPrefs.SetInt(items[i] + ID, 0);
				PlayerPrefs.SetInt(items[i] + "_Chip1" + ID, 0);
				PlayerPrefs.SetInt(items[i] + "__" + ID, 0);
				PlayerPrefs.SetInt(items[i] + "___" + ID, 0);
			}
			PlayerPrefs.SetInt("fresh" + ID, 0);
			PlayerPrefs.SetString("Dialogue" + ID, null);
			PlayerPrefs.SetInt("Memories" + ID, 0);
			PlayerPrefs.SetInt("CS" + ID, 0);
			PlayerPrefs.SetInt("DroneGun" + ID, 0);
			PlayerPrefs.SetInt("DroneGunS2" + ID, 0);
			PlayerPrefs.SetInt("S1" + ID, 0);
			PlayerPrefs.SetInt("stamina" + ID, 0);
			PlayerPrefs.SetInt("Progress" + ID, 0);
			PlayerPrefs.SetInt("ui" + ID, 0);
			PlayerPrefs.SetInt("Scanner" + ID, 0);
			PlayerPrefs.SetInt("Compass" + ID, 0);
			PlayerPrefs.SetInt("coreDash" + ID, 0);
			PlayerPrefs.SetInt("glitch" + ID, 0);
			PlayerPrefs.SetInt("W1" + ID, 0);
			PlayerPrefs.SetInt("W2" + ID, 0);
			PlayerPrefs.SetInt("Dress" + ID, 0);
			PlayerPrefs.SetFloat("GameTime" + ID, 0f);
			PlayerPrefs.SetInt("Timer" + ID, 0);
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool Update(SaveSlotSelect __instance) {
			Player player = ReInput.players.GetPlayer("Player0");
			if (player.GetNegativeButtonDown("MovementY") || player.GetButtonDown("D_Down"))
			{
				__instance.currentSlot++;
				if (__instance.currentSlot > __instance.slots.Length - 1)
				{
					__instance.currentSlot = 0;
				}
				if (__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot") != null && __instance.slots[__instance.currentSlot].gameObject.Find("Reset").activeSelf) {
					if (__instance.selectionX > 1)
					{
						__instance.selectionX = 0;
					}
				}
				else {
					__instance.selectionX = 0;
				}
				ScrollToElement(__instance.slots[__instance.currentSlot].gameObject);
				__instance.Promt.gameObject.SetActive(value: false);
			}
			if (player.GetButtonDown("MovementY") || player.GetButtonDown("D_Up"))
			{
				__instance.Promt.gameObject.SetActive(value: false);
				__instance.currentSlot--;
				if (__instance.currentSlot < 0)
				{
					__instance.currentSlot = __instance.slots.Length - 1;
				}
				if (__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot") != null && __instance.slots[__instance.currentSlot].gameObject.Find("Reset").activeSelf) {
					if (__instance.selectionX > 1)
					{
						__instance.selectionX = 0;
					}
				}
				else {
					__instance.selectionX = 0;
				}
				ScrollToElement(__instance.slots[__instance.currentSlot].gameObject);
			}
			if (player.GetNegativeButtonDown("MovementX") || player.GetButtonDown("D_Left"))
			{
				__instance.Promt.gameObject.SetActive(value: false);
				if (__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot") == null) {
					__instance.selectionX++;
					if (__instance.selectionX > 2)
					{
						__instance.selectionX = 0;
					}
				}
				else {
					if (__instance.slots[__instance.currentSlot].gameObject.Find("Reset").activeSelf) {
						__instance.selectionX++;
						if (__instance.selectionX > 1)
						{
							__instance.selectionX = 0;
						}
					}
				}
			}
			if (player.GetButtonDown("MovementX") || player.GetButtonDown("SwitchItem"))
			{
				__instance.Promt.gameObject.SetActive(value: false);
				if (__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot") == null) {
					__instance.selectionX--;
					if (__instance.selectionX < 0)
					{
						__instance.selectionX = 2;
					}
				}
				else {
					if (__instance.slots[__instance.currentSlot].gameObject.Find("Reset").activeSelf) {
						__instance.selectionX--;
						if (__instance.selectionX < 0)
						{
							__instance.selectionX = 1;
						}
					}
				}
			}
			if (player.GetButtonDown("Menu")) 
			{
				__instance.Back.Invoke();
			}
			if (player.GetButtonDown("Action"))
			{
				if (__instance.currentSlot == 1 && __instance.selectionX == 0)
				{
					__instance.Promt.gameObject.SetActive(value: true);
					__instance.Promt.GetComponentInChildren<InputField>().ActivateInputField();
					return false;
				}
				if (__instance.selectionX == 1)
				{
					if (!__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot")) {
						ClearSlotData(__instance.currentSlot);
						PlayerPrefs.SetFloat("GameTime" + __instance.slots[__instance.currentSlot].ID, 0f);
					}
					else {
						Plugin.Instance.ExtraSlots--;
						PlayerPrefs.SetInt("save-extender-extra-slots", Plugin.Instance.ExtraSlots);
						PlayerPrefs.SetFloat("GameTime" + __instance.slots[__instance.currentSlot].ID, 0f);
						GameObject.Destroy(__instance.slots[__instance.slots.Length - 2].gameObject);
						GameObject.Destroy(__instance.slots[__instance.slots.Length - 1].gameObject);
						List<SaveSlot> slots = __instance.slots.ToList();
						slots.RemoveAt(slots.Count() - 2);
						slots.RemoveAt(slots.Count() - 1);
						__instance.slots = slots.ToArray();
						__instance.currentSlot--;
						Plugin.Instance.CurrentPlusObject = Plugin.Instance.CreatePlusObject();
						if (Plugin.Instance.ExtraSlots <= 0) {
							__instance.selectionX = 0;
							Plugin.Instance.CurrentPlusObject.Find("Reset").SetActive(false);
						}
						return false;
					}
				}
				if (__instance.selectionX == 0)
				{
					if (!__instance.slots[__instance.currentSlot].gameObject.transform.Find("isExtensionSlot")) {
						PlayerPrefs.SetInt("Slot", __instance.slots[__instance.currentSlot].ID);
						UnityEngine.Object.FindObjectOfType<HomeScreen>().Star();
					}
					else {
						Plugin.Instance.TurnPlusObjectIntoSave(Plugin.Instance.CurrentPlusObject);
						Plugin.Instance.ExtraSlots++;
						Plugin.Instance.CurrentPlusObject = Plugin.Instance.CreatePlusObject();
						PlayerPrefs.SetInt("save-extender-extra-slots", Plugin.Instance.ExtraSlots);
					}
				}
				if (__instance.selectionX == 2 && __instance.slots[__instance.currentSlot].Selects[2].gameObject.activeSelf)
				{
					__instance.slots[__instance.currentSlot].Selects[2].gameObject.SetActive(value: false);
					__instance.slots[__instance.currentSlot].Selects[3].gameObject.SetActive(value: true);
					PlayerPrefs.SetInt("Timer" + __instance.slots[__instance.currentSlot].ID, 1);
					return false;
				}
				if (__instance.selectionX == 2 && __instance.slots[__instance.currentSlot].Selects[3].gameObject.activeSelf)
				{
					__instance.slots[__instance.currentSlot].Selects[2].gameObject.SetActive(value: true);
					__instance.slots[__instance.currentSlot].Selects[3].gameObject.SetActive(value: false);
					PlayerPrefs.SetInt("Timer" + __instance.slots[__instance.currentSlot].ID, 0);
				}
			}
			SaveSlot[] array = __instance.slots;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Selected(stat: false);
			}
			__instance.slots[__instance.currentSlot].Selected(stat: true);
			__instance.slots[__instance.currentSlot].selectionX = __instance.selectionX;
			return false;
		}
	}
}