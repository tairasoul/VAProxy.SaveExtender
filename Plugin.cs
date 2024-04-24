using System.Reflection;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace SaveExtender {
    internal static class GameObjectExtensions {
        public static GameObject Find(this GameObject @object, string name) {
            return @object.transform.Find(name).gameObject;
        }
    }
    [BepInPlugin("tairasoul.vaproxy.saveextender", "SaveExtender", "1.0.0")]
    public class Plugin  : BaseUnityPlugin
    {
        internal int ExtraSlots = PlayerPrefs.GetInt("save-extender-extra-slots", 0);
        internal GameObject CurrentPlusObject;
        internal static Plugin Instance;
        internal Harmony harmony = new("tairasoul.vaproxy.saveextender");

        internal GameObject CreatePlusObject() {
            GameObject Slots = GameObject.Find("Canvas").Find("SlotSelect").Find("Scroll View").Find("Viewport").Find("Content");
            GameObject Slot = Slots.Find("Slot3");
            GameObject clone = Instantiate(Slot);
            int ChildAmount = Slots.transform.childCount + 1;
            SaveSlot save = clone.GetComponent<SaveSlot>();
            save.ID = ChildAmount;
            RectTransform transform = clone.GetComponent<RectTransform>();
            transform.localScale = new(1, 1, 1);
            clone.name = $"ExtraSlot{ChildAmount - 5}";
            clone.transform.SetParent(Slots.transform, false);
            clone.Find("select").SetActive(false);
            clone.Find("Timer").SetActive(false);
            clone.Find("TOFF").SetActive(false);
            if (ChildAmount - 5 > 0)  {
                clone.Find("Reset").GetComponent<Text>().text = "Remove last";
                clone.Find("Reset").GetComponent<RectTransform>().anchoredPosition = new(-264.2893f, -30f);
            }
            else
                clone.Find("Reset").SetActive(false);
            clone.Find("TON").SetActive(false);
            clone.Find("Start").GetComponent<Text>().text = "Create new save";
            clone.Find("Frame").Find("Continue").SetActive(false);
            GameObject IsSlot = new("isExtensionSlot");
            IsSlot.transform.SetParent(clone.transform);
            SaveSlotSelect select = Slots.GetComponent<SaveSlotSelect>();
            List<SaveSlot> slotList = select.slots.ToList();
            slotList.Add(save);
            select.slots = slotList.ToArray();
            return clone;
        }

        internal void TurnPlusObjectIntoSave(GameObject PlusObject) {
            Destroy(PlusObject.Find("isExtensionSlot"));
            PlusObject.Find("Timer").SetActive(true);
            PlusObject.Find("TOFF").SetActive(true);
            PlusObject.Find("Reset").SetActive(true);
            PlusObject.Find("Reset").GetComponent<Text>().text = "Reset";
            PlusObject.Find("Reset").GetComponent<RectTransform>().anchoredPosition = new(-189, -30);
            PlusObject.Find("Start").GetComponent<Text>().text = "Start";
        }

        void Awake() {
            Instance = this;
            SceneManager.sceneLoaded += SceneLoaded;
            if (ExtraSlots < 0) {
                ExtraSlots = 0;
                PlayerPrefs.SetInt("save-extender-extra-slots", 0);
            }
            harmony.PatchAll();
        }

        internal bool HasCreatedExtraSlots = false;

        void SceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == "Menu") {
                CurrentPlusObject = CreatePlusObject();
                if (!HasCreatedExtraSlots && ExtraSlots > 0) {
                    for (int i = 0; i < ExtraSlots; i++) {
                        TurnPlusObjectIntoSave(CurrentPlusObject);
                        CurrentPlusObject = CreatePlusObject();
                    }
                    HasCreatedExtraSlots = true;
                }
            }
        }
    }
}