using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace SaveExtender {
    [HarmonyPatch(typeof(SaveSlotSelect))]
    internal class SaveSlotSelectPatch {
        static private int GetCurrentSlot(SaveSlotSelect __instance) {
            FieldInfo currentSlotInfo = typeof(SaveSlotSelect).GetField("currentSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)currentSlotInfo.GetValue(__instance);
        }

        static private void SetCurrentSlot(SaveSlotSelect __instance, int slot) {
            FieldInfo currentSlotInfo = typeof(SaveSlotSelect).GetField("currentSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            currentSlotInfo.SetValue(__instance, slot);
        }

        static private int GetSelectionX(SaveSlotSelect __instance) {
            FieldInfo selectionXInfo = typeof(SaveSlotSelect).GetField("selectionX", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)selectionXInfo.GetValue(__instance);
        }

        static private void SetSelectionX(SaveSlotSelect __instance, int selection) {
            FieldInfo selectionXInfo = typeof(SaveSlotSelect).GetField("selectionX", BindingFlags.Instance | BindingFlags.NonPublic);
            selectionXInfo.SetValue(__instance, selection);
        }

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

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static bool Update(SaveSlotSelect __instance) {
            int currentSlot = GetCurrentSlot(__instance);
            int selectionX = GetSelectionX(__instance);
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                currentSlot++;
                if (currentSlot > __instance.slots.Length - 1)
                {
                    currentSlot = 0;
                }
                if (__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot") != null && __instance.slots[currentSlot].gameObject.Find("Reset").activeSelf) {
                    if (selectionX > 1)
                    {
                        selectionX = 0;
                    }
                }
                else {
                    selectionX = 0;
                }
                SetSelectionX(__instance, selectionX);
                SetCurrentSlot(__instance, currentSlot);
                ScrollToElement(__instance.slots[currentSlot].gameObject);
                __instance.Promt.gameObject.SetActive(value: false);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                __instance.Promt.gameObject.SetActive(value: false);
                currentSlot--;
                if (currentSlot < 0)
                {
                    currentSlot = __instance.slots.Length - 1;
                }
                if (__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot") != null && __instance.slots[currentSlot].gameObject.Find("Reset").activeSelf) {
                    if (selectionX > 1)
                    {
                        selectionX = 0;
                    }
                }
                else {
                    selectionX = 0;
                }
                SetSelectionX(__instance, selectionX);
                ScrollToElement(__instance.slots[currentSlot].gameObject);
                SetCurrentSlot(__instance, currentSlot);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                __instance.Promt.gameObject.SetActive(value: false);
                if (__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot") == null) {
                    selectionX++;
                    if (selectionX > 2)
                    {
                        selectionX = 0;
                    }
                    SetSelectionX(__instance, selectionX);
                }
                else {
                    if (__instance.slots[currentSlot].gameObject.Find("Reset").activeSelf) {
                        selectionX++;
                        if (selectionX > 1)
                        {
                            selectionX = 0;
                        }
                        SetSelectionX(__instance, selectionX);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                __instance.Promt.gameObject.SetActive(value: false);
                if (__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot") == null) {
                    selectionX--;
                    if (selectionX < 0)
                    {
                        selectionX = 2;
                    }
                    SetSelectionX(__instance, selectionX);
                }
                else {
                    if (__instance.slots[currentSlot].gameObject.Find("Reset").activeSelf) {
                        selectionX--;
                        if (selectionX < 0)
                        {
                            selectionX = 1;
                        }
                        SetSelectionX(__instance, selectionX);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (currentSlot == 1 && selectionX == 0)
                {
                    __instance.Promt.gameObject.SetActive(value: true);
                    __instance.Promt.GetComponentInChildren<InputField>().ActivateInputField();
                    return false;
                }
                if (selectionX == 1)
                {
                    if (!__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot")) {
                        __instance.ClearSlotData(currentSlot);
                        PlayerPrefs.SetFloat("GameTime" + __instance.slots[currentSlot].ID, 0f);
                    }
                    else {
                        Plugin.Instance.ExtraSlots--;
                        PlayerPrefs.SetInt("save-extender-extra-slots", Plugin.Instance.ExtraSlots);
                        PlayerPrefs.SetFloat("GameTime" + __instance.slots[currentSlot].ID, 0f);
                        GameObject.Destroy(__instance.slots[__instance.slots.Length - 2].gameObject);
                        GameObject.Destroy(__instance.slots[__instance.slots.Length - 1].gameObject);
                        List<SaveSlot> slots = __instance.slots.ToList();
                        slots.RemoveAt(slots.Count() - 2);
                        slots.RemoveAt(slots.Count() - 1);
                        __instance.slots = slots.ToArray();
                        currentSlot--;
                        SetCurrentSlot(__instance, currentSlot);
                        Plugin.Instance.CurrentPlusObject = Plugin.Instance.CreatePlusObject();
                        if (Plugin.Instance.ExtraSlots <= 0) {
                            selectionX = 0;
                            SetSelectionX(__instance, selectionX);
                            Plugin.Instance.CurrentPlusObject.Find("Reset").SetActive(false);
                        }
                        return false;
                    }
                }
                if (selectionX == 0)
                {
                    if (!__instance.slots[currentSlot].gameObject.transform.Find("isExtensionSlot")) {
                        PlayerPrefs.SetInt("Slot", __instance.slots[currentSlot].ID);
                        UnityEngine.Object.FindObjectOfType<HomeScreen>().Star();
                    }
                    else {
                        Plugin.Instance.TurnPlusObjectIntoSave(Plugin.Instance.CurrentPlusObject);
                        Plugin.Instance.ExtraSlots++;
                        Plugin.Instance.CurrentPlusObject = Plugin.Instance.CreatePlusObject();
                        PlayerPrefs.SetInt("save-extender-extra-slots", Plugin.Instance.ExtraSlots);
                    }
                }
                if (selectionX == 2 && __instance.slots[currentSlot].Selects[2].gameObject.activeSelf)
                {
                    __instance.slots[currentSlot].Selects[2].gameObject.SetActive(value: false);
                    __instance.slots[currentSlot].Selects[3].gameObject.SetActive(value: true);
                    PlayerPrefs.SetInt("Timer" + __instance.slots[currentSlot].ID, 1);
                    return false;
                }
                if (selectionX == 2 && __instance.slots[currentSlot].Selects[3].gameObject.activeSelf)
                {
                    __instance.slots[currentSlot].Selects[2].gameObject.SetActive(value: true);
                    __instance.slots[currentSlot].Selects[3].gameObject.SetActive(value: false);
                    PlayerPrefs.SetInt("Timer" + __instance.slots[currentSlot].ID, 0);
                }
            }
            SaveSlot[] array = __instance.slots;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Selected(stat: false);
            }
            __instance.slots[currentSlot].Selected(stat: true);
            __instance.slots[currentSlot].selectionX = selectionX;
            return false;
        }
    }
}