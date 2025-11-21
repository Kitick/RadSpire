using System;
using SaveSystem;

namespace Components 
	public class ItemBaseData : ISaveable<ItemBaseDataData> {
        //Basic properties of all items
        public string Id {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public string Name {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public string Description {
            get;
            set {
                if(string.IsNullOrWhiteSpace(value)) {
                    return;
                }
                field = value;
            }
        }

        public bool IsStackable {
            get;
            set {
                if(!value) {
                    MaxStackSize = 1;
                }
                field = value;
            }
        }
        
        public int MaxStackSize {
            get;
            set {
                if(value < 1) {
                    field = 1;
                    return;
                }
                if(!IsStackable && value > 1) {
                    field = 1;
                    return;
                }
                field = value;
            }
        }

        public bool IsConsumable {
            get;
            set;
        }

        public string IconPath {
            get;
            set;
        }

        //Optional components
        public WeaponBaseData? WeaponBaseComponent {
            get;
            set;
        }

        public ItemBaseDataData Serialize() => new ItemBaseDataData {
            Id = Id,
            Name = Name,
            Description = Description,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            IsConsumable = IsConsumable,
            IconPath = IconPath,
            WeaponBaseComponent = WeaponBaseComponent?.Serialize()
        };

		public void Deserialize(in ItemBaseDataData data) {
            Id = data.Id;
			Name = data.Name;
            Description = data.Description;
            IsStackable = data.IsStackable;
            MaxStackSize = data.MaxStackSize;
            IsConsumable = data.IsConsumable;
            IconPath = data.IconPath;

            if(data.WeaponBaseComponent is null) {
                WeaponBaseComponent = null;
                return;
            }
            else {
                WeaponBaseComponent = new WeaponBaseData(1);
                WeaponBaseComponent.Deserialize(data.WeaponBaseComponent.Value);
            }
		}
	}
}

namespace SaveSystem {
	public readonly struct ItemBaseDataData : ISaveData {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public bool IsStackable { get; init; }
        public int MaxStackSize { get; init; }
        public bool IsConsumable { get; init; }
        public string IconPath { get; init; }
        public WeaponBaseDataData? WeaponBaseComponent { get; init; }
	}
}