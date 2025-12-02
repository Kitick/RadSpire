using Godot;
using System;
using System.Collections.Generic;
using InputSystem;
using SaveSystem;

namespace Host {
    public sealed partial class HostPanel : Control, ISaveable<HostPanelData> {
        public event Action? OnMenuClosed;
        private event Action? OnExit;

        private const string SAVEFILE = "host";

        public override void _Ready() {
            ProcessMode = ProcessModeEnum.Always;

            SetInputCallbacks();
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
                var data = SaveService. Load<HostPanelData>(SAVEFILE);
                Deserialize(data);
            }
        }

        private ISaveable<T> CastISaveable<T>(ISaveable<T> saveable) where T : ISaveData {
			return saveable;
		}

        public HostPanelData Serialize() => new HostPanelData {
            
        };

        public void Deserialize(in HostPanelData data) {
            
        }
    }
}

namespace SaveSystem {
    public readonly record struct HostPanelData : ISaveData {
        
    }
}