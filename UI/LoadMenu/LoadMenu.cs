using Godot;
using System;
using System.Collections.Generic;
using InputSystem;
using SaveSystem;

namespace LoadMenuScene {
    public sealed partial class LoadMenu : Control, ISaveable<LoadMenuData> {
        public event Action? OnMenuClosed;
        private event Action? OnExit;

        private const string SAVEFILE = "load";

        private const string BACK_BUTTON = "BackButton";
        private Button[] loadButtons = null!;
        private Label[] infoLabels = null!;

        public override void _Ready() {
            ProcessMode = ProcessModeEnum.Always;

            GetComponents();
            SetCallbacks();
            SetInputCallbacks();
        }

        private void GetComponents() {
            loadButtons = new Button[5];
            infoLabels = new Label[5];
            
            for(int i = 0; i < 5; i++) {
                loadButtons[i] = GetNode<Button>($"VLoadContainer/Load{i + 1}");
                infoLabels[i] = GetNode<Label>($"VLoadContainer/Load{i + 1}/InfoText{i + 1}");

                int slotIndex = i;
                loadButtons[i].Pressed += () => OnLoadSlotPressed(slotIndex);
                //Input for OnLoadSlotPresssed Here
            }

            LoadSavedGames();
        }

        private void SetCallbacks() {
            GetNode<Button>(BACK_BUTTON).Pressed += OnBackButtonPressed;
        }

        private void OnBackButtonPressed() {
            CloseMenu();
        }

        private void LoadSavedGames() {
            //Implementation Here
        }

        private void OnLoadSlotPressed(int index) {
            //Implementation Here
        }

        public override void _ExitTree() {
            OnExit?.Invoke();
            OnMenuClosed?.Invoke();
        }

        private void SetInputCallbacks() {
            OnExit += ActionEvent.MenuBack.WhenPressed(CloseMenu);
            OnExit += ActionEvent.MenuExit.WhenPressed(CloseMenu);
        }
       public void OpenMenu() {
            LoadData();
        }

        private void CloseMenu() {
            SaveData();
            QueueFree();
        }

        private void SaveData() {
            SaveService.Save(SAVEFILE, Serialize());
        }

        private void LoadData() {
            if(SaveService.Exists(SAVEFILE)) {
                var data = SaveService. Load<LoadMenuData>(SAVEFILE);
                Deserialize(data);
            }
        }

        private ISaveable<T> CastISaveable<T>(ISaveable<T> saveable) where T : ISaveData {
			return saveable;
		}

        public LoadMenuData Serialize() => new LoadMenuData {
            
        };

        public void Deserialize(in LoadMenuData data) {
            
        }
    }
}

namespace SaveSystem {
    public readonly record struct LoadMenuData : ISaveData {
        
    }
}